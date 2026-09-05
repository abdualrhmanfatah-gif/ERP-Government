import { Button as ButtonPrimitive } from "@base-ui/react/button"
import { cva, type VariantProps } from "class-variance-authority"

import { cn } from "@/lib/utils"

const buttonVariants = cva(
  "group/button inline-flex shrink-0 items-center justify-center rounded-lg border border-transparent bg-clip-padding text-sm font-medium whitespace-nowrap transition-colors transition-transform outline-none select-none active:not-aria-[haspopup]:translate-y-px disabled:pointer-events-none aria-invalid:border-[var(--color-error)] aria-invalid:ring-3 aria-invalid:ring-[color-mix(in_srgb,var(--color-error)_20%,transparent)] dark:aria-invalid:border-[color-mix(in_srgb,var(--color-error)_50%,transparent)] dark:aria-invalid:ring-[color-mix(in_srgb,var(--color-error)_40%,transparent)] [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4",
  {
    variants: {
      variant: {
        default:
          "bg-[var(--color-primary)] text-[var(--color-on-primary)] hover:bg-[var(--color-primary-container)] hover:text-[var(--color-on-primary)] active:bg-[var(--color-primary-container)] disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:opacity-100",
        primary:
          "bg-[var(--color-primary)] text-[var(--color-on-primary)] hover:bg-[var(--color-primary-container)] hover:text-[var(--color-on-primary)] active:bg-[var(--color-primary-container)] disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:opacity-100",
        outline:
          "border border-[var(--color-primary)] bg-transparent text-[var(--color-primary)] hover:bg-[var(--color-surface-container-low)] hover:text-[var(--color-primary)] active:bg-[var(--color-surface-container)] aria-expanded:bg-[var(--color-surface-container-low)] aria-expanded:text-[var(--color-primary)] disabled:border-[color-mix(in_srgb,var(--color-disabled-fg)_30%,transparent)] disabled:text-[var(--color-disabled-fg)] disabled:bg-transparent disabled:opacity-100",
        secondary:
          "border border-[var(--color-secondary-container)] bg-[var(--color-secondary-container)] text-[var(--color-on-secondary-container)] hover:bg-[var(--color-secondary)] hover:text-[var(--color-on-secondary)] active:bg-[var(--color-secondary)] disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:opacity-100",
        ghost:
          "border-transparent bg-transparent text-[var(--color-on-surface)] hover:bg-[var(--color-surface-container-low)] hover:text-[var(--color-on-surface)] active:bg-[var(--color-surface-container)] aria-expanded:bg-[var(--color-surface-container-low)] aria-expanded:text-[var(--color-on-surface)] disabled:text-[var(--color-disabled-fg)] disabled:opacity-100",
        destructive:
          "bg-[var(--color-error)] text-[var(--color-on-error)] hover:bg-[var(--color-error-container)] hover:text-[var(--color-on-error-container)] active:bg-[var(--color-error)] disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:opacity-100 focus-visible:border-[color-mix(in_srgb,var(--color-error)_40%,transparent)] focus-visible:ring-[color-mix(in_srgb,var(--color-error)_20%,transparent)] dark:bg-[var(--color-error)] dark:hover:bg-[var(--color-error-container)] dark:focus-visible:ring-[color-mix(in_srgb,var(--color-error)_40%,transparent)]",
        link: "text-[var(--color-link)] underline-offset-4 hover:underline disabled:text-[var(--color-disabled-fg)] disabled:no-underline",
      },
      size: {
        default:
          "h-11 gap-1.5 px-4 has-data-[icon=inline-end]:pe-3 has-data-[icon=inline-start]:ps-3",
        xs: "h-8 gap-1 rounded-[min(var(--radius-md),10px)] px-2.5 text-xs in-data-[slot=button-group]:rounded-lg has-data-[icon=inline-end]:pe-1.5 has-data-[icon=inline-start]:ps-1.5 [&_svg:not([class*='size-'])]:size-3",
        sm: "h-9 gap-1.5 rounded-[min(var(--radius-md),12px)] px-3 text-[0.8rem] in-data-[slot=button-group]:rounded-lg has-data-[icon=inline-end]:pe-2 has-data-[icon=inline-start]:ps-2 [&_svg:not([class*='size-'])]:size-3.5",
        lg: "h-12 gap-1.5 px-4 has-data-[icon=inline-end]:pe-3 has-data-[icon=inline-start]:ps-3",
        icon: "size-11",
        "icon-xs":
          "size-9 rounded-[min(var(--radius-md),10px)] in-data-[slot=button-group]:rounded-lg [&_svg:not([class*='size-'])]:size-3",
        "icon-sm":
          "size-10 rounded-[min(var(--radius-md),12px)] in-data-[slot=button-group]:rounded-lg",
        "icon-lg": "size-12",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

function Button({
  className,
  variant = "default",
  size = "default",
  loading,
  icon,
  iconEnd,
  children,
  ...props
}: ButtonPrimitive.Props & VariantProps<typeof buttonVariants> & { loading?: boolean; icon?: React.ReactNode; iconEnd?: React.ReactNode }) {
  return (
    <ButtonPrimitive
      data-slot="button"
      className={cn(buttonVariants({ variant, size, className }))}
      aria-busy={loading || undefined}
      disabled={loading || props.disabled}
      {...props}
    >
      {icon && <span className="shrink-0 [&_svg]:size-4">{icon}</span>}
      {children}
      {iconEnd && <span className="shrink-0 [&_svg]:size-4">{iconEnd}</span>}
    </ButtonPrimitive>
  )
}

export { Button, buttonVariants }
