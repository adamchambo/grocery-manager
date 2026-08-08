"use client"

import { useEffect, useMemo, useState, type FormEvent } from "react"
import { useRouter } from "next/navigation"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { PantryAssignmentBoard } from "@/features/onboarding/components/pantry-assignment-board"
import { ApiError } from "@/lib/api/api-client"
import { getApiCategories } from "@/lib/api/generated/categories/categories"
import { getApiItemTemplates } from "@/lib/api/generated/item-templates/item-templates"
import type { CategoryResponse, ItemTemplateResponse, StorageLocationResponse } from "@/lib/api/generated/models"
import { getApiPantriesCurrent, postApiPantries } from "@/lib/api/generated/pantries/pantries"
import { getApiPantryItems, postApiPantryItems } from "@/lib/api/generated/pantry-items/pantry-items"
import { deleteApiStorageLocationsLocationId, getApiStorageLocations, putApiStorageLocationsOrder } from "@/lib/api/generated/storage-locations/storage-locations"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Checkbox } from "@/shared/components/ui/checkbox"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Progress } from "@/shared/components/ui/progress"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

const draftKey = "grocery-manager:onboarding:selected-template-ids"

export function OnboardingFlow() {
  const router = useRouter()
  const [step, setStep] = useState(0)
  const [templates, setTemplates] = useState<ItemTemplateResponse[]>([])
  const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [locations, setLocations] = useState<StorageLocationResponse[]>([])
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [createdIds, setCreatedIds] = useState<string[]>([])
  const [configurationStep, setConfigurationStep] = useState<"locations" | "assign" | "review">("locations")
  const [enabledLocationIds, setEnabledLocationIds] = useState<string[]>([])
  const [locationOrder, setLocationOrder] = useState<string[]>([])
  const [assignments, setAssignments] = useState<Record<string, string>>({})
  const [itemOrder, setItemOrder] = useState<string[]>([])
  const [error, setError] = useState<string>()
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  useErrorToast(error)

  useEffect(() => {
    async function load() {
      try {
        const itemTemplates = await getApiItemTemplates()
        setTemplates(itemTemplates)
        setSelectedIds(readDraft())

        try {
          await getApiPantriesCurrent()
          const [pantryCategories, pantryLocations, pantryItems] = await Promise.all([
            getApiCategories(),
            getApiStorageLocations(),
            getApiPantryItems({ page: 1, pageSize: 100 }),
          ])
          setCategories(pantryCategories.filter((category) => !category.isArchived))
          setLocations(pantryLocations.filter((location) => !location.isArchived))
          setEnabledLocationIds(pantryLocations.filter((location) => !location.isArchived).map((location) => location.id))
          setLocationOrder(pantryLocations.filter((location) => !location.isArchived).map((location) => location.id))
          setCreatedIds(pantryItems.items.flatMap((item) => item.sourceTemplateId ? [item.sourceTemplateId] : []))
          setStep(1)
        } catch (pantryError) {
          if (!(pantryError instanceof ApiError) || pantryError.status !== 404) throw pantryError
        }
      } catch (loadError) {
        if (loadError instanceof ApiError && loadError.status === 401) {
          router.replace("/login")
          return
        }
        setError(getApiErrorMessage(loadError, "Unable to load onboarding."))
      } finally {
        setIsLoading(false)
      }
    }

    void load()
  }, [router])

  const availableTemplates = useMemo(
    () => templates.filter((template) => !createdIds.includes(template.id)),
    [createdIds, templates],
  )
  const selectedTemplates = useMemo(
    () => availableTemplates.filter((template) => selectedIds.includes(template.id)),
    [availableTemplates, selectedIds],
  )
  const allTemplatesSelected = availableTemplates.length > 0
    && availableTemplates.every((template) => selectedIds.includes(template.id))
  const someTemplatesSelected = availableTemplates.some((template) => selectedIds.includes(template.id))

  async function createPantry(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setError(undefined)
    const data = new FormData(event.currentTarget)

    try {
      await postApiPantries({ name: String(data.get("pantryName")) })
      const [pantryCategories, pantryLocations] = await Promise.all([
        getApiCategories(),
        getApiStorageLocations(),
      ])
      setCategories(pantryCategories)
      setLocations(pantryLocations)
      setEnabledLocationIds(pantryLocations.map((location) => location.id))
      setLocationOrder(pantryLocations.map((location) => location.id))
      setStep(1)
    } catch (submissionError) {
      setError(getApiErrorMessage(submissionError, "Unable to create your pantry."))
    } finally {
      setIsSubmitting(false)
    }
  }

  function toggleTemplate(templateId: string, checked: boolean) {
    const next = checked
      ? [...selectedIds, templateId]
      : selectedIds.filter((id) => id !== templateId)
    setSelectedIds(next)
    localStorage.setItem(draftKey, JSON.stringify(next))
  }

  function toggleAllTemplates(checked: boolean) {
    const next = checked ? availableTemplates.map((template) => template.id) : []
    setSelectedIds(next)
    localStorage.setItem(draftKey, JSON.stringify(next))
  }

  function startConfiguration() {
    setAssignments(Object.fromEntries(selectedTemplates.map((template) => [
      template.id,
      suggestedLocationId(template, locations),
    ])))
    setItemOrder(selectedTemplates.map((template) => template.id))
    setConfigurationStep("locations")
    setStep(2)
  }

  function toggleLocation(locationId: string, enabled: boolean) {
    if (!enabled && enabledLocationIds.length === 1) return

    const nextIds = enabled
      ? [...enabledLocationIds, locationId]
      : enabledLocationIds.filter((id) => id !== locationId)
    const fallbackId = nextIds[0] ?? ""

    setEnabledLocationIds(nextIds)
    if (!enabled) {
      setAssignments((current) => Object.fromEntries(
        Object.entries(current).map(([templateId, assignedId]) => [
          templateId,
          assignedId === locationId ? fallbackId : assignedId,
        ]),
      ))
    }
  }

  function assignTemplate(templateId: string, locationId: string) {
    setAssignments((current) => ({ ...current, [templateId]: locationId }))
  }

  async function createSelectedItems() {
    setIsSubmitting(true)
    setError(undefined)

    try {
      for (const template of selectedTemplates) {
        const category = findTemplateCategory(template, categories)
        const assignedLocationId = assignments[template.id]
        if (!category) throw new Error(`No category is available for ${template.name}.`)
        if (!assignedLocationId) throw new Error(`No location is assigned to ${template.name}.`)

        await postApiPantryItems({
          categoryId: category.id,
          sourceTemplateId: template.id,
          defaultStorageLocationId: assignedLocationId,
          name: template.name,
          icon: null,
          brand: null,
          preferredProduct: null,
          notes: null,
          trackingUnit: template.defaultTrackingUnit,
          packageSize: null,
          packageUnit: null,
          consumptionQuantity: null,
          consumptionPeriodDays: null,
          bufferDays: 0,
          locations: [{
            storageLocationId: assignedLocationId,
            currentQuantity: 0,
            sortOrder: itemOrder.filter((id) => assignments[id] === assignedLocationId).indexOf(template.id),
          }],
        })
        setCreatedIds((current) => [...current, template.id])
      }
      await putApiStorageLocationsOrder({ storageLocationIds: locationOrder })
      await Promise.all(
        locations
          .filter((location) => !enabledLocationIds.includes(location.id))
          .map((location) => deleteApiStorageLocationsLocationId(location.id)),
      )
      localStorage.removeItem(draftKey)
      setStep(3)
    } catch (submissionError) {
      setError(getApiErrorMessage(submissionError, "Unable to add the selected pantry items."))
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <div className="flex min-h-72 items-center justify-center"><Spinner className="size-6" /></div>
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <div className="flex items-center justify-between text-sm">
          <span className="font-medium">Pantry setup</span>
          <span className="text-muted-foreground">Step {Math.min(step + 1, 4)} of 4</span>
        </div>
        <Progress value={(Math.min(step + 1, 4) / 4) * 100} />
      </div>

      {step === 0 && (
        <Card>
          <CardHeader><CardTitle>Name your pantry</CardTitle><CardDescription>We’ll add editable starter categories, storage locations, and an Everything preset.</CardDescription></CardHeader>
          <CardContent>
            <form className="space-y-6" onSubmit={createPantry}>
              <Field><FieldLabel htmlFor="pantry-name">Pantry name</FieldLabel><Input id="pantry-name" name="pantryName" defaultValue="My Pantry" maxLength={120} disabled={isSubmitting} required /></Field>
              <Button type="submit" disabled={isSubmitting}>{isSubmitting && <Spinner />}{isSubmitting ? "Creating pantry…" : "Create pantry"}</Button>
            </form>
          </CardContent>
        </Card>
      )}

      {step === 1 && (
        <Card>
          <CardHeader><CardTitle>Select common items</CardTitle><CardDescription>Choose the groceries you want to track now. You can add more later.</CardDescription></CardHeader>
          <CardContent className="space-y-6">
            <label className="flex cursor-pointer items-center gap-3 rounded-lg border bg-surface-muted p-3">
              <Checkbox
                checked={allTemplatesSelected ? true : someTemplatesSelected ? "indeterminate" : false}
                onCheckedChange={(checked) => toggleAllTemplates(checked === true)}
              />
              <span className="text-sm font-semibold">Select all common items</span>
            </label>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {availableTemplates.map((template) => (
                <label key={template.id} className="flex cursor-pointer items-center gap-3 rounded-lg border p-3 hover:bg-accent">
                  <Checkbox checked={selectedIds.includes(template.id)} onCheckedChange={(checked) => toggleTemplate(template.id, checked === true)} />
                  <span className="text-sm font-medium">{template.name}</span>
                </label>
              ))}
            </div>
            <div className="flex justify-end"><Button disabled={selectedTemplates.length === 0} onClick={startConfiguration}>Continue with {selectedTemplates.length} item{selectedTemplates.length === 1 ? "" : "s"}</Button></div>
          </CardContent>
        </Card>
      )}

      {step === 2 && (
        <Card>
          <CardHeader>
            <CardTitle>Configure your pantry</CardTitle>
            <CardDescription>
              {configurationStep === "locations" && "Choose which of the default storage locations you use."}
              {configurationStep === "assign" && "Move each item into its usual storage location."}
              {configurationStep === "review" && "Review the category and location assigned to every item."}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="flex gap-2" aria-label="Configuration progress">
              {(["locations", "assign", "review"] as const).map((item, index) => (
                <div key={item} className="flex flex-1 items-center gap-2 text-xs text-muted-foreground">
                  <span className={configurationStep === item ? "flex size-6 items-center justify-center rounded-full bg-primary font-semibold text-primary-foreground" : "flex size-6 items-center justify-center rounded-full bg-muted"}>{index + 1}</span>
                  <span className="hidden capitalize sm:inline">{item}</span>
                </div>
              ))}
            </div>

            {configurationStep === "locations" && (
              <div className="grid gap-3 sm:grid-cols-2">
                {locations.map((location) => {
                  const enabled = enabledLocationIds.includes(location.id)
                  return <label key={location.id} className="flex cursor-pointer items-center gap-3 rounded-lg border p-4 hover:bg-accent"><Checkbox checked={enabled} disabled={enabled && enabledLocationIds.length === 1} onCheckedChange={(checked) => toggleLocation(location.id, checked === true)} /><span className="font-medium">{location.name}</span></label>
                })}
              </div>
            )}

            {configurationStep === "assign" && (
              <PantryAssignmentBoard
                templates={selectedTemplates}
                locations={locationOrder.flatMap((id) => {
                  const location = locations.find((candidate) => candidate.id === id)
                  return location && enabledLocationIds.includes(id) ? [location] : []
                })}
                assignments={assignments}
                itemOrder={itemOrder}
                locationOrder={locationOrder.filter((id) => enabledLocationIds.includes(id))}
                onAssign={assignTemplate}
                onItemOrderChange={setItemOrder}
                onLocationOrderChange={(enabledOrder) => setLocationOrder([
                  ...enabledOrder,
                  ...locationOrder.filter((id) => !enabledLocationIds.includes(id)),
                ])}
              />
            )}

            {configurationStep === "review" && (
              <div className="divide-y rounded-lg border">
                {selectedTemplates.map((template) => <div key={template.id} className="grid gap-1 p-3 text-sm sm:grid-cols-[1fr_1fr_0.7fr_1fr]"><span className="font-medium">{template.name}</span><span className="text-muted-foreground">{findTemplateCategory(template, categories)?.name ?? "Miscellaneous"}</span><span className="text-muted-foreground">{trackingUnitName(template.defaultTrackingUnit)}</span><span className="text-muted-foreground sm:text-right">{locations.find((location) => location.id === assignments[template.id])?.name}</span></div>)}
              </div>
            )}

            <div className="flex justify-between gap-3">
              <Button variant="outline" onClick={() => configurationStep === "locations" ? setStep(1) : setConfigurationStep(configurationStep === "review" ? "assign" : "locations")} disabled={isSubmitting}>Back</Button>
              {configurationStep === "locations" && <Button onClick={() => setConfigurationStep("assign")}>Assign items</Button>}
              {configurationStep === "assign" && <Button onClick={() => setConfigurationStep("review")} disabled={selectedTemplates.some((template) => !assignments[template.id])}>Review</Button>}
              {configurationStep === "review" && <Button onClick={createSelectedItems} disabled={isSubmitting}>{isSubmitting && <Spinner />}{isSubmitting ? "Adding items…" : "Finish setup"}</Button>}
            </div>
          </CardContent>
        </Card>
      )}

      {step === 3 && (
        <Card className="text-center"><CardHeader><CardTitle>Your pantry is ready</CardTitle><CardDescription>Your defaults, selected items, and Everything preset are ready to use.</CardDescription></CardHeader><CardContent><Button onClick={() => router.replace("/app")}>Continue to dashboard</Button></CardContent></Card>
      )}
    </div>
  )
}

function readDraft() {
  try {
    return JSON.parse(localStorage.getItem(draftKey) ?? "[]") as string[]
  } catch {
    return []
  }
}

function findTemplateCategory(template: ItemTemplateResponse, categories: CategoryResponse[]) {
  return categories.find((category) => categoryKey(category.name) === template.defaultCategoryKey)
    ?? categories.find((category) => category.name === "Miscellaneous")
}

function categoryKey(name: string) {
  return name.toLowerCase().replace(/&/g, " ").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "")
}

function suggestedLocationId(template: ItemTemplateResponse, locations: StorageLocationResponse[]) {
  const suggestedName = (() => {
    if (["Milk", "Eggs", "Butter", "Cheese", "Yoghurt"].includes(template.name)) return "Fridge"
    if (["Chicken", "Mince"].includes(template.name) || template.defaultCategoryKey === "frozen") return "Freezer"
    if (template.name === "Toilet Paper") return "Bathroom"
    if (template.name === "Dishwashing Liquid") return "Kitchen Cupboard"
    return "Pantry"
  })()

  return locations.find((location) => location.name === suggestedName)?.id
    ?? locations.find((location) => location.name === "Pantry")?.id
    ?? locations[0]?.id
    ?? ""
}

function trackingUnitName(unit: number) {
  return ["Package", "Item", "Weight", "Volume"][unit] ?? "Item"
}
