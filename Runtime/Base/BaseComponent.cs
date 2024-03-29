using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class BaseComponent : MonoBehaviour, IEntityComponent
    {
        public Entity Entity { get; private set; } = default;
        public World World => Entity.World;

        public Id<IEntity> EntityId => Entity.EntityId;
        public Id<IComponent> ComponentId { get; private set; }

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
            Entity = GetComponent<Entity>();
            Entity.TryRegister();
            ComponentId = World.ComponentsRegistry.Counter.GetNext();
        }
        protected virtual void OnEnable() => World.ComponentsRegistry.Add(this);
        protected virtual void OnDisable() => World.ComponentsRegistry.Remove(this);
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
