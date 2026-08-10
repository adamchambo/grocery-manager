"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useCallback, useEffect, useRef, useState, type CSSProperties } from "react"

import { cn } from "@/lib/utilities/cn"

const items = [
  { href: "/app/pantry", label: "Items", exact: true },
  { href: "/app/pantry/categories", label: "Categories", exact: false },
  { href: "/app/pantry/locations", label: "Locations", exact: false },
] as const

type RailStyle = CSSProperties & {
  left: number
  width: number
}

export function PantryNavigation() {
  const pathname = usePathname()
  const navRef = useRef<HTMLElement>(null)
  const linkRefs = useRef<(HTMLAnchorElement | null)[]>([])
  const interactingRef = useRef(false)
  const [railStyle, setRailStyle] = useState<RailStyle>({ left: 0, width: 0, opacity: 0 })
  const activeIndex = items.findIndex((item) => item.exact
    ? pathname === item.href || pathname.startsWith("/app/pantry/items/") || pathname === "/app/pantry/new"
    : pathname.startsWith(item.href))

  const positionRail = useCallback((index: number) => {
    const link = linkRefs.current[index]
    if (!link) return
    setRailStyle({ left: link.offsetLeft, width: link.offsetWidth, opacity: 1 })
  }, [])

  useEffect(() => {
    if (interactingRef.current || activeIndex < 0) return
    const frame = requestAnimationFrame(() => positionRail(activeIndex))
    return () => cancelAnimationFrame(frame)
  }, [activeIndex, positionRail])

  useEffect(() => {
    const nav = navRef.current
    if (!nav) return
    const observer = new ResizeObserver(() => {
      if (!interactingRef.current && activeIndex >= 0) positionRail(activeIndex)
    })
    observer.observe(nav)
    return () => observer.disconnect()
  }, [activeIndex, positionRail])

  function restoreActiveRail() {
    interactingRef.current = false
    if (activeIndex >= 0) positionRail(activeIndex)
  }

  return (
    <nav
      ref={navRef}
      aria-label="Pantry sections"
      className="relative flex gap-1 overflow-x-auto border-b border-border/50"
      onMouseLeave={restoreActiveRail}
      onBlur={(event) => { if (!event.currentTarget.contains(event.relatedTarget)) restoreActiveRail() }}
    >
      {items.map((item, index) => {
        const active = index === activeIndex
        return <Link ref={(element) => { linkRefs.current[index] = element }} key={item.href} href={item.href} aria-current={active ? "page" : undefined} onMouseEnter={() => { interactingRef.current = true; positionRail(index) }} onFocus={() => { interactingRef.current = true; positionRail(index) }} className={cn("px-3 py-2 text-sm font-medium text-muted-foreground transition-colors duration-200 hover:text-foreground focus-visible:text-foreground focus-visible:outline-none", active && "text-foreground")}>{item.label}</Link>
      })}
      <span aria-hidden="true" className="pointer-events-none absolute bottom-0 h-0.5 rounded-full bg-primary transition-[left,width,opacity] duration-300 ease-out motion-reduce:transition-none" style={railStyle} />
    </nav>
  )
}
