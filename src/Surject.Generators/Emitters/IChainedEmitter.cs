using System.CodeDom.Compiler;

namespace Surject.Generators.Emitters;

internal interface IChainedEmitter {
    internal void Emit(IndentedTextWriter writer);
}