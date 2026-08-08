"use client"

import { useMemo, useState } from "react"
import { CircleHelpIcon, SearchIcon } from "lucide-react"

import { Button } from "@/shared/components/ui/button"
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/shared/components/ui/dialog"
import { Input } from "@/shared/components/ui/input"

type IconEntry = readonly [string, string]

const iconGroups: ReadonlyArray<{ label: string; icons: ReadonlyArray<IconEntry> }> = [
  { label: "Food", icons: [["🥫", "canned food pantry"], ["🥛", "milk dairy"], ["🧀", "cheese dairy"], ["🧈", "butter dairy"], ["🥚", "eggs"], ["🍞", "bread bakery"], ["🥐", "pastry bakery"], ["🥯", "bagel bakery"], ["🫓", "flatbread tortilla wrap"], ["🌮", "taco tortilla"], ["🥪", "sandwich lunch"], ["🍔", "burger meal"], ["🍕", "pizza meal"], ["🍣", "sushi meal"], ["🍲", "soup meal"], ["🍛", "curry meal"], ["🍚", "rice grain"], ["🍝", "pasta noodles"], ["🍜", "noodles ramen"], ["🥣", "cereal oats"], ["🥔", "potato vegetable"], ["🥬", "leafy greens vegetable"], ["🥕", "carrot vegetable"], ["🍅", "tomato vegetable"], ["🧅", "onion vegetable"], ["🧄", "garlic vegetable"], ["🌽", "corn vegetable"], ["🫑", "capsicum pepper vegetable"], ["🥦", "broccoli vegetable"], ["🍎", "apple fruit"], ["🍌", "banana fruit"], ["🍊", "orange citrus fruit"], ["🍓", "berry fruit"], ["🍇", "grapes fruit"], ["🍋", "lemon citrus fruit"], ["🥩", "meat steak"], ["🍗", "chicken poultry"], ["🥓", "bacon meat"], ["🐟", "fish seafood"], ["🦐", "prawns seafood"], ["🫘", "beans legumes"], ["🥜", "nuts peanut"], ["🧂", "salt seasoning"], ["🌶️", "chilli spice seasoning"], ["🫙", "jar sauce condiment"]], },
  { label: "Drinks", icons: [["☕", "coffee drink"], ["🫖", "tea drink"], ["🧃", "juice drink"], ["💧", "water drink"], ["🥤", "soft drink beverage"], ["🍺", "beer alcohol"], ["🍷", "wine alcohol"], ["🧋", "bubble tea drink"]], },
  { label: "Frozen and snacks", icons: [["❄️", "frozen"], ["🍦", "ice cream frozen"], ["🍫", "chocolate snack"], ["🍪", "biscuit cookie snack"], ["🍿", "popcorn snack"], ["🍩", "doughnut snack"], ["🍰", "cake dessert"], ["🍬", "lollies candy snack"]], },
  { label: "Household and other", icons: [["🧻", "paper household"], ["🧴", "cleaning household"], ["🧼", "soap cleaning"], ["🧽", "sponge cleaning"], ["🪣", "bucket cleaning"], ["🗑️", "bin rubbish bags"], ["🦴", "pet food supplies"], ["🐾", "pet supplies"], ["👶", "baby supplies"]], },
]

const icons = iconGroups.flatMap((group) => group.icons)
const legacyIconMap: Record<string, string> = {
  "fluent-emoji-high-contrast:canned-food": "🥫", "fluent-emoji-high-contrast:glass-of-milk": "🥛", "fluent-emoji-high-contrast:cheese-wedge": "🧀", "fluent-emoji-high-contrast:egg": "🥚", "fluent-emoji-high-contrast:bread": "🍞", "fluent-emoji-high-contrast:flatbread": "🫓", "fluent-emoji-high-contrast:taco": "🌮", "fluent-emoji-high-contrast:cooked-rice": "🍚", "fluent-emoji-high-contrast:spaghetti": "🍝", "fluent-emoji-high-contrast:potato": "🥔", "fluent-emoji-high-contrast:leafy-green": "🥬", "fluent-emoji-high-contrast:carrot": "🥕", "fluent-emoji-high-contrast:tomato": "🍅", "fluent-emoji-high-contrast:onion": "🧅", "fluent-emoji-high-contrast:red-apple": "🍎", "fluent-emoji-high-contrast:banana": "🍌", "fluent-emoji-high-contrast:cut-of-meat": "🥩", "fluent-emoji-high-contrast:fish": "🐟", "fluent-emoji-high-contrast:beans": "🫘", "fluent-emoji-high-contrast:hot-beverage": "☕", "fluent-emoji-high-contrast:teapot": "🫖", "fluent-emoji-high-contrast:snowflake": "❄️", "fluent-emoji-high-contrast:ice-cream": "🍦", "fluent-emoji-high-contrast:chocolate-bar": "🍫", "fluent-emoji-high-contrast:cookie": "🍪", "fluent-emoji-high-contrast:roll-of-paper": "🧻", "fluent-emoji-high-contrast:lotion-bottle": "🧴", "fluent-emoji-high-contrast:soap": "🧼", "fluent-emoji-high-contrast:paw-prints": "🐾",
}

export function getCategoryIcon(category: string) {
  const name = category.toLowerCase()
  if (name.includes("dairy") || name.includes("egg")) return "🥛"
  if (name.includes("meat") || name.includes("seafood")) return "🥩"
  if (name.includes("bakery")) return "🍞"
  if (name.includes("fruit") || name.includes("vegetable")) return "🥬"
  if (name.includes("drink")) return "☕"
  if (name.includes("frozen")) return "❄️"
  if (name.includes("household")) return "🧴"
  return "🥫"
}

export function PantryItemIcon({ icon, fallback = "🥫", className }: Readonly<{ icon: string | null; fallback?: string; className?: string }>) {
  const resolvedIcon = legacyIconMap[icon ?? ""] ?? (icon?.startsWith("fluent-emoji-high-contrast:") ? fallback : icon) ?? fallback
  return <span className={className}>{resolvedIcon}</span>
}

export function PantryItemIconPicker({ value, fallback = "🥫", onChange }: Readonly<{ value: string | null; fallback?: string; onChange: (icon: string | null) => void }>) {
  const [query, setQuery] = useState("")
  const matches = useMemo(() => icons.filter(([, keywords]) => keywords.includes(query.trim().toLowerCase())), [query])

  return <div className="flex items-center gap-3"><Dialog onOpenChange={(open) => { if (!open) setQuery("") }}><DialogTrigger asChild><Button type="button" variant="outline" className="size-16 text-3xl" aria-label="Choose item icon"><PantryItemIcon icon={value} fallback={fallback} /></Button></DialogTrigger><div><p className="text-sm font-medium">Item image</p><p className="text-sm text-muted-foreground">Choose an icon to recognise this item quickly.</p></div><DialogContent><DialogHeader><DialogTitle>Choose an item icon</DialogTitle><DialogDescription>Food, drinks, frozen goods, household essentials, and other pantry items.</DialogDescription></DialogHeader><div className="relative"><SearchIcon className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" /><Input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search icons, e.g. tortilla or pasta" className="pl-9" /></div>{query ? <IconGrid icons={matches} value={value} onChange={onChange} /> : <div className="max-h-80 space-y-4 overflow-y-auto pr-1">{iconGroups.map((group) => <div key={group.label}><h3 className="mb-2 text-sm font-medium">{group.label}</h3><IconGrid icons={group.icons} value={value} onChange={onChange} /></div>)}</div>}{query && matches.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No pantry icons match that search.</p>}<DialogFooter><DialogClose asChild><Button type="button" variant="ghost" onClick={() => onChange("❓")}><CircleHelpIcon />Use unknown icon</Button></DialogClose><DialogClose asChild><Button type="button" variant="outline">Cancel</Button></DialogClose></DialogFooter></DialogContent></Dialog></div>
}

function IconGrid({ icons, value, onChange }: Readonly<{ icons: readonly (readonly [string, string])[]; value: string | null; onChange: (icon: string | null) => void }>) {
  return <div className="grid grid-cols-6 gap-2 sm:grid-cols-8">{icons.map(([icon, keywords]) => <DialogClose key={icon} asChild><Button type="button" variant={value === icon ? "default" : "outline"} className="size-11 text-xl" aria-label={`Choose ${keywords} icon`} onClick={() => onChange(icon)}>{icon}</Button></DialogClose>)}</div>
}
