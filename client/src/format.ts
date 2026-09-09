const currency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})

const dateTime = new Intl.DateTimeFormat('en-US', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

export function formatCurrency(amount: number): string {
  return currency.format(amount)
}

export function formatDateTime(iso: string): string {
  return dateTime.format(new Date(iso))
}
