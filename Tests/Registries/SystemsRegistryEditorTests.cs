using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class SystemsRegistryEditorTests
    {
        private class FakeSystem : ISystem { public JobHandle Schedule(JobHandle _) => default; }
        private class FakeSystem1 : FakeSystem { }
        private class FakeSystem2 : FakeSystem { }
        private class FakeSystem3 : FakeSystem { }

        private SystemsRegistry registry;

        [Test]
        public void AddSystemTest()
        {
            registry = new();
            var s = new FakeSystem1();

            registry.Add(s);

            Assert.That(registry.Get<FakeSystem1>(), Is.EqualTo(s));
            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeSystem2>());
            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeSystem3>());
        }

        [Test]
        public void RemoveSystemTest()
        {
            registry = new();
            var s = new FakeSystem1();

            registry.Add(s);
            registry.Remove(s);

            Assert.That(registry, Is.Empty);
        }

        [Test]
        public void ThrowOnMultipleAddSystemOfGivenTypeTest()
        {
            registry = new();

            registry.Add<FakeSystem1>(new());
            registry.Add<FakeSystem2>(new());

            Assert.Throws<ArgumentException>(() => registry.Add<FakeSystem1>(new()));
        }

        [Test]
        public void OnRegistryChangeTest()
        {
            var registryChanged = false;
            registry = new();
            registry.OnRegistryChange += () => registryChanged = true;

            registry.Add<FakeSystem1>(new());

            Assert.That(registryChanged, Is.True);
        }

        [Test]
        public void IEnumerableTest()
        {
            var s1 = new FakeSystem1();
            var s2 = new FakeSystem2();
            var s3 = new FakeSystem3();
            registry = new() { s1, s2, s3 };

            var expected = new Dictionary<Type, ISystem>
            {
                [typeof(FakeSystem1)] = s1,
                [typeof(FakeSystem2)] = s2,
                [typeof(FakeSystem3)] = s3,
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ClearTest()
        {
            registry = new();

            registry.Add<FakeSystem1>(new());
            registry.Clear();

            Assert.Throws<KeyNotFoundException>(() => registry.Get<FakeSystem1>());
        }
    }
}