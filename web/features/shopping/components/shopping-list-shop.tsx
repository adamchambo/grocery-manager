"use client"

import { useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { useParams, useRouter } from "next/navigation"
import { CheckIcon, DownloadIcon, XIcon } from "lucide-react"
import { toast } from "sonner"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListItemResponse, ShoppingListResponse } from "@/lib/api/generated/models"
import { getApiShoppingListsListId, getApiShoppingListsListIdPdf, postApiShoppingListsListIdComplete, putApiShoppingListsListIdItemsItemId } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListShop() {
  const { listId } = useParams<{ listId: string }>()
  const router = useRouter()
  const [list, setList] = useState<ShoppingListResponse>()
  const [loading, setLoading] = useState(true)
  const [savingId, setSavingId] = useState<string>()
  const [completing, setCompleting] = useState(false)
  const [category, setCategory] = useState("all")
  const [error, setError] = useState<string>()
  useErrorToast(error, "Shopping list not updated")

  async function load() {
    try { setList(await getApiShoppingListsListId(listId)) }
    catch (loadError) { setError(getApiErrorMessage(loadError, "Unable to load shopping list.")) }
    finally { setLoading(false) }
  }
  useEffect(() => { void load() }, [listId])

  const categories = useMemo(() => [...new Set(list?.items.map((item) => item.categoryName || "Other items"))].sort(), [list])
  const visibleItems = useMemo(() => list?.items.filter((item) => category === "all" || (item.categoryName || "Other items") === category) ?? [], [list, category])
  const pending = visibleItems.filter((item) => item.outcome === 0)
  const resolved = visibleItems.filter((item) => item.outcome !== 0)

  async function resolve(item: ShoppingListItemResponse, outcome: 0 | 1 | 3) {
    setSavingId(item.id)
    try {
      await putApiShoppingListsListIdItemsItemId(listId, item.id, { suggestedPurchaseQuantity: item.suggestedPurchaseQuantity, outcome, version: item.version })
      await load()
    } catch (value) { setError(getApiErrorMessage(value, "Unable to update item.")) }
    finally { setSavingId(undefined) }
  }

  async function download() {
    try {
      const blob = await getApiShoppingListsListIdPdf(listId)
      const url = URL.createObjectURL(blob)
      window.open(url, "_blank", "noopener,noreferrer")
      window.setTimeout(() => URL.revokeObjectURL(url), 60_000)
    } catch (value) { setError(getApiErrorMessage(value, "Unable to export PDF.")) }
  }

  async function complete() {
    if (list?.items.some((item) => item.outcome === 0)) return
    setCompleting(true)
    try { await postApiShoppingListsListIdComplete(listId); toast.success("Shopping list completed"); router.replace(`/app/shopping-lists/${listId}`) }
    catch (value) { setError(getApiErrorMessage(value, "Unable to complete shopping list.")); setCompleting(false) }
  }

  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!list) return <div className="py-12 text-center text-sm text-muted-foreground">Shopping list unavailable.</div>

  return <div className="mx-auto max-w-2xl space-y-5">
    <header className="flex flex-wrap items-start justify-between gap-3">
      <div><h1 className="text-2xl font-semibold">{list.name}</h1><p className="mt-1 text-sm text-muted-foreground">Check items off as you shop. Your next stocktake will set stock again.</p></div>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="sm" onClick={() => void download()}><DownloadIcon />Export PDF</Button>
        <Button variant="outline" size="sm" asChild><Link href={`/app/shopping-lists/${list.id}/review`}>Edit list</Link></Button>
      </div>
    </header>
    <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg bg-surface-muted px-4 py-3 text-sm"><span>{resolved.length} of {visibleItems.length} shown resolved</span><div className="flex items-center gap-2"><span className="text-muted-foreground">Category</span><Select value={category} onValueChange={setCategory}><SelectTrigger className="w-44"><SelectValue /></SelectTrigger><SelectContent position="popper"><SelectItem value="all">All categories</SelectItem>{categories.map((value) => <SelectItem key={value} value={value}>{value}</SelectItem>)}</SelectContent></Select><Badge>{pending.length ? `${pending.length} left` : "Ready to finish"}</Badge></div></div>
    <section className="space-y-2">{visibleItems.map((item) => item.outcome === 0 ? <Card key={item.id} className="gap-3 py-4"><CardContent className="space-y-3"><div><p className="font-medium">{item.itemName}</p><p className="text-sm text-muted-foreground">{item.suggestedPurchaseQuantity ?? 0} {item.trackingUnit ?? ""}</p></div><div className="flex flex-wrap gap-2"><Button size="sm" disabled={savingId === item.id} onClick={() => void resolve(item, 1)}><CheckIcon />Purchased</Button><Button size="sm" variant="outline" disabled={savingId === item.id} onClick={() => void resolve(item, 3)}><XIcon />Not purchased</Button></div></CardContent></Card> : <div key={item.id} className="flex items-center justify-between rounded-lg border border-border/40 bg-muted/30 px-4 py-3 opacity-65"><div><p className="font-medium line-through">{item.itemName}</p><p className="text-sm text-muted-foreground">{item.suggestedPurchaseQuantity ?? 0} {item.trackingUnit ?? ""} · {item.outcome === 3 ? "Not purchased" : "Purchased"}</p></div><Button size="sm" variant="ghost" disabled={savingId === item.id} onClick={() => void resolve(item, 0)}>Undo</Button></div>)}</section>
    <Card><CardHeader><CardTitle>Finish shopping</CardTitle><CardDescription>{list.items.some((item) => item.outcome === 0) ? "Resolve every item before completing this list." : "Completing keeps this list as a simple record of the trip."}</CardDescription></CardHeader><CardContent><Button disabled={list.items.some((item) => item.outcome === 0) || completing} onClick={() => void complete()}>{completing && <Spinner />}{completing ? "Completing…" : "Complete shopping list"}</Button></CardContent></Card>
  </div>
}
