"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"

import { cn } from "@/lib/utilities/cn"

const items = [
  { href: "/app/pantry", label: "Items", exact: true },
  { href: "/app/pantry/categories", label: "Categories", exact: false },
  { href: "/app/pantry/locations", label: "Locations", exact: false },
] as const

export function PantryNavigation() {
  const pathname = usePathname()
  return (
    <nav aria-label="Pantry sections" className="flex gap-1 overflow-x-auto border-b">
      {items.map((item) => {
        const active = item.exact ? pathname === item.href || pathname.startsWith("/app/pantry/items/") || pathname === "/app/pantry/new" : pathname.startsWith(item.href)
        return <Link key={item.href} href={item.href} aria-current={active ? "page" : undefined} className={cn("border-b-[3px] border-transparent px-3 py-2 text-sm font-medium text-muted-foreground transition-colors duration-200 hover:text-foreground", active && "border-primary text-foreground")}>{item.label}</Link>
      })}
    </nav>
  )
}
