using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class DefaultJobsOrderEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private DefaultJobsOrder asset;

        private class FakeWorld : IWorld
        {
            public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
            public ComponentsRegistry ComponentsRegistry { get; } = new();
            public SystemsRegistry SystemsRegistry { get; } = new();
        }

        private class FakeSolver : ISolver
        {
            public List<Func<JobHandle, JobHandle>> Jobs { get; } = new();
            public event Action OnScheduling;
            public event Action OnJobsComplete;
        }

        private DefaultJobsOrder jobsOrder;
        private FakeWorld world;
        private FakeSolver solver;

        [SetUp]
        public void SetUp()
        {
            jobsOrder = Instantiate(asset);
            world = new();
            solver = new();
        }

        [Test]
        public void GenerateJobsOrderTest()
        {
            jobsOrder.GenerateJobs(solver, world);

            var expected = new[] { typeof(FakeSystem1), typeof(FakeSystem2) };
            Assert.That(jobsOrder, Is.EqualTo(expected));
        }

        [Test]
        public void AddSolverJobsTest()
        {
            var s1 = new FakeSystem1();
            var s2 = new FakeSystem2();
            world.SystemsRegistry.Add(s1);
            world.SystemsRegistry.Add(s2);

            jobsOrder.GenerateJobs(solver, world);

            var expected = new Func<JobHandle, JobHandle>[] { s1.Schedule, s2.Schedule };
            Assert.That(solver.Jobs, Is.EqualTo(expected));
        }

        [Test]
        public void OnValidateTest()
        {
            // HACK: this test setup is rather a hack.
            // Scriptable objects are validated during editor,
            // so there is no way to provide object which should be validated during test.
            // We "mock" here the situation where one of the system is not present
            // at serialized object at all (corresponds to situation when new (undefined) system
            // is created in project), and the other system which corresponds to
            // transition from "Undefined" to "OnScheduling".
            var so = new SerializedObject(jobsOrder);
            var enabledTypes = so.FindProperty("enabledTypes");
            enabledTypes.arraySize = 0;
            so.ApplyModifiedProperties();

            jobsOrder.InvokeUnityCallback().OnValidate();

            so = new(jobsOrder);
            var undefinedTypes = so.FindProperty("undefinedTypes");
            Assert.That(undefinedTypes.arraySize, Is.EqualTo(2));

            var jobStatus = undefinedTypes
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("<Job>k__BackingField");
            jobStatus.intValue = 0;
            so.ApplyModifiedProperties();

            world.SystemsRegistry.Add<FakeSystem1>(new());
            world.SystemsRegistry.Add<FakeSystem2>(new());

            jobsOrder.InvokeUnityCallback().OnValidate();
            jobsOrder.GenerateJobs(solver, world);

            Assert.That(solver.Jobs, Has.Count.EqualTo(1));
        }
    }
}
