"use client"

import { useEffect, useState, type FormEvent } from "react"
import { useRouter } from "next/navigation"

import { getApiCategories } from "@/lib/api/generated/categories/categories"
import type { CategoryResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { postApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiStorageLocations } from "@/lib/api/generated/storage-locations/storage-locations"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Checkbox } from "@/shared/components/ui/checkbox"
import { Field, FieldGroup } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { Textarea } from "@/shared/components/ui/textarea"
import { PantryItemFieldLabel } from "@/features/pantry/components/pantry-item-field-label"
import { PantryItemIconPicker } from "@/features/pantry/components/pantry-item-icon-picker"
import { showErrorToast, showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function NewPantryItemForm() {
  const router = useRouter()
  const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [locations, setLocations] = useState<StorageLocationResponse[]>([])
  const [categoryId, setCategoryId] = useState("")
  const [selectedLocations, setSelectedLocations] = useState<Record<string, string>>({})
  const [trackingUnit, setTrackingUnit] = useState("0")
  const [icon, setIcon] = useState<string | null>(null)
  const [error, setError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)
  useErrorToast(error, "Item not saved")

  useEffect(() => {
    Promise.all([getApiCategories(), getApiStorageLocations()]).then(([categoryRows, locationRows]) => {
      const activeCategories = categoryRows.filter((row) => !row.isArchived)
      const activeLocations = locationRows.filter((row) => !row.isArchived)
      setCategories(activeCategories); setLocations(activeLocations)
      setCategoryId(activeCategories[0]?.id ?? ""); setSelectedLocations(activeLocations[0] ? { [activeLocations[0].id]: "0" } : {})
    }).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load item options.")))
  }, [])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSubmitting(true); setError(undefined)
    const data = new FormData(event.currentTarget)
    const consumptionQuantity = optionalNumber(data, "consumptionQuantity")
    const consumptionPeriodDays = optionalNumber(data, "consumptionPeriodDays")
    if ((consumptionQuantity === null) !== (consumptionPeriodDays === null)) {
      showErrorToast("Consumption quantity and period must be supplied together.", "Item not saved"); setSubmitting(false); return
    }
    const locationEntries = Object.entries(selectedLocations)
    if (!locationEntries.length) { showErrorToast("Select at least one storage location.", "Item not saved"); setSubmitting(false); return }
    try {
      const item = await postApiPantryItems({
        categoryId, sourceTemplateId: null, defaultStorageLocationId: locationEntries[0][0],
        name: String(data.get("name")), icon, brand: optionalText(data, "brand"), preferredProduct: optionalText(data, "preferredProduct"), notes: optionalText(data, "notes"),
        trackingUnit: Number(trackingUnit), packageSize: optionalNumber(data, "packageSize"), packageUnit: optionalText(data, "packageUnit"),
        consumptionQuantity, consumptionPeriodDays, bufferDays: Number(data.get("bufferDays") || 0),
        locations: locationEntries.map(([storageLocationId, quantity], index) => ({ storageLocationId, currentQuantity: Number(quantity || 0), sortOrder: index })),
      })
      showSuccessToast("Item added")
      router.replace(`/app/pantry/items/${item.id}`)
    } catch (submissionError) { setError(getApiErrorMessage(submissionError, "Unable to create item.")); setSubmitting(false) }
  }

  return <div className="space-y-6"><header><h1 className="text-2xl font-semibold">Add pantry item</h1><p className="mt-1 text-sm text-muted-foreground">Add the product and its initial stock locations.</p></header><Card><CardHeader><CardTitle>Item details</CardTitle><CardDescription>Consumption information is optional and can be added later.</CardDescription></CardHeader><CardContent><form className="space-y-6" onSubmit={submit}><FieldGroup className="grid gap-5 md:grid-cols-2"><Field className="md:col-span-2"><PantryItemIconPicker value={icon} onChange={setIcon} /></Field><Field><PantryItemFieldLabel field="name" htmlFor="name" /><Input id="name" name="name" maxLength={160} required /></Field><Field><PantryItemFieldLabel field="category" /><Select value={categoryId} onValueChange={setCategoryId}><SelectTrigger className="w-full"><SelectValue /></SelectTrigger><SelectContent>{categories.map((row) => <SelectItem key={row.id} value={row.id}>{row.name}</SelectItem>)}</SelectContent></Select></Field><Field><PantryItemFieldLabel field="trackingUnit" /><Select value={trackingUnit} onValueChange={setTrackingUnit}><SelectTrigger className="w-full"><SelectValue /></SelectTrigger><SelectContent>{["Package", "Item", "Weight", "Volume"].map((label, index) => <SelectItem key={label} value={String(index)}>{label}</SelectItem>)}</SelectContent></Select></Field><Field><PantryItemFieldLabel field="brand" htmlFor="brand" /><Input id="brand" name="brand" maxLength={120} /></Field><Field className="md:col-span-2"><PantryItemFieldLabel field="locations" /><div className="grid gap-3 sm:grid-cols-2">{locations.map((row) => { const selected = row.id in selectedLocations; return <div key={row.id} className="flex items-center gap-3 rounded-lg bg-surface-muted p-3"><Checkbox checked={selected} onCheckedChange={(checked) => setSelectedLocations((current) => { const next = { ...current }; if (checked === true) next[row.id] = "0"; else delete next[row.id]; return next })} /><span className="flex-1 text-sm font-medium">{row.name}</span><Input aria-label={`Starting quantity in ${row.name}`} type="number" min="0" step="0.001" className="w-24" disabled={!selected} value={selectedLocations[row.id] ?? ""} onChange={(event) => setSelectedLocations((current) => ({ ...current, [row.id]: event.target.value }))} /></div>})}</div></Field><Field><PantryItemFieldLabel field="preferredProduct" htmlFor="preferredProduct" /><Input id="preferredProduct" name="preferredProduct" maxLength={200} /></Field><Field><PantryItemFieldLabel field="packageSize" htmlFor="packageSize" /><Input id="packageSize" name="packageSize" type="number" min="0" step="0.001" /></Field><Field><PantryItemFieldLabel field="packageUnit" htmlFor="packageUnit" /><Input id="packageUnit" name="packageUnit" maxLength={32} placeholder="kg, L, pack" /></Field><Field><PantryItemFieldLabel field="consumptionQuantity" htmlFor="consumptionQuantity" /><Input id="consumptionQuantity" name="consumptionQuantity" type="number" min="0.001" step="0.001" /></Field><Field><PantryItemFieldLabel field="consumptionPeriodDays" htmlFor="consumptionPeriodDays" /><Input id="consumptionPeriodDays" name="consumptionPeriodDays" type="number" min="0.001" step="0.001" /></Field><Field><PantryItemFieldLabel field="bufferDays" htmlFor="bufferDays" /><Input id="bufferDays" name="bufferDays" type="number" min="0" step="0.001" defaultValue="0" /></Field><Field className="md:col-span-2"><PantryItemFieldLabel field="notes" htmlFor="notes" /><Textarea id="notes" name="notes" maxLength={2000} /></Field></FieldGroup><div className="flex justify-end gap-3"><Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button><Button type="submit" disabled={submitting || !categoryId || !Object.keys(selectedLocations).length}>{submitting && <Spinner />}{submitting ? "Saving…" : "Add item"}</Button></div></form></CardContent></Card></div>
}

function optionalText(data: FormData, key: string) { const value = String(data.get(key) ?? "").trim(); return value || null }
function optionalNumber(data: FormData, key: string) { const value = String(data.get(key) ?? ""); return value ? Number(value) : null }
