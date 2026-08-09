"use client"

import { useEffect, useState, type FormEvent } from "react"
import { useRouter } from "next/navigation"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { getApiShoppingPresets } from "@/lib/api/generated/shopping-presets/shopping-presets"
import { postApiStocktakes } from "@/lib/api/generated/stocktakes/stocktakes"
import type { ShoppingPresetResponse } from "@/lib/api/generated/models"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function NewStocktakeForm() {
  const router = useRouter(); const [presets, setPresets] = useState<ShoppingPresetResponse[]>([]); const [presetId, setPresetId] = useState(""); const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false)
  useErrorToast(error, "Stocktake not started")
  useEffect(() => { getApiShoppingPresets().then((rows) => { const active = rows.filter((row) => !row.isArchived); setPresets(active); setPresetId(active.find((row) => row.isEverythingPreset)?.id ?? active[0]?.id ?? "") }).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load shopping presets."))) }, [])
  async function submit(event: FormEvent<HTMLFormElement>) { event.preventDefault(); if (!presetId) return; setSaving(true); try { const stocktake = await postApiStocktakes({ shoppingPresetId: presetId }); showSuccessToast("Stocktake started"); router.replace(`/app/stocktakes/${stocktake.id}`) } catch (startError) { setError(getApiErrorMessage(startError, "Unable to start stocktake.")); setSaving(false) } }
  return <div className="mx-auto max-w-xl space-y-6"><header><h1 className="text-2xl font-semibold">Start stocktake</h1><p className="mt-1 text-sm text-muted-foreground">Choose the shopping preset you are preparing for.</p></header><Card><CardHeader><CardTitle>What are you shopping for?</CardTitle><CardDescription>We&apos;ll include only the items in this preset and arrange them by storage location.</CardDescription></CardHeader><CardContent><form className="space-y-6" onSubmit={submit}><Field><FieldLabel>Shopping preset</FieldLabel><Select value={presetId} onValueChange={setPresetId}><SelectTrigger className="w-full"><SelectValue placeholder="Choose a preset" /></SelectTrigger><SelectContent>{presets.map((preset) => <SelectItem key={preset.id} value={preset.id}>{preset.name}{preset.isEverythingPreset ? " (Everything)" : ""}</SelectItem>)}</SelectContent></Select></Field><div className="flex justify-end gap-3"><Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button><Button type="submit" disabled={!presetId || saving}>{saving && <Spinner />}{saving ? "Starting…" : "Start stocktake"}</Button></div></form></CardContent></Card></div>
}
