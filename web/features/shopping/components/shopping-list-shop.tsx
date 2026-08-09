"use client"

import { useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { useParams, useRouter } from "next/navigation"
import { CheckIcon, XIcon } from "lucide-react"
import { toast } from "sonner"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListItemResponse, ShoppingListResponse } from "@/lib/api/generated/models"
import { getApiShoppingListsListId, postApiShoppingListsListIdComplete, putApiShoppingListsListIdItemsItemId } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListShop() {
  const { listId } = useParams<{ listId: string }>(); const router = useRouter(); const [list, setList] = useState<ShoppingListResponse>(); const [loading, setLoading] = useState(true); const [savingId, setSavingId] = useState<string>(); const [completing, setCompleting] = useState(false); const [error, setError] = useState<string>()
  useErrorToast(error, "Shopping list not updated")
  async function load() { try { setList(await getApiShoppingListsListId(listId)) } catch (loadError) { setError(getApiErrorMessage(loadError, "Unable to load shopping list.")) } finally { setLoading(false) } }
  useEffect(() => { void load() }, [listId])
  const pending = useMemo(() => list?.items.filter((item) => item.outcome === 0) ?? [], [list]); const resolved = list?.items.filter((item) => item.outcome !== 0) ?? []
  async function resolve(item: ShoppingListItemResponse, outcome: 1 | 3) { setSavingId(item.id); try { await putApiShoppingListsListIdItemsItemId(listId, item.id, { suggestedPurchaseQuantity: item.suggestedPurchaseQuantity, outcome, version: item.version }); await load(); toast.success(outcome === 1 ? `${item.itemName} checked off` : `${item.itemName} marked not purchased`) } catch (value) { setError(getApiErrorMessage(value, "Unable to update item.")) } finally { setSavingId(undefined) } }
  async function complete() { if (pending.length) return; setCompleting(true); try { await postApiShoppingListsListIdComplete(listId); toast.success("Shopping list completed"); router.replace(`/app/shopping-lists/${listId}`) } catch (value) { setError(getApiErrorMessage(value, "Unable to complete shopping list.")); setCompleting(false) } }
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!list) return <div className="py-12 text-center text-sm text-muted-foreground">Shopping list unavailable.</div>
  return <div className="mx-auto max-w-2xl space-y-5"><header className="flex flex-wrap items-start justify-between gap-3"><div><h1 className="text-2xl font-semibold">{list.name}</h1><p className="mt-1 text-sm text-muted-foreground">Check items off as you shop. Your next stocktake will set stock again.</p></div><Button variant="outline" size="sm" asChild><Link href={`/app/shopping-lists/${list.id}/review`}>Review</Link></Button></header><div className="flex items-center justify-between rounded-lg bg-surface-muted px-4 py-3 text-sm"><span>{resolved.length} of {list.items.length} resolved</span><Badge>{pending.length ? `${pending.length} left` : "Ready to finish"}</Badge></div><section className="space-y-2">{pending.map((item) => <Card key={item.id} className="gap-3 py-4"><CardContent className="space-y-3"><div><p className="font-medium">{item.itemName}</p><p className="text-sm text-muted-foreground">{item.suggestedPurchaseQuantity ?? 0} {item.trackingUnit ?? ""}</p></div><div className="flex flex-wrap gap-2"><Button size="sm" disabled={savingId === item.id} onClick={() => void resolve(item, 1)}><CheckIcon />Purchased</Button><Button size="sm" variant="outline" disabled={savingId === item.id} onClick={() => void resolve(item, 3)}><XIcon />Not purchased</Button></div></CardContent></Card>)}</section>{resolved.length > 0 && <section className="space-y-2"><h2 className="text-sm font-medium text-muted-foreground">Resolved</h2>{resolved.map((item) => <div key={item.id} className="flex items-center justify-between rounded-lg border px-3 py-2 text-sm"><span>{item.itemName}</span><span className="text-muted-foreground">{item.outcome === 3 ? "Not purchased" : "Purchased"}</span></div>)}</section>}<Card><CardHeader><CardTitle>Finish shopping</CardTitle><CardDescription>{pending.length ? "Resolve every item before completing this list." : "Completing keeps this list as a simple record of the trip."}</CardDescription></CardHeader><CardContent><Button disabled={pending.length > 0 || completing} onClick={() => void complete()}>{completing && <Spinner />}{completing ? "Completing…" : "Complete shopping list"}</Button></CardContent></Card></div>
}
