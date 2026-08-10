"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useState } from "react"
import {
  BoxesIcon,
  ClipboardCheckIcon,
  HouseIcon,
  ListChecksIcon,
  LogOutIcon,
  SettingsIcon,
  UserIcon,
} from "lucide-react"
import { toast } from "sonner"

import { postApiAccountsLogout } from "@/lib/api/generated/accounts/accounts"
import { cn } from "@/lib/utilities/cn"
import { Brand } from "@/shared/components/layout/brand"
import { ThemeToggle } from "@/shared/components/layout/theme-toggle"
import { Button } from "@/shared/components/ui/button"

const primaryItems = [
  { href: "/app", label: "Dashboard", icon: HouseIcon, exact: true },
  { href: "/app/pantry", label: "Pantry", icon: BoxesIcon, exact: false },
  { href: "/app/stocktakes", label: "Stocktake", icon: ClipboardCheckIcon, exact: false },
  { href: "/app/shopping-lists", label: "Shopping lists", icon: ListChecksIcon, exact: false },
] as const

const settingsItem = { href: "/app/settings", label: "Settings", icon: SettingsIcon } as const

function isActive(pathname: string, href: string, exact = false) {
  return exact ? pathname === href : pathname === href || pathname.startsWith(`${href}/`)
}

export function ApplicationShell({ children }: Readonly<{ children: React.ReactNode }>) {
  const pathname = usePathname()
  const router = useRouter()
  const [isSigningOut, setIsSigningOut] = useState(false)

  async function signOut() {
    setIsSigningOut(true)
    try {
      await postApiAccountsLogout()
      router.replace("/login")
      router.refresh()
    } catch {
      toast.error("Unable to sign out. Please try again.")
      setIsSigningOut(false)
    }
  }

  return (
    <div className="min-h-svh bg-background">
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 border-r border-border/60 bg-surface p-4 shadow-[4px_0_24px_rgb(24_32_24_/_0.03)] backdrop-blur md:flex md:flex-col">
        <Brand className="px-2 py-1" />
        <nav aria-label="Primary navigation" className="mt-8 flex flex-1 flex-col gap-1">
          {primaryItems.map((item) => (
            <DesktopNavLink key={item.href} item={item} active={isActive(pathname, item.href, item.exact)} />
          ))}
        </nav>
        <div className="space-y-1 border-t pt-4">
          <DesktopNavLink item={settingsItem} active={isActive(pathname, settingsItem.href)} />
          <Button variant="ghost" className="w-full justify-start gap-3 px-3 text-muted-foreground cursor-pointer" onClick={signOut} disabled={isSigningOut}>
            <LogOutIcon className="size-4" aria-hidden="true" />
            {isSigningOut ? "Signing out…" : "Sign out"}
          </Button>
        </div>
      </aside>

      <div className="md:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-border/70 bg-background/90 px-4 backdrop-blur sm:px-6">
          <Brand className="md:hidden" />
          <p className="hidden text-sm text-muted-foreground md:block">Your pantry, kept practical.</p>
          <div className="flex items-center gap-1">
            <ThemeToggle />
            <Button variant="ghost" size="icon" aria-label="Open account settings" asChild>
              <Link href="/app/settings"><UserIcon aria-hidden="true" /></Link>
            </Button>
          </div>
        </header>
        <main className="mx-auto w-full max-w-7xl px-4 py-6 pb-24 sm:px-6 md:pb-8 lg:px-8">{children}</main>
      </div>

      <nav aria-label="Mobile navigation" className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-4 border-t border-border/70 bg-card/95 px-1 pb-[env(safe-area-inset-bottom)] backdrop-blur md:hidden">
        {primaryItems.map((item) => (
          <MobileNavLink key={item.href} item={item} active={isActive(pathname, item.href, item.exact)} />
        ))}
      </nav>
    </div>
  )
}

type NavigationItem = (typeof primaryItems)[number] | typeof settingsItem

function DesktopNavLink({ item, active }: Readonly<{ item: NavigationItem; active: boolean }>) {
  return (
    <Link href={item.href} aria-current={active ? "page" : undefined} className={cn("flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-[background-color,color,transform] duration-150 hover:bg-accent hover:text-accent-foreground hover:translate-x-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring", active && "bg-primary/10 text-primary hover:bg-primary/15 hover:text-primary")}>
      <item.icon className="size-4" aria-hidden="true" />
      {item.label}
    </Link>
  )
}

function MobileNavLink({ item, active }: Readonly<{ item: (typeof primaryItems)[number]; active: boolean }>) {
  return (
    <Link href={item.href} aria-current={active ? "page" : undefined} className={cn("flex min-h-16 flex-col items-center justify-center gap-1 rounded-md text-center text-[0.68rem] font-medium text-muted-foreground transition-colors duration-150 hover:bg-accent/70", active && "text-primary")}>
      <item.icon className="size-5" aria-hidden="true" />
      {item.label}
    </Link>
  )
}
