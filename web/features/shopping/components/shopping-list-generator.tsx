"use client"

import { useEffect, useState } from "react"
import { useRouter, useSearchParams } from "next/navigation"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { StocktakeResponse } from "@/lib/api/generated/models"
import { postApiShoppingLists } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { getApiStocktakes } from "@/lib/api/generated/stocktakes/stocktakes"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Label } from "@/shared/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

export function ShoppingListGenerator() {
  const router = useRouter(); const search = useSearchParams(); const [stocktakes, setStocktakes] = useState<StocktakeResponse[]>([]); const [stocktakeId, setStocktakeId] = useState(search.get("stocktakeId") ?? ""); const [loading, setLoading] = useState(true); const [saving, setSaving] = useState(false); const [error, setError] = useState<string>()
  useErrorToast(error, "Shopping list not created")
  useEffect(() => { getApiStocktakes({ page: 1, pageSize: 50 }).then((response) => setStocktakes(response.items.filter((stocktake) => stocktake.status === 1))).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load completed stocktakes."))).finally(() => setLoading(false)) }, [])
  async function generate() { if (!stocktakeId) { setError("Choose a completed stocktake."); return } setSaving(true); try { const list = await postApiShoppingLists({ stocktakeId, name: null }); router.replace(`/app/shopping-lists/${list.id}/review`) } catch (generateError) { setError(getApiErrorMessage(generateError, "Unable to generate shopping list.")); setSaving(false) } }
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  return <div className="mx-auto max-w-2xl space-y-6"><header><h1 className="text-2xl font-semibold">New shopping list</h1><p className="mt-1 text-sm text-muted-foreground">Generate your list from a completed stocktake.</p></header><Card><CardHeader><CardTitle>Completed stocktake</CardTitle><CardDescription>Your shopping routine and the quantities you counted determine what to buy.</CardDescription></CardHeader><CardContent className="space-y-5"><div className="space-y-2"><Label htmlFor="stocktake">Stocktake</Label><Select value={stocktakeId} onValueChange={setStocktakeId}><SelectTrigger id="stocktake" className="w-full"><SelectValue placeholder="Choose a completed stocktake" /></SelectTrigger><SelectContent position="popper" className="w-[var(--radix-select-trigger-width)]">{stocktakes.map((stocktake) => <SelectItem key={stocktake.id} value={stocktake.id}>{new Date(stocktake.completedAtUtc ?? stocktake.startedAtUtc).toLocaleDateString()} · {stocktake.entries.length} items counted</SelectItem>)}</SelectContent></Select></div><div className="flex justify-end"><Button onClick={() => void generate()} disabled={!stocktakeId || saving}>{saving && <Spinner />}{saving ? "Generating…" : "Generate list"}</Button></div></CardContent></Card></div>
}
