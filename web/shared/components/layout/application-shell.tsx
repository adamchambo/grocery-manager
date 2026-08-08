"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  BoxesIcon,
  ClipboardCheckIcon,
  EllipsisIcon,
  HistoryIcon,
  HouseIcon,
  ListChecksIcon,
  SettingsIcon,
  SlidersHorizontalIcon,
  UserIcon,
} from "lucide-react"

import { cn } from "@/lib/utilities/cn"
import { Brand } from "@/shared/components/layout/brand"
import { Button } from "@/shared/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu"

const primaryItems = [
  { href: "/app", label: "Dashboard", icon: HouseIcon, exact: true },
  { href: "/app/pantry", label: "Pantry", icon: BoxesIcon, exact: false },
  { href: "/app/stocktakes", label: "Stocktake", icon: ClipboardCheckIcon, exact: false },
  { href: "/app/shopping-lists", label: "Shopping lists", icon: ListChecksIcon, exact: false },
] as const

const moreItems = [
  { href: "/app/presets", label: "Shopping presets", icon: SlidersHorizontalIcon },
  { href: "/app/history", label: "Inventory history", icon: HistoryIcon },
  { href: "/app/settings/account", label: "Settings", icon: SettingsIcon },
] as const

function isActive(pathname: string, href: string, exact = false) {
  return exact ? pathname === href : pathname === href || pathname.startsWith(`${href}/`)
}

export function ApplicationShell({ children }: Readonly<{ children: React.ReactNode }>) {
  const pathname = usePathname()
  const moreIsActive = moreItems.some((item) => isActive(pathname, item.href))

  return (
    <div className="min-h-svh bg-background">
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 border-r bg-card p-4 md:flex md:flex-col">
        <Brand className="px-2 py-1" />
        <nav aria-label="Primary navigation" className="mt-8 flex flex-1 flex-col gap-1">
          {primaryItems.map((item) => (
            <DesktopNavLink key={item.href} item={item} active={isActive(pathname, item.href, item.exact)} />
          ))}
          <p className="mb-1 mt-6 px-3 text-xs font-medium uppercase tracking-wider text-muted-foreground">More</p>
          {moreItems.map((item) => (
            <DesktopNavLink key={item.href} item={item} active={isActive(pathname, item.href)} />
          ))}
        </nav>
      </aside>

      <div className="md:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b bg-background/95 px-4 backdrop-blur sm:px-6">
          <Brand className="md:hidden" />
          <p className="hidden text-sm text-muted-foreground md:block">Your pantry, kept practical.</p>
          <Button variant="ghost" size="icon" aria-label="Open account settings" asChild>
            <Link href="/app/settings/account"><UserIcon aria-hidden="true" /></Link>
          </Button>
        </header>
        <main className="mx-auto w-full max-w-7xl px-4 py-6 pb-24 sm:px-6 md:pb-8 lg:px-8">{children}</main>
      </div>

      <nav aria-label="Mobile navigation" className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-5 border-t bg-card px-1 pb-[env(safe-area-inset-bottom)] md:hidden">
        {primaryItems.map((item) => (
          <MobileNavLink key={item.href} item={item} active={isActive(pathname, item.href, item.exact)} />
        ))}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button className={cn("flex min-h-16 flex-col items-center justify-center gap-1 text-[0.68rem] font-medium text-muted-foreground", moreIsActive && "text-primary")}>
              <EllipsisIcon className="size-5" aria-hidden="true" />
              More
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" side="top" className="w-56">
            <DropdownMenuLabel>More</DropdownMenuLabel>
            <DropdownMenuSeparator />
            {moreItems.map((item) => (
              <DropdownMenuItem key={item.href} asChild>
                <Link href={item.href}><item.icon aria-hidden="true" />{item.label}</Link>
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </nav>
    </div>
  )
}

type NavigationItem = (typeof primaryItems)[number] | (typeof moreItems)[number]

function DesktopNavLink({ item, active }: Readonly<{ item: NavigationItem; active: boolean }>) {
  return (
    <Link href={item.href} aria-current={active ? "page" : undefined} className={cn("flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground", active && "bg-accent text-accent-foreground")}>
      <item.icon className="size-4" aria-hidden="true" />
      {item.label}
    </Link>
  )
}

function MobileNavLink({ item, active }: Readonly<{ item: (typeof primaryItems)[number]; active: boolean }>) {
  return (
    <Link href={item.href} aria-current={active ? "page" : undefined} className={cn("flex min-h-16 flex-col items-center justify-center gap-1 text-center text-[0.68rem] font-medium text-muted-foreground", active && "text-primary")}>
      <item.icon className="size-5" aria-hidden="true" />
      {item.label}
    </Link>
  )
}
