# 06: Agent docs still describe the pre-port Windows-only codebase

**What to build:** Tickets 01 to 03 are merged, but the agent-facing docs still describe the codebase from before the port. In CLAUDE.md: line 3-5 says the app is 'Today ... Windows-only (WinForms, net8.0-windows)'; Build and test has the note 'Until Ticket 02 retargets to net10.0 ... It can't run on Linux until Ticket 02'; Layout says 'publish.sh (Ticket 03)'. In docs/architecture.md: it presents the modules as the 'target' shape and keeps a 'Current state (before the Tickets)' section describing WinForms, a self-created SoundPlayer and Properties/Resources.*, all of which are gone. Rewrite both so they describe the current Avalonia/net10.0 code as it is, keeping the module table, seams and where-new-code-belongs guidance.

**Blocked by:** None (can start immediately)

**Status:** done

**Severity:** minor

**Paths:** `CLAUDE.md`, `docs/architecture.md`

**Labels:** docs

- [ ] CLAUDE.md no longer says the app is Windows-only or targets net8.0-windows, and has no conditional notes about Tickets 02/03 still to come
- [ ] docs/architecture.md describes the modules as the current state; the 'Current state (before the Tickets)' section and the mentions of Properties/Resources are removed or clearly marked as history
- [ ] The module table in docs/architecture.md still matches the files at the repo root (Program.cs, Igniter.cs, ITonePlayer.cs, WindowsTonePlayer.cs, LinuxTonePlayer.cs, SingleInstanceGuard.cs, App.axaml*, MessageDialog.axaml*, publish.sh, packaging/linux/**)
