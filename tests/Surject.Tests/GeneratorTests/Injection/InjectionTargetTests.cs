using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Lifecycle;
using Surject.Generators;
using VerifyTests;
using Xunit;
using static VerifyXunit.Verifier;

namespace Surject.Tests.GeneratorTests.Injection;

public sealed class InjectionTargetTests {
    [Fact]
    internal async Task FieldInjection_AssignsFieldFromResolver() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;
              
              namespace TestApp {
                  public class Foo { }
                  
                  [Injectable]
                  public partial class Consumer : MonoBehaviour {
                      [Inject]
                      private Foo _foo;
                  }
              }
              """
        );

        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains(nameof(IInjectable.__Surject_Inject), source);
        Assert.Contains("this._foo = resolver.Resolve<", source);
        Assert.DoesNotContain("Key = ", source);
    }

    [Fact]
    internal async Task PropertyInjection_AssignsPropertyFromResolver() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
                  
                  [Injectable]
                  public partial class Consumer : MonoBehaviour {
                      [Inject]
                      public Foo FooProp { get; set; }
                  }
              }    
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Contains("this.FooProp = resolver.Resolve<", source);
    }

    [Fact]
    internal async Task MixedFieldsAndProperties_AreAllIndependentlyInjected() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;
              
              namespace TestApp {
                  public class Foo { }
                  public class Bar { }

                  [Injectable]
                  public partial class Consumer : MonoBehaviour {
                      [Inject] private Foo _foo;
                      [Inject] public Bar BarProp { get; set; }
                  }
              }    
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Contains("this._foo = resolver.Resolve<", source);
        Assert.Contains("this.BarProp = resolver.Resolve<", source);
        
    }

    [Fact]
    internal async Task MethodInjection_InjectsAllParametersByDefault() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
                  public class Bar { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [Inject]
                      private void Init(Foo foo, Bar bar) { }
                  }
              }    
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Contains("this.Init(", source);
        Assert.Contains("resolver.Resolve<global::TestApp.Foo,", source);
        Assert.Contains("resolver.Resolve<global::TestApp.Bar,", source);
    }

    [Fact]
    internal async Task MethodInjection_PerParameterOptionalOverridesMethodLevelDefault() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
                  public class Bar { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [Inject]
                      private void Init(Foo foo, [InjectOptional] Bar bar) { }
                  }
              } 
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Contains("resolver.Resolve<global::TestApp.Foo,", source);
        Assert.Contains("resolver.ResolveOptional<global::TestApp.Bar,", source);
    }

    [Fact]
    internal async Task InjectAndInjectOptionalStacked_StillResolvesAsOptional() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [Inject, InjectOptional]
                      private Foo _foo;
                  }
              }    
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("resolver.ResolveOptional<", source);
    }

    [Fact]
    internal async Task KeyedField_UsesIdAttributeAsResolveKey() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [Inject]
                      [Id("primary")]
                      private Foo _foo;
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("resolver.Resolve<", source);
        Assert.Contains("Key = ", source);
    }

    [Fact]
    internal async Task InjectAllField_UsesResolveAllAndArrayElementType() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;
              
              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [InjectAll]
                      private Foo[] _foos;
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("this._foos = resolver.ResolveAll<global::TestApp.Foo", source);
    }

    [Fact]
    internal async Task InjectAsyncField_UsesResolveAsyncAndUnwrapsTaskType() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using System.Threading.Tasks;
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [InjectAsync]
                      private ValueTask<Foo> _fooTask;
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("this._fooTask = resolver.ResolveAsync<global::TestApp.Foo", source);
    }

    [Fact]
    internal async Task InjectAsyncOptionalField_UsesResolveOptionalAsync() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using System.Threading.Tasks;
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [InjectAsync, InjectOptional]
                      private ValueTask<Foo> _fooTask;
                  }
              }    
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("resolver.ResolveOptionalAsync<global::TestApp.Foo", source);
    }

    [Fact]
    internal async Task InjectAsyncAllField_UsesResolveAllAsyncAndUnwrapsTaskOfArray() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using System.Threading.Tasks;
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                      [InjectAsync, InjectAll]
                      private ValueTask<Foo[]> _fooTask;
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");

        Assert.Contains("resolver.ResolveAllAsync<global::TestApp.Foo", source);
    }

    [Fact]
    internal async Task InjectableClassWithNoInjectionTargets_GeneratesEmptyMethodBody() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
               
                  [Injectable]
                  public partial class Consumer : MonoBehaviour { 
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Contains("__Surject_Inject", source);
        Assert.DoesNotContain("resolver.Resolve", source);
    }

    [Fact]
    internal async Task MultipleInjectableTypes_EachGetsItsOwnIndependentInjectMethod() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }
                  public class Bar { }
                  
                  [Injectable]
                  public partial class ConsumerA : MonoBehaviour {
                      [Inject]
                      private Foo _foo;
                  }

                  [Injectable]
                  public partial class ConsumerB : MonoBehaviour {
                      [Inject]
                      private Bar _bar;
                  }
              }
              """
        );
        
        _ = await Verify(result).HashParameters();
        string sourceA = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "ConsumerA");
        string sourceB = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "ConsumerB");
        
        Assert.Contains("this._foo = resolver.Resolve<", sourceA);
        Assert.DoesNotContain("_bar", sourceA);
 
        Assert.Contains("this._bar = resolver.Resolve<", sourceB);
        Assert.DoesNotContain("_foo", sourceB);
    }

    [Fact]
    internal async Task GenerationNotTriggered_OnAbsenceOfInjectableAttribute() {
        GeneratorDriverRunResult result = GeneratorTestsHelper.RunGenerator<SurjectGenerator>(
            $$"""
              using Surject.Abstractions.Attributes;
              using UnityEngine;

              namespace TestApp {
                  public class Foo { }

                  public partial class Consumer : MonoBehaviour {
                      [Inject]
                      private Foo _foo;
                  }
              }    
              """,
            SurjectGenerator.TrackingNames.InjectableContainers
        );
        
        _ = await Verify(result).HashParameters();
        string source = GeneratorTestsHelper.GetSourceFileFromGeneratorRunResult(result, "Consumer");
        
        Assert.Empty(source);
    }
}