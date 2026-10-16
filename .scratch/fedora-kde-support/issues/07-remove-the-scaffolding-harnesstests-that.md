# 07: Remove the scaffolding HarnessTests that test .NET rather than the Igniter

**What to build:** RokitIgniter.Tests/HarnessTests.cs was added during scaffolding to prove that FakeTimeProvider drives a PeriodicTimer, before the Igniter could be tested. It tests the framework, not RokitIgniter, and IgniterTests now covers the same ground through the Igniter's interface, which is the spec's one test seam. CLAUDE.md still names HarnessTests.cs as the pattern for awaiting effects, but the pattern the real tests use is FakeTonePlayer.WaitForCountAsync. Remove HarnessTests.cs and point CLAUDE.md at the pattern IgniterTests and FakeTonePlayer use.

**Blocked by:** None (can start immediately)

**Status:** done

**Severity:** minor

**Paths:** `RokitIgniter.Tests/HarnessTests.cs`, `CLAUDE.md`

**Labels:** tests

- [ ] RokitIgniter.Tests contains no test that exercises PeriodicTimer or FakeTimeProvider directly; every test goes through the Igniter's interface (construct, IgniteAsync, Dispose)
- [ ] CLAUDE.md's test-harness rules point to IgniterTests.cs / FakeTonePlayer.WaitForCountAsync as the pattern for awaiting wake tones, not HarnessTests.cs
- [ ] dotnet test RokitIgniter.Tests passes
