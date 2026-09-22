using System;
using System.Collections.Generic;

public static class DictionaryExtensions {
    extension<TKey, TValue>(Dictionary<TKey, TValue> self) {
        public bool Remove(TKey key, out TValue value) =>
            self.TryGetValue(key, out value) && self.Remove(key);

        public TValue? GetValueOrDefault(TKey key, TValue? @default = default) =>
            self.TryGetValue(key, out TValue value) ? value : @default;

        public TValue GetOrAdd(TKey key, TValue value) {
            if (!self.TryGetValue(key, out TValue result)) {
                result = value;
                self[key] = result;
            }
            
            return result;
        }
    }
}