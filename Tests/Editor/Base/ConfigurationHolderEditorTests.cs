using andywiecko.ECS.Tests;
using NUnit.Framework;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class ConfigurationHolderEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private FakeConfigurationHolder holder;

        [SetUp]
        public void SetUp()
        {
            holder.World.Clear();
        }

        [Test]
        public void RegisterConfigurationTest()
        {
            holder.InvokeUnityCallback().Awake();

            var config = holder.World.ConfigurationsRegistry.Get<FakeConfiguration>();
            Assert.That(config.Value, Is.EqualTo(42));
        }
    }
}