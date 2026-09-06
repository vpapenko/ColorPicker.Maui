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
5. Open **Actions → Release → Run workflow** and make the first coordinated
   preview release (section 5).

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
- ✅ Deployment branches: only allow `main`.

The environment name **must** match the `Environment` field in the
Trusted Publishing policy above, otherwise OIDC exchange will be rejected.

---

## 4. GitHub Packages (preview feed)

No setup needed — `ci.yml` pushes a coherent Picker/Core preview pair to
`https://nuget.pkg.github.com/vpapenko/index.json` using the built-in
`GITHUB_TOKEN`. Both packages receive the same
`0.0.0-preview.<run>` version, and Picker depends on that exact Core preview.
Core is uploaded and restored from the feed before Picker is uploaded; rerunning
the workflow keeps the same package version.

The exact version is shown in the workflow summary. To consume it locally:

```sh
dotnet nuget add source https://nuget.pkg.github.com/vpapenko/index.json \
  --name github-vpapenko \
  --username <your-github-username> \
  --password <a-github-pat-with-read:packages>
dotnet add package ColorPicker.Maui --version <version-from-workflow>
dotnet add package ColorPicker.Maui.Core --version <version-from-workflow>
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
promoted to a stable release.

---

## 5. Stable releases from the GitHub UI

Open **Actions → Release → Run workflow**, keep the branch set to `main`, and
choose a target:

| Target | Required fields | Result |
|---|---|---|
| `picker` | Picker version; Core version may be blank | Publishes only `ColorPicker.Maui`. Blank Core selects the latest `core-v*` release. |
| `core` | Core version | Publishes only `ColorPicker.Maui.Core`. |
| `both` | Picker version and Core version | Packs and tests both, publishes Core first, waits for NuGet indexing, then publishes Picker. |

The `nuget-prod` environment asks for approval immediately before publication.
The workflow validates semantic versions, package contents, consumers, and safe
retries. It creates package-specific tags and GitHub releases automatically:

- `picker-v2.0.0`
- `core-v1.0.0`

No local tag or `dotnet nuget push` command is required.

For the first coordinated release, choose `both`, Picker `2.0.0-preview.1`, and
Core `1.0.0-preview.1`. When ready for stable packages, run `both` again with
Picker `2.0.0` and Core `1.0.0`.

---

## 6. Independent stable versioning

- Picker-only UI, renderer, or MAUI changes bump only `ColorPicker.Maui`.
- Core-only compatible changes bump only `ColorPicker.Maui.Core`.
- When Picker needs a new Core API, release `both`; Picker is packaged against
  the new Core nupkg before either package is published.
- Picker declares a compatible Core major range. For example, a minimum Core
  version of `1.1.0` produces `[1.1.0,2.0.0)`.
- A breaking Core major normally requires a Picker major because Picker exposes
  Core types and maintains type forwarders.
- Package-specific tags record exactly which commit produced each stable package.
