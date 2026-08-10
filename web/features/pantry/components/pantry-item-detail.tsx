"use client"

import { useParams, useRouter } from "next/navigation"
import { InfoIcon } from "lucide-react"
import { useEffect, useState, type FormEvent } from "react"
import type { PantryItemResponse } from "@/lib/api/generated/models"
import { deleteApiPantryItemsItemId, getApiPantryItemsItemId, putApiPantryItemsItemId } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function PantryItemDetail() {
  const { itemId } = useParams<{ itemId: string }>(); const router = useRouter(); const [item, setItem] = useState<PantryItemResponse>(); const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false)
  useErrorToast(error, item ? "Item not saved" : "Item unavailable")
  useEffect(() => { getApiPantryItemsItemId(itemId).then(setItem).catch((value) => setError(getApiErrorMessage(value, "Unable to load item."))) }, [itemId])
  async function save(event: FormEvent<HTMLFormElement>) { event.preventDefault(); if (!item) return; const data = new FormData(event.currentTarget); setSaving(true); try { const updated = await putApiPantryItemsItemId(item.id, { categoryId: item.categoryId, defaultStorageLocationId: item.defaultStorageLocationId, name: String(data.get("name")), icon: null, brand: null, preferredProduct: null, notes: null, trackingUnit: item.trackingUnit, packageSize: null, packageUnit: null, consumptionQuantity: Number(data.get("consumptionQuantity")), consumptionPeriodDays: Number(data.get("consumptionPeriodDays")), bufferDays: Number(data.get("bufferDays") || 0), version: item.version }); setItem(updated); showSuccessToast("Item saved") } catch (value) { setError(getApiErrorMessage(value, "Unable to save item.")) } finally { setSaving(false) } }
  async function archive() { if (!item) return; await deleteApiPantryItemsItemId(item.id); showSuccessToast("Item archived"); router.replace("/app/pantry") }
  if (!item) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  return <div className="space-y-6"><header><h1 className="text-2xl font-semibold">{item.name}</h1><p className="mt-1 text-sm text-muted-foreground">Settings used to build your next shopping list.</p></header><Card><CardHeader><CardTitle>Regular item</CardTitle><CardDescription>Stock areas are managed when you add or organise an item. Stock is counted during stocktake.</CardDescription></CardHeader><CardContent><form className="grid gap-5 md:grid-cols-2" onSubmit={save}><Field className="md:col-span-2"><FieldLabel htmlFor="name">Name</FieldLabel><Input id="name" name="name" defaultValue={item.name} required /></Field><Field><FieldLabel htmlFor="consumptionQuantity">We use</FieldLabel><Input id="consumptionQuantity" name="consumptionQuantity" type="number" min="0.001" step="0.001" defaultValue={item.consumptionQuantity ?? ""} required /></Field><Field><FieldLabel htmlFor="consumptionPeriodDays">Every how many days?</FieldLabel><Input id="consumptionPeriodDays" name="consumptionPeriodDays" type="number" min="1" step="1" defaultValue={item.consumptionPeriodDays ?? ""} required /></Field><Field><div className="flex items-center gap-1.5"><FieldLabel htmlFor="bufferDays">Extra to keep on hand</FieldLabel><InfoIcon className="size-4 text-muted-foreground" title="Additional quantity to keep after covering normal use." /></div><Input id="bufferDays" name="bufferDays" type="number" min="0" step="0.5" defaultValue={item.bufferDays} /></Field><div className="flex justify-between md:col-span-2"><AlertDialog><AlertDialogTrigger asChild><Button type="button" variant="destructive">Archive item</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Archive {item.name}?</AlertDialogTitle><AlertDialogDescription>This stops it appearing in future stocktakes.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Cancel</AlertDialogCancel><AlertDialogAction variant="destructive" onClick={archive}>Archive</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog><Button type="submit" disabled={saving}>{saving && <Spinner />}Save</Button></div></form></CardContent></Card></div>
}
