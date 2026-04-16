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
