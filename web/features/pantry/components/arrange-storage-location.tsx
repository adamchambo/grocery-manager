"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { closestCenter, DndContext, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from "@dnd-kit/core"
import { arrayMove, SortableContext, sortableKeyboardCoordinates, useSortable, verticalListSortingStrategy } from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"
import { GripVerticalIcon } from "lucide-react"

import type { PantryItemResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { getApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { getApiStorageLocations, putApiStorageLocationsLocationIdItemOrder } from "@/lib/api/generated/storage-locations/storage-locations"
import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent } from "@/shared/components/ui/card"
import { Spinner } from "@/shared/components/ui/spinner"
import { showSuccessToast, useErrorToast } from "@/shared/hooks/use-error-toast"

type OrderedItem = { item: PantryItemResponse; pantryItemLocationId: string }

export function ArrangeStorageLocation() {
  const { locationId } = useParams<{ locationId: string }>(); const router = useRouter()
  const [location, setLocation] = useState<StorageLocationResponse>(); const [items, setItems] = useState<OrderedItem[]>([]); const [error, setError] = useState<string>(); const [saving, setSaving] = useState(false)
  useErrorToast(error, "Request failed")
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }), useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }))
  useEffect(() => { Promise.all([getApiStorageLocations(), getApiPantryItems({ page: 1, pageSize: 100 })]).then(([locations, response]) => { setLocation(locations.find((row) => row.id === locationId)); setItems(response.items.flatMap((item) => { const itemLocation = item.locations.find((row) => row.storageLocationId === locationId); return itemLocation ? [{ item, pantryItemLocationId: itemLocation.id, sortOrder: Number(itemLocation.sortOrder) }] : [] }).sort((a, b) => a.sortOrder - b.sortOrder).map(({ item, pantryItemLocationId }) => ({ item, pantryItemLocationId }))) }).catch((loadError) => setError(getApiErrorMessage(loadError, "Unable to load location items."))) }, [locationId])
  function dragEnd(event: DragEndEvent) { if (!event.over || event.active.id === event.over.id) return; setItems((current) => arrayMove(current, current.findIndex((row) => row.pantryItemLocationId === event.active.id), current.findIndex((row) => row.pantryItemLocationId === event.over?.id))) }
  async function save() { setSaving(true); try { await putApiStorageLocationsLocationIdItemOrder(locationId, { pantryItemLocationIds: items.map((row) => row.pantryItemLocationId) }); showSuccessToast("Item order saved"); router.replace("/app/pantry/locations") } catch (saveError) { setError(getApiErrorMessage(saveError, "Unable to save item order.")); setSaving(false) } }
  if (!location && !error) return <div className="flex min-h-64 items-center justify-center"><Spinner className="size-6" /></div>
  return <div className="mx-auto max-w-2xl space-y-6"><header><h1 className="text-2xl font-semibold">Arrange {location?.name ?? "location"}</h1><p className="mt-1 text-sm text-muted-foreground">Match the physical order you move through this location during a stocktake.</p></header><DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragEnd}><SortableContext items={items.map((row) => row.pantryItemLocationId)} strategy={verticalListSortingStrategy}><div className="space-y-3">{items.map((row) => <SortableRow key={row.pantryItemLocationId} row={row} />)}{!items.length && <Card><CardContent className="pt-6 text-center text-sm text-muted-foreground">No pantry items use this location yet.</CardContent></Card>}</div></SortableContext></DndContext><div className="flex justify-end gap-3"><Button variant="outline" onClick={() => router.back()}>Cancel</Button><Button onClick={save} disabled={saving || !items.length}>{saving && <Spinner />}{saving ? "Saving…" : "Save order"}</Button></div></div>
}

function SortableRow({ row }: Readonly<{ row: OrderedItem }>) { const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: row.pantryItemLocationId }); return <div ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} {...attributes} {...listeners} className={`flex cursor-grab touch-none items-center gap-3 rounded-lg bg-card p-4 shadow-sm ring-1 ring-border/40 ${isDragging ? "z-10 scale-[1.02] opacity-70 shadow-lg" : ""}`}><GripVerticalIcon className="size-5 text-muted-foreground" /><span className="font-medium">{row.item.name}</span></div> }
