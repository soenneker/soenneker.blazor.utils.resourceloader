namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

internal readonly record struct ScriptLoadArgs(string? Integrity, string? CrossOrigin, bool LoadInHead, bool Async, bool Defer, bool IsModule);