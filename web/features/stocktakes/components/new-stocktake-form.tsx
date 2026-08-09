"use client"

import { useState, type FormEvent } from "react"
import { useRouter } from "next/navigation"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { postApiStocktakes } from "@/lib/api/generated/stocktakes/stocktakes"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

export function NewStocktakeForm() {
  const router = useRouter(); const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false)
  useErrorToast(error, "Stocktake not started")
  async function submit(event: FormEvent<HTMLFormElement>) { event.preventDefault(); setSaving(true); try { const stocktake = await postApiStocktakes({}); showSuccessToast("Stocktake started"); router.replace(`/app/stocktakes/${stocktake.id}`) } catch (startError) { setError(getApiErrorMessage(startError, "Unable to start stocktake.")); setSaving(false) } }
  return <div className="mx-auto max-w-xl space-y-6"><header><h1 className="text-2xl font-semibold">Start stocktake</h1><p className="mt-1 text-sm text-muted-foreground">Count your regular items by stock area.</p></header><Card><CardHeader><CardTitle>Ready to count?</CardTitle><CardDescription>We&apos;ll include every active regular item in the areas where you store it.</CardDescription></CardHeader><CardContent><form className="flex justify-end gap-3" onSubmit={submit}><Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button><Button type="submit" disabled={saving}>{saving && <Spinner />}{saving ? "Starting…" : "Start stocktake"}</Button></form></CardContent></Card></div>
}
