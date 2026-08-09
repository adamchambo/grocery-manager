"use client"

import Link from "next/link"
import { useEffect, useMemo, useState } from "react"
import { MapPinIcon, PackagePlusIcon, SearchIcon, SearchXIcon } from "lucide-react"

import { getApiCategories } from "@/lib/api/generated/categories/categories"
import type { CategoryResponse, PantryItemResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { getApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiStorageLocations } from "@/lib/api/generated/storage-locations/storage-locations"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Input } from "@/shared/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"
import { getCategoryIcon as categoryIcon, PantryItemIcon } from "@/features/pantry/components/pantry-item-icon-picker"

export function PantryOverview() {
  const [items, setItems] = useState<PantryItemResponse[]>([])
  const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [locations, setLocations] = useState<StorageLocationResponse[]>([])
  const [query, setQuery] = useState("")
  const [categoryId, setCategoryId] = useState("all")
  const [locationId, setLocationId] = useState("all")
  const [sort, setSort] = useState("alphabetical")
  const [error, setError] = useState<string>()
  const [loading, setLoading] = useState(true)
  useErrorToast(error, "Pantry unavailable")

  useEffect(() => {
    Promise.all([getApiPantryItems({ page: 1, pageSize: 100 }), getApiCategories(), getApiStorageLocations()])
      .then(([response, categoryRows, locationRows]) => {
        setItems(response.items)
        setCategories(categoryRows.filter((row) => !row.isArchived))
        setLocations(locationRows.filter((row) => !row.isArchived))
      })
      .catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load pantry items.")))
      .finally(() => setLoading(false))
  }, [])

  const filteredItems = useMemo(() => {
    const filtered = items.filter((item) =>
      item.name.toLowerCase().includes(query.trim().toLowerCase())
      && (categoryId === "all" || item.categoryId === categoryId)
      && (locationId === "all" || item.locations.some((location) => location.storageLocationId === locationId)))
    return filtered.sort((left, right) => {
      if (sort === "recent") return Date.parse(right.createdAtUtc) - Date.parse(left.createdAtUtc)
      if (sort === "category") return left.categoryName.localeCompare(right.categoryName) || left.name.localeCompare(right.name)
      if (sort === "location") return firstLocation(left).localeCompare(firstLocation(right)) || left.name.localeCompare(right.name)
      return left.name.localeCompare(right.name)
    })
  }, [categoryId, items, locationId, query, sort])

  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>

  return (
    <div className="space-y-5">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div><h1 className="text-2xl font-semibold tracking-tight">Pantry items</h1><p className="mt-1 text-sm text-muted-foreground">Track the products and locations that make up your pantry.</p></div>
        <Button asChild className="transition-all duration-200"><Link href="/app/pantry/new"><PackagePlusIcon />Add item</Link></Button>
      </header>
      <div className="flex flex-wrap gap-3">
        <Card className="w-52 gap-1 self-start py-3 shadow-none"><CardHeader className="px-4"><CardTitle className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Tracked products</CardTitle></CardHeader><CardContent className="px-4 text-xl font-semibold">{items.length}</CardContent></Card>
        <Card className="w-52 gap-1 self-start py-3 shadow-none"><CardHeader className="px-4"><CardTitle className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Storage locations</CardTitle></CardHeader><CardContent className="px-4 text-xl font-semibold">{locations.length}</CardContent></Card>
      </div>
      <div className="grid gap-3 rounded-2xl bg-surface-muted/80 p-3 md:grid-cols-[minmax(14rem,1fr)_12rem_12rem_12rem]">
        <div className="relative"><SearchIcon className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" /><Input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search products" className="bg-background pl-9 transition-shadow duration-200" /></div>
        <Select value={categoryId} onValueChange={setCategoryId}><SelectTrigger className="w-full bg-background transition-all duration-200"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">All categories</SelectItem>{categories.map((category) => <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>)}</SelectContent></Select>
        <Select value={locationId} onValueChange={setLocationId}><SelectTrigger className="w-full bg-background transition-all duration-200"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">All locations</SelectItem>{locations.map((location) => <SelectItem key={location.id} value={location.id}>{location.name}</SelectItem>)}</SelectContent></Select>
        <Select value={sort} onValueChange={setSort}><SelectTrigger className="w-full bg-background transition-all duration-200"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="alphabetical">Alphabetical</SelectItem><SelectItem value="recent">Recently added</SelectItem><SelectItem value="category">Category</SelectItem><SelectItem value="location">Location</SelectItem></SelectContent></Select>
      </div>
      {filteredItems.length === 0 ? (
        <Card className="gap-3 py-8 text-center">
          <CardHeader className="w-full justify-items-center px-6">
            <span className="mb-1 flex size-11 items-center justify-center rounded-full bg-muted text-muted-foreground"><SearchXIcon className="size-5" aria-hidden="true" /></span>
            <CardTitle className="whitespace-nowrap">{items.length ? "No matching products" : "Your pantry is empty"}</CardTitle>
          </CardHeader>
          <CardContent className="w-full space-y-4 px-6 text-sm text-muted-foreground">
            <p>{items.length ? "Try a different search or clear the current filters." : "Add your first product to start tracking stock."}</p>
            {items.length ? <Button variant="outline" onClick={() => { setQuery(""); setCategoryId("all"); setLocationId("all") }}>Clear filters</Button> : <Button asChild><Link href="/app/pantry/new">Add first item</Link></Button>}
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {filteredItems.map((item) => <PantryItemCard key={item.id} item={item} />)}
        </div>
      )}
    </div>
  )
}

function PantryItemCard({ item }: Readonly<{ item: PantryItemResponse }>) {
  const quantity = item.locations.reduce((total, location) => total + Number(location.currentQuantity), 0)
  const stock = stockStatus(item, quantity)
  return <Link href={`/app/pantry/items/${item.id}`} className="group cursor-pointer rounded-2xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"><Card className="h-full gap-3 py-4 shadow-xs group-hover:-translate-y-0.5 group-hover:border-primary/20 group-hover:shadow-md"><CardHeader className="px-5"><div className="flex items-start justify-between gap-4"><div className="min-w-0"><CardTitle className="truncate text-lg"><PantryItemIcon icon={item.icon} fallback={categoryIcon(item.categoryName)} className="mr-1 inline-block size-5 align-text-bottom" />{item.name}</CardTitle><p className="mt-1 text-sm text-muted-foreground">{item.categoryName}</p></div><Badge variant="ghost" className={`shrink-0 px-1.5 py-0 text-[0.68rem] ${stock.className}`}><span className="size-1.5 rounded-full bg-current" />{formatQuantity(quantity, item.trackingUnit)}</Badge></div></CardHeader><CardContent className="px-5"><p className="flex items-center gap-2 text-sm text-muted-foreground"><MapPinIcon className="size-4 shrink-0" />{item.locations.map((location) => location.storageLocationName).join(", ")}</p></CardContent></Card></Link>
}

function formatQuantity(quantity: number, unit: number) {
  return `${quantity} ${["packages", "items", "weight", "volume"][unit] ?? "units"}`
}

function firstLocation(item: PantryItemResponse) { return item.locations[0]?.storageLocationName ?? "" }

function stockStatus(item: PantryItemResponse, quantity: number) {
  if (quantity <= 0) return { className: "bg-destructive/10 text-destructive" }
  if (item.consumptionQuantity !== null && quantity <= Number(item.consumptionQuantity)) return { className: "bg-warning/10 text-warning" }
  return { className: "bg-success/10 text-success" }
}
