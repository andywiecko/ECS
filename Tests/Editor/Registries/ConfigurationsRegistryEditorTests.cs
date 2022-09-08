using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace andywiecko.ECS.Editor.Tests
{
    public class ConfigurationsRegistryEditorTests
    {
        private class FakeConfig1 : IConfiguration { }
        private class FakeConfig2 : IConfiguration { }
        private class FakeConfig3 : IConfiguration { }

        private ConfigurationsRegistry registry;

        [Test]
        public void SetConfigurationTest()
        {
            registry = new();
            var config = new FakeConfig1();

            registry.Set(config);

            Assert.That(registry.Get<FakeConfig1>(), Is.EqualTo(config));
            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeConfig2>());
            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeConfig3>());
        }

        [Test]
        public void OnRegistryChangeTest()
        {
            var registryChanged = false;
            registry = new();
            registry.OnRegistryChange += () => registryChanged = true;

            registry.Set<FakeConfig1>(new());

            Assert.That(registryChanged, Is.True);
        }

        [Test]
        public void ConfigurationCanBeSetOnceTest()
        {
            registry = new();
            var c1 = new FakeConfig1();
            var c2 = new FakeConfig1();

            registry.Set(c1);

            Assert.Throws<ArgumentException>(() => registry.Set(c2));
        }

        [Test]
        public void IEnumerableTest()
        {
            registry = new();
            var c1 = new FakeConfig1();
            var c2 = new FakeConfig2();
            var c3 = new FakeConfig3();

            registry.Set(c1);
            registry.Set(c2);
            registry.Set(c3);

            var expected = new Dictionary<Type, IConfiguration>
            {
                [typeof(FakeConfig1)] = c1,
                [typeof(FakeConfig2)] = c2,
                [typeof(FakeConfig3)] = c3,
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ClearTest()
        {
            registry = new();

            registry.Set<FakeConfig1>(new());
            registry.Clear();

            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeConfig1>());
        }

        [Test]
        public void GetOrCreateTest()
        {
            registry = new();
            var c1 = new FakeConfig1();

            registry.Set(c1);
            registry.GetOrCreate<FakeConfig1>();
            var c2 = registry.GetOrCreate<FakeConfig2>();

            var expected = new Dictionary<Type, IConfiguration>
            {
                [typeof(FakeConfig1)] = c1,
                [typeof(FakeConfig2)] = c2,
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }
    }
}
