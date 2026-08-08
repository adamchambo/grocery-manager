"use client"

import Link from "next/link"
import { useParams, useRouter } from "next/navigation"
import { useEffect, useState, type FormEvent } from "react"

import { getApiCategories } from "@/lib/api/generated/categories/categories"
import { postApiInventoryAdjustments } from "@/lib/api/generated/inventory-adjustments/inventory-adjustments"
import type { CategoryResponse, PantryItemResponse } from "@/lib/api/generated/models"
import { deleteApiPantryItemsItemId, getApiPantryItemsItemId, putApiPantryItemsItemId } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { Textarea } from "@/shared/components/ui/textarea"
import { PantryItemFieldLabel } from "@/features/pantry/components/pantry-item-field-label"
import { getCategoryIcon as categoryIcon, PantryItemIconPicker } from "@/features/pantry/components/pantry-item-icon-picker"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function PantryItemDetail() {
  const { itemId } = useParams<{ itemId: string }>(); const router = useRouter()
  const [item, setItem] = useState<PantryItemResponse>(); const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [categoryId, setCategoryId] = useState(""); const [trackingUnit, setTrackingUnit] = useState("0")
  const [icon, setIcon] = useState<string | null>(null)
  const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false)
  useErrorToast(error, item ? "Request failed" : "Item unavailable")

  async function load() { try { const [row, categoryRows] = await Promise.all([getApiPantryItemsItemId(itemId), getApiCategories()]); setItem(row); setIcon(row.icon); setCategoryId(row.categoryId); setTrackingUnit(String(row.trackingUnit)); setCategories(categoryRows.filter((category) => !category.isArchived)) } catch (loadError) { setError(getApiErrorMessage(loadError, "Unable to load item.")) } }
  useEffect(() => {
    Promise.all([getApiPantryItemsItemId(itemId), getApiCategories()])
      .then(([row, categoryRows]) => {
        setItem(row); setIcon(row.icon); setCategoryId(row.categoryId); setTrackingUnit(String(row.trackingUnit)); setCategories(categoryRows.filter((category) => !category.isArchived))
      })
      .catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load item.")))
  }, [itemId])

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!item) return; setSaving(true); setError(undefined); const data = new FormData(event.currentTarget)
    try { const updated = await putApiPantryItemsItemId(item.id, { categoryId, defaultStorageLocationId: item.defaultStorageLocationId, name: String(data.get("name")), icon, brand: text(data, "brand"), preferredProduct: text(data, "preferredProduct"), notes: text(data, "notes"), trackingUnit: Number(trackingUnit), packageSize: number(data, "packageSize"), packageUnit: text(data, "packageUnit"), consumptionQuantity: number(data, "consumptionQuantity"), consumptionPeriodDays: number(data, "consumptionPeriodDays"), bufferDays: Number(data.get("bufferDays") || 0), version: item.version }); setItem(updated); setIcon(updated.icon); showSuccessToast("Item saved") } catch (saveError) { setError(getApiErrorMessage(saveError, "Unable to update item.")) } finally { setSaving(false) }
  }

  async function adjust(locationId: string, form: HTMLFormElement) { const data = new FormData(form); setSaving(true); try { await postApiInventoryAdjustments({ pantryItemLocationId: locationId, quantityDelta: Number(data.get("quantityDelta")), notes: text(data, "notes"), idempotencyKey: crypto.randomUUID() }); await load(); form.reset(); showSuccessToast("Stock adjustment recorded") } catch (adjustmentError) { setError(getApiErrorMessage(adjustmentError, "Unable to adjust quantity.")) } finally { setSaving(false) } }
  async function archive() { if (!item) return; await deleteApiPantryItemsItemId(item.id); showSuccessToast("Item archived"); router.replace("/app/pantry") }

  if (!item && !error) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!item) return <div className="flex min-h-64 items-center justify-center text-sm text-muted-foreground">Item unavailable.</div>

  return <div className="space-y-6"><header className="flex items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">{item.name}</h1><p className="mt-1 text-sm text-muted-foreground">Edit product details or record a stock adjustment.</p></div><Button variant="outline" asChild><Link href={`/app/history?itemId=${item.id}`}>View history</Link></Button></header><Card><CardHeader><CardTitle>Item details</CardTitle></CardHeader><CardContent><form className="grid gap-5 md:grid-cols-2" onSubmit={save}><Field className="md:col-span-2"><PantryItemIconPicker value={icon} fallback={categoryIcon(item.categoryName)} onChange={setIcon} /></Field><Field><PantryItemFieldLabel field="name" htmlFor="edit-name" /><Input id="edit-name" name="name" defaultValue={item.name} required /></Field><Field><PantryItemFieldLabel field="category" /><Select value={categoryId} onValueChange={setCategoryId}><SelectTrigger className="w-full"><SelectValue /></SelectTrigger><SelectContent>{categories.map((row) => <SelectItem key={row.id} value={row.id}>{row.name}</SelectItem>)}</SelectContent></Select></Field><Field><PantryItemFieldLabel field="trackingUnit" /><Select value={trackingUnit} onValueChange={setTrackingUnit}><SelectTrigger className="w-full"><SelectValue /></SelectTrigger><SelectContent>{["Package", "Item", "Weight", "Volume"].map((label, index) => <SelectItem key={label} value={String(index)}>{label}</SelectItem>)}</SelectContent></Select></Field><Field><PantryItemFieldLabel field="brand" htmlFor="edit-brand" /><Input id="edit-brand" name="brand" defaultValue={item.brand ?? ""} /></Field><Field><PantryItemFieldLabel field="preferredProduct" htmlFor="edit-preferred-product" /><Input id="edit-preferred-product" name="preferredProduct" defaultValue={item.preferredProduct ?? ""} /></Field><Field><PantryItemFieldLabel field="packageSize" htmlFor="edit-package-size" /><Input id="edit-package-size" name="packageSize" type="number" step="0.001" min="0" defaultValue={item.packageSize ?? ""} /></Field><Field><PantryItemFieldLabel field="packageUnit" htmlFor="edit-package-unit" /><Input id="edit-package-unit" name="packageUnit" defaultValue={item.packageUnit ?? ""} /></Field><Field><PantryItemFieldLabel field="consumptionQuantity" htmlFor="edit-consumption-quantity" /><Input id="edit-consumption-quantity" name="consumptionQuantity" type="number" step="0.001" min="0.001" defaultValue={item.consumptionQuantity ?? ""} /></Field><Field><PantryItemFieldLabel field="consumptionPeriodDays" htmlFor="edit-consumption-period" /><Input id="edit-consumption-period" name="consumptionPeriodDays" type="number" step="0.001" min="0.001" defaultValue={item.consumptionPeriodDays ?? ""} /></Field><Field><PantryItemFieldLabel field="bufferDays" htmlFor="edit-buffer-days" /><Input id="edit-buffer-days" name="bufferDays" type="number" step="0.001" min="0" defaultValue={item.bufferDays} /></Field><Field className="md:col-span-2"><PantryItemFieldLabel field="notes" htmlFor="edit-notes" /><Textarea id="edit-notes" name="notes" defaultValue={item.notes ?? ""} /></Field><div className="flex justify-between md:col-span-2"><AlertDialog><AlertDialogTrigger asChild><Button type="button" variant="destructive">Archive item</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Are you sure you want to archive {item.name}?</AlertDialogTitle><AlertDialogDescription>The item remains in inventory history but disappears from active pantry views.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Cancel</AlertDialogCancel><AlertDialogAction variant="destructive" onClick={archive}>Archive</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog><Button type="submit" disabled={saving}>{saving && <Spinner />}Save details</Button></div></form></CardContent></Card><section className="space-y-3"><h2 className="text-lg font-semibold">Stock by location</h2><div className="grid gap-3 md:grid-cols-2">{item.locations.map((location) => <Card key={location.id}><CardHeader><div className="flex justify-between"><CardTitle>{location.storageLocationName}</CardTitle><Badge>{location.currentQuantity}</Badge></div></CardHeader><CardContent><form className="space-y-3" onSubmit={(event) => { event.preventDefault(); void adjust(location.id, event.currentTarget) }}><Field><FieldLabel>Quantity change</FieldLabel><Input name="quantityDelta" type="number" step="0.001" placeholder="e.g. 2 or -1" required /></Field><Field><FieldLabel>Reason</FieldLabel><Input name="notes" placeholder="Optional note" /></Field><Button type="submit" variant="outline" disabled={saving}>Record adjustment</Button></form></CardContent></Card>)}</div></section></div>
}

function text(data: FormData, key: string) { const value = String(data.get(key) ?? "").trim(); return value || null }
function number(data: FormData, key: string) { const value = String(data.get(key) ?? ""); return value ? Number(value) : null }
