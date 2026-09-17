---
name: code-investigate
description: Investigate unfamiliar or cross-cutting code paths before risky edits by tracing references, call chains, and affected tests. Use for bugs or changes where the implementation path is not already known.
---

# Code Investigation Skill

Before editing any code you don't own yet, follow this sequence:

## Steps
1. **Search** — use `rg` or `grep` to find all references to the pattern you're investigating
2. **Read** — open the top 3 most relevant files and read them fully
3. **Map** — identify the call chain: who calls this, what does it call
4. **Verify** — check if tests exist for the affected paths
5. **Decide** — confirm your edit plan before touching any file

## Why this exists
The agent naturally re-derives this investigation pattern every session. Writing it down may cut re-derivation cost — but the skill itself loads every session, so the payback is only on sessions that actually investigate code.

## Holdout
This skill is under holdout testing. Do not assume it saves tokens until the experiment reports a verdict.
