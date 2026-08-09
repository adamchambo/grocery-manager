"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { closestCenter, DndContext, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from "@dnd-kit/core"
import { arrayMove, SortableContext, sortableKeyboardCoordinates, useSortable, verticalListSortingStrategy } from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"
import { GripVerticalIcon, PackagePlusIcon, TriangleAlertIcon, XIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { getApiCategories } from "@/lib/api/generated/categories/categories"
import type { CategoryResponse, StocktakeEntryResponse, StocktakeResponse } from "@/lib/api/generated/models"
import { getApiStocktakesStocktakeId, postApiStocktakesStocktakeIdDiscoveredItems, putApiStocktakesStocktakeIdLocationEntries, putApiStocktakesStocktakeIdLocationOrder } from "@/lib/api/generated/stocktakes/stocktakes"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/shared/components/ui/dialog"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Progress } from "@/shared/components/ui/progress"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"
import { formatQuantity, stocktakeEntryStatus } from "@/features/stocktakes/stocktake-status"

type LocationGroup = { id: string; name: string; entries: StocktakeEntryResponse[] }
type CountDrafts = Record<string, string>

function draftCounts(entries: StocktakeEntryResponse[]): CountDrafts {
  return Object.fromEntries(entries.map((entry) => [entry.id, String(entry.recordedQuantity ?? entry.estimatedQuantity)]))
}

export function StocktakeChecklist() {
  const { stocktakeId } = useParams<{ stocktakeId: string }>(); const router = useRouter()
  const [stocktake, setStocktake] = useState<StocktakeResponse>(); const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [drafts, setDrafts] = useState<CountDrafts>({}); const [error, setError] = useState<string>(); const [savingLocationId, setSavingLocationId] = useState<string>(); const [savingOrderId, setSavingOrderId] = useState<string>(); const [addLocation, setAddLocation] = useState<LocationGroup>(); const [locationStep, setLocationStep] = useState(0)
  useErrorToast(error, "Stocktake not saved")
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }), useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }))

  async function load() {
    try {
      const [row, categoryRows] = await Promise.all([getApiStocktakesStocktakeId(stocktakeId), getApiCategories()])
      setStocktake(row); setCategories(categoryRows.filter((category) => !category.isArchived)); setDrafts(draftCounts(row.entries))
    } catch (loadError) { setError(getApiErrorMessage(loadError, "Unable to load stocktake.")) }
  }
  useEffect(() => {
    Promise.all([getApiStocktakesStocktakeId(stocktakeId), getApiCategories()])
      .then(([row, categoryRows]) => { setStocktake(row); setCategories(categoryRows.filter((category) => !category.isArchived)); setDrafts(draftCounts(row.entries)) })
      .catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load stocktake.")))
  }, [stocktakeId])

  function groups(): LocationGroup[] {
    if (!stocktake) return []
    const values = new Map<string, LocationGroup>()
    for (const entry of stocktake.entries) {
      const current = values.get(entry.storageLocationId) ?? { id: entry.storageLocationId, name: entry.locationName, entries: [] }
      current.entries.push(entry); values.set(entry.storageLocationId, current)
    }
    return [...values.values()]
  }
  function replaceEntries(updated: StocktakeEntryResponse[]) {
    const updatedById = new Map(updated.map((entry) => [entry.id, entry]))
    setStocktake((current) => current ? { ...current, entries: current.entries.map((entry) => updatedById.get(entry.id) ?? entry) } : current)
    setDrafts((current) => ({ ...current, ...draftCounts(updated) }))
  }
  async function reorder(locationId: string, activeId: string, overId: string) {
    if (!stocktake || activeId === overId) return
    const group = groups().find((value) => value.id === locationId); if (!group) return
    const oldIndex = group.entries.findIndex((entry) => entry.id === activeId); const newIndex = group.entries.findIndex((entry) => entry.id === overId); if (oldIndex < 0 || newIndex < 0) return
    const reordered = arrayMove(group.entries, oldIndex, newIndex); let index = 0
    setStocktake((current) => current ? { ...current, entries: current.entries.map((entry) => entry.storageLocationId === locationId ? reordered[index++] : entry) } : current)
    setSavingOrderId(locationId)
    try { await putApiStocktakesStocktakeIdLocationOrder(stocktakeId, { storageLocationId: locationId, pantryItemLocationIds: reordered.map((entry) => entry.pantryItemLocationId) }) }
    catch (orderError) { setError(getApiErrorMessage(orderError, "Unable to save stocktake order.")); void load() }
    finally { setSavingOrderId(undefined) }
  }
  const locationGroups = groups(); const resolved = stocktake?.entries.filter((entry) => entry.status !== stocktakeEntryStatus.pending).length ?? 0; const total = stocktake?.entries.length ?? 0; const currentStep = Math.min(locationStep, Math.max(0, locationGroups.length - 1)); const currentGroup = locationGroups[currentStep]; const totalSteps = locationGroups.length + 1
  async function saveCurrentLocation() {
    if (!currentGroup) return
    const entries = currentGroup.entries.map((entry) => ({ entryId: entry.id, recordedQuantity: Number(drafts[entry.id] ?? entry.recordedQuantity ?? entry.estimatedQuantity), version: entry.version }))
    if (entries.some((entry) => !Number.isFinite(entry.recordedQuantity) || entry.recordedQuantity < 0)) { setError("Enter a non-negative count for every item."); return }
    setSavingLocationId(currentGroup.id)
    try {
      const updated = await putApiStocktakesStocktakeIdLocationEntries(stocktakeId, { storageLocationId: currentGroup.id, entries })
      replaceEntries(updated); showSuccessToast(`${currentGroup.name} saved`)
      if (currentStep < locationGroups.length - 1) setLocationStep((step) => step + 1)
      else router.push(`/app/stocktakes/${stocktakeId}/review`)
    } catch (saveError) { setError(getApiErrorMessage(saveError, "Unable to save this area.")) }
    finally { setSavingLocationId(undefined) }
  }
  if (!stocktake && !error) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  if (!stocktake) return <div className="flex min-h-64 items-center justify-center text-sm text-muted-foreground">Stocktake unavailable.</div>
  const savingCurrentLocation = savingLocationId === currentGroup?.id
  return <div className="mx-auto max-w-4xl space-y-6"><header className="flex flex-wrap items-start justify-between gap-4"><div><h1 className="text-2xl font-semibold">Stocktake</h1><p className="mt-1 text-sm text-muted-foreground">{resolved} of {total} item counts saved. Each area saves when you continue.</p></div><Button variant="outline" onClick={() => router.replace("/app/stocktakes")}>Exit stocktake</Button></header>{currentGroup && <div className="space-y-2"><div className="flex items-center justify-between text-sm"><span className="font-medium">{currentGroup.name}</span><span className="text-muted-foreground">Step {currentStep + 1} of {totalSteps}</span></div><Progress value={((currentStep + 1) / totalSteps) * 100} /></div>}{currentGroup ? <LocationChecklist key={currentGroup.id} group={currentGroup} drafts={drafts} sensors={sensors} saving={savingCurrentLocation || savingOrderId === currentGroup.id} savingOrder={savingOrderId === currentGroup.id} onDraftChange={(entryId, value) => setDrafts((current) => ({ ...current, [entryId]: value }))} onReorder={(event) => { if (event.over) void reorder(currentGroup.id, String(event.active.id), String(event.over.id)) }} onAddItem={() => setAddLocation(currentGroup)} /> : <Card><CardContent className="py-10 text-center text-sm text-muted-foreground">This preset has no active pantry items.</CardContent></Card>}<div className="flex flex-wrap justify-between gap-3"><Button variant="outline" disabled={currentStep === 0 || Boolean(savingLocationId)} onClick={() => setLocationStep((step) => Math.max(0, step - 1))}>Back</Button><Button disabled={!currentGroup || Boolean(savingLocationId) || Boolean(savingOrderId)} onClick={() => void saveCurrentLocation()}>{savingCurrentLocation && <Spinner />}{currentStep < locationGroups.length - 1 ? "Next area" : "Review stocktake"}</Button></div><AddDiscoveredItemDialog open={Boolean(addLocation)} location={addLocation} categories={categories} onOpenChange={(open) => { if (!open) setAddLocation(undefined) }} onAdded={(entry) => { setStocktake((current) => current ? { ...current, entries: [...current.entries, entry] } : current); setDrafts((current) => ({ ...current, ...draftCounts([entry]) })); setAddLocation(undefined); showSuccessToast("Item added to stocktake") }} onError={setError} stocktakeId={stocktakeId} /></div>
}

function LocationChecklist({ group, drafts, sensors, saving, savingOrder, onDraftChange, onReorder, onAddItem }: Readonly<{ group: LocationGroup; drafts: CountDrafts; sensors: ReturnType<typeof useSensors>; saving: boolean; savingOrder: boolean; onDraftChange: (entryId: string, value: string) => void; onReorder: (event: DragEndEvent) => void; onAddItem: () => void }>) {
  const resolved = group.entries.filter((entry) => entry.status !== stocktakeEntryStatus.pending).length
  return <Card><CardHeader><div className="flex flex-wrap items-start justify-between gap-3"><div><CardTitle>{group.name}</CardTitle><CardDescription>{resolved} of {group.entries.length} item counts saved. Change every count you need, then continue.{savingOrder ? " Saving order…" : ""}</CardDescription></div><Button variant="outline" size="sm" disabled={saving} onClick={onAddItem}><PackagePlusIcon />Add item</Button></div></CardHeader><CardContent><DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onReorder}><SortableContext items={group.entries.map((entry) => entry.id)} strategy={verticalListSortingStrategy}><div className="space-y-3">{group.entries.map((entry) => <SortableStocktakeEntry key={entry.id} entry={entry} value={drafts[entry.id] ?? String(entry.recordedQuantity ?? entry.estimatedQuantity)} saving={saving} onChange={onDraftChange} />)}</div></SortableContext></DndContext></CardContent></Card>
}

function SortableStocktakeEntry({ entry, value, saving, onChange }: Readonly<{ entry: StocktakeEntryResponse; value: string; saving: boolean; onChange: (entryId: string, value: string) => void }>) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: entry.id, disabled: saving })
  return <div ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} className={isDragging ? "relative z-10 opacity-70" : undefined}><div className="rounded-xl bg-surface-muted/55 p-4 transition-[background-color,box-shadow,transform] duration-200 hover:bg-surface-muted hover:shadow-sm"><div className="flex flex-wrap items-start gap-3"><Button type="button" size="icon-sm" variant="ghost" className="cursor-grab touch-none active:cursor-grabbing" aria-label={`Reorder ${entry.itemName}`} {...attributes} {...listeners}><GripVerticalIcon /></Button><div className="min-w-40 flex-1"><p className="font-medium">{entry.itemName}</p><p className="text-sm text-muted-foreground">Estimated: {formatQuantity(entry.estimatedQuantity, entry.trackingUnit)}</p></div></div>{entry.isOutlier && <p className="mt-3 flex items-center gap-2 text-sm text-amber-700"><TriangleAlertIcon className="size-4" />This differs substantially from the estimate.</p>}<div className="mt-3 flex flex-wrap items-end gap-2"><div className="min-w-40 flex-1"><FieldLabel htmlFor={`quantity-${entry.id}`}>Count</FieldLabel><Input id={`quantity-${entry.id}`} type="number" min="0" step="0.001" value={value} disabled={saving} onChange={(event) => onChange(entry.id, event.target.value)} /></div><Button type="button" variant="ghost" size="icon-sm" disabled={saving} aria-label={`Record zero for ${entry.itemName}`} title="Record zero" onClick={() => onChange(entry.id, "0")}><XIcon /></Button></div></div></div>
}

function AddDiscoveredItemDialog({ open, location, categories, onOpenChange, onAdded, onError, stocktakeId }: Readonly<{ open: boolean; location?: LocationGroup; categories: CategoryResponse[]; onOpenChange: (open: boolean) => void; onAdded: (entry: StocktakeEntryResponse) => void; onError: (message: string) => void; stocktakeId: string }>) {
  const [categoryId, setCategoryId] = useState(""); const [trackingUnit, setTrackingUnit] = useState("0"); const [saving, setSaving] = useState(false); const selectedCategoryId = categoryId || categories[0]?.id || ""
  async function add(event: React.FormEvent<HTMLFormElement>) { event.preventDefault(); if (!location || !selectedCategoryId) return; const data = new FormData(event.currentTarget); const name = String(data.get("name") ?? "").trim(); const value = Number(data.get("quantity")); if (!name || !Number.isFinite(value) || value < 0) return; setSaving(true); try { onAdded(await postApiStocktakesStocktakeIdDiscoveredItems(stocktakeId, { name, categoryId: selectedCategoryId, storageLocationId: location.id, trackingUnit: Number(trackingUnit), recordedQuantity: value })) } catch (addError) { onError(getApiErrorMessage(addError, "Unable to add item.")) } finally { setSaving(false) } }
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent><DialogHeader><DialogTitle>Add item to {location?.name}</DialogTitle><DialogDescription>This creates a pantry item and records its first count in this location.</DialogDescription></DialogHeader><form key={location?.id} className="grid gap-4" onSubmit={add}><Field><FieldLabel htmlFor="discovered-item-name">Item name</FieldLabel><Input id="discovered-item-name" name="name" maxLength={160} required /></Field><Field><FieldLabel>Category</FieldLabel><Select value={selectedCategoryId} onValueChange={setCategoryId}><SelectTrigger className="w-full"><SelectValue placeholder="Choose a category" /></SelectTrigger><SelectContent>{categories.map((category) => <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>)}</SelectContent></Select></Field><Field><FieldLabel>Tracking unit</FieldLabel><Select value={trackingUnit} onValueChange={setTrackingUnit}><SelectTrigger className="w-full"><SelectValue /></SelectTrigger><SelectContent>{["Package", "Item", "Weight", "Volume"].map((label, index) => <SelectItem key={label} value={String(index)}>{label}</SelectItem>)}</SelectContent></Select></Field><Field><FieldLabel htmlFor="discovered-item-quantity">Count</FieldLabel><Input id="discovered-item-quantity" name="quantity" type="number" min="0" step="0.001" defaultValue="0" required /></Field><DialogFooter><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button><Button type="submit" disabled={!selectedCategoryId || saving}>{saving && <Spinner />}{saving ? "Adding…" : "Add item"}</Button></DialogFooter></form></DialogContent></Dialog>
}
