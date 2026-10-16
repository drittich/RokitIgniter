# 04: Single-instance guard changes Windows mutex scope from per-session to machine-wide Global namespace

**What to build:** SingleInstanceGuard.cs creates its mutex with NamedWaitHandleOptions { CurrentUserOnly = true, CurrentSessionOnly = false } on every OS. That is right for Linux, where it lets a launcher start and a terminal start see each other. On Windows, CurrentSessionOnly = false moves the mutex from the per-session (Local\) namespace into the machine-wide Global\ namespace. Two Windows sessions, such as fast user switching or RDP, now share one mutex that used to be separate per session, so a launch in another session may be refused or may fail to open the mutex. That contradicts the spec's 'Windows behaviour stays the same' and 'keep the named-mutex approach'. Restore per-session scoping on Windows and keep the cross-session, per-user scoping on Linux. The per-OS choice must sit where the architecture puts OS choices: in the composition root (Program.cs), or else documented as a deliberate exception. Document the per-OS scoping in docs/architecture.md and GLOSSARY.md.

**Blocked by:** None (can start immediately)

**Status:** done

**Severity:** major

**Paths:** `SingleInstanceGuard.cs`, `Program.cs`, `docs/architecture.md`, `GLOSSARY.md`

**Labels:** windows, single-instance-guard

- [ ] On Windows, the guard's mutex is scoped to the current session, as before the port, so instances in different Windows sessions don't block each other or fail to open the mutex
- [ ] On Linux, the guard is still shared across sessions and private to the current user, so a launcher start and a terminal start of the same user still detect each other
- [ ] The OS-dependent choice is made in Program.cs (the composition root), or is recorded in docs/architecture.md as a deliberate exception
- [ ] docs/architecture.md (Single-instance guard row) and GLOSSARY.md (Single-instance guard) describe the per-OS mutex scope
- [ ] dotnet build RokitIgniter.sln gives no CA1416 warnings, and dotnet test RokitIgniter.Tests passes
