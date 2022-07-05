using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.ECS
{
    [DisallowMultipleComponent]
    public abstract class Entity : MonoBehaviour, IEntity
    {
        public Id<IEntity> EntityId { get; private set; } = Id<IEntity>.Invalid;

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

        protected virtual void Awake()
        {
            EntityId = World.EntitiesCounter.GetNext();
        }

        protected virtual void OnDestroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
            refsToDisposeOnDestroy.Clear();
        }
    }
}