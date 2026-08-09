"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useParams, useRouter } from "next/navigation"
import { DownloadIcon, RotateCcwIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListResponse } from "@/lib/api/generated/models"
import { getApiShoppingListsListId, getApiShoppingListsListIdPdf, postApiShoppingListsListIdUndo } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListDetail() {
  const { listId } = useParams<{ listId: string }>(); const router = useRouter(); const [list, setList] = useState<ShoppingListResponse>(); const [loading, setLoading] = useState(true); const [undoing, setUndoing] = useState(false); const [error, setError] = useState<string>(); useErrorToast(error, "Shopping list unavailable")
  useEffect(() => { getApiShoppingListsListId(listId).then(setList).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load shopping list."))).finally(() => setLoading(false)) }, [listId])
  async function download() { try { const blob = await getApiShoppingListsListIdPdf(listId); const url = URL.createObjectURL(blob); const anchor = document.createElement("a"); anchor.href = url; anchor.download = `${list?.name ?? "shopping-list"}.pdf`; anchor.click(); URL.revokeObjectURL(url); } catch (downloadError) { setError(getApiErrorMessage(downloadError, "Unable to download PDF.")) } }
  async function undo() { setUndoing(true); try { await postApiShoppingListsListIdUndo(listId); showSuccessToast("Shopping list reopened"); router.replace(`/app/shopping-lists/${listId}/review`) } catch (undoError) { setError(getApiErrorMessage(undoError, "Unable to undo shopping list.")); setUndoing(false) } }
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!list) return <div className="py-12 text-center text-sm text-muted-foreground">Shopping list unavailable.</div>
  return <div className="mx-auto max-w-3xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-3"><div><h1 className="text-2xl font-semibold">{list.name}</h1><p className="mt-1 text-sm text-muted-foreground">Completed {new Date(list.completedAtUtc ?? list.generatedAtUtc).toLocaleDateString()}</p></div><div className="flex gap-2"><Button size="sm" variant="outline" onClick={() => void download()}><DownloadIcon />PDF</Button><Button size="sm" variant="outline" asChild><Link href="/app/shopping-lists">All lists</Link></Button></div></header><Card><CardHeader><div className="flex items-center justify-between"><CardTitle>Outcomes</CardTitle><Badge variant="secondary">Completed</Badge></div><CardDescription>Final purchase quantities are retained with this list.</CardDescription></CardHeader><CardContent className="space-y-2">{list.items.map((item) => <div key={item.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg bg-surface-muted/60 px-3 py-2 text-sm"><span className="font-medium">{item.itemName}</span><span className="text-muted-foreground">{item.outcome === 3 ? "Not purchased" : `${item.actualPurchaseQuantity ?? 0} ${item.trackingUnit ?? ""} purchased`}</span></div>)}</CardContent></Card><Card><CardHeader><CardTitle>Undo completed list</CardTitle><CardDescription>Reopens the list and records reversing inventory adjustments. This is blocked if it would make stock negative.</CardDescription></CardHeader><CardContent><AlertDialog><AlertDialogTrigger asChild><Button variant="outline" disabled={undoing}><RotateCcwIcon />Undo list</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Undo this completed list?</AlertDialogTitle><AlertDialogDescription>Purchased quantities will be reversed where it is safe to do so, and the list will reopen.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Keep completed</AlertDialogCancel><AlertDialogAction onClick={() => void undo()}>Undo list</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog></CardContent></Card></div>
}
