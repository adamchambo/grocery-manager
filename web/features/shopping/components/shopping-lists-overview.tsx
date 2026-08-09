"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { ListPlusIcon, PlayIcon, Trash2Icon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import type { ShoppingListResponse } from "@/lib/api/generated/models"
import { getApiShoppingLists } from "@/lib/api/generated/shopping-lists/shopping-lists"
import { apiFetch } from "@/lib/api/api-client"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"

const active = 0

export function ShoppingListsOverview() {
  const [lists, setLists] = useState<ShoppingListResponse[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState<string>()
  useErrorToast(error, "Shopping lists unavailable")
  useEffect(() => { getApiShoppingLists({ page: 1, pageSize: 50 }).then((response) => setLists(response.items)).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load shopping lists."))).finally(() => setLoading(false)) }, [])
  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  const current = lists.filter((list) => list.status === active); const history = lists.filter((list) => list.status === 1)
  async function deleteList(id: string) { try { await apiFetch<void>(`/api/shopping-lists/${id}`, { method: "DELETE" }); setLists((current) => current.filter((list) => list.id !== id)) } catch (value) { setError(getApiErrorMessage(value, "Unable to delete shopping list.")) } }
  return <div className="space-y-7"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Shopping lists</h1><p className="mt-1 text-sm text-muted-foreground">Build a list, use it while shopping, and keep a useful record.</p></div><Button asChild><Link href="/app/shopping-lists/new"><ListPlusIcon />New shopping list</Link></Button></header><section className="space-y-3"><h2 className="text-lg font-semibold">Active lists</h2>{current.length ? <div className="grid gap-3 md:grid-cols-2">{current.map((list) => <ActiveListCard key={list.id} list={list} onDelete={() => void deleteList(list.id)} />)}</div> : <Card><CardContent className="py-9 text-center text-sm text-muted-foreground">No active shopping lists. Generate one from a completed stocktake.</CardContent></Card>}</section><section className="space-y-3"><h2 className="text-lg font-semibold">Completed lists</h2>{history.length ? <div className="grid gap-3 md:grid-cols-2">{history.map((list) => <Card key={list.id} className="h-full"><CardHeader><div className="flex items-center justify-between gap-3"><CardTitle className="text-base"><Link href={`/app/shopping-lists/${list.id}`} className="hover:underline">{list.name}</Link></CardTitle><Badge variant="secondary">Completed</Badge></div><CardDescription>{list.items.filter((item) => item.outcome === 1 || item.outcome === 2).length} purchased · {new Date(list.completedAtUtc ?? list.generatedAtUtc).toLocaleDateString()}</CardDescription></CardHeader><CardContent><DeleteListButton listName={list.name} onDelete={() => void deleteList(list.id)} /></CardContent></Card>)}</div> : <Card><CardContent className="py-9 text-center text-sm text-muted-foreground">Completed lists will appear here.</CardContent></Card>}</section></div>
}

function ActiveListCard({ list, onDelete }: Readonly<{ list: ShoppingListResponse; onDelete: () => void }>) {
  const resolved = list.items.filter((item) => item.outcome !== 0).length
  return <Card><CardHeader><div className="flex items-start justify-between gap-3"><div><CardTitle>{list.name}</CardTitle><CardDescription>{resolved} of {list.items.length} items resolved</CardDescription></div><Badge>Active</Badge></div></CardHeader><CardContent><div className="flex flex-wrap gap-2"><Button asChild><Link href={`/app/shopping-lists/${list.id}/shop`}><PlayIcon />Resume shopping</Link></Button><Button variant="outline" asChild><Link href={`/app/shopping-lists/${list.id}/review`}>Review list</Link></Button><DeleteListButton listName={list.name} onDelete={onDelete} /></div></CardContent></Card>
}

function DeleteListButton({ listName, onDelete }: Readonly<{ listName: string; onDelete: () => void }>) {
  return <AlertDialog><AlertDialogTrigger asChild><Button size="icon-sm" variant="ghost" aria-label={`Delete ${listName}`}><Trash2Icon /></Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Delete {listName}?</AlertDialogTitle><AlertDialogDescription>This shopping list and its items will be permanently removed.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Keep list</AlertDialogCancel><AlertDialogAction variant="destructive" onClick={onDelete}>Delete list</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog>
}
