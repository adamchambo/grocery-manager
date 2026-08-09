"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { ClipboardPlusIcon, PlayIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { getApiStocktakes, postApiStocktakesStocktakeIdCancel } from "@/lib/api/generated/stocktakes/stocktakes"
import type { StocktakeResponse } from "@/lib/api/generated/models"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"
import { stocktakeStatus, stocktakeStatusLabel } from "@/features/stocktakes/stocktake-status"

export function StocktakesOverview() {
  const [stocktakes, setStocktakes] = useState<StocktakeResponse[]>([])
  const [error, setError] = useState<string>()
  const [loading, setLoading] = useState(true)
  const [cancelling, setCancelling] = useState(false)
  useErrorToast(error, "Stocktakes unavailable")

  async function load() {
    try { const response = await getApiStocktakes({ page: 1, pageSize: 50 }); setStocktakes(response.items) }
    catch (loadError) { setError(getApiErrorMessage(loadError, "Unable to load stocktakes.")) }
    finally { setLoading(false) }
  }
  useEffect(() => {
    getApiStocktakes({ page: 1, pageSize: 50 })
      .then((response) => setStocktakes(response.items))
      .catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load stocktakes.")))
      .finally(() => setLoading(false))
  }, [])

  const active = stocktakes.find((stocktake) => stocktake.status === stocktakeStatus.inProgress)
  const history = stocktakes.filter((stocktake) => stocktake.status !== stocktakeStatus.inProgress)
  async function cancel() {
    if (!active) return
    setCancelling(true)
    try { await postApiStocktakesStocktakeIdCancel(active.id); showSuccessToast("Stocktake cancelled"); await load() }
    catch (cancelError) { setError(getApiErrorMessage(cancelError, "Unable to cancel stocktake.")) }
    finally { setCancelling(false) }
  }

  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  return <div className="space-y-6"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Stocktakes</h1><p className="mt-1 text-sm text-muted-foreground">Count your pantry one location at a time.</p></div>{!active && <Button asChild><Link href="/app/stocktakes/new"><ClipboardPlusIcon />Start stocktake</Link></Button>}</header>{active && <Card className="border-primary/30 bg-primary/5"><CardHeader><div className="flex items-center justify-between gap-3"><div><CardTitle>Resume your stocktake</CardTitle><CardDescription>{active.entries.filter((entry) => entry.status !== 0).length} of {active.entries.length} entries saved</CardDescription></div><Badge>In progress</Badge></div></CardHeader><CardContent className="flex flex-wrap gap-3"><Button asChild><Link href={`/app/stocktakes/${active.id}`}><PlayIcon />Resume</Link></Button><AlertDialog><AlertDialogTrigger asChild><Button variant="outline" disabled={cancelling}>Cancel stocktake</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Cancel this stocktake?</AlertDialogTitle><AlertDialogDescription>Your pantry quantities will not change. Saved entry counts will remain in the cancelled record.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Keep stocktake</AlertDialogCancel><AlertDialogAction variant="destructive" onClick={() => void cancel()}>Cancel stocktake</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog></CardContent></Card>}<section className="space-y-3"><h2 className="text-lg font-semibold">Past stocktakes</h2>{history.length ? <div className="grid gap-3 md:grid-cols-2">{history.map((stocktake) => <Link key={stocktake.id} href={`/app/stocktakes/${stocktake.id}/review`} className="rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"><Card className="h-full transition-shadow hover:shadow-md"><CardHeader><div className="flex items-center justify-between gap-3"><CardTitle className="text-base">{new Date(stocktake.startedAtUtc).toLocaleDateString()}</CardTitle><Badge variant={stocktake.status === stocktakeStatus.completed ? "secondary" : "outline"}>{stocktakeStatusLabel(stocktake.status)}</Badge></div><CardDescription>{stocktake.entries.length} entries</CardDescription></CardHeader></Card></Link>)}</div> : <Card><CardContent className="py-10 text-center text-sm text-muted-foreground">No stocktakes yet. Start one when you&apos;re ready to count your pantry.</CardContent></Card>}</section></div>
}
