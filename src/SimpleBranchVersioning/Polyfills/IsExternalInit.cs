// Polyfill for init-only properties in netstandard2.0
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

#pragma warning disable S2094 // Classes should not be empty - Required polyfill for init-only setters
internal static class IsExternalInit
{
}
#pragma warning restore S2094
