import { cn } from '@/lib/utils'

interface BadgeProps {
  variant?: 'default' | 'success' | 'destructive' | 'outline' | 'secondary'
  className?: string
  children: React.ReactNode
}

export function Badge({ variant = 'default', className, children }: BadgeProps) {
  return (
    <span className={cn(
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold',
      variant === 'default' && 'bg-primary text-primary-foreground',
      variant === 'success' && 'bg-green-100 text-green-800',
      variant === 'destructive' && 'bg-red-100 text-red-800',
      variant === 'outline' && 'border border-input text-foreground',
      variant === 'secondary' && 'bg-secondary text-secondary-foreground',
      className
    )}>
      {children}
    </span>
  )
}
