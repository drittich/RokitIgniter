# 01: Igniter receives its tone player and TimeProvider, with scheduling tests

**What to build:** Preparatory work so the rest of the port is easy; it ships no new user-visible behaviour, and the Windows app behaves as it does now. Igniter stops creating its own dependencies. Its constructor takes a tone player port, a TimeProvider and an optional interval that defaults to 25 minutes. The tone player port is a single-operation interface that plays the wake tone once and knows nothing about scheduling. Igniter builds its PeriodicTimer from the TimeProvider. Its public behaviour stays the same: it plays the wake tone once on construction and then every interval. Ignite cancels the current cycle, plays the tone right away and starts a fresh cycle. Dispose stops the cycle and waits for it to finish. If the tone player throws, Igniter writes the error to stderr or debug output and keeps its schedule. Ticket 2 will call Ignite on the Avalonia UI thread and Dispose synchronously from Exit, on the same thread. Dispose must not deadlock in that case: the timer loop must not resume on the caller's synchronization context, for example by using ConfigureAwait(false) or running the loop on the thread pool. Move the existing SoundPlayer playback into a Windows tone player that implements the port. It finds 10Hz.wav relative to AppContext.BaseDirectory instead of Environment.CurrentDirectory. Change the one line in Program.cs that builds the Igniter so it passes the Windows tone player and TimeProvider.System; leave the rest of Program.cs alone, because Ticket 2 rewrites it. Add an xUnit test project, RokitIgniter.Tests, to the solution. It tests through Igniter's public interface only, using a fake tone player and FakeTimeProvider from Microsoft.Extensions.TimeProvider.Testing. It must not reference the app project, Avalonia or any OS-specific API; it compiles the Igniter and tone player port source files as linked files (see openQuestions). Exclude the test folder from the app's .csproj so the app doesn't compile the test sources through its default globs.

**Blocked by:** None (can start immediately)

**Status:** done

**Paths:** `Igniter.cs`, `ITonePlayer.cs`, `WindowsTonePlayer.cs`, `Program.cs`, `RokitIgniter.csproj`, `RokitIgniter.sln`, `RokitIgniter.Tests/**`, `README.md`

- [ ] Igniter's constructor takes a tone player port, a TimeProvider and an optional interval defaulting to 25 minutes; it no longer references SoundPlayer or Environment.CurrentDirectory
- [ ] The Windows tone player wraps SoundPlayer and finds 10Hz.wav through AppContext.BaseDirectory
- [ ] Program.cs builds the Igniter with the Windows tone player and TimeProvider.System; the Windows app's behaviour is otherwise unchanged
- [ ] Tests, run with the virtual time of FakeTimeProvider: exactly one tone plays right after construction
- [ ] Test: no further tone plays before 25 minutes, and exactly one plays at 25 minutes
- [ ] Test: over N × 25 minutes, N + 1 tones play
- [ ] Test: Ignite plays a tone right away
- [ ] Test: after Ignite at minute 10, the next automatic tone plays at minute 35, not minute 25
- [ ] Test: several quick Ignite calls each play one tone and leave a single active cycle (no overlapping tones afterwards)
- [ ] Test: after Dispose, advancing time plays no further tones, and Dispose returns without hanging
- [ ] Test: when the tone player throws, later scheduled tones still play
- [ ] The tests never inspect Igniter's private state, cancellation tokens or timer objects
- [ ] The test project references neither the app project, Avalonia, nor any OS-specific API
- [ ] The test project is in RokitIgniter.sln, and `dotnet test RokitIgniter.Tests` passes on Fedora with the .NET 10 SDK
- [ ] Building the app project doesn't compile any file from the test folder
- [ ] The README gains a short Development section on running the tests with `dotnet test`
