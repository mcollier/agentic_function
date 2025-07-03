# Copilot Instructions for agentic_function

## General
- Use C# 12+ features and .NET 9 patterns where appropriate.
- Follow .NET naming conventions: PascalCase for types and methods, camelCase for local variables and parameters.

## Formatting
- Prefer file-scoped namespace declarations and single-line using directives.
- Prefer explicit access modifiers (`public`, `private`, etc.).
- Use top-level statements for minimal hosting models when possible.
- Insert a newline before the opening curly brace of any code block (e.g., after if, for, while, foreach, using, try, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use nameof instead of string literals when referring to member names.

## Nullable Reference Types
- Declare variables non-nullable, and check for null at entry points.
- Always use is null or is not null instead of == null or != null.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Project Structure
- Organize code by feature and responsibility: API, Web, ServiceDefaults, AppHost.
- Place reusable extensions and service configuration in `ServiceDefaults`.
- Use dependency injection for services and clients.

## Patterns & Practices
- Use async/await for all I/O-bound operations.
- Prefer records for immutable data models (e.g., `WeatherForecast`).
- Use `HttpClient` with typed clients for external API calls.
- Add health checks and OpenTelemetry tracing/metrics as shown in `ServiceDefaults/Extensions.cs`.
- Use comments to explain non-obvious logic and reference relevant documentation links.

## Blazor/Web
- Place Blazor components in the `Components` folder.
- Use Razor syntax and follow Blazor component naming conventions.
- Use `appsettings.json` for configuration and secrets management.

## Testing & Quality
- Write unit tests for all public methods where possible.
- Use nullability annotations (`?`) and handle nulls defensively.
- Prefer `var` for local variable declarations when the type is obvious.

## Documentation
- Add XML documentation comments for public APIs.
- Reference official .NET Aspire and OpenTelemetry docs where relevant.
