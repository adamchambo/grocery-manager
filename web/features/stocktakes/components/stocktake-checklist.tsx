"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { StocktakeEntryResponse, StocktakeResponse } from "@/lib/api/generated/models"
import { getApiStocktakesStocktakeId, putApiStocktakesStocktakeIdLocationEntries } from "@/lib/api/generated/stocktakes/stocktakes"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Progress } from "@/shared/components/ui/progress"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"
import { trackingUnitLabel } from "@/features/stocktakes/stocktake-status"

type Area = { id: string; name: string; entries: StocktakeEntryResponse[] }

export function StocktakeChecklist() {
  const { stocktakeId } = useParams<{ stocktakeId: string }>(); const router = useRouter(); const [stocktake, setStocktake] = useState<StocktakeResponse>(); const [drafts, setDrafts] = useState<Record<string, string>>({}); const [areaIndex, setAreaIndex] = useState(0); const [saving, setSaving] = useState(false); const [error, setError] = useState<string>()
  useErrorToast(error, "Stocktake not saved")
  async function load() { try { const value = await getApiStocktakesStocktakeId(stocktakeId); setStocktake(value); setDrafts(Object.fromEntries(value.entries.map((entry) => [entry.id, entry.recordedQuantity == null ? "" : String(entry.recordedQuantity)]))) } catch (value) { setError(getApiErrorMessage(value, "Unable to load stocktake.")) } }
  useEffect(() => { void load() }, [stocktakeId])
  if (!stocktake) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  const areas = Object.values(stocktake.entries.reduce<Record<string, Area>>((result, entry) => { (result[entry.storageLocationId] ??= { id: entry.storageLocationId, name: entry.locationName, entries: [] }).entries.push(entry); return result }, {})); const area = areas[areaIndex]
  async function saveArea() { if (!area) return; const entries = area.entries.map((entry) => ({ entryId: entry.id, recordedQuantity: Number(drafts[entry.id]), version: entry.version })); if (entries.some((entry) => !Number.isFinite(entry.recordedQuantity) || entry.recordedQuantity < 0)) { setError("Enter a quantity for every item in this area."); return } setSaving(true); try { await putApiStocktakesStocktakeIdLocationEntries(stocktakeId, { storageLocationId: area.id, entries }); showSuccessToast(`${area.name} saved`); if (areaIndex < areas.length - 1) setAreaIndex((value) => value + 1); else router.push(`/app/stocktakes/${stocktakeId}/review`) } catch (value) { setError(getApiErrorMessage(value, "Unable to save this area.")) } finally { setSaving(false) } }
  return <div className="mx-auto max-w-4xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Stocktake</h1><p className="mt-1 text-sm text-muted-foreground">Count what you have in each area.</p></div><Button variant="outline" onClick={() => router.replace("/app/stocktakes")}>Exit</Button></header><div className="space-y-2"><div className="flex items-center justify-between text-sm"><span className="font-medium">{area?.name}</span><span className="text-muted-foreground">Area {areaIndex + 1} of {areas.length}</span></div><Progress value={areas.length ? ((areaIndex + 1) / areas.length) * 100 : 0} /></div><Card><CardHeader><CardTitle>{area?.name}</CardTitle><CardDescription>Enter the quantity you can see. Items appear here only because they are stored in this area.</CardDescription></CardHeader><CardContent className="space-y-3">{area?.entries.map((entry) => <div key={entry.id} className="rounded-xl bg-surface-muted/55 px-4 py-3 sm:grid sm:grid-cols-[minmax(0,1fr)_7rem_6rem] sm:items-center sm:gap-3"><div><FieldLabel htmlFor={`quantity-${entry.id}`} className="font-medium text-foreground">{entry.itemName}</FieldLabel><p className="mt-0.5 text-xs text-muted-foreground">Count in {trackingUnitLabel(entry.trackingUnit)}</p></div><Input id={`quantity-${entry.id}`} aria-label={`${entry.itemName} quantity in ${trackingUnitLabel(entry.trackingUnit)}`} type="number" min="0" step="0.001" value={drafts[entry.id] ?? ""} disabled={saving} onChange={(event) => setDrafts((value) => ({ ...value, [entry.id]: event.target.value }))} className="mt-2 w-full sm:mt-0" /><span className="mt-1 text-sm text-muted-foreground sm:mt-0">{trackingUnitLabel(entry.trackingUnit)}</span></div>)}</CardContent></Card><div className="flex justify-between gap-3"><Button variant="outline" disabled={areaIndex === 0 || saving} onClick={() => setAreaIndex((value) => value - 1)}>Back</Button><Button disabled={!area || saving} onClick={() => void saveArea()}>{saving && <Spinner />}{areaIndex < areas.length - 1 ? "Next area" : "Generate shopping list"}</Button></div></div>
}
