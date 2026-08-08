import { InfoIcon } from "lucide-react"

import { FieldLabel } from "@/shared/components/ui/field"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/components/ui/tooltip"

const fieldHelp = {
  name: ["Name", "The general grocery item name, such as Tortillas or Milk."],
  category: ["Category", "Groups similar pantry items for browsing, presets, and shopping."],
  trackingUnit: ["Tracking unit", "How stock is counted: packages, individual items, weight, or volume."],
  brand: ["Brand", "The product brand, if you normally buy a particular one."],
  locations: ["Storage locations and starting quantities", "Where this item is stored and how much is currently in each location."],
  preferredProduct: ["Preferred product", "The particular variety or product you prefer to buy, such as wholemeal or low-fat."],
  packageSize: ["Package size", "The numeric amount contained in one package, such as 500 for a 500 g bag."],
  packageUnit: ["Package unit", "The unit used by the package size, such as g, kg, mL, L, or pack."],
  consumptionQuantity: ["Consumed quantity", "How much of this item you normally use during the consumption period."],
  consumptionPeriodDays: ["Consumption period (days)", "The number of days over which you normally use the consumed quantity."],
  bufferDays: ["Buffer days", "Extra days of stock to keep as a safety margin when calculating what to buy."],
  notes: ["Notes", "Optional information you want to remember about this pantry item."],
} as const

type PantryItemField = keyof typeof fieldHelp

export function PantryItemFieldLabel({ field, htmlFor }: Readonly<{ field: PantryItemField; htmlFor?: string }>) {
  const [label, description] = fieldHelp[field]
  return <div className="flex items-center gap-1.5"><FieldLabel htmlFor={htmlFor}>{label}</FieldLabel><Tooltip><TooltipTrigger type="button" aria-label={`About ${label}`} className="rounded-full text-muted-foreground outline-none hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring"><InfoIcon className="size-3.5" /></TooltipTrigger><TooltipContent className="max-w-72" sideOffset={6}>{description}</TooltipContent></Tooltip></div>
}
