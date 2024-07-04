[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.resourceloader/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.resourceloader/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.resourceloader.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.resourceloader/)

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

`LoadScriptAndWaitForVariable` is also available. It waits for a specified JavaScript variable to be available:

```csharp
await resourceLoader.LoadScriptAndWaitForVariable("https://example.com/script.js", "variableName");
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

You probably want `ImportModuleAndWaitUntilAvailable`, as that waits until the module is loaded, and accessible:

```csharp
// 'ResourceLoader' is the name of the export class
var module = await resourceLoader.ImportModuleAndWaitUntilAvailable("Soenneker.Blazor.Utils.ResourceLoader/resourceloader.js", "ResourceLoader");
```

### Waiting for Variables

To wait for a JavaScript variable to be available, use the `WaitForVariable` method:

```csharp
await resourceLoader.WaitForVariable("variableName");
```

### Disposing Modules

Be sure to dispose of a module after you're done interacting with it. To dispose of a JavaScript module, use the `DisposeModule` method:

```csharp
await resourceLoader.DisposeModule("moduleName");
```