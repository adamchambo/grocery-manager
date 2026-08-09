"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { TriangleAlertIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { getApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import type { PantryItemResponse, StocktakeEntryResponse, StocktakeResponse } from "@/lib/api/generated/models"
import { getApiStocktakesStocktakeId, postApiStocktakesStocktakeIdComplete } from "@/lib/api/generated/stocktakes/stocktakes"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Checkbox } from "@/shared/components/ui/checkbox"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"
import { entryStatusLabel, formatQuantity, stocktakeEntryStatus, stocktakeStatus, stocktakeStatusLabel } from "@/features/stocktakes/stocktake-status"

export function StocktakeReview() {
  const { stocktakeId } = useParams<{ stocktakeId: string }>(); const router = useRouter(); const [stocktake, setStocktake] = useState<StocktakeResponse>(); const [saveOrder, setSaveOrder] = useState(false); const [saving, setSaving] = useState(false); const [error, setError] = useState<string>()
  useErrorToast(error, "Stocktake not completed")
  useEffect(() => { getApiStocktakesStocktakeId(stocktakeId).then(setStocktake).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load stocktake."))) }, [stocktakeId])
  const pending = stocktake?.entries.some((entry) => entry.status === stocktakeEntryStatus.pending) ?? false
  async function complete() {
    if (!stocktake || pending) return; setSaving(true)
    try {
      const locationItemOrders = saveOrder ? await buildLocationItemOrders(stocktake.entries) : undefined
      await postApiStocktakesStocktakeIdComplete(stocktake.id, locationItemOrders ? { locationItemOrders } : undefined)
      showSuccessToast("Stocktake completed"); router.replace("/app/stocktakes")
    } catch (completeError) { setError(getApiErrorMessage(completeError, "Unable to complete stocktake.")); setSaving(false) }
  }
  if (!stocktake && !error) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!stocktake) return <div className="flex min-h-64 items-center justify-center text-sm text-muted-foreground">Stocktake unavailable.</div>
  const skipped = stocktake.entries.filter((entry) => entry.status === stocktakeEntryStatus.skipped); const outliers = stocktake.entries.filter((entry) => entry.isOutlier)
  return <div className="mx-auto max-w-4xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Review stocktake</h1><p className="mt-1 text-sm text-muted-foreground">{stocktakeStatusLabel(stocktake.status)} · {stocktake.entries.length} entries</p></div>{stocktake.status === stocktakeStatus.inProgress && <Button variant="outline" onClick={() => router.push(`/app/stocktakes/${stocktake.id}`)}>Back to count</Button>}</header>{pending && <Card className="border-amber-500/40"><CardContent className="flex items-center gap-3 py-5 text-sm"><TriangleAlertIcon className="size-5 text-amber-600" />Return to the checklist and resolve the remaining pending entries before completing.</CardContent></Card>}<div className="grid gap-3 sm:grid-cols-3"><SummaryCard label="Confirmed" value={stocktake.entries.filter((entry) => entry.status !== stocktakeEntryStatus.pending && entry.status !== stocktakeEntryStatus.skipped).length} /><SummaryCard label="Skipped" value={skipped.length} /><SummaryCard label="Unusual counts" value={outliers.length} /></div>{skipped.length > 0 && <EntrySection title="Skipped entries" description="Skipped entries keep their previous confirmed quantity and are marked unverified." entries={skipped} />}{outliers.length > 0 && <EntrySection title="Unusual counts" description="These differ substantially from the estimated quantity but do not block completion." entries={outliers} />}{stocktake.status === stocktakeStatus.inProgress && <Card><CardHeader><CardTitle>Finish stocktake</CardTitle><CardDescription>Completing applies confirmed quantities to your pantry and records inventory history.</CardDescription></CardHeader><CardContent className="space-y-5"><label className="flex cursor-pointer items-start gap-3 rounded-lg bg-surface-muted p-4"><Checkbox checked={saveOrder} onCheckedChange={(checked) => setSaveOrder(checked === true)} /><span><span className="block font-medium">Save this item order for future stocktakes</span><span className="block text-sm text-muted-foreground">Keep the drag order you used in each location as the new pantry default.</span></span></label><AlertDialog><AlertDialogTrigger asChild><Button disabled={pending || saving}>{saving && <Spinner />}{saving ? "Completing…" : "Complete stocktake"}</Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Complete stocktake?</AlertDialogTitle><AlertDialogDescription>Confirmed counts will become your trusted pantry quantities. This cannot be edited as an active stocktake afterward.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Keep counting</AlertDialogCancel><AlertDialogAction onClick={() => void complete()}>Complete stocktake</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog></CardContent></Card>}<EntrySection title="All entries" entries={stocktake.entries} /></div>
}

function SummaryCard({ label, value }: Readonly<{ label: string; value: number }>) { return <Card><CardContent className="py-5"><p className="text-sm text-muted-foreground">{label}</p><p className="mt-1 text-2xl font-semibold">{value}</p></CardContent></Card> }
function EntrySection({ title, description, entries }: Readonly<{ title: string; description?: string; entries: StocktakeEntryResponse[] }>) { return <Card><CardHeader><CardTitle>{title}</CardTitle>{description && <CardDescription>{description}</CardDescription>}</CardHeader><CardContent className="space-y-2">{entries.map((entry) => <div key={entry.id} className="flex flex-wrap items-center justify-between gap-3 rounded-lg border px-4 py-3"><div><p className="font-medium">{entry.itemName}</p><p className="text-sm text-muted-foreground">{entry.locationName} · {entry.recordedQuantity === null ? "No count recorded" : formatQuantity(entry.recordedQuantity, entry.trackingUnit)}</p></div><Badge variant={entry.status === stocktakeEntryStatus.skipped ? "outline" : "secondary"}>{entryStatusLabel(entry.status)}</Badge></div>)}</CardContent></Card> }

async function buildLocationItemOrders(entries: StocktakeEntryResponse[]) {
  const pantryItems = await loadAllPantryItems(); const locations = new Map<string, { id: string; sortOrder: number }[]>()
  for (const item of pantryItems) for (const location of item.locations) { const rows = locations.get(location.storageLocationId) ?? []; rows.push({ id: location.id, sortOrder: Number(location.sortOrder) }); locations.set(location.storageLocationId, rows) }
  return [...new Map(entries.map((entry) => [entry.storageLocationId, true])).keys()].map((storageLocationId) => {
    const stocktakeRows = entries.filter((entry) => entry.storageLocationId === storageLocationId); const ordered = stocktakeRows.map((entry) => entry.pantryItemLocationId); const included = new Set(ordered); let next = 0
    const pantryItemLocationIds = [...(locations.get(storageLocationId) ?? [])].sort((left, right) => left.sortOrder - right.sortOrder).map((row) => included.has(row.id) ? ordered[next++] : row.id)
    return { storageLocationId, pantryItemLocationIds }
  })
}

async function loadAllPantryItems() { const items: PantryItemResponse[] = []; let page = 1; while (true) { const response = await getApiPantryItems({ page, pageSize: 100 }); items.push(...response.items); if (items.length >= Number(response.totalCount)) return items; page += 1 } }
