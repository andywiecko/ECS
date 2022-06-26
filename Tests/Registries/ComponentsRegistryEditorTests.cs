using andywiecko.BurstCollections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace andywiecko.ECS.Editor.Tests
{
    public class ComponentsRegistryEditorTests
    {
        private interface IFake1 : IComponent { }
        private interface IFake2 : IComponent { }

        private class Fake1 : IFake1
        {
            public Id<IComponent> ComponentId => default;
        }

        private class Fake2 : Fake1, IFake2 { }

        private ComponentsRegistry registry;

        [Test]
        public void AddComponentTest()
        {
            registry = new();

            registry.Add(new Fake1());
            registry.Add(new Fake1());
            registry.Add(new Fake1());
            registry.Add(new Fake2());
            registry.Add(new Fake2());

            Assert.That(registry.GetComponents<IFake1>().Count, Is.EqualTo(5));
            Assert.That(registry.GetComponents<IFake2>().Count, Is.EqualTo(2));
            Assert.Throws<KeyNotFoundException>(() => registry.GetComponents<Fake1>());
            Assert.Throws<KeyNotFoundException>(() => registry.GetComponents<Fake2>());
        }

        [Test]
        public void RemoveComponentTest()
        {
            registry = new();
            var fake1 = new Fake1();
            var fake2 = new Fake2();

            registry.Add(fake1);
            registry.Add(fake2);
            registry.Remove(fake1);

            Assert.That(registry.GetComponents<IFake1>(), Is.EqualTo(new[] { fake2 }));
            Assert.That(registry.GetComponents<IFake2>(), Is.EqualTo(new[] { fake2 }));
        }

        [Test]
        public void SubscribeOnAddTest()
        {
            var invoked = false;
            registry = new();
            registry.SubscribeOnAdd<IFake1>(_ => invoked = true);

            registry.Add<Fake1>(new());

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void SubscribeOnRemoveTest()
        {
            var invoked = false;
            registry = new() { };
            var f1 = new Fake1();
            registry.Add(f1);
            registry.SubscribeOnRemove<IFake1>(_ => invoked = true);

            registry.Remove(f1);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void UnsubscribeOnAddTest()
        {
            var invoked = false;
            registry = new();
            void action(object _) => invoked = true;
            registry.SubscribeOnAdd<IFake1>(action);
            registry.UnsubscribeOnAdd<IFake1>(action);

            registry.Add<Fake1>(new());

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void UnsubscribeOnRemoveTest()
        {
            var invoked = false;
            registry = new() { };
            var f1 = new Fake1();
            registry.Add(f1);
            void action(object _) => invoked = true;
            registry.SubscribeOnAdd<IFake1>(action);
            registry.UnsubscribeOnAdd<IFake1>(action);

            registry.Remove(f1);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void IEnumerableTest()
        {
            registry = new(new[] { typeof(Fake1), typeof(Fake2) });

            var f1 = new Fake1();
            var f2 = new Fake1();
            var f3 = new Fake2();

            registry.Add(f1);
            registry.Add(f2);
            registry.Add(f3);

            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new[] { f1, f2, f3 },
                [typeof(IFake1)] = new[] { f1, f2, f3 },
                [typeof(IFake2)] = new[] { f3 },
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ClearTest()
        {
            registry = new(new[] { typeof(Fake1) }) { new Fake1() };

            registry.Clear();

            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { },
                [typeof(IFake1)] = new IComponent[] { },
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void OnRegistryChangedTest()
        {
            var changed = false;
            registry = new();
            registry.OnRegistryChange += () => changed = true;

            registry.Add<Fake1>(new());

            Assert.That(changed, Is.True);
        }
    }
}