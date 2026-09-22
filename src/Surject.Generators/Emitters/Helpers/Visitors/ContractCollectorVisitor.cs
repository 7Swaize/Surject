using System.Collections.Generic;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Helpers.Visitors;

internal readonly struct ContractCollectorVisitor : IModifierCommandVisitor<VoidVisitor> {
    private readonly ITypeReferenceModel _selfType;
    private readonly List<ITypeReferenceModel> _destination;

    internal ContractCollectorVisitor(ITypeReferenceModel selfType, List<ITypeReferenceModel> destination) {
        _selfType = selfType;
        _destination = destination;
    }
    
    public VoidVisitor VisitWithId(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitEager(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitLazy(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitWithArgument(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitOverrideExisting(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAsCollection(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAsPrimary(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitDoNotDispose(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitTrackDisposable(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitUnderTransform(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitUnderObjectOfType(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitWithGameObjectName(in ModifierCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitDoNotDestroy(in ModifierCommandModel cmd) => VoidVisitor.Default;
    
    public VoidVisitor VisitTo(in ModifierCommandModel cmd) {
        _destination.Add(cmd.TypeArg);
        return VoidVisitor.Default; 
    }
    
    public VoidVisitor ToImmediateImplementedInterfaces(in ModifierCommandModel cmd) {
        foreach (ITypeReferenceModel iface in _selfType.ImmediateInterfaces) {
            _destination.Add(iface);
        }
        
        return VoidVisitor.Default;
    }
    
    public VoidVisitor VisitToAllImplementedInterfaces(in ModifierCommandModel cmd) {
        foreach (ITypeReferenceModel iface in _selfType.AllInterfaces) {
            _destination.Add(iface);
        }
        
        return VoidVisitor.Default;
    }
}