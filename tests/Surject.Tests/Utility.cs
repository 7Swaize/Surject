using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Surject.Abstractions.Attributes;
using UnityEngine;

namespace Surject.Tests;

public static class Utility {
#if NET8_0
	public static ReferenceAssemblies ReferenceAssemblies => Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net80;
	public static IEnumerable<MetadataReference> NetCoreAssemblies => Basic.Reference.Assemblies.Net80.References.All;
#elif NET9_0
	public static ReferenceAssemblies ReferenceAssemblies => ReferenceAssemblies.Net.Net90;
	public static IEnumerable<MetadataReference> NetCoreAssemblies => Basic.Reference.Assemblies.Net90.References.All;
#elif NET10_0
    public static ReferenceAssemblies ReferenceAssemblies => Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net100;
    public static ImmutableArray<PortableExecutableReference> NetCoreAssemblies => Basic.Reference.Assemblies.Net100.References.All;
#endif

    public static MetadataReference[] GetAdditionalReferences() => [
        MetadataReference.CreateFromFile(typeof(InjectAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(MonoBehaviour).Assembly.Location)
    ];
}