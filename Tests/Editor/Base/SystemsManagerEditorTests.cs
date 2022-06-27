using NUnit.Framework;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class SystemsManagerEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private SystemsManager managerAsset;

        private SystemsManager manager;
        private SystemsRegistry registry;

        [SetUp]
        public void SetUp()
        {
            manager = Instantiate(managerAsset);
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
    }
}