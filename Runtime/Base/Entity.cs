using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.ECS
{
    [DisallowMultipleComponent]
    public abstract class Entity : MonoBehaviour, IEntity
    {
        private static readonly IdCounter<IEntity> Counter = new(); // CLean this to be related with world itself
        public Id<IEntity> EntityId { get; } = Counter.GetNext();

        [field: SerializeField]
        public World World { get; private set; } = default;

        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }



        protected virtual void OnDestroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
        }
    }
}