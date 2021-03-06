using andywiecko.BurstCollections;
using System;
using System.Collections.Generic;

namespace andywiecko.ECS
{
    public abstract class ComponentsTuple
    {
        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        public void Destroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
            refsToDisposeOnDestroy.Clear();
        }

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }
    }

    public abstract class ComponentsTuple<T1, T2> : ComponentsTuple, IComponent
        where T1 : IComponent
        where T2 : IComponent
    {
        public Id<IComponent> ComponentId { get; }
        protected ComponentsRegistry ComponentsRegistry { get; }
        protected T1 Item1 { get; }
        protected T2 Item2 { get; }

        protected ComponentsTuple(T1 item1, T2 item2, ComponentsRegistry componentsRegistry)
        {
            if (!InstantiateWhen(item1, item2))
            {
                return;
            }

            ComponentsRegistry = componentsRegistry;
            Item1 = item1;
            Item2 = item2;

            Initialize();

            ComponentsRegistry.Add(this);
            ComponentsRegistry.SubscribeOnRemove<T1>(OnRemoveComponentItem1);
            ComponentsRegistry.SubscribeOnRemove<T2>(OnRemoveComponentItem2);

            ComponentId = ComponentsRegistry.Counter.GetNext();
        }

        protected virtual void Initialize() { }
        protected virtual bool InstantiateWhen(T1 item1, T2 item2) => true;

        private void OnRemoveComponentItem1(object item1)
        {
            if (Item1.Equals(item1))
            {
                RemoveTuple();
            }
        }

        private void OnRemoveComponentItem2(object item2)
        {
            if (Item2.Equals(item2))
            {
                RemoveTuple();
            }
        }

        private void RemoveTuple()
        {
            ComponentsRegistry.Remove(this);
            ComponentsRegistry.UnsubscribeOnRemove<T1>(OnRemoveComponentItem1);
            ComponentsRegistry.UnsubscribeOnRemove<T2>(OnRemoveComponentItem2);
            Destroy();
        }
    }
}
