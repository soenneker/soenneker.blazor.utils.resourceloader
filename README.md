[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.resourceloader/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.resourceloader/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.resourceloader/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.resourceloader/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.ResourceLoader

Loads scripts and styles into a Blazor application's document, waits for browser load events, and coalesces concurrent requests that use the same URL and element options.

## Installation

```bash
dotnet add package Soenneker.Blazor.Utils.ResourceLoader
```

Register the scoped service:

```csharp
using Soenneker.Blazor.Utils.ResourceLoader.Registrars;

builder.Services.AddResourceLoaderAsScoped();
```

Inject `IResourceLoader` into a component or service:

```razor
@using Soenneker.Blazor.Utils.ResourceLoader.Abstract
@inject IResourceLoader ResourceLoader
```

## Load a script

`LoadScript` appends a classic `<script>` element and completes after the browser fires its `load` event:

```csharp
await ResourceLoader.LoadScript("/js/widgets.js");
```

For a pinned third-party asset, pass its Subresource Integrity hash. The server must also permit the requested CORS mode:

```csharp
await ResourceLoader.LoadScript(
    "https://cdn.example.com/widgets/3.2.1/widgets.min.js",
    integrity: "sha384-...",
    crossOrigin: "anonymous");
```

`crossOrigin` accepts `"anonymous"`, `"use-credentials"`, `null`, or an empty string. URLs may be relative to the document or absolute, but must resolve to HTTP or HTTPS and cannot contain embedded credentials.

Use `LoadModuleScript` when the resource must execute as `<script type="module">`:

```csharp
await ResourceLoader.LoadModuleScript("/js/dashboard.js");
```

This loads a module script tag; it does not return the module's exports. Use a module-import interop when you need an `IJSObjectReference` for exported functions.

## Load a stylesheet

```csharp
await ResourceLoader.LoadStyle("/css/widgets.css");
```

The same SRI and CORS options are available for styles:

```csharp
await ResourceLoader.LoadStyle(
    "https://cdn.example.com/widgets/3.2.1/widgets.min.css",
    integrity: "sha384-...",
    crossOrigin: "anonymous",
    media: "screen");
```

## Wait for a global

Some older libraries signal readiness by assigning a browser global. Load the script and wait for a dotted global path in one operation:

```csharp
await ResourceLoader.LoadScriptAndWaitForVariable(
    "/js/legacy-widget.js",
    "LegacyWidget.ready",
    timeout: 10_000,
    cancellationToken: cancellationToken);
```

If another part of the application loads the resource, wait without injecting another element:

```csharp
await ResourceLoader.WaitForVariable(
    "LegacyWidget.ready",
    timeout: 10_000,
    cancellationToken: cancellationToken);
```

Prefer an ES module import over polling for a global when the library exposes module exports.

## Loading and lifetime behavior

- Concurrent calls with the same URL and options share one load operation.
- Reusing a URL with different integrity, CORS, placement, script, media, or MIME options is rejected. Keep those options consistent throughout the application.
- A failed load is removed from the loader's cache and DOM, so a later call can retry.
- Cancellation stops the .NET caller from waiting; it cannot reliably stop a browser request or undo an element already appended to the document.
- Loaded `<script>` and `<link>` elements remain in the document. Disposing the scoped service does not unload executed JavaScript or remove applied styles.
- Dynamically inserted script ordering follows browser semantics. Await each call when one script depends on another, and avoid `async: true` for order-dependent classic scripts.

## Security

Loading a script grants it the same page privileges as your application. Use trusted, version-pinned HTTPS URLs. For third-party assets, use the publisher's exact SRI hash with an appropriate CORS mode and enforce a restrictive Content Security Policy. Never pass user-controlled URLs to this API.

## Disposal

The dependency-injection scope disposes `IResourceLoader`. Manually created instances should be disposed with `await using` or `DisposeAsync()`.
