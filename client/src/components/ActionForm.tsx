import { useState, type FormEvent } from 'react'
import type { Account } from '../types'

interface ActionFormProps {
  label: string
  accounts: Account[]
  disabled: boolean
  onSubmit: (accountId: string, amount: number) => void
}

/** Amount form shared by the deposit and withdraw actions. */
export function ActionForm({ label, accounts, disabled, onSubmit }: ActionFormProps) {
  const [accountId, setAccountId] = useState('')
  const [amount, setAmount] = useState('')

  const selectedAccount = accountId || accounts[0]?.id || ''
  const parsedAmount = Number(amount)
  const canSubmit = selectedAccount !== '' && parsedAmount > 0

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canSubmit) {
      return
    }
    onSubmit(selectedAccount, parsedAmount)
    setAmount('')
  }

  return (
    <form className="action" onSubmit={handleSubmit}>
      <h3>{label}</h3>
      <label>
        Account
        <select value={selectedAccount} onChange={(event) => setAccountId(event.target.value)}>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>
              {account.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Amount
        <input
          type="number"
          min="0"
          step="0.01"
          placeholder="0.00"
          value={amount}
          onChange={(event) => setAmount(event.target.value)}
        />
      </label>
      <button type="submit" disabled={disabled || !canSubmit}>
        {label}
      </button>
    </form>
  )
}
