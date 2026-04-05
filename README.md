[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.resourceloader/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.resourceloader/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.resourceloader/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.resourceloader/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.Utils.ResourceLoader
### A Blazor JavaScript interop for dynamically loading scripts, styles, and modules

## Overview

The `ResourceLoader` class is designed to manage the loading and initialization of scripts, styles, and JavaScript modules in a Blazor application. It provides methods to asynchronously load scripts and styles, wait for variables to be available, and manage the lifecycle of JavaScript modules.

It ensures that each resource is only loaded once (through this interop), even with multiple concurrent calls.

## Table of Contents
- [Installation](#installation)
- [Usage](#usage)
  - [Loading Scripts](#loading-scripts)
  - [Loading Styles](#loading-styles)
  - [Importing Modules](#importing-modules)
  - [Waiting for Variables](#waiting-for-variables)
  - [Disposing Modules](#disposing-modules)

## Installation

```
dotnet add package Soenneker.Blazor.Utils.ResourceLoader
```

## Usage

### Loading Scripts

To load a script, use the `LoadScript` method. It injects the file into the DOM.

```csharp
await resourceLoader.LoadScript("https://example.com/script.js");
```

`LoadScriptAndWaitForVariable` is also available as a legacy fallback for third-party scripts that expose globals instead of ES module exports:

```csharp
await resourceLoader.LoadScriptAndWaitForVariable("https://example.com/script.js", "variableName");
```

To load an ES module script tag, use `LoadModuleScript`:

```csharp
await resourceLoader.LoadModuleScript("https://example.com/module.js");
```

If that module assigns a global and you need to wait for it, use:

```csharp
await resourceLoader.LoadModuleScriptAndWaitForVariable("https://example.com/module.js", "myGlobal");
```

### Loading Styles

To load a style, use the `LoadStyle` method. It injects the file into the DOM.

```csharp
await resourceLoader.LoadStyle("https://example.com/style.css");
```

### Importing Modules

To import a JavaScript module, use the `ImportModule` method:

```csharp
var module = await resourceLoader.ImportModule("moduleName");
```

`ImportModule` already waits for the ES module import to complete, so Soenneker-owned interops should import the module directly and invoke its exports.

To import an external ES module by absolute URI, use `ImportExternalModule`:

```csharp
var module = await resourceLoader.ImportExternalModule("https://cdn.jsdelivr.net/npm/some-package/+esm");
```

This is useful for ESM-first libraries that do not expose browser globals.

### Waiting for Variables

To wait for a JavaScript global to be available, use the `WaitForVariable` method:

```csharp
await resourceLoader.WaitForVariable("variableName");
```

### Disposing Modules

Be sure to dispose of a module after you're done interacting with it. To dispose of a JavaScript module, use the `DisposeModule` method:

```csharp
await resourceLoader.DisposeModule("moduleName");
```

External modules imported by URL can also be disposed:

```csharp
await resourceLoader.DisposeExternalModule("https://cdn.jsdelivr.net/npm/some-package/+esm");
```
