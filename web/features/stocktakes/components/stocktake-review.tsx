"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { TriangleAlertIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { StocktakeEntryResponse, StocktakeResponse } from "@/lib/api/generated/models"
import { getApiStocktakesStocktakeId, postApiStocktakesStocktakeIdComplete } from "@/lib/api/generated/stocktakes/stocktakes"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"
import { entryStatusLabel, formatQuantity, stocktakeEntryStatus, stocktakeStatus, stocktakeStatusLabel } from "@/features/stocktakes/stocktake-status"

export function StocktakeReview() {
  const { stocktakeId } = useParams<{ stocktakeId: string }>(); const router = useRouter(); const [stocktake, setStocktake] = useState<StocktakeResponse>(); const [saving, setSaving] = useState(false); const [error, setError] = useState<string>()
  useErrorToast(error, "Stocktake not completed")
  useEffect(() => { getApiStocktakesStocktakeId(stocktakeId).then(setStocktake).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load stocktake."))) }, [stocktakeId])
  const pending = stocktake?.entries.some((entry) => entry.status === stocktakeEntryStatus.pending) ?? false
  async function complete() {
    if (!stocktake || pending) return; setSaving(true)
    try {
      await postApiStocktakesStocktakeIdComplete(stocktake.id)
      showSuccessToast("Stocktake completed"); router.replace("/app/stocktakes")
    } catch (completeError) { setError(getApiErrorMessage(completeError, "Unable to complete stocktake.")); setSaving(false) }
  }
  if (!stocktake && !error) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!stocktake) return <div className="flex min-h-64 items-center justify-center text-sm text-muted-foreground">Stocktake unavailable.</div>
  const skipped = stocktake.entries.filter((entry) => entry.status === stocktakeEntryStatus.skipped); const outliers = stocktake.entries.filter((entry) => entry.isOutlier)
  return <div className="mx-auto max-w-4xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Review stocktake</h1><p className="mt-1 text-sm text-muted-foreground">{stocktakeStatusLabel(stocktake.status)} · {stocktake.entries.length} entries</p></div>{stocktake.status === stocktakeStatus.inProgress && <Button variant="outline" onClick={() => router.push(`/app/stocktakes/${stocktake.id}`)}>Back to count</Button>}</header>{pending && <Card className="border-amber-500/40"><CardContent className="flex items-center gap-3 py-5 text-sm"><TriangleAlertIcon className="size-5 text-amber-600" />Return to the checklist and resolve the remaining pending entries before completing.</CardContent></Card>}<div className="flex flex-wrap gap-3"><SummaryCard label="Confirmed" value={stocktake.entries.filter((entry) => entry.status !== stocktakeEntryStatus.pending && entry.status !== stocktakeEntryStatus.skipped).length} /><SummaryCard label="Skipped" value={skipped.length} /><SummaryCard label="Unusual counts" value={outliers.length} /></div>{skipped.length > 0 && <EntrySection title="Skipped entries" description="Skipped entries keep their previous confirmed quantity and are marked unverified." entries={skipped} />}{outliers.length > 0 && <EntrySection title="Unusual counts" description="These differ substantially from the estimated quantity but do not block completion." entries={outliers} />}{stocktake.status === stocktakeStatus.inProgress && <Card><CardHeader><CardTitle>Finish stocktake</CardTitle><CardDescription>Completing applies the counts you entered to your pantry and records inventory history.</CardDescription></CardHeader><CardContent><AlertDialog><AlertDialogTrigger asChild><Button disabled={pending || saving}>{saving && <Spinner />}{saving ? "Completing…" : "Complete stocktake"}</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Complete stocktake?</AlertDialogTitle><AlertDialogDescription>Your entered counts will become the pantry quantities. This stocktake cannot be changed afterward.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Keep counting</AlertDialogCancel><AlertDialogAction onClick={() => void complete()}>Complete stocktake</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog></CardContent></Card>}<EntrySection title="All entries" entries={stocktake.entries} /></div>
}

function SummaryCard({ label, value }: Readonly<{ label: string; value: number }>) { return <Card className="w-44 gap-1 self-start py-3 shadow-none"><CardContent className="flex items-baseline justify-between gap-3 py-0"><p className="text-sm text-muted-foreground">{label}</p><p className="text-xl font-semibold">{value}</p></CardContent></Card> }
function EntrySection({ title, description, entries }: Readonly<{ title: string; description?: string; entries: StocktakeEntryResponse[] }>) { return <Card><CardHeader><CardTitle>{title}</CardTitle>{description && <CardDescription>{description}</CardDescription>}</CardHeader><CardContent className="space-y-2">{entries.map((entry) => <div key={entry.id} className="flex flex-wrap items-center justify-between gap-3 rounded-xl bg-surface-muted/60 px-4 py-3 transition-colors duration-150 hover:bg-surface-muted"><div><p className="font-medium">{entry.itemName}</p><p className="text-sm text-muted-foreground">{entry.locationName} · {entry.recordedQuantity === null ? "No count recorded" : formatQuantity(entry.recordedQuantity, entry.trackingUnit)}</p></div><Badge variant={entry.status === stocktakeEntryStatus.skipped ? "outline" : "secondary"}>{entryStatusLabel(entry.status)}</Badge></div>)}</CardContent></Card> }
