using andywiecko.BurstCollections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace andywiecko.ECS.Editor.Tests
{
    public class ComponentsTupleEditorTests
    {
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
            public Ref<NativeArray<int>> Disposable;
            public static Func<IFake1, IFake2, bool> When = (_, _) => true;
            public IFake1 Fake1 { get; }
            public IFake2 Fake2 { get; }

            public Fake12(IFake1 f1, IFake2 f2, ComponentsRegistry c) : base(f1, f2, c) { Fake1 = f1; Fake2 = f2; }
            protected override bool InstantiateWhen(IFake1 item1, IFake2 item2) => When(item1, item2);

            protected override void Initialize()
            {
                Disposable = new(new(1, Allocator.Persistent));
                DisposeOnDestroy(Disposable);
            }
        }

        private class SimplifiedFake12 : ComponentsTuple<IFake1, IFake2>, IFake12
        {
            public SimplifiedFake12(IFake1 f1, IFake2 f2, ComponentsRegistry c) : base(f1, f2, c) { }
        }

        private ComponentsRegistry registry;
        private Fake12 tuple;

        [SetUp]
        public void SetUp()
        {
            Fake12.When = (_, _) => true;
        }

        [TearDown]
        public void TearDown()
        {
            tuple?.Destroy();
        }

        [Test]
        public void ComponentsTupleAutoCreationTest()
        {
            var f1 = new Fake1();
            var f2 = new Fake2();
            var f3 = new Fake3();
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12), typeof(Fake3) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);
            registry.Add(f3);

            Assert.That(tuple.ComponentId, Is.EqualTo(new Id<IComponent>(0)));
            Assert.That(tuple.Fake1, Is.EqualTo(f1));
            Assert.That(tuple.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f1, f2, tuple, f3 },
                [typeof(IFake1)] = new[] { f1 },
                [typeof(IFake2)] = new[] { f2 },
                [typeof(IFake3)] = new[] { f3 },
                [typeof(IFake12)] = new[] { tuple }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ComponentsTupleAutoCreationReversedAddTest()
        {
            var f1 = new Fake1();
            var f2 = new Fake2();
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12), typeof(Fake3) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f2);
            registry.Add(f1);

            Assert.That(tuple.Fake1, Is.EqualTo(f1));
            Assert.That(tuple.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f2, tuple, f1 },
                [typeof(IFake1)] = new[] { f1 },
                [typeof(IFake2)] = new[] { f2 },
                [typeof(IFake3)] = new IFake3[] { },
                [typeof(IFake12)] = new[] { tuple }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ComponentsTupleAutoRemoveTest()
        {
            var f1 = new Fake1() { ComponentId = new(0) };
            var f2 = new Fake2() { ComponentId = new(0) };
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12), typeof(Fake3) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);
            registry.Remove(f1);

            Assert.That(tuple.Fake1, Is.EqualTo(f1));
            Assert.That(tuple.Fake2, Is.EqualTo(f2));
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
        public void ComponentsTupleAutoRemoveReversedTest()
        {
            var f1 = new Fake1() { ComponentId = new(0) };
            var f2 = new Fake2() { ComponentId = new(0) };
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12), typeof(Fake3) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);
            registry.Remove(f2);

            Assert.That(tuple.Fake1, Is.EqualTo(f1));
            Assert.That(tuple.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f1 },
                [typeof(IFake1)] = new IFake1[] { f1 },
                [typeof(IFake2)] = new IFake2[] { },
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
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12), typeof(Fake3) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f1a);
            registry.Add(f1b);
            registry.Add(f2);

            Assert.That(tuple.Fake1, Is.EqualTo(f1a));
            Assert.That(tuple.Fake2, Is.EqualTo(f2));
            var expected = new Dictionary<Type, IList>
            {
                [typeof(IComponent)] = new IComponent[] { f1a, f1b, f2, tuple },
                [typeof(IFake1)] = new[] { f1a, f1b },
                [typeof(IFake2)] = new[] { f2 },
                [typeof(IFake3)] = new IFake3[] { },
                [typeof(IFake12)] = new[] { tuple }
            };
            Assert.That(registry, Is.EquivalentTo(expected));
        }

        [Test]
        public void ComponentsTupleDisposeTest()
        {
            var f1 = new Fake1() { ComponentId = new(0) };
            var f2 = new Fake2() { ComponentId = new(0) };
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(Fake12) });
            registry.SubscribeOnAdd<IFake12>(i => tuple = i as Fake12);

            registry.Add(f1);
            registry.Add(f2);
            registry.Remove(f1);

            Assert.That(tuple.Disposable.Value.IsCreated, Is.False);
        }

        [Test]
        public void SimplifiedTupleTest()
        {
            var f1 = new Fake1();
            var f2 = new Fake2();
            var t = default(SimplifiedFake12);
            registry = new(new[] { typeof(Fake1), typeof(Fake2), typeof(SimplifiedFake12) });
            registry.SubscribeOnAdd<IFake12>(i => t = i as SimplifiedFake12);

            registry.Add(f1);
            registry.Add(f2);

            Assert.That(t, Is.Not.Null);
        }
    }
}
