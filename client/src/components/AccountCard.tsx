import { formatCurrency, formatDateTime } from '../format'
import type { Account } from '../types'

interface AccountCardProps {
  account: Account
}

export function AccountCard({ account }: AccountCardProps) {
  const history = [...account.transactions].reverse()

  return (
    <section className="card">
      <header className="card__header">
        <h2>{account.name}</h2>
        <p className="card__balance">{formatCurrency(account.balance)}</p>
      </header>

      {history.length === 0 ? (
        <p className="card__empty">No transactions yet.</p>
      ) : (
        <table className="history">
          <thead>
            <tr>
              <th>When</th>
              <th>Type</th>
              <th className="num">Amount</th>
              <th className="num">Balance</th>
              <th>Note</th>
            </tr>
          </thead>
          <tbody>
            {history.map((entry) => (
              <tr key={entry.id}>
                <td>{formatDateTime(entry.occurredAt)}</td>
                <td>{entry.type}</td>
                <td className="num">{formatCurrency(entry.amount)}</td>
                <td className="num">{formatCurrency(entry.balanceAfter)}</td>
                <td>{entry.description ?? ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
