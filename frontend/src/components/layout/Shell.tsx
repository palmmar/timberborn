import { Link, useLocation, Outlet } from 'react-router-dom'
import { cn } from '@/lib/utils'
import { LayoutDashboard, Zap, Radio, Workflow, History } from 'lucide-react'

const nav = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/levers', label: 'Levers', icon: Zap },
  { to: '/adapters', label: 'Adapters', icon: Radio },
  { to: '/programs', label: 'Programs', icon: Workflow },
  { to: '/history', label: 'History', icon: History },
]

export function Shell() {
  const { pathname } = useLocation()
  return (
    <div className="flex h-screen bg-background">
      <aside className="w-56 border-r flex flex-col">
        <div className="p-4 border-b">
          <h1 className="font-bold text-lg">Timberborn</h1>
          <p className="text-xs text-muted-foreground">Automation</p>
        </div>
        <nav className="flex-1 p-2 space-y-1">
          {nav.map(({ to, label, icon: Icon }) => (
            <Link
              key={to}
              to={to}
              className={cn(
                'flex items-center gap-2 rounded-md px-3 py-2 text-sm transition-colors',
                pathname === to
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
              )}
            >
              <Icon size={16} />
              {label}
            </Link>
          ))}
        </nav>
      </aside>
      <main className="flex-1 overflow-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
