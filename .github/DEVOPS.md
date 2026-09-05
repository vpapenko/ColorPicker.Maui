# DevOps setup checklist

One-time GitHub configuration required to activate the
`pr.yml`, `ci.yml`, and `release.yml` workflows.

---

## 1. Order of operations (solo-owner shortcut)

Required-status-check names only appear in GitHub's dropdown **after they've
run at least once on the default branch**. Easiest order:

1. Create the Trusted Publishing policy on nuget.org (section 2 below) and
   the `nuget-prod` GitHub environment (section 3).
2. Merge PR #3 as-is — you're the only one with push rights anyway.
3. Wait for `ci.yml` to finish on `main` (success or failure both register
   the job names).
4. Configure branch protection (section 1a) in one pass, including required
   checks.
5. Tag `v2.0.0-preview.1` to dry-run `release.yml` (section 5).

## 1a. Branch protection on `main`

**Settings → Branches → Add rule**, branch name pattern: `main`.

- ✅ **Require a pull request before merging**
  - Require approvals: **0** (raise to 1 once you have co-maintainers)
  - ✅ Require review from Code Owners
- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Required checks:
    - `Build Android`
    - `Build Windows`
    - `UI Tests (Windows)`
    - `Pack NuGet`
    - `Consumer Smoke (packed nupkg)`
- ✅ **Require linear history**
- ✅ **Restrict who can push to matching branches** → allow only **vpapenko**
- ✅ **Block force pushes**
- ✅ **Block deletions**
- ❌ "Do not allow bypassing the above settings" — leave OFF (break-glass).

External contributors can fork and open PRs freely; CI runs on every PR;
only you can hit merge.

---

## 2. NuGet.org Trusted Publishing (OIDC — no API key)

nuget.org supports Trusted Publishing: GitHub Actions exchanges a short-lived
OIDC token for a one-hour API key at publish time. No long-lived secret in
the repo, nothing to rotate.

1. Sign in at https://www.nuget.org → top-right avatar → **Trusted Publishing**.
2. **Add new policy** with these exact values:

   | Field                | Value                                                |
   |----------------------|------------------------------------------------------|
   | Policy Name          | `ColorPicker.Maui — GitHub Actions release`          |
   | Package Owner        | `VictorPapenko`                                      |
   | Repository Owner     | `vpapenko`                                           |
   | Repository           | `ColorPicker.Maui`                                   |
   | Workflow File        | `release.yml`                                        |
   | Environment          | `nuget-prod`                                         |
   | Package Glob / IDs   | `ColorPicker.Maui*`                                  |

3. Save. First push from `release.yml` (after the package exists) will
   bind the policy to the package owner — that's normal.

If the package doesn't exist on nuget.org yet, the **very first** publish
still needs a one-shot API key:
- Generate at https://www.nuget.org/account/apikeys
  scope **Push new packages and package versions**, Glob `ColorPicker.Maui*`,
  expires in **1 day**.
- Add as repo secret `NUGET_API_KEY` temporarily.
- Temporarily change `release.yml` to use `${{ secrets.NUGET_API_KEY }}`
  instead of the Trusted-Publishing step, run release once, then revert
  and delete the secret + revoke the key.

`GITHUB_TOKEN` is built-in; nothing to configure.

---

## 3. Environment

**Settings → Environments → New environment**: `nuget-prod`

- ✅ Required reviewers: **vpapenko**
  → Manual approval gate before any push to nuget.org.
- ✅ Deployment branches and tags: only allow tags matching `v*`.

The environment name **must** match the `Environment` field in the
Trusted Publishing policy above, otherwise OIDC exchange will be rejected.

---

## 4. GitHub Packages (preview feed)

No setup needed — `ci.yml` pushes to `https://nuget.pkg.github.com/vpapenko/index.json`
using the built-in `GITHUB_TOKEN`. To consume previews locally:

```sh
dotnet nuget add source https://nuget.pkg.github.com/vpapenko/index.json \
  --name github-vpapenko \
  --username <your-github-username> \
  --password <a-github-pat-with-read:packages>
dotnet add package ColorPicker.Maui --prerelease
dotnet add package ColorPicker.Maui.Core --prerelease
```

### 4a. Consumer-smoke validation of the packed nupkg

`ColorPickerTestApp` references the library via `ProjectReference`, so it
can't catch packaging regressions (missing `.targets`, broken MAUI resource
glob, dropped public type, TFM mismatch, transitive-dep hole). Two extra
jobs close that gap:

| Job | Triggered on | Source of the package | Purpose |
|-----|--------------|-----------------------|---------|
| `build-and-test.yml → consumer-smoke` | every PR (when `pack: true`) | local feed = the just-packed `nupkgs/` artifact | catch packaging bugs **before merge** |
| `ci.yml → consumer-e2e-github-packages` | every push to `main` | GitHub Packages (just-published preview) | catch upload/index/auth issues that only show up via the real feed |

Both jobs build [`samples/ConsumerSmoke/`](../samples/ConsumerSmoke/README.md)
against `ColorPicker.Maui` on Android and Windows,
[`samples/CoreConsumerSmoke/`](../samples/CoreConsumerSmoke/README.md) against
`ColorPicker.Maui.Core` on `netstandard2.0` and `net8.0`, and run
`samples/PackageCompatibilitySmoke/` to verify the Core assembly identity and
every type forwarder in `ColorPicker.dll`. The MAUI package also consumes Core
transitively, so the smoke pass verifies the dependency between the two
packages. If either job fails on a PR, the packages must not be
promoted to a release tag.

---

## 5. First release dry-run

After this PR merges:

```sh
git checkout main && git pull
git tag v2.0.0-preview.1
git push origin v2.0.0-preview.1
```

Watch `release.yml` run; approve the `nuget-prod` deployment when prompted.
After it succeeds you should see `2.0.0-preview.1` on
https://www.nuget.org/packages/ColorPicker.Maui and
https://www.nuget.org/packages/ColorPicker.Maui.Core.

When you're confident in the API, cut a stable release:

```sh
git tag v2.0.0
git push origin v2.0.0
```

---

## 6. Versioning rules (enforced by MinVer)

- No tag yet → `0.0.0-preview.0.<height>` on every push to main.
- Tag `v2.0.0-preview.5` → that commit packs as `2.0.0-preview.5`.
  Subsequent commits pack as `2.0.0-preview.5.<height>`.
- Tag `v2.0.0` → packs as exactly `2.0.0`. Subsequent commits pack as
  `2.0.1-preview.0.<height>` (next-patch preview).
- Bumping major/minor: just push a tag. No file edits, no PRs.

---

## 7. Useful labels (apply to PRs for release-drafter categorization)

Create these labels in **Issues → Labels**:

| Label | Color | Effect on changelog | Effect on next version |
|---|---|---|---|
| `feature` / `enhancement` | `#84b6eb` | "🚀 Features" | minor bump |
| `fix` / `bug` | `#d73a4a` | "🐛 Bug fixes" | patch bump |
| `breaking` / `major` | `#b60205` | implicit | **major bump** |
| `test` | `#fbca04` | "🧪 Tests" | patch |
| `docs` | `#0075ca` | "📚 Documentation" | patch |
| `ci` / `devops` | `#5319e7` | "🏗️ CI / DevOps" | patch |
| `dependencies` | `#0366d6` | "⬆️ Dependencies" | patch |
| `refactor` / `chore` | `#cfd3d7` | "♻️ Refactor / chores" | patch |
| `skip-changelog` | `#cccccc` | excluded | n/a |
