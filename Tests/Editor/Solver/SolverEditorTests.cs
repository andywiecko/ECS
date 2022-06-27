using NUnit.Framework;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class SolverEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private Solver solver = default;

        [SerializeField]
        private FakeSolverActionsOrder actionsOrder = default;

        [SerializeField]
        private FakeSolverJobsOrder jobsOrder = default;

        [SetUp]
        public void SetUp()
        {
            actionsOrder.Reset();
            jobsOrder.Reset();
            solver.World.Clear();
        }

        [Test]
        public void GenerateOnStartTest()
        {
            solver.InvokeUnityCallback().Start();

            Assert.That(actionsOrder.GenerationCount, Is.EqualTo(1));
            Assert.That(jobsOrder.GenerationCount, Is.EqualTo(1));
        }

        [Test]
        public void RegenerateOnRegistryChangeTest()
        {
            solver.InvokeUnityCallback().Awake();
            solver.World.SystemsRegistry.Add<FakeSystem1>(new());

            Assert.That(actionsOrder.GenerationCount, Is.EqualTo(1));
            Assert.That(jobsOrder.GenerationCount, Is.EqualTo(1));
        }

        [Test]
        public void UnsubscribeRegenerateOnRegistryChangeTest()
        {
            solver.InvokeUnityCallback().Awake();
            solver.InvokeUnityCallback().OnDestroy();
            solver.World.SystemsRegistry.Add<FakeSystem1>(new());

            Assert.That(actionsOrder.GenerationCount, Is.EqualTo(0));
            Assert.That(jobsOrder.GenerationCount, Is.EqualTo(0));
        }

        [Test]
        public void UpdateTest()
        {
            solver.InvokeUnityCallback().Awake();
            solver.InvokeUnityCallback().Start();
            solver.InvokeUnityCallback().Update();
            solver.InvokeUnityCallback().OnDestroy();

            Assert.That(actionsOrder.UpdateCount, Is.EqualTo(2));
            Assert.That(jobsOrder.UpdateCount, Is.EqualTo(1));
        }
    }
}
