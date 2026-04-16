# Copilot Instructions for voluntr.io

## Project Context
- This repository is a monorepo managed with `pnpm` workspaces and `Turborepo`.
- Main areas:
  - `apps/web`: Next.js app (App Router, TypeScript)
  - `apps/api-dotnet`: .NET 10 solution (`Voluntrio.slnx`)
  - `packages/*`: shared JS/TS packages

## Development Rules
- Prefer small, focused changes and keep existing architecture.
- Do not introduce new dependencies unless necessary.
- Keep code and filenames in ASCII unless existing files require otherwise.
- For frontend code, follow existing Next.js conventions in `apps/web`.
- For backend code, keep compatibility with `.NET 10`.

## Monorepo Conventions
- Use `pnpm` for all Node tasks.
- Use workspace-aware commands, for example:
  - `pnpm --filter web dev`
  - `pnpm --filter @voluntr/agent-package-manager test`
- Keep lockfile only at repository root.

## Quality Bar
- Include tests for non-trivial logic.
- Avoid broad refactors when implementing small features.
- Update documentation when adding new modules or workflows.

## .NET Coding Conventions
- Follow standard C# conventions (PascalCase for types/methods, camelCase for variables).
- Use `async/await` for asynchronous code.
- Keep methods focused and avoid large classes.
- Use dependency injection where appropriate.
- Handle exceptions gracefully and log errors.
- Use XML documentation comments for public APIs.
- Follow existing patterns in `apps/api-dotnet` for consistency.
- Keep buisness logic in services, controllers should be thin.
- Use Entity Framework Core for data access, following existing patterns in the codebase.
- Write unit tests for critical business logic and services, using xUnit or NUnit as per existing tests nsubstitute for mocking dependencies and AutoFixture for generating test data.
- Ensure that all public methods and classes have XML documentation comments to improve maintainability and readability of the codebase.
- Use consistent naming conventions for files and folders, following the existing structure in the `apps/api-dotnet` project. For example, place controllers in a `Controllers` folder, services in a `Services` folder, and data models in a `Models` folder.
- Avoid using magic strings or numbers in the code. Instead, define constants or enums where appropriate to improve readability and maintainability.
- Ensure that all database interactions are performed through a repository or data access layer, following the existing patterns in the codebase. This helps to keep the business logic separate from the data access code and makes it easier to maintain and test the application.
- Do not use following packages: MediatR, AutoMapper, FluentAssertions or any other pasckage that is not free to use in commercial projects without attribution. If you need to use a package, please check the license and ensure it is compatible with our project before adding it as a dependency.
- Ask when in doubt about architectural decisions or conventions. It's better to clarify than to introduce inconsistencies.
- do not use undercore prefixes for private fields in C#. Follow standard C# conventions and use camelCase for private fields without underscores.
- Keep actuall ode in src folder and tests in tests folder. Do not mix them together. For example, if you have a service called `UserService`, the implementation should be in `src/Services/UserService.cs` and the tests should be in `tests/Services/UserServiceTests.cs`. This separation helps to keep the codebase organized and makes it easier to find and maintain both the implementation and the tests.
- Keep only limited number of test projects, one for UnitTesting, FunctionalTesting and IntegrationTesting. Do not create separate test projects for each module or feature. Instead, organize the tests within the existing test projects using folders or namespaces to keep them organized and maintainable.
- Avoid using regions in C# code. Regions can make the code harder to read and navigate, especially when they are overused or nested. Instead, focus on keeping methods and classes small and focused, which will naturally improve readability and maintainability of the codebase.
- Use `var` for local variable declarations when the type is obvious from the right-hand side of the assignment. For example, `var user = new User();` is acceptable, but `var result = SomeMethod();` should be avoided if the return type of `SomeMethod()` is not clear.

## Architectural Guidelines
- Follow existing architectural patterns in the codebase.
- .NET API should be RESTful and follow standard conventions for controllers and routing.
- Aplicastion is modulat monolith with clear separation of concerns between layers (Controllers, Services, Repositories).
- Keep business logic in services, controllers should be thin.
- Module shoukld be in separated Library projects within the solution, with clear dependencies and boundaries.
- Use dependency injection to manage dependencies between modules and layers.
- Avoid circular dependencies between modules.
- For shared code or reusable code use shared modules in `src/Shared` folder, but avoid creating shared modules for code that is only used in one module or feature. This helps to keep the codebase organized and prevents unnecessary coupling between modules.
- Introduce Multiple libaries in modules only  for shareable contracts, interfaces or models. For example, if you have a module called `UserManagement`, you could have a `UserManagement.Contracts` library that contains shared contracts and interfaces, while the implementation of the module would be in a separate library called `UserManagement`. This separation allows for better modularity and maintainability of the codebase.