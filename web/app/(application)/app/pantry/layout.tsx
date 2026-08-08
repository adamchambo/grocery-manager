import { PantryNavigation } from "@/features/pantry/components/pantry-navigation"

export default function PantryLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <div className="space-y-6"><PantryNavigation />{children}</div>
}
