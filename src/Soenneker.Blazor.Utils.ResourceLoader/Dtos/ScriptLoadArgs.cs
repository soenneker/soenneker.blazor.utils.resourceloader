using System;

﻿namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

internal readonly record struct ScriptLoadArgs(string Uri, string? Integrity, string? CrossOrigin, bool LoadInHead, bool Async, bool Defer, bool IsModule)
{
    // A URL normally has one option set. Equality still checks every option;
    // hashing SRI strings and all flags only adds work to successful lookups.
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Uri);
}
