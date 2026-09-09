export type TransactionType = 'Deposit' | 'Withdrawal'

export interface Transaction {
  id: string
  type: TransactionType
  amount: number
  balanceAfter: number
  occurredAt: string
  description: string | null
}

export interface Account {
  id: string
  name: string
  balance: number
  transactions: Transaction[]
}

export interface TransferResult {
  from: Account
  to: Account
}
