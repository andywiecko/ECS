using NUnit.Framework;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class SystemsManagerEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private SystemsManager asset;

        private SystemsManager manager;
        private SystemsRegistry registry;

        [SetUp]
        public void SetUp()
        {
            manager = Instantiate(asset);
            registry = manager.World.SystemsRegistry;
        }

        [Test]
        public void EmptyRegistryAfterAwakeTest()
        {
            manager.InvokeUnityCallback().Awake();

            Assert.That(registry, Is.Empty);
        }

        [Test]
        public void NonEmptyRegistryAfterStartTest()
        {
            manager.InvokeUnityCallback().Awake();
            manager.InvokeUnityCallback().Start();

            Assert.That(registry.TryGetSystem<FakeSystem1>(out var _), Is.True);
            Assert.That(registry.TryGetSystem<FakeSystem2>(out var _), Is.False);
        }

        [Test]
        public void SetSystemActiveTest()
        {
            manager.InvokeUnityCallback().Awake();
            manager.InvokeUnityCallback().Start();

            manager.SetSystemActive<FakeSystem1>(false);
            manager.SetSystemActive<FakeSystem2>(true);

            Assert.That(registry.TryGetSystem<FakeSystem1>(out var _), Is.False);
            Assert.That(registry.TryGetSystem<FakeSystem2>(out var _), Is.True);
        }

        [Test]
        public void SetSystemActiveIdentityTest()
        {
            manager.InvokeUnityCallback().Awake();
            manager.InvokeUnityCallback().Start();

            manager.SetSystemActive<FakeSystem1>(true);
            manager.SetSystemActive<FakeSystem2>(false);

            Assert.That(registry.TryGetSystem<FakeSystem1>(out var _), Is.True);
            Assert.That(registry.TryGetSystem<FakeSystem2>(out var _), Is.False);
        }

        [Test]
        public void OnValidateDefaultingWorldTest()
        {
            // HACK: this test setup is rather a hack.
            // We "mock" here the situation where one set world as null.
            var instance = Instantiate(asset);
            var so = new UnityEditor.SerializedObject(instance);
            var worldProperty = so.FindProperty("<World>k__BackingField");
            worldProperty.objectReferenceValue = default;
            so.ApplyModifiedProperties();

            instance.InvokeUnityCallback().OnValidate();

            so = new(instance);
            var systemsProperty = so.FindProperty("serializedSystems");
            Assert.That(systemsProperty.arraySize, Is.EqualTo(0));
        }

        [Test]
        public void OnValidateNewSystemTest()
        {
            // HACK: this test setup is rather a hack.
            // We "mock" here the situation where one of the system is not present
            // at serialized object at all (corresponds to situation when new (undefined) system
            // is created in project).
            var instance = Instantiate(asset);
            var so = new UnityEditor.SerializedObject(instance);
            var worldProperty = so.FindProperty("serializedSystems");
            worldProperty.DeleteArrayElementAtIndex(0);
            so.ApplyModifiedProperties();

            instance.InvokeUnityCallback().OnValidate();
            instance.InvokeUnityCallback().Awake();
            instance.InvokeUnityCallback().Start();

            instance.SetSystemActive<FakeSystem1>(true);
            instance.SetSystemActive<FakeSystem2>(false);

            var registry = instance.World.SystemsRegistry;
            Assert.That(registry.TryGetSystem<FakeSystem1>(out var _), Is.True);
            Assert.That(registry.TryGetSystem<FakeSystem2>(out var _), Is.False);
        }
    }
}