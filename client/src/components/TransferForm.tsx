import { useState, type FormEvent } from 'react'
import type { Account } from '../types'

interface TransferFormProps {
  accounts: Account[]
  disabled: boolean
  onSubmit: (fromAccountId: string, toAccountId: string, amount: number) => void
}

export function TransferForm({ accounts, disabled, onSubmit }: TransferFormProps) {
  const [fromId, setFromId] = useState('')
  const [toId, setToId] = useState('')
  const [amount, setAmount] = useState('')

  const from = fromId || accounts[0]?.id || ''
  const to = toId || accounts.find((account) => account.id !== from)?.id || ''
  const parsedAmount = Number(amount)
  const sameAccount = from !== '' && from === to
  const canSubmit = from !== '' && to !== '' && !sameAccount && parsedAmount > 0

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canSubmit) {
      return
    }
    onSubmit(from, to, parsedAmount)
    setAmount('')
  }

  return (
    <form className="action" onSubmit={handleSubmit}>
      <h3>Transfer</h3>
      <label>
        From
        <select value={from} onChange={(event) => setFromId(event.target.value)}>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>
              {account.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        To
        <select value={to} onChange={(event) => setToId(event.target.value)}>
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
      {sameAccount && <p className="action__hint">Choose two different accounts.</p>}
      <button type="submit" disabled={disabled || !canSubmit}>
        Transfer
      </button>
    </form>
  )
}
