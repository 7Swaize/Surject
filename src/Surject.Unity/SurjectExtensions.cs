using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Surject.Unity;

public static class SurjectExtensions {
    [Preserve]
    public static List<TTarget> PreformTraversalWithBoundary<TTarget, TBoundary>(GameObject self, Func<GameObject, TTarget[]> selector) 
        where TTarget : class
        where TBoundary : Component
    {
        List<TTarget> result = [];
        TTarget[] candidates = selector(self);
        
        foreach (TTarget candidate in candidates) {
            Component? component = candidate as Component;

            if (component == null) {
                continue;
            }

            if (ReferenceEquals(component.GetComponentInParent<TBoundary>().gameObject, self)) {
                result.Add(candidate);
            }
        }
        
        return result;
    }
}