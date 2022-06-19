using andywiecko.BurstCollections;
using NUnit.Framework;
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
    }
}