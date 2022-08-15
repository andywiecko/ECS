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

        public void TryRegister()
        {
            EntityId = EntityId == Id<IEntity>.Invalid ? World.EntitiesCounter.GetNext() : EntityId;
        }

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }

        protected virtual void Awake() => TryRegister();

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