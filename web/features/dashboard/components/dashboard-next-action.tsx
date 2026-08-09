"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { ClipboardCheckIcon, PackagePlusIcon, ShoppingCartIcon } from "lucide-react"

import { getApiPantriesCurrentRoutine } from "@/lib/api/generated/pantries/pantries"
import { getApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiShoppingLists } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { getApiStocktakes } from "@/lib/api/generated/stocktakes/stocktakes"
import type { ShoppingListResponse, ShoppingRoutineResponse, StocktakeResponse } from "@/lib/api/generated/models"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"

const inProgress = 0

export function DashboardNextAction() {
  const [stocktake, setStocktake] = useState<StocktakeResponse>(); const [shoppingList, setShoppingList] = useState<ShoppingListResponse>(); const [routine, setRoutine] = useState<ShoppingRoutineResponse>(); const [hasItems, setHasItems] = useState<boolean>()
  useEffect(() => { void Promise.allSettled([getApiStocktakes({ page: 1, pageSize: 50 }), getApiShoppingLists({ page: 1, pageSize: 50 }), getApiPantriesCurrentRoutine(), getApiPantryItems({ page: 1, pageSize: 1 })]).then(([stocktakes, lists, routineResult, items]) => { if (stocktakes.status === "fulfilled") setStocktake(stocktakes.value.items.find((item) => item.status === inProgress)); if (lists.status === "fulfilled") setShoppingList(lists.value.items.find((item) => item.status === inProgress)); if (routineResult.status === "fulfilled") setRoutine(routineResult.value); if (items.status === "fulfilled") setHasItems(Number(items.value.totalCount) > 0) }) }, [])
  if (hasItems === undefined) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  const action = !hasItems ? <ActionCard icon={PackagePlusIcon} title="Add your regular items" description="Set up the things you usually buy, then stocktake will do the rest." href="/app/pantry/items/new" label="Add regular items" /> : stocktake ? <ActionCard icon={ClipboardCheckIcon} title="Resume your stocktake" description={`${stocktake.entries.filter((item) => item.status !== 0).length} of ${stocktake.entries.length} items counted.`} href={`/app/stocktakes/${stocktake.id}`} label="Resume stocktake" /> : shoppingList ? <ActionCard icon={ShoppingCartIcon} title="Continue shopping" description={`${shoppingList.items.filter((item) => item.outcome !== 0).length} of ${shoppingList.items.length} items resolved.`} href={`/app/shopping-lists/${shoppingList.id}/shop`} label="Continue shopping" /> : <ActionCard icon={ClipboardCheckIcon} title="Ready for your next shop?" description="Count what you have and we’ll calculate what to buy." href="/app/stocktakes/new" label="Start stocktake" />
  return <div className="mx-auto max-w-3xl space-y-6"><header><h1 className="text-2xl font-semibold tracking-tight">Home</h1><p className="mt-1 text-sm text-muted-foreground">Your next shopping action, without the inventory admin.</p></header>{action}{routine && <Card className="gap-0 py-0"><CardContent className="flex flex-wrap items-center justify-between gap-4 p-5"><div><p className="font-medium">Shopping routine</p><p className="mt-1 text-sm text-muted-foreground">Every {routine.shoppingIntervalDays} days{routine.primaryShopName ? ` · ${routine.primaryShopName}` : ""}</p></div><Button variant="outline" size="sm" asChild><Link href="/app/settings/routine">Edit routine</Link></Button></CardContent></Card>}</div>
}

function ActionCard({ icon: Icon, title, description, href, label }: Readonly<{ icon: typeof ClipboardCheckIcon; title: string; description: string; href: string; label: string }>) { return <Card className="border-primary/25 bg-primary/5"><CardHeader><div className="flex items-start justify-between gap-4"><div><CardTitle>{title}</CardTitle><CardDescription className="mt-1">{description}</CardDescription></div><Badge>Next step</Badge></div></CardHeader><CardContent><Button asChild><Link href={href}><Icon />{label}</Link></Button></CardContent></Card> }
