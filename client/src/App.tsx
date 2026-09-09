import { useState } from 'react'
import { ApiError, deposit, transfer, withdraw } from './api'
import { AccountCard } from './components/AccountCard'
import { ActionForm } from './components/ActionForm'
import { ErrorBanner } from './components/ErrorBanner'
import { TransferForm } from './components/TransferForm'
import { useAccounts } from './hooks/useAccounts'

export default function App() {
  const { accounts, loading, error, reload } = useAccounts()
  const [submitting, setSubmitting] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  async function run(operation: () => Promise<unknown>) {
    setSubmitting(true)
    setActionError(null)
    try {
      await operation()
      await reload()
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : 'Something went wrong.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <main className="app">
      <h1>ATM</h1>

      {error && <ErrorBanner message={error} />}
      {actionError && (
        <ErrorBanner message={actionError} onDismiss={() => setActionError(null)} />
      )}

      {loading && accounts.length === 0 ? (
        <p>Loading&hellip;</p>
      ) : (
        <>
          <section className="accounts">
            {accounts.map((account) => (
              <AccountCard key={account.id} account={account} />
            ))}
          </section>

          {accounts.length > 0 && (
            <section className="actions">
              <ActionForm
                label="Deposit"
                accounts={accounts}
                disabled={submitting}
                onSubmit={(id, amount) => void run(() => deposit(id, amount))}
              />
              <ActionForm
                label="Withdraw"
                accounts={accounts}
                disabled={submitting}
                onSubmit={(id, amount) => void run(() => withdraw(id, amount))}
              />
              <TransferForm
                accounts={accounts}
                disabled={submitting}
                onSubmit={(from, to, amount) => void run(() => transfer(from, to, amount))}
              />
            </section>
          )}
        </>
      )}
    </main>
  )
}
