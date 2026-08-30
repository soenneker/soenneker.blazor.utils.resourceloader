namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

internal readonly record struct StyleLoadArgs(string Uri, string? Integrity, string? CrossOrigin, string? Media, string? Type);
