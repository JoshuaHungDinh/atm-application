## Summary

<!-- One or two sentences. What changes, and why? -->

**Type:** `feature` · `refactor` · `fix` · `perf` · `infra`
**PR:** <!-- e.g. 1 of 5 — see the README status table -->

---

## Context · Approach

<!--
  Why this change exists, what it does, and how.
  Alternatives considered and rejected.
  Where should the reviewer start?
-->

---

## Risk & Notes

|                      |                                 |
| -------------------- | ------------------------------- |
| **Breaking changes** | yes / no — describe             |
| **Schema / data**    | new tables, migrations — or N/A |
| **New dependencies** | packages added — or N/A         |

---

## Verification

- [ ] `dotnet build -c Release` clean (warnings-as-errors)
- [ ] `dotnet test` green
- [ ] `dotnet format --verify-no-changes` clean
- [ ] Manually exercised the changed paths

---

## Follow-ups

<!-- Out of scope but noted. -->

-
