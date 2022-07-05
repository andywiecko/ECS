using andywiecko.ECS.Tests;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class EntityComponentEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private FakeEntity asset = default;

        private FakeEntity entity;
        private FakeComponent1 component1;
        private FakeComponent2 component2;
        private World world;

        [SetUp]
        public void SetUp()
        {
            entity = Instantiate(asset);
            component1 = entity.GetComponent<FakeComponent1>();
            component2 = entity.GetComponent<FakeComponent2>();
            world = entity.GetComponent<World>();
        }

        [TearDown]
        public void TearDown()
        {
            entity.InvokeUnityCallback().OnDestroy();
            component1.InvokeUnityCallback().OnDestroy();
            component2.InvokeUnityCallback().OnDestroy();
        }

        [Test]
        public void EntityDisposeOnDestroyTest()
        {
            entity.InvokeUnityCallback().Awake();
            var isCreated = entity.Data.Value.IsCreated;
            entity.InvokeUnityCallback().OnDestroy();
            var isDestroyed = !entity.Data.Value.IsCreated;

            Assert.That(isCreated, Is.True);
            Assert.That(isDestroyed, Is.True);
        }

        [Test]
        public void ComponentDisposeOnDestroyTest()
        {
            component1.InvokeUnityCallback().Awake();
            var isCreated = component1.Data.Value.IsCreated;
            component1.InvokeUnityCallback().OnDestroy();
            var isDestroyed = !component1.Data.Value.IsCreated;

            Assert.That(isCreated, Is.True);
            Assert.That(isDestroyed, Is.True);
        }

        [Test]
        public void ComponentRegisterOnEnableTest()
        {
            component1.InvokeUnityCallback().Awake();
            component1.InvokeUnityCallback().OnEnable();

            Assert.That(world.ComponentsRegistry.GetComponents<IFakeComponent1>(), Is.EqualTo(new[] { component1 }));
        }

        [Test]
        public void ComponentDeregisterOnDisableTest()
        {
            component1.InvokeUnityCallback().Awake();
            component1.InvokeUnityCallback().OnEnable();
            var wasAdded = world.ComponentsRegistry.GetComponents<IFakeComponent1>().Contains(component1);

            component1.InvokeUnityCallback().OnDisable();

            Assert.That(wasAdded, Is.True);
            Assert.That(world.ComponentsRegistry.GetComponents<IFakeComponent1>(), Is.Empty);
        }

        [Test]
        public void ComponentDifferentIdsTest()
        {
            component1.InvokeUnityCallback().Awake();
            component2.InvokeUnityCallback().Awake();

            Assert.That(component1.ComponentId, Is.Not.EqualTo(component2.ComponentId));
        }

        [Test]
        public void EntityComponentIdTest()
        {
            component1.InvokeUnityCallback().Awake();
            component2.InvokeUnityCallback().Awake();

            Assert.That(component1.EntityId, Is.EqualTo(entity.EntityId));
            Assert.That(component2.EntityId, Is.EqualTo(entity.EntityId));
        }

        [Test]
        public void EntityEditorSelectionTest()
        {
            // HACK: tricky solution for testing the entity editor.
            UnityEditor.Selection.activeObject = entity;
        }
    }
}