"use client"

import Link from "next/link"
import { useEffect, useLayoutEffect, useRef, useState, type CSSProperties, type FormEvent } from "react"
import { closestCenter, DndContext, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from "@dnd-kit/core"
import { arrayMove, SortableContext, sortableKeyboardCoordinates, useSortable, verticalListSortingStrategy } from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"
import { ArrowDownIcon, ArrowUpIcon, ArchiveIcon, GripVerticalIcon, PlusIcon } from "lucide-react"

import { deleteApiCategoriesCategoryId, getApiCategories, postApiCategories, putApiCategoriesCategoryId, putApiCategoriesOrder } from "@/lib/api/generated/categories/categories"
import type { CategoryResponse, PantryItemResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { getApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { deleteApiStorageLocationsLocationId, getApiStorageLocations, postApiStorageLocations, putApiStorageLocationsLocationId, putApiStorageLocationsOrder } from "@/lib/api/generated/storage-locations/storage-locations"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from "@/shared/components/ui/alert-dialog"
import { Badge } from "@/shared/components/ui/badge"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Input } from "@/shared/components/ui/input"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

type Row = CategoryResponse | StorageLocationResponse

export function PantryStructureManager({ kind }: Readonly<{ kind: "categories" | "locations" }>) {
  const [rows, setRows] = useState<Row[]>([]); const [items, setItems] = useState<PantryItemResponse[]>([])
  const [names, setNames] = useState<Record<string, string>>({}); const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false); const [reordering, setReordering] = useState(false); const [loading, setLoading] = useState(true)
  const rowElements = useRef(new Map<string, HTMLDivElement>()); const previousPositions = useRef(new Map<string, number>())
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }), useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }))
  const singular = kind === "categories" ? "category" : "location"; const title = kind === "categories" ? "Categories" : "Storage locations"
  useErrorToast(error, "Request failed")

  async function load() { try { const [loadedRows, pantryItems] = await Promise.all([kind === "categories" ? getApiCategories() : getApiStorageLocations(), getApiPantryItems({ page: 1, pageSize: 100 })]); const active = loadedRows.filter((row) => !row.isArchived).sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder)); setRows(active); setNames(Object.fromEntries(active.map((row) => [row.id, row.name]))); setItems(pantryItems.items) } catch (loadError) { setError(getApiErrorMessage(loadError, `Unable to load ${kind}.`)) } finally { setLoading(false) } }
  useEffect(() => {
    Promise.all([kind === "categories" ? getApiCategories() : getApiStorageLocations(), getApiPantryItems({ page: 1, pageSize: 100 })])
      .then(([loadedRows, pantryItems]) => {
        const active = loadedRows.filter((row) => !row.isArchived).sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder))
        setRows(active); setNames(Object.fromEntries(active.map((row) => [row.id, row.name]))); setItems(pantryItems.items)
      })
      .catch((loadError) => setError(getApiErrorMessage(loadError, `Unable to load ${kind}.`)))
      .finally(() => setLoading(false))
  }, [kind])

  async function add(event: FormEvent<HTMLFormElement>) { event.preventDefault(); setSaving(true); const form = event.currentTarget; const data = new FormData(form); try { if (kind === "categories") await postApiCategories({ name: String(data.get("name")), sortOrder: rows.length }); else await postApiStorageLocations({ name: String(data.get("name")), sortOrder: rows.length }); form.reset(); await load(); showSuccessToast(`${singular[0].toUpperCase()}${singular.slice(1)} added`) } catch (requestError) { setError(getApiErrorMessage(requestError, `Unable to add ${singular}.`)) } finally { setSaving(false) } }
  async function rename(row: Row) { setSaving(true); try { if (kind === "categories") await putApiCategoriesCategoryId(row.id, { name: names[row.id], sortOrder: row.sortOrder, version: row.version }); else await putApiStorageLocationsLocationId(row.id, { name: names[row.id], sortOrder: row.sortOrder, version: row.version }); await load(); showSuccessToast(`${singular[0].toUpperCase()}${singular.slice(1)} saved`) } catch (requestError) { setError(getApiErrorMessage(requestError, `Unable to update ${singular}.`)) } finally { setSaving(false) } }
  async function archive(row: Row) { setSaving(true); try { if (kind === "categories") await deleteApiCategoriesCategoryId(row.id); else await deleteApiStorageLocationsLocationId(row.id); await load(); showSuccessToast(`${singular[0].toUpperCase()}${singular.slice(1)} archived`) } catch (requestError) { setError(getApiErrorMessage(requestError, `Unable to archive ${singular}.`)) } finally { setSaving(false) } }
  function capturePositions() { previousPositions.current = new Map([...rowElements.current].map(([id, element]) => [id, element.getBoundingClientRect().top])) }
  useLayoutEffect(() => {
    if (!previousPositions.current.size) return
    rowElements.current.forEach((element, id) => {
      const previousTop = previousPositions.current.get(id); if (previousTop === undefined) return
      const distance = previousTop - element.getBoundingClientRect().top
      if (distance) element.animate([{ transform: `translateY(${distance}px)` }, { transform: "translateY(0)" }], { duration: 220, easing: "cubic-bezier(0.2, 0, 0, 1)" })
    })
    previousPositions.current.clear()
  }, [rows])

  async function persistOrder(next: Row[]) { setReordering(true); setError(undefined); try { if (kind === "categories") await putApiCategoriesOrder({ categoryIds: next.map((row) => row.id) }); else await putApiStorageLocationsOrder({ storageLocationIds: next.map((row) => row.id) }); await load(); showSuccessToast(`${title} reordered`) } catch (requestError) { setError(getApiErrorMessage(requestError, `Unable to reorder ${kind}.`)); await load() } finally { setReordering(false) } }
  function move(index: number, direction: -1 | 1) { const nextIndex = index + direction; if (nextIndex < 0 || nextIndex >= rows.length || reordering) return; capturePositions(); const next = arrayMove(rows, index, nextIndex); setRows(next); void persistOrder(next) }
  function dragEnd(event: DragEndEvent) { if (event.active.id === event.over?.id || !event.over || reordering) return; const oldIndex = rows.findIndex((row) => row.id === event.active.id); const newIndex = rows.findIndex((row) => row.id === event.over?.id); if (oldIndex < 0 || newIndex < 0) return; const next = arrayMove(rows, oldIndex, newIndex); setRows(next); void persistOrder(next) }

  if (loading) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  return <div className="space-y-6"><header><h1 className="text-2xl font-semibold">{title}</h1><p className="mt-1 text-sm text-muted-foreground">Add, rename, reorder, or archive your pantry {kind}.</p></header><Card><CardHeader><CardTitle>Add {singular}</CardTitle><CardDescription>New {kind} appear at the end of the current order.</CardDescription></CardHeader><CardContent><form className="flex flex-col gap-3 sm:flex-row" onSubmit={add}><Input name="name" maxLength={120} placeholder={`${singular[0].toUpperCase()}${singular.slice(1)} name`} required /><Button type="submit" disabled={saving}><PlusIcon />Add</Button></form></CardContent></Card><DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragEnd}><SortableContext items={rows.map((row) => row.id)} strategy={verticalListSortingStrategy}><div className="space-y-3">{rows.map((row, index) => <SortableStructureRow key={row.id} row={row} index={index} rowCount={rows.length} name={names[row.id] ?? row.name} itemCount={kind === "locations" ? locationCount(row.id, items) : undefined} kind={kind} disabled={saving || reordering} setElement={(element) => { if (element) rowElements.current.set(row.id, element); else rowElements.current.delete(row.id) }} onNameChange={(value) => setNames((current) => ({ ...current, [row.id]: value }))} onMove={move} onRename={() => void rename(row)} onArchive={() => void archive(row)} />)}</div></SortableContext></DndContext></div>
}

type SortableStructureRowProps = Readonly<{ row: Row; index: number; rowCount: number; name: string; itemCount?: number; kind: "categories" | "locations"; disabled: boolean; setElement: (element: HTMLDivElement | null) => void; onNameChange: (value: string) => void; onMove: (index: number, direction: -1 | 1) => void; onRename: () => void; onArchive: () => void }>

function SortableStructureRow({ row, index, rowCount, name, itemCount, kind, disabled, setElement, onNameChange, onMove, onRename, onArchive }: SortableStructureRowProps) {
  const { attributes, listeners, setNodeRef, setActivatorNodeRef, transform, transition, isDragging } = useSortable({ id: row.id, disabled })
  const style: CSSProperties = { transform: CSS.Transform.toString(transform), transition }
  function setRefs(element: HTMLDivElement | null) { setNodeRef(element); setElement(element) }
  return <div ref={setRefs} style={style} className={isDragging ? "relative z-10 opacity-70" : undefined}><Card className="gap-0 py-0"><CardContent className="grid gap-3 p-4 sm:grid-cols-[auto_minmax(12rem,1fr)_auto] sm:items-center"><Button ref={setActivatorNodeRef} type="button" size="icon-sm" variant="ghost" className="hidden cursor-grab touch-none text-muted-foreground active:cursor-grabbing sm:inline-flex" aria-label={`Drag to reorder ${row.name}`} {...attributes} {...listeners}><GripVerticalIcon /></Button><Input value={name} onChange={(event) => onNameChange(event.target.value)} /><div className="flex flex-wrap items-center gap-1.5 sm:justify-end">{row.isDefault && <Badge variant="secondary">Default</Badge>}{itemCount !== undefined && <Badge variant="outline">{itemCount} items</Badge>}<Button size="icon-sm" variant="ghost" aria-label={`Move ${row.name} up`} disabled={disabled || index === 0} onClick={() => onMove(index, -1)}><ArrowUpIcon /></Button><Button size="icon-sm" variant="ghost" aria-label={`Move ${row.name} down`} disabled={disabled || index === rowCount - 1} onClick={() => onMove(index, 1)}><ArrowDownIcon /></Button><Button variant="outline" size="sm" onClick={onRename} disabled={disabled || !name.trim()}>Save</Button>{kind === "locations" && <Button variant="outline" size="sm" asChild><Link href={`/app/pantry/locations/${row.id}/arrange`}>Arrange</Link></Button>}<AlertDialog><AlertDialogTrigger asChild><Button type="button" size="icon-sm" variant="ghost" aria-label={`Archive ${row.name}`} disabled={disabled}><ArchiveIcon /></Button></AlertDialogTrigger><AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Are you sure you want to archive {row.name}?</AlertDialogTitle><AlertDialogDescription>This {kind === "categories" ? "category" : "storage location"} will disappear from active pantry views, but its historical records will be preserved.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Cancel</AlertDialogCancel><AlertDialogAction variant="destructive" onClick={onArchive}>Archive</AlertDialogAction></AlertDialogFooter></AlertDialogContent></AlertDialog></div></CardContent></Card></div>
}

function locationCount(locationId: string, items: PantryItemResponse[]) { return items.filter((item) => item.locations.some((location) => location.storageLocationId === locationId)).length }
