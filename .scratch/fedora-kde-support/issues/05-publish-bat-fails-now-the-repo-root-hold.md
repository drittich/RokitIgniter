# 05: publish.bat fails now the repo root holds both the solution and the app project

**What to build:** publish.bat runs `dotnet publish` with no project argument, from the repo root. The root now holds both RokitIgniter.sln and RokitIgniter.csproj, so dotnet stops with MSB1011 ('more than one project or solution file'). Even if it picked the solution, it would publish the test project into the release output, which CLAUDE.md warns against. Release instructions.txt still says publish.bat builds the two Windows archives. The spec allows keeping publish.bat working or replacing it with a note pointing to publish.sh, as long as the release instructions match. Either name RokitIgniter.csproj explicitly in both publish calls, the way publish.sh does, or replace publish.bat with a pointer to publish.sh. Then make Release instructions.txt match.

**Blocked by:** None (can start immediately)

**Status:** done

**Severity:** minor

**Paths:** `publish.bat`, `Release instructions.txt`

**Labels:** packaging, windows

- [ ] Either publish.bat names RokitIgniter.csproj explicitly in both dotnet publish calls and produces RokitIgniter-<tag>.7z and RokitIgniter-SelfContained-<tag>.7z with the same names and layout as before, without test assemblies; or publish.bat is replaced by a note pointing to publish.sh
- [ ] Release instructions.txt describes what publish.bat actually does
- [ ] publish.sh is unchanged and still produces all four archives
