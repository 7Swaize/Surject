using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;

namespace Surject.Tests;

public static class ModuleInitializer {
    [ModuleInitializer]
    public static void Initialize() {
        Verifier.UseSourceFileRelativeDirectory("Snapshots");
        VerifierSettings.AutoVerify();
        VerifySourceGenerators.Initialize();
    }
}