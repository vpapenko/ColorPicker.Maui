# DevOps setup checklist

One-time GitHub configuration required to activate the
`pr.yml`, `ci.yml`, and `release.yml` workflows.

---

## 1. Branch protection on `main`

**Settings → Branches → Add rule**, branch name pattern: `main`.

- ✅ **Require a pull request before merging**
  - Require approvals: **0**  (solo-friendly; raise to 1 once you have co-maintainers)
  - ✅ Require review from Code Owners (CODEOWNERS auto-requests review from @vpapenko)
- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Required checks (add after first PR run completes so they appear in the dropdown):
    - `Build and test (reusable) / Build Android`
    - `Build and test (reusable) / Build Windows`
    - `Build and test (reusable) / UI Tests (Windows)`
    - `Build and test (reusable) / Pack NuGet`
- ✅ **Require linear history**
- ✅ **Restrict who can push to matching branches**
  - Allow only: **vpapenko**
- ✅ **Block force pushes**
- ✅ **Block deletions**
- ❌ "Do not allow bypassing the above settings" — leave OFF so you can break-glass if needed.

External contributors can fork and open PRs freely; CI runs on every PR;
only you can hit the merge button.

---

## 2. Secrets

**Settings → Secrets and variables → Actions**:

- `NUGET_API_KEY` — repository secret. Generate at
  https://www.nuget.org/account/apikeys with scope:
  - **Push** → Glob: `ColorPicker.Maui` (one package only)
  - Expires in 365 days; calendar-reminder to rotate.

`GITHUB_TOKEN` is built-in; nothing to configure.

---

## 3. Environment

**Settings → Environments → New environment**: `nuget-prod`

- ✅ Required reviewers: **vpapenko**
  → Manual approval gate before any push to nuget.org.
- ✅ Deployment branches and tags: only allow tags matching `v*`.

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
```

---

## 5. First release dry-run

After this PR merges:

```sh
git checkout main && git pull
git tag v0.1.0-preview.1
git push origin v0.1.0-preview.1
```

Watch `release.yml` run; approve the `nuget-prod` deployment when prompted.
After it succeeds you should see `0.1.0-preview.1` on
https://www.nuget.org/packages/ColorPicker.Maui.

When you're confident in the API, cut a stable release:

```sh
git tag v0.1.0
git push origin v0.1.0
```

---

## 6. Versioning rules (enforced by MinVer)

- No tag yet → `0.0.0-preview.0.<height>` on every push to main.
- Tag `v1.0.0-preview.5` → that commit packs as `1.0.0-preview.5`.
  Subsequent commits pack as `1.0.0-preview.5.<height>`.
- Tag `v1.0.0` → packs as exactly `1.0.0`. Subsequent commits pack as
  `1.0.1-preview.0.<height>` (next-patch preview).
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
