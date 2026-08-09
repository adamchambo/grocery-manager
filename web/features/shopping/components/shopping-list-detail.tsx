"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { DownloadIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListResponse } from "@/lib/api/generated/models"
import { getApiShoppingListsListId, getApiShoppingListsListIdPdf } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListDetail() {
  const { listId } = useParams<{ listId: string }>(); const [list, setList] = useState<ShoppingListResponse>(); const [loading, setLoading] = useState(true); const [error, setError] = useState<string>(); useErrorToast(error, "Shopping list unavailable")
  useEffect(() => { getApiShoppingListsListId(listId).then(setList).catch((value) => setError(getApiErrorMessage(value, "Unable to load shopping list."))).finally(() => setLoading(false)) }, [listId])
  async function download() { try { const blob = await getApiShoppingListsListIdPdf(listId); const url = URL.createObjectURL(blob); window.open(url, "_blank", "noopener,noreferrer"); window.setTimeout(() => URL.revokeObjectURL(url), 60_000) } catch (value) { setError(getApiErrorMessage(value, "Unable to open PDF.")) } }
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!list) return <div className="py-12 text-center text-sm text-muted-foreground">Shopping list unavailable.</div>
  return <div className="mx-auto max-w-3xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-3"><div><h1 className="text-2xl font-semibold">{list.name}</h1><p className="mt-1 text-sm text-muted-foreground">Completed {new Date(list.completedAtUtc ?? list.generatedAtUtc).toLocaleDateString()}</p></div><div className="flex gap-2"><Button size="sm" variant="outline" onClick={() => void download()}><DownloadIcon />Export PDF</Button><Button size="sm" variant="outline" asChild><Link href="/app/shopping-lists">All lists</Link></Button></div></header><Card><CardHeader><div className="flex items-center justify-between"><CardTitle>Shopping list</CardTitle><Badge variant="secondary">Completed</Badge></div><CardDescription>A simple record of this trip.</CardDescription></CardHeader><CardContent className="space-y-2">{list.items.map((item) => <div key={item.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg bg-surface-muted/60 px-3 py-2 text-sm"><span className="font-medium">{item.itemName}</span><span className="text-muted-foreground">{item.outcome === 3 ? "Not purchased" : `${item.suggestedPurchaseQuantity ?? 0} ${item.trackingUnit ?? ""} purchased`}</span></div>)}</CardContent></Card></div>
}
