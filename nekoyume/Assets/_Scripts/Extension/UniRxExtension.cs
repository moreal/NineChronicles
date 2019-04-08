using System;
using System.Collections.Generic;
using UniRx;
using Uno.Extensions;

namespace Nekoyume
{
    public static class UniRxExtension
    {
        public static void DisposeAll<T>(this ReactiveProperty<T> property) where T : IDisposable
        {
            property.Value?.Dispose();
            property.Dispose();
        }
        
        public static void DisposeAll<T>(this ReactiveProperty<List<T>> property) where T : IDisposable
        {
            property.Value?.ForEach(d => d.Dispose());
            property.Dispose();
        }
        
        public static void DisposeAll<T>(this ReactiveCollection<T> collection) where T : IDisposable
        {
            collection.ForEach(obj => obj.Dispose());
            collection.Dispose();
        }
    }
}
