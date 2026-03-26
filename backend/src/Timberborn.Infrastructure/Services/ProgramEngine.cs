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
        var leverRepo = scope.ServiceProvider.GetRequiredService<ILeverRepository>();
        var actionLogRepo = scope.ServiceProvider.GetRequiredService<IActionLogRepository>();

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
                var actions = Propagate(adapterNode.Id, "on", isOn, graph, program.Id)
                    .Concat(Propagate(adapterNode.Id, "off", !isOn, graph, program.Id))
                    .ToList();

                foreach (var (leverId, leverState) in actions)
                {
                    var lever = await leverRepo.GetByIdAsync(leverId);
                    if (lever is null || !lever.IsEnabled) continue;

                    var result = await _leverCaller.CallAsync(lever, leverState);
                    var actionLog = await actionLogRepo.CreateAsync(new ActionLog
                    {
                        Id = Guid.NewGuid(),
                        LeverId = leverId,
                        ProgramId = program.Id,
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
        }
    }

    private List<(Guid leverId, string state)> Propagate(
        string srcNodeId, string srcHandle, bool value, GraphData graph, Guid programId)
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
                        results.AddRange(Propagate(targetNode.Id, "out", newOut, graph, programId));
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
                    results.AddRange(Propagate(targetNode.Id, "out", notOut, graph, programId));
                    break;
                }
            }
        }

        return results;
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
            var signals = ComputeSignals(program.GraphJson, adapterLastStates);
            _broadcaster.Publish(new LogEvent("signal_update", new { programId = program.Id, signals }));
        }
    }

    public static Dictionary<string, bool> ComputeSignals(string graphJson, Dictionary<string, string?> adapterLastStates)
    {
        var result = new Dictionary<string, bool>();
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
                }
            }
        }

        return result;
    }

    private record GraphData(List<GraphNode> Nodes, List<GraphEdge> Edges);
    private record GraphNode(string Id, string Type, GraphNodeData Data);
    private record GraphNodeData(string? AdapterId, string? LeverId, string? Name, int? InputCount);
    private record GraphEdge(string Id, string Source, string SourceHandle, string Target, string? TargetHandle);
}
