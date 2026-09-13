using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Surject.Generators;
using Xunit;

namespace Surject.Tests.GeneratorTests;

// Implementation taken from the following:
// See ref: https://github.com/ImmediatePlatform/Immediate.Handlers/blob/main/tests/Immediate.Handlers.Tests/GeneratorTests/GeneratorTestHelper.cs
// See ref: https://andrewlock.net/creating-a-source-generator-part-10-testing-your-incremental-generator-pipeline-outputs-are-cacheable/

public static class GeneratorTestsHelper {
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static ReadOnlySpan<string> TrackedSteps => new string[] {
        SurjectGenerator.TrackingNames.InjectableContainers
    };

    private static HashSet<string> SkippedDiagnostics => [
        "CS8618", // Non-nullable field must contain a non-null value when exiting constructor
        "CS0169", // Field is never used
        "CS0649" // Field never assigned to, and will always have its default value
    ];

    public static string GetSourceFileFromGeneratorRunResult(GeneratorDriverRunResult driverResults, string fileNameHint) {
        foreach (GeneratorRunResult result in driverResults.Results) {
            foreach (GeneratedSourceResult file in result.GeneratedSources) {
                if (file.HintName.Contains(fileNameHint)) {
                    return file.SourceText.ToString();
                }
            }
        }
        
        return string.Empty;
    }

    public static GeneratorDriverRunResult RunGenerator<TGenerator>(
        [StringSyntax("c#-test")] string source,
        params ReadOnlySpan<string> skippedSteps)
            where TGenerator : IIncrementalGenerator, new()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            ParseOptions,
            cancellationToken: TestContext.Current.CancellationToken
        );
        
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: [
                ..Utility.NetCoreAssemblies,
                ..Utility.GetAdditionalReferences()
            ],
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
        );
        
        // For some reason the docs say that this overload of 'CSharpGeneratorDriver.Create()' is present on 'Microsoft.CodeAnalysis.CSharp' v4.3.0.
        // However, it is not.
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.csharpgeneratordriver.create?view=roslyn-dotnet-4.3.0
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new TGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true)
        );
        
        driver = RunGeneratorAndAssert(driver, compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();
        
        VerifyIncrementality(driver, compilation, skippedSteps);

        return result;
    }
    
    private static GeneratorDriver RunGeneratorAndAssert(
        GeneratorDriver driver,
        Compilation compilation)
    {
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics,
            TestContext.Current.CancellationToken
        );
        
        Assert.DoesNotContain(
            outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken),
            d => (d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning) 
                 && !SkippedDiagnostics.Contains(d.Id)
        );

        Assert.Empty(diagnostics);
        return driver;
    }
    
    private static void VerifyIncrementality(
        GeneratorDriver driver,
        Compilation compilation,
        ReadOnlySpan<string> skippedSteps)
    {
        Compilation clone = compilation.Clone().AddSyntaxTrees(
            CSharpSyntaxTree.ParseText(
                "// dummy",
                ParseOptions,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        
        driver = RunGeneratorAndAssert(driver, clone);
        if (
            driver.GetRunResult() is not {
                Results: [
                    {
                        TrackedOutputSteps: { } outputSteps,
                        TrackedSteps: { } trackedSteps,
                    }
                ]
            }
        )
        {
            Assert.Fail("Unable to verify incrementality: unexpected result shape.");
            return;
        }
        
        foreach (var (_, step) in outputSteps) {
            AssertStepsCached(step);
        }
        
        foreach (var step in TrackedSteps) {
            if (skippedSteps.Contains(step)) {
                if (trackedSteps.ContainsKey(step)) {
                    Assert.Fail($"Step `{step}` should have been skipped, but is present.");
                }
            }
            else {
                if (!trackedSteps.TryGetValue(step, out var outputs)) {
                    Assert.Fail($"Step `{step}` expected, but is missing.");
                }

                AssertStepsCached(outputs);
            }
        }
    }

    private static void AssertStepsCached(ImmutableArray<IncrementalGeneratorRunStep> steps) {
        IEnumerable<(object Value, IncrementalStepRunReason Reason)> outputs = steps.SelectMany(o => o.Outputs);

        Assert.All(
            outputs, o => Assert.True(o.Reason is IncrementalStepRunReason.Unchanged or IncrementalStepRunReason.Cached)
        );
    }
}