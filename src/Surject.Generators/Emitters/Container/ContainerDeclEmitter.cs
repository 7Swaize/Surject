using System.CodeDom.Compiler;
using System.Linq;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Container;

internal readonly struct ContainerDeclEmitter {
    private readonly ContainerModel _container;
    
    internal ContainerDeclEmitter(ContainerModel container) {
        _container = container;
    }

    internal void Emit(IndentedTextWriter writer) {
        ClassDeclModel declModel = _container.Decl;
        
        string accessibility = declModel.AccessModifier.AsDeclString();
        string isPartial = declModel.IsPartial ? "partial " : string.Empty;
        string isSealed = declModel.IsSealed ? "sealed " : string.Empty;
        string isStatic = declModel.IsStatic ? "static " : string.Empty;
        
        string typeParams = declModel.ClassAsTypeRef.TypeParameters.Length > 0
            ? "<" + string.Join(", ", declModel.ClassAsTypeRef.TypeParameters.Select(static tp => tp.FQNGenericBased)) + ">"
            : string.Empty;

        string constraints = declModel.ClassAsTypeRef.Constraints.Length > 0
            ? string.Join(" ", declModel.ClassAsTypeRef.Constraints.Select(static ct => ct.ToString()))
            : string.Empty;
        
        writer.WriteLine(
            $"{accessibility} {isPartial} {isSealed} {isStatic} class " +
            $"{declModel.FQNNoGlobal}{typeParams} " +
            $"{constraints} {{"
        );
    }
}