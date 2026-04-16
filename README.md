# voluntr.io monorepo

Szkielet monorepo oparty o `pnpm` + `Turborepo` z:
- `apps/web` - miejsce na aplikację `Next.js` (inicjalizowaną z template)
- `apps/api-dotnet` - pusta solucja `.NET 10` w formacie `slnx`

## Wymagania

- Node.js (zalecane LTS)
- pnpm
- .NET SDK 10

## Struktura

- `apps/web`
- `apps/api-dotnet/Voluntrio.slnx`
- `packages/`
- `packages/agent-package-manager`

## Start

```bash
pnpm install
pnpm dev
```

Uruchomienie samego weba:

```bash
pnpm --filter web dev
```

## CI/CD (trunk-based + tag-only release)

Repozytorium używa workflowów GitHub Actions:

- `PR_build` - uruchamiany na `pull_request` do `main`; wykrywa zmienione obszary i uruchamia tylko odpowiednie joby dla `web`, `api` i `packages`.
- `release-api` - uruchamiany tylko na tagu `api/vX.Y.Z`; buduje i publikuje obraz `voluntrio-api`, deployuje na staging i uruchamia testy staging.
- `release-web` - uruchamiany tylko na tagu `web/vX.Y.Z`; buduje i publikuje obraz `voluntrio-web`, deployuje na staging i uruchamia testy staging.
- `promote-to-production` - uruchamiany ręcznie (`workflow_dispatch`) z parametrami `app`, `source` (`latest` lub `release_tag`) i opcjonalnym `tag`; promuje artefakt po digeście na produkcję, bez rebuildu.

### Wymagane sekrety GitHub

- `SCW_ACCESS_KEY`
- `SCW_SECRET_KEY`
- `SCW_REGISTRY_ENDPOINT` (np. `rg.fr-par.scw.cloud`)
- `SCW_REGISTRY_NAMESPACE`
- `SCW_STAGING_DEPLOY_CMD_API`
- `SCW_STAGING_TEST_CMD_API`
- `SCW_STAGING_DEPLOY_CMD_WEB`
- `SCW_STAGING_TEST_CMD_WEB`
- `SCW_PROD_DEPLOY_CMD_API`
- `SCW_PROD_DEPLOY_CMD_WEB`

Sekrety `*_DEPLOY_CMD_*` i `*_TEST_CMD_*` to komendy wykonywane przez `bash -lc` z dostępnymi zmiennymi:

- `IMAGE_REF` - pełny tag obrazu, np. `rg.fr-par.scw.cloud/ns/voluntrio-api:1.2.3`
- `IMAGE_DIGEST` - digest obrazu, np. `sha256:...`

### Przykładowe tagowanie release

```bash
git tag api/v0.1.0
git push origin api/v0.1.0

git tag web/v0.1.0
git push origin web/v0.1.0
```

### Branch protection (manualnie w GitHub)

Na gałęzi `main` ustaw:

- wymagany pull request,
- wymagane status checks (co najmniej workflow `PR_build`),
- brak direct push.
