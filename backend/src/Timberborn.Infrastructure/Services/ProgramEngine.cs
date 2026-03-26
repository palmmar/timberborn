using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;

namespace Timberborn.Infrastructure.Services;

public class ProgramEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILeverCaller _leverCaller;
    private readonly ILogBroadcaster _broadcaster;

    // programId → "nodeId:handleId" → bool
    private readonly Dictionary<Guid, Dictionary<string, bool>> _gateInputStates = new();
    // programId → nodeId → CancellationTokenSource (pending delay timers)
    private readonly Dictionary<Guid, Dictionary<string, CancellationTokenSource>> _delayTimers = new();
    private readonly object _lock = new();

    public ProgramEngine(IServiceScopeFactory scopeFactory, ILeverCaller leverCaller, ILogBroadcaster broadcaster)
    {
        _scopeFactory = scopeFactory;
        _leverCaller = leverCaller;
        _broadcaster = broadcaster;
    }

    public async Task HandleSignalAsync(Adapter adapter, string? state, Guid adapterLogId)
    {
        if (state is null) return;

        using var scope = _scopeFactory.CreateScope();
        var programRepo = scope.ServiceProvider.GetRequiredService<IProgramRepository>();

        var programs = await programRepo.GetEnabledAsync();
        foreach (var program in programs)
        {
            GraphData? graph;
            try
            {
                graph = JsonSerializer.Deserialize<GraphData>(program.GraphJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { continue; }
            if (graph is null) continue;

            var adapterNodes = graph.Nodes
                .Where(n => n.Type == "adapterNode" && n.Data.AdapterId == adapter.Id.ToString())
                .ToList();

            foreach (var adapterNode in adapterNodes)
            {
                bool isOn = state == "on";
                var actions = Propagate(adapterNode.Id, "on", isOn, graph, program.Id, adapterLogId)
                    .Concat(Propagate(adapterNode.Id, "off", !isOn, graph, program.Id, adapterLogId))
                    .ToList();

                await FireActionsAsync(actions, program.Id, adapterLogId);
            }
        }
    }

    private async Task FireActionsAsync(List<(Guid leverId, string state)> actions, Guid programId, Guid adapterLogId)
    {
        if (actions.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var leverRepo = scope.ServiceProvider.GetRequiredService<ILeverRepository>();
        var actionLogRepo = scope.ServiceProvider.GetRequiredService<IActionLogRepository>();

        foreach (var (leverId, leverState) in actions)
        {
            var lever = await leverRepo.GetByIdAsync(leverId);
            if (lever is null || !lever.IsEnabled) continue;

            var result = await _leverCaller.CallAsync(lever, leverState);
            var actionLog = await actionLogRepo.CreateAsync(new ActionLog
            {
                Id = Guid.NewGuid(),
                LeverId = leverId,
                ProgramId = programId,
                AdapterLogId = adapterLogId,
                RequestBody = lever.BodyTemplate,
                ResponseStatusCode = result.StatusCode,
                ResponseBody = result.Body,
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                CalledAt = DateTime.UtcNow,
                Source = "Automation"
            });

            _broadcaster.Publish(new LogEvent("action_log", actionLog));
        }
    }

    private List<(Guid leverId, string state)> Propagate(
        string srcNodeId, string srcHandle, bool value, GraphData graph, Guid programId, Guid adapterLogId)
    {
        var results = new List<(Guid, string)>();

        var outEdges = graph.Edges
            .Where(e => e.Source == srcNodeId && e.SourceHandle == srcHandle)
            .ToList();

        foreach (var edge in outEdges)
        {
            var targetNode = graph.Nodes.FirstOrDefault(n => n.Id == edge.Target);
            if (targetNode is null) continue;

            switch (targetNode.Type)
            {
                case "leverNode":
                    if (value && Guid.TryParse(targetNode.Data.LeverId, out var leverId))
                        results.Add((leverId, edge.TargetHandle ?? "on"));
                    break;

                case "andNode":
                case "orNode":
                {
                    bool changed;
                    bool newOut;
                    lock (_lock)
                    {
                        if (!_gateInputStates.ContainsKey(programId))
                            _gateInputStates[programId] = new Dictionary<string, bool>();
                        var stateDict = _gateInputStates[programId];
                        stateDict[$"{targetNode.Id}:{edge.TargetHandle}"] = value;

                        int inputCount = targetNode.Data.InputCount ?? 2;
                        var inputVals = Enumerable.Range(0, inputCount)
                            .Select(i => stateDict.TryGetValue($"{targetNode.Id}:in{i}", out var v) ? v : false)
                            .ToList();
                        newOut = targetNode.Type == "andNode"
                            ? inputVals.All(x => x)
                            : inputVals.Any(x => x);

                        string outKey = $"{targetNode.Id}:out";
                        bool prevOut = stateDict.TryGetValue(outKey, out var po) ? po : false;
                        changed = newOut != prevOut;
                        if (changed) stateDict[outKey] = newOut;
                    }
                    if (changed)
                        results.AddRange(Propagate(targetNode.Id, "out", newOut, graph, programId, adapterLogId));
                    break;
                }

                case "notNode":
                {
                    bool notOut = !value;
                    lock (_lock)
                    {
                        if (!_gateInputStates.ContainsKey(programId))
                            _gateInputStates[programId] = new Dictionary<string, bool>();
                        _gateInputStates[programId][$"{targetNode.Id}:out"] = notOut;
                    }
                    results.AddRange(Propagate(targetNode.Id, "out", notOut, graph, programId, adapterLogId));
                    break;
                }

                case "delayNode":
                {
                    int delaySecs = targetNode.Data.DelaySeconds ?? 5;
                    string nodeId = targetNode.Id;
                    string outKey = $"{nodeId}:out";

                    lock (_lock)
                    {
                        _gateInputStates.TryAdd(programId, new Dictionary<string, bool>());
                        _delayTimers.TryAdd(programId, new Dictionary<string, CancellationTokenSource>());
                    }

                    if (value)
                    {
                        CancellationTokenSource cts;
                        lock (_lock)
                        {
                            if (_delayTimers[programId].TryGetValue(nodeId, out var oldCts))
                            {
                                oldCts.Cancel();
                                oldCts.Dispose();
                            }
                            cts = new CancellationTokenSource();
                            _delayTimers[programId][nodeId] = cts;
                        }

                        var capturedGraph = graph;
                        var capturedProgramId = programId;
                        var capturedAdapterLogId = adapterLogId;
                        _ = Task.Run(async () =>
                        {
                            try { await Task.Delay(TimeSpan.FromSeconds(delaySecs), cts.Token); }
                            catch (OperationCanceledException) { return; }

                            List<(Guid, string)> timerActions;
                            lock (_lock)
                            {
                                _gateInputStates[capturedProgramId][outKey] = true;
                                _delayTimers[capturedProgramId].Remove(nodeId);
                            }
                            timerActions = Propagate(nodeId, "out", true, capturedGraph, capturedProgramId, capturedAdapterLogId);
                            await FireActionsAsync(timerActions, capturedProgramId, capturedAdapterLogId);
                            await BroadcastSignalUpdatesAsync();
                        });
                    }
                    else
                    {
                        bool wasOn;
                        lock (_lock)
                        {
                            if (_delayTimers[programId].TryGetValue(nodeId, out var oldCts))
                            {
                                oldCts.Cancel();
                                oldCts.Dispose();
                                _delayTimers[programId].Remove(nodeId);
                            }
                            wasOn = _gateInputStates[programId].TryGetValue(outKey, out var v) && v;
                            if (wasOn) _gateInputStates[programId][outKey] = false;
                        }
                        if (wasOn)
                            results.AddRange(Propagate(nodeId, "out", false, graph, programId, adapterLogId));
                    }
                    break;
                }
            }
        }

        return results;
    }

    public async Task<Dictionary<string, bool>> GetSignalsAsync(Guid programId)
    {
        using var scope = _scopeFactory.CreateScope();
        var programRepo = scope.ServiceProvider.GetRequiredService<IProgramRepository>();
        var adapterRepo = scope.ServiceProvider.GetRequiredService<IAdapterRepository>();

        var program = await programRepo.GetByIdAsync(programId);
        if (program is null) return new();

        var adapters = await adapterRepo.GetAllAsync();
        var adapterLastStates = adapters.ToDictionary(a => a.Id.ToString(), a => a.LastState);

        Dictionary<string, bool>? seedSignals = null;
        lock (_lock)
        {
            if (_gateInputStates.TryGetValue(programId, out var programState))
                seedSignals = new Dictionary<string, bool>(programState);
        }

        return ComputeSignals(program.GraphJson, adapterLastStates, seedSignals);
    }

    public async Task BroadcastSignalUpdatesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var programRepo = scope.ServiceProvider.GetRequiredService<IProgramRepository>();
        var adapterRepo = scope.ServiceProvider.GetRequiredService<IAdapterRepository>();

        var adapters = await adapterRepo.GetAllAsync();
        var adapterLastStates = adapters.ToDictionary(a => a.Id.ToString(), a => a.LastState);

        var programs = await programRepo.GetEnabledAsync();
        foreach (var program in programs)
        {
            Dictionary<string, bool>? seedSignals = null;
            lock (_lock)
            {
                if (_gateInputStates.TryGetValue(program.Id, out var programState))
                    seedSignals = new Dictionary<string, bool>(programState);
            }

            var signals = ComputeSignals(program.GraphJson, adapterLastStates, seedSignals);
            _broadcaster.Publish(new LogEvent("signal_update", new { programId = program.Id, signals }));
        }
    }

    public static Dictionary<string, bool> ComputeSignals(
        string graphJson,
        Dictionary<string, string?> adapterLastStates,
        Dictionary<string, bool>? seedSignals = null)
    {
        // Seed with any stored state (e.g. delay node outputs)
        var result = seedSignals != null
            ? new Dictionary<string, bool>(seedSignals)
            : new Dictionary<string, bool>();

        GraphData? graph;
        try
        {
            graph = JsonSerializer.Deserialize<GraphData>(graphJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return result; }
        if (graph is null) return result;

        foreach (var node in graph.Nodes.Where(n => n.Type == "adapterNode" && n.Data.AdapterId is not null))
        {
            var lastState = adapterLastStates.GetValueOrDefault(node.Data.AdapterId!);
            result[$"{node.Id}:on"] = lastState == "on";
            result[$"{node.Id}:off"] = lastState == "off";
        }

        // Iterative propagation until stable (handles arbitrary topologies)
        bool changed = true;
        int guard = graph.Nodes.Count + 1;
        while (changed && guard-- > 0)
        {
            changed = false;
            foreach (var edge in graph.Edges)
            {
                if (!result.TryGetValue($"{edge.Source}:{edge.SourceHandle}", out bool srcVal)) continue;
                var target = graph.Nodes.FirstOrDefault(n => n.Id == edge.Target);
                if (target is null) continue;

                switch (target.Type)
                {
                    case "andNode":
                    case "orNode":
                    {
                        string inKey = $"{target.Id}:{edge.TargetHandle}";
                        if (!result.TryGetValue(inKey, out bool prev) || prev != srcVal)
                        { result[inKey] = srcVal; changed = true; }

                        int inputCount = target.Data.InputCount ?? 2;
                        bool newOut = target.Type == "andNode"
                            ? Enumerable.Range(0, inputCount).All(i => result.TryGetValue($"{target.Id}:in{i}", out var v) && v)
                            : Enumerable.Range(0, inputCount).Any(i => result.TryGetValue($"{target.Id}:in{i}", out var v) && v);
                        string outKey = $"{target.Id}:out";
                        if (!result.TryGetValue(outKey, out bool prevOut) || prevOut != newOut)
                        { result[outKey] = newOut; changed = true; }
                        break;
                    }
                    case "notNode":
                    {
                        string outKey = $"{target.Id}:out";
                        bool notOut = !srcVal;
                        if (!result.TryGetValue(outKey, out bool prevOut) || prevOut != notOut)
                        { result[outKey] = notOut; changed = true; }
                        break;
                    }
                    // delayNode: output is timer-driven; seeded above from engine state, not recomputed here
                }
            }
        }

        return result;
    }

    private record GraphData(List<GraphNode> Nodes, List<GraphEdge> Edges);
    private record GraphNode(string Id, string Type, GraphNodeData Data);
    private record GraphNodeData(string? AdapterId, string? LeverId, string? Name, int? InputCount, int? DelaySeconds);
    private record GraphEdge(string Id, string Source, string SourceHandle, string Target, string? TargetHandle);
}
