# RokitIgniter

A tray-only utility that plays an inaudible 10Hz **wake tone** every 25 minutes so KRK Rokit speakers don't go to
sleep. It is an Avalonia 11 app on `net10.0` and runs on Windows and on Linux (Fedora 44 KDE Plasma).

Read before working:

- `.scratch/fedora-kde-support/spec.md`: the spec, the source of truth for behaviour.
- `docs/architecture.md`: modules, their interfaces, the seams, and where new code belongs.
- `GLOSSARY.md`: domain words (wake tone, ignite, cycle, Igniter, tone player). Use them in code, tests and commits.

## Build and test

Requires the .NET 10 SDK (`dotnet-sdk-10.0` on Fedora). Everything works on Linux; no Windows machine needed.

```sh
dotnet build RokitIgniter.sln          # app + tests
dotnet test RokitIgniter.Tests         # tests only
```

- The repo root holds both `RokitIgniter.sln` and `RokitIgniter.csproj`, so always name the one you mean. Publish the
  app project explicitly (`dotnet publish RokitIgniter.csproj -r linux-x64 ...`) so the test project isn't published
  into the release output.
- The gating checks are in `.sleeper/checks.json` (build, test, `bash -n` on every tracked `*.sh`). Run them all
  before you commit; each must exit 0.

## Layout

- App project: flat at the repo root (`RokitIgniter.csproj`, `Program.cs`, `Igniter.cs`, ...). Keep it flat.
- Tests: `RokitIgniter.Tests/` (xUnit, `net10.0`). The app csproj excludes this folder from its default globs via
  `DefaultItemExcludes`; keep that exclusion.
- Linux packaging: `packaging/linux/`. Release scripts: `publish.sh` (all four archives, from Linux) and
  `publish.bat` (Windows archives, from Windows), both at the repo root.
- Tickets: `.scratch/fedora-kde-support/issues/NN-*.md`. Stay within a Ticket's `Paths:` list.

## Test harness rules

- The test project must **not** reference `RokitIgniter.csproj`, Avalonia, `System.Media` or any OS-specific API, so it
  runs on Fedora and Windows. Bring shared code in as linked source files:
  `<Compile Include="..\Igniter.cs" Link="Linked\Igniter.cs" />`.
- Test the Igniter only through its interface (construct, `IgniteAsync`, `Dispose`) with a fake tone player and
  `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`, already referenced). Drive virtual time with
  `Advance` and assert on how many wake tones played and when. Never inspect private state, tokens or timers.
- Timer continuations run on the thread pool after `Advance`; await the observable effect with a timeout rather
  than sleeping. `IgniterTests.cs` shows the pattern: after each `Advance`, `await player.WaitForCountAsync(n)`
  (in `FakeTonePlayer.cs`) waits for the nth wake tone and fails after 5 s.
- Name tests after behaviour a user would notice, in glossary terms (e.g. `Ignite_at_minute_10_moves_next_wake_tone_to_minute_35`).
- Tray, dialogs, the real tone players, the single-instance guard and install scripts are not unit-tested; they are
  covered by the manual smoke checklist in the spec.

## Conventions

- C#: tabs for indentation, block-scoped `namespace RokitIgniter { ... }`, nullable and implicit usings on, LF line
  endings. Match the style of `Igniter.cs`.
- Platform-specific choices happen only in `Program.cs` (the composition root). Windows-only types carry
  `[SupportedOSPlatform("windows")]`; the build must stay free of CA1416 warnings.
- Locate bundled files with `AppContext.BaseDirectory`, never `Environment.CurrentDirectory`.
- Shell scripts: bash, `set -euo pipefail`, must pass `bash -n`.
- Don't edit Ticket files under `.scratch/` or their `Status:` lines; the Conductor owns them.
- Record a hard-to-reverse decision that the spec doesn't already cover as an ADR in `docs/adr/` (see
  `docs/agents/domain.md`).

## Agent skills

### Issue tracker

Issues and specs live as local markdown files under `.scratch/<feature-slug>/`. See `docs/agents/issue-tracker.md`.

### Triage labels

The five default triage roles (needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix), recorded as a `Status:` line in each issue file. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `GLOSSARY.md` and `docs/adr/` at the repo root, created when first needed. See `docs/agents/domain.md`.
