using andywiecko.BurstCollections;
using NUnit.Framework;
using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class BaseSystemEditorTests
    {
        private interface IFakeComponent : IComponent { }

        private class FakeComponent : IFakeComponent
        {
            public Id<IComponent> ComponentId { get; set; }
        }

        private class FakeConfiguration : IConfiguration
        {
            public readonly int Value = 42;
        }

        private class FakeWorld : IWorld
        {
            public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
            public ComponentsRegistry ComponentsRegistry { get; } = new(new[] { typeof(FakeComponent) });
            public SystemsRegistry SystemsRegistry { get; } = new();
        }

        private class FakeSystem : BaseSystem
        {
            public bool IsScheduled { get; private set; }
            public override JobHandle Schedule(JobHandle dependencies)
            {
                IsScheduled = true;
                return dependencies;
            }
        }

        private class FakeSystemT : BaseSystem<IFakeComponent>
        {
            public int Value = 0;

            public override JobHandle Schedule(JobHandle dependencies)
            {
                foreach (var _ in References)
                {
                    Value++;
                }

                return dependencies;
            }
        }

        private class FakeSystemC : BaseSystemWithConfiguration<FakeConfiguration>
        {
            public int ConfigValue = 0;

            public override JobHandle Schedule(JobHandle dependencies)
            {
                ConfigValue = Configuration.Value;

                return dependencies;
            }
        }

        private class FakeSystemTC : BaseSystemWithConfiguration<IFakeComponent, FakeConfiguration>
        {
            public int ConfigValue = 0;
            public int ComponentsValue = 0;
            public override JobHandle Schedule(JobHandle dependencies)
            {
                ConfigValue = Configuration.Value;
                foreach (var _ in References)
                {
                    ComponentsValue++;
                }

                return dependencies;
            }
        }

        private FakeWorld world;

        [SetUp]
        public void SetUp()
        {
            world = new();
            world.ComponentsRegistry.Add<FakeComponent>(new());
            world.ConfigurationsRegistry.Set<FakeConfiguration>(new());
        }

        [Test]
        public void BaseSystemTest()
        {
            var system = new FakeSystem() { World = world };
            system.Schedule(default);
            Assert.That(system.IsScheduled, Is.True);
        }

        [Test]
        public void BaseSystemRunTest()
        {
            var system = new FakeSystem() { World = world };
            system.Run();
            Assert.That(system.IsScheduled, Is.True);
        }

        [Test]
        public void BaseSystemWithComponentsTest()
        {
            var system = new FakeSystemT() { World = world };
            system.Schedule(default);
            Assert.That(system.Value, Is.EqualTo(1));
        }

        [Test]
        public void BaseSystemWithConfigurationTest()
        {
            var system = new FakeSystemC() { World = world };
            system.Schedule(default);
            Assert.That(system.ConfigValue, Is.EqualTo(42));
        }

        [Test]
        public void BaseSystemWithConfigurationAndComponentsTest()
        {
            var system = new FakeSystemTC() { World = world };
            system.Schedule(default);
            Assert.That(system.ComponentsValue, Is.EqualTo(1));
            Assert.That(system.ConfigValue, Is.EqualTo(42));
        }
    }
}