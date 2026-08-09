"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { DownloadIcon, PlusIcon, Trash2Icon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListItemResponse, ShoppingListResponse } from "@/lib/api/generated/models"
import { deleteApiShoppingListsListIdItemsItemId, getApiShoppingListsListId, getApiShoppingListsListIdPdf, postApiShoppingListsListIdItems, putApiShoppingListsListIdItemsItemId } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Input } from "@/shared/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListReview() {
  const { listId } = useParams<{ listId: string }>()
  const [list, setList] = useState<ShoppingListResponse>(); const [loading, setLoading] = useState(true); const [error, setError] = useState<string>()
  const [view, setView] = useState("all")
  useErrorToast(error, "Shopping list not updated")
  async function load() { try { setList(await getApiShoppingListsListId(listId)) } catch (value) { setError(getApiErrorMessage(value, "Unable to load shopping list.")) } finally { setLoading(false) } }
  useEffect(() => { void load() }, [listId])
  async function saveSuggestion(item: ShoppingListItemResponse, raw: string) {
    const quantity = Number(raw); if (!Number.isFinite(quantity) || quantity < 0) { setError("Enter a valid quantity."); return }
    try { await putApiShoppingListsListIdItemsItemId(listId, item.id, { suggestedPurchaseQuantity: quantity, outcome: item.outcome, version: item.version }); await load() } catch (value) { setError(getApiErrorMessage(value, "Unable to update quantity.")) }
  }
  async function remove(itemId: string) { try { await deleteApiShoppingListsListIdItemsItemId(listId, itemId); showSuccessToast("Item removed"); await load() } catch (value) { setError(getApiErrorMessage(value, "Unable to remove item.")) } }
  async function download() { try { const blob = await getApiShoppingListsListIdPdf(listId); const url = URL.createObjectURL(blob); window.open(url, "_blank", "noopener,noreferrer"); window.setTimeout(() => URL.revokeObjectURL(url), 60_000) } catch (value) { setError(getApiErrorMessage(value, "Unable to open PDF.")) } }
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!list) return <div className="py-12 text-center text-sm text-muted-foreground">Shopping list unavailable.</div>
  const locations = [...new Set(list.items.map((item) => item.locationName).filter((name): name is string => Boolean(name)))].sort()
  const visibleItems = view === "all" ? list.items : list.items.filter((item) => item.locationName === view)
  const groups = Object.entries(visibleItems.reduce<Record<string, ShoppingListItemResponse[]>>((result, item) => { const key = item.categoryName || "Other items"; (result[key] ??= []).push(item); return result }, {}))
  return <div className="mx-auto max-w-4xl space-y-6 pb-24"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">{list.name}</h1><p className="mt-1 text-sm text-muted-foreground">Your stocktake has calculated what to buy.</p></div><div className="flex gap-2"><Button size="sm" variant="outline" onClick={() => void download()}><DownloadIcon />Export PDF</Button><Button size="sm" asChild><Link href={`/app/shopping-lists/${list.id}/shop`}>Start shopping</Link></Button></div></header><Card><CardHeader><div className="flex flex-wrap items-center justify-between gap-3"><div><CardTitle>Items</CardTitle><CardDescription>View everything together or by storage location.</CardDescription></div><Select value={view} onValueChange={setView}><SelectTrigger className="w-52"><SelectValue /></SelectTrigger><SelectContent position="popper" className="border-border/60 shadow-sm"><SelectItem value="all">All locations</SelectItem>{locations.map((location) => <SelectItem key={location} value={location}>{location}</SelectItem>)}</SelectContent></Select></div></CardHeader><CardContent className="space-y-5">{groups.map(([group, items]) => <section key={group} className="space-y-2"><h2 className="text-sm font-medium text-muted-foreground">{group}</h2>{items.map((item) => <ReviewItem key={item.id} item={item} onSave={saveSuggestion} onRemove={remove} />)}</section>)}{!visibleItems.length && <p className="py-6 text-center text-sm text-muted-foreground">No items are assigned to this location.</p>}<ManualItemForm listId={listId} onAdded={load} onError={setError} /></CardContent></Card><div className="fixed inset-x-0 bottom-0 z-20 border-t border-border/60 bg-card/95 px-4 py-3 shadow-[0_-8px_24px_rgb(24_32_24_/_0.08)] backdrop-blur supports-[padding:max(0px)]:pb-[max(0.75rem,env(safe-area-inset-bottom))]"><div className="mx-auto flex max-w-4xl items-center justify-between gap-4"><span className="text-sm font-medium">{list.items.length} {list.items.length === 1 ? "item" : "items"} ready</span><Button asChild><Link href={`/app/shopping-lists/${list.id}/shop`}>Start shopping</Link></Button></div></div></div>
}

function ReviewItem({ item, onSave, onRemove }: Readonly<{ item: ShoppingListItemResponse; onSave: (item: ShoppingListItemResponse, value: string) => Promise<void>; onRemove: (itemId: string) => Promise<void> }>) {
  const measurementType = item.trackingUnit ?? "Regular item"
  const unit = item.packageSize && item.packageUnit ? `${item.packageSize} ${item.packageUnit}` : measurementUnit(measurementType)
  return <div className="grid grid-cols-[minmax(0,1fr)_6rem_6rem_2.25rem] items-center gap-3 rounded-xl border border-border/60 bg-card px-3 py-2 shadow-xs">
    <div className="min-w-0"><p className="truncate font-medium">{item.itemName}</p><p className="text-xs text-muted-foreground">{item.isManual ? "One-off item" : measurementType}</p></div>
    <Input aria-label={`${item.itemName} quantity`} type="number" min="0" step="0.001" defaultValue={String(item.suggestedPurchaseQuantity ?? "")} onBlur={(event) => void onSave(item, event.target.value)} className="w-full" />
    <span className="text-sm text-muted-foreground">{unit}</span>
    <Button size="icon-sm" variant="ghost" aria-label={`Remove ${item.itemName}`} onClick={() => void onRemove(item.id)}><Trash2Icon /></Button>
  </div>
}

function measurementUnit(value: string) {
  return value.toLowerCase() === "volume" ? "L" : value.toLowerCase() === "weight" ? "kg" : value.toLowerCase() === "item" ? "items" : value.toLowerCase() === "package" ? "pack" : ""
}

function ManualItemForm({ listId, onAdded, onError }: Readonly<{ listId: string; onAdded: () => Promise<void>; onError: (value: string) => void }>) {
  const [name, setName] = useState(""); const [quantity, setQuantity] = useState("1"); const [saving, setSaving] = useState(false)
  async function add() { const numeric = Number(quantity); if (!name.trim() || !Number.isFinite(numeric) || numeric <= 0) { onError("Enter an item name and quantity greater than zero."); return } setSaving(true); try { await postApiShoppingListsListIdItems(listId, { name: name.trim(), suggestedPurchaseQuantity: numeric }); setName(""); setQuantity("1"); showSuccessToast("One-off item added"); await onAdded() } catch (value) { onError(getApiErrorMessage(value, "Unable to add item.")) } finally { setSaving(false) } }
  return <div className="space-y-3 border-t pt-5"><div><p className="font-medium">Add one-off item</p><p className="text-sm text-muted-foreground">This item stays on this list only.</p></div><div className="grid gap-3 sm:grid-cols-[1fr_8rem_auto]"><Input value={name} onChange={(event) => setName(event.target.value)} placeholder="Item name" /><Input value={quantity} onChange={(event) => setQuantity(event.target.value)} type="number" min="0.001" step="0.001" /><Button onClick={() => void add()} disabled={saving}>{saving && <Spinner />}<PlusIcon />Add</Button></div></div>
}
