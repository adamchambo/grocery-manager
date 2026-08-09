export const stocktakeStatus = { inProgress: 0, completed: 1, cancelled: 2 } as const
export const stocktakeEntryStatus = { pending: 0, confirmed: 1, corrected: 2, zero: 3, skipped: 4 } as const

export function stocktakeStatusLabel(status: number) {
  return ["In progress", "Completed", "Cancelled"][status] ?? "Unknown"
}

export function entryStatusLabel(status: number) {
  return ["Pending", "Confirmed", "Corrected", "Zero", "Skipped"][status] ?? "Unknown"
}

export function formatQuantity(quantity: number | string, trackingUnit: string) {
  return `${quantity} ${trackingUnit.toLowerCase()}`
}
