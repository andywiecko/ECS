using andywiecko.ECS.Tests;
using NUnit.Framework;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class ConfigurationHolderEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private FakeConfigurationHolder asset;

        private FakeConfigurationHolder holder;

        [SetUp]
        public void SetUp()
        {
            holder = Instantiate(asset);
        }

        [Test]
        public void RegisterConfigurationTest()
        {
            holder.InvokeUnityCallback().Awake();

            var config = holder.World.ConfigurationsRegistry.Get<FakeConfiguration>();
            Assert.That(config.Value, Is.EqualTo(42));
        }

        [Test]
        public void AttachWorldOnResetTest()
        {
            var initialWorld = holder.World;

            var so = new UnityEditor.SerializedObject(holder);
            var world = so.FindProperty("<World>k__BackingField");
            world.objectReferenceValue = null;
            holder.InvokeUnityCallback().Reset();

            Assert.That(holder.World, Is.EqualTo(initialWorld));
        }
    }
}