import Link from "next/link"
import { ShoppingBasketIcon } from "lucide-react"

import { cn } from "@/lib/utilities/cn"

export function Brand({ className }: Readonly<{ className?: string }>) {
  return (
    <Link
      href="/"
      className={cn("inline-flex items-center gap-2 font-semibold", className)}
    >
      <span className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground">
        <ShoppingBasketIcon className="size-5" aria-hidden="true" />
      </span>
      <span>Grocery Manager</span>
    </Link>
  )
}
