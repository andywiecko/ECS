using andywiecko.BurstCollections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace andywiecko.ECS.Editor.Tests
{
    public class ComponentsTupleEditorTests
    {
        private static Type[] TargetTypes => typeof(ComponentsTupleEditorTests).GetNestedTypes(BindingFlags.NonPublic);

        private interface IFake1 : IComponent { }
        private interface IFake2 : IComponent { }
        private interface IFake3 : IComponent { }
        private interface IFake12 : IComponent { }

        private class FakeComponent : IComponent
        {
            public Id<IComponent> ComponentId { get; set; } = new();
        }

        private class Fake1 : FakeComponent, IFake1 { }
        private class Fake2 : FakeComponent, IFake2 { }
        private class Fake3 : FakeComponent, IFake3 { }

        private class Fake12 : ComponentsTuple<IFake1, IFake2>, IFake12
        {
            public static Func<IFake1, IFake2, bool> When = (_, _) => true;
            public IFake1 Fake1 { get; }
            public IFake2 Fake2 { get; }
            public Fake12(IFake1 f1, IFake2 f2, ComponentsRegistry c) : base(f1, f2, c) { Fake1 = f1; Fake2 = f2; }
            protected override bool InstantiateWhen(IFake1 item1, IFake2 item2) => When(item1, item2);
        }

        private ComponentsRegistry registry;

        [SetUp]
        public void SetUp()
        {
            Fake12.When = (_, _) => true;
        }

        [Test]
        public void ComponentsTupleAutoCreationTest()
        {
            var f1 = new Fake1();
            var f2 = new Fake2();
            var f12 = default(Fake12);
            registry = new(TargetTypes);
            registry.SubscribeOnAdd<IFake12>(i => f12 = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);

            Assert.That(f12.Fake1, Is.EqualTo(f1));
            Assert.That(f12.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f1, f12, f2 },
                [typeof(IFake1)] = new[] { f1 },
                [typeof(IFake2)] = new[] { f2 },
                [typeof(IFake3)] = new IFake3[] { },
                [typeof(IFake12)] = new[] { f12 }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ComponentsTupleAutoRemoveTest()
        {
            var f1 = new Fake1() { ComponentId = new(0) };
            var f2 = new Fake2() { ComponentId = new(0) };
            var f12 = default(Fake12);
            registry = new(TargetTypes);
            registry.SubscribeOnAdd<IFake12>(i => f12 = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);
            registry.Remove(f1);

            Assert.That(f12.Fake1, Is.EqualTo(f1));
            Assert.That(f12.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f2 },
                [typeof(IFake1)] = new IFake1[] { },
                [typeof(IFake2)] = new IFake2[] { f2 },
                [typeof(IFake3)] = new IFake3[] { },
                [typeof(IFake12)] = new IFake12[] { }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void InstantiateWhenTest()
        {
            var f1a = new Fake1() { ComponentId = new(0) };
            var f1b = new Fake1() { ComponentId = new(1) };
            var f2 = new Fake2() { ComponentId = new(0) };
            Fake12.When = (a, b) => a.ComponentId == b.ComponentId;
            var f12 = default(Fake12);
            registry = new(TargetTypes);
            registry.SubscribeOnAdd<IFake12>(i => f12 = i as Fake12);

            registry.Add(f1a);
            registry.Add(f1b);
            registry.Add(f2);

            Assert.That(f12.Fake1, Is.EqualTo(f1a));
            Assert.That(f12.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f1a, f1b, f12, f2 },
                [typeof(IFake1)] = new[] { f1a, f1b },
                [typeof(IFake2)] = new[] { f2 },
                [typeof(IFake3)] = new IFake3[] { },
                [typeof(IFake12)] = new[] { f12 }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }
    }
}
