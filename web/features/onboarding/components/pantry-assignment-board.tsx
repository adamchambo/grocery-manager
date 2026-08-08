"use client"

import { useMemo, useRef, useState } from "react"
import {
  closestCenter,
  DndContext,
  DragOverlay,
  KeyboardSensor,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
  type DragOverEvent,
  type DragStartEvent,
} from "@dnd-kit/core"
import {
  arrayMove,
  horizontalListSortingStrategy,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"
import { GripVerticalIcon, MoreHorizontalIcon, SearchIcon } from "lucide-react"

import type { ItemTemplateResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { cn } from "@/lib/utilities/cn"
import { Button } from "@/shared/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu"
import { Input } from "@/shared/components/ui/input"

type PantryAssignmentBoardProps = Readonly<{
  templates: ItemTemplateResponse[]
  locations: StorageLocationResponse[]
  assignments: Record<string, string>
  itemOrder: string[]
  locationOrder: string[]
  onAssign: (templateId: string, locationId: string) => void
  onItemOrderChange: (itemOrder: string[]) => void
  onLocationOrderChange: (locationOrder: string[]) => void
}>

const columnPrefix = "location:"

export function PantryAssignmentBoard({
  templates,
  locations,
  assignments,
  itemOrder,
  locationOrder,
  onAssign,
  onItemOrderChange,
  onLocationOrderChange,
}: PantryAssignmentBoardProps) {
  const [query, setQuery] = useState("")
  const [activeId, setActiveId] = useState<string>()
  const [targetLocationId, setTargetLocationId] = useState<string>()
  const dragStartState = useRef<{ assignments: Record<string, string>; itemOrder: string[]; locationOrder: string[] } | undefined>(undefined)
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 6 } }),
    useSensor(TouchSensor, { activationConstraint: { delay: 150, tolerance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  )
  const templateById = useMemo(
    () => new Map(templates.map((template) => [template.id, template])),
    [templates],
  )
  const visibleIds = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()
    return itemOrder.filter((id) => !normalizedQuery || templateById.get(id)?.name.toLowerCase().includes(normalizedQuery))
  }, [itemOrder, query, templateById])
  const assignedCount = Object.keys(assignments).filter((id) => assignments[id]).length

  function findLocation(id: string) {
    return id.startsWith(columnPrefix) ? id.slice(columnPrefix.length) : assignments[id]
  }

  function handleDragStart(event: DragStartEvent) {
    const id = String(event.active.id)
    dragStartState.current = { assignments: { ...assignments }, itemOrder: [...itemOrder], locationOrder: [...locationOrder] }
    setActiveId(id)
    setTargetLocationId(id.startsWith(columnPrefix) ? undefined : assignments[id])
  }

  function handleDragOver(event: DragOverEvent) {
    if (!event.over) return
    const draggedId = String(event.active.id)
    if (draggedId.startsWith(columnPrefix)) return
    const overId = String(event.over.id)
    const destinationId = findLocation(overId)
    if (!destinationId) return

    setTargetLocationId(destinationId)
    if (assignments[draggedId] !== destinationId) onAssign(draggedId, destinationId)

    const fromIndex = itemOrder.indexOf(draggedId)
    const overIndex = itemOrder.indexOf(overId)
    const toIndex = overIndex >= 0 ? overIndex : itemOrder.length - 1
    if (fromIndex >= 0 && fromIndex !== toIndex) onItemOrderChange(arrayMove(itemOrder, fromIndex, toIndex))
  }

  function handleDragCancel() {
    const previous = dragStartState.current
    if (previous && activeId) {
      if (!activeId.startsWith(columnPrefix)) onAssign(activeId, previous.assignments[activeId])
      onItemOrderChange(previous.itemOrder)
      onLocationOrderChange(previous.locationOrder)
    }
    finishDrag()
  }

  function finishDrag() {
    setActiveId(undefined)
    setTargetLocationId(undefined)
    dragStartState.current = undefined
  }

  function handleDragEnd(event: { active: { id: string | number }; over: { id: string | number } | null }) {
    const active = String(event.active.id)
    const over = event.over ? String(event.over.id) : undefined

    if (active.startsWith(columnPrefix) && over?.startsWith(columnPrefix) && active !== over) {
      const activeLocationId = active.slice(columnPrefix.length)
      const overLocationId = over.slice(columnPrefix.length)
      onLocationOrderChange(arrayMove(
        locationOrder,
        locationOrder.indexOf(activeLocationId),
        locationOrder.indexOf(overLocationId),
      ))
    }
    finishDrag()
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative w-full sm:max-w-sm">
          <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
          <Input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search selected items" aria-label="Search selected items" className="pl-9" />
        </div>
        <p className="text-sm text-muted-foreground">{assignedCount} of {templates.length} items assigned</p>
      </div>

      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
        onDragCancel={handleDragCancel}
      >
        <SortableContext items={locationOrder.map((id) => `${columnPrefix}${id}`)} strategy={horizontalListSortingStrategy}>
          <div className="flex gap-5 overflow-x-auto pb-3">
            {locations.map((location) => {
            const ids = visibleIds.filter((id) => assignments[id] === location.id)
            return (
              <LocationColumn
                key={location.id}
                location={location}
                itemIds={ids}
                itemCount={itemOrder.filter((id) => assignments[id] === location.id).length}
                highlighted={targetLocationId === location.id}
              >
                {ids.map((id) => {
                  const template = templateById.get(id)
                  return template ? <SortablePantryItem key={id} template={template} locations={locations} activeId={activeId} onAssign={onAssign} /> : null
                })}
              </LocationColumn>
            )
            })}
          </div>
        </SortableContext>
        <DragOverlay dropAnimation={{ duration: 180, easing: "ease" }}>
          {activeId && templateById.get(activeId) ? <PantryItemCard template={templateById.get(activeId)!} overlay /> : null}
        </DragOverlay>
      </DndContext>
    </div>
  )
}

function LocationColumn({
  location,
  itemIds,
  itemCount,
  highlighted,
  children,
}: Readonly<{
  location: StorageLocationResponse
  itemIds: string[]
  itemCount: number
  highlighted: boolean
  children: React.ReactNode
}>) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: `${columnPrefix}${location.id}` })

  return (
    <section ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition: transition ?? "transform 180ms ease" }} className={cn("flex h-[30rem] min-w-80 flex-1 flex-col overflow-hidden rounded-xl bg-surface-muted/70 shadow-sm ring-1 ring-border/40 transition-[background-color,box-shadow,opacity] duration-200", highlighted && "bg-primary/8 shadow-md ring-2 ring-primary/60", isDragging && "z-20 opacity-70 shadow-xl")}>
      <header {...attributes} {...listeners} className="sticky top-0 z-10 flex cursor-grab touch-none items-center justify-between bg-surface-muted px-4 py-3 shadow-xs active:cursor-grabbing">
        <h3 className="flex items-center gap-2 font-semibold"><GripVerticalIcon className="size-4 text-muted-foreground" aria-hidden="true" />{locationIcon(location.name)} {location.name}</h3>
        <span className="rounded-full bg-background/80 px-2 py-0.5 text-xs text-muted-foreground">{itemCount} {itemCount === 1 ? "item" : "items"}</span>
      </header>
      <SortableContext items={itemIds} strategy={verticalListSortingStrategy}>
        <div className="flex-1 space-y-3 overflow-y-auto p-3">
          {children}
          {itemIds.length === 0 && <div className={cn("flex min-h-24 items-center justify-center rounded-lg border border-dashed border-border/60 p-4 text-center text-sm text-muted-foreground transition-colors", highlighted && "border-primary/70 bg-primary/5 text-primary")}>Drop items here</div>}
        </div>
      </SortableContext>
    </section>
  )
}

function SortablePantryItem({ template, locations, activeId, onAssign }: Readonly<{ template: ItemTemplateResponse; locations: StorageLocationResponse[]; activeId?: string; onAssign: (templateId: string, locationId: string) => void }>) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging, isOver } = useSortable({ id: template.id })

  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition: transition ?? "transform 180ms ease" }}
      className={cn("relative", isOver && activeId !== template.id && "before:absolute before:-top-2 before:z-10 before:h-0.5 before:w-full before:rounded-full before:bg-primary")}
    >
      <PantryItemCard
        template={template}
        locations={locations}
        onAssign={onAssign}
        dragAttributes={attributes}
        dragListeners={listeners}
        dragging={isDragging}
      />
    </div>
  )
}

function PantryItemCard({ template, locations = [], onAssign, dragAttributes, dragListeners, dragging, overlay }: Readonly<{ template: ItemTemplateResponse; locations?: StorageLocationResponse[]; onAssign?: (templateId: string, locationId: string) => void; dragAttributes?: React.HTMLAttributes<HTMLElement>; dragListeners?: React.HTMLAttributes<HTMLElement>; dragging?: boolean; overlay?: boolean }>) {
  return (
    <article
      {...dragAttributes}
      {...dragListeners}
      className={cn("group flex cursor-grab touch-none items-center gap-2 rounded-lg bg-card p-3 shadow-sm ring-1 ring-border/30 transition-[transform,box-shadow,opacity,background-color] duration-200 hover:-translate-y-0.5 hover:bg-accent/40 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring active:cursor-grabbing", dragging && "scale-[0.98] opacity-30 shadow-none", overlay && "scale-[1.03] cursor-grabbing shadow-xl ring-primary/40")}
    >
      <GripVerticalIcon className="size-5 shrink-0 text-muted-foreground transition-colors group-hover:text-foreground" aria-hidden="true" />
      <span className="min-w-0 flex-1 truncate text-sm font-medium">{categoryIcon(template)} {template.name}</span>
      {!overlay && onAssign && (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon-sm" aria-label={`Move ${template.name} to another location`} onPointerDown={(event) => event.stopPropagation()}>
              <MoreHorizontalIcon aria-hidden="true" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuLabel>Move to…</DropdownMenuLabel>
            <DropdownMenuSeparator />
            {locations.map((location) => <DropdownMenuItem key={location.id} onSelect={() => onAssign(template.id, location.id)}>{locationIcon(location.name)} {location.name}</DropdownMenuItem>)}
          </DropdownMenuContent>
        </DropdownMenu>
      )}
    </article>
  )
}

function locationIcon(name: string) {
  if (name === "Fridge") return "🧊"
  if (name === "Freezer") return "❄️"
  if (name.includes("Bathroom")) return "🧻"
  if (name.includes("Laundry")) return "🧺"
  if (name.includes("Garage")) return "📦"
  if (name.includes("Cupboard")) return "🚪"
  return "🥫"
}

function categoryIcon(template: ItemTemplateResponse) {
  if (template.name === "Milk") return "🥛"
  if (["Chicken", "Mince"].includes(template.name)) return "🥩"
  if (template.name === "Pasta") return "🍝"
  if (template.name === "Bread") return "🍞"
  if (template.name === "Eggs") return "🥚"
  if (template.defaultCategoryKey === "fruit-vegetables") return "🥬"
  if (template.defaultCategoryKey === "drinks") return "☕"
  if (template.defaultCategoryKey === "household") return "🧴"
  return "•"
}
