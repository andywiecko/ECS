using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class DefaultActionsOrderEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private DefaultActionsOrder asset;

        private class FakeSolver : ISolver
        {
            public event Action OnScheduling;
            public event Action OnJobsComplete;
            public List<Func<JobHandle, JobHandle>> Jobs { get; } = new();

            public Delegate[] OnSchedulingInvocationList() => OnScheduling?.GetInvocationList();
            public Delegate[] OnJobsCompleteInvocationList() => OnJobsComplete?.GetInvocationList();
        }

        private class FakeWorld : IWorld
        {
            public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
            public ComponentsRegistry ComponentsRegistry { get; } = new();
            public SystemsRegistry SystemsRegistry { get; } = new();
        }

        private DefaultActionsOrder actionsOrder;
        private FakeSolver solver;
        private FakeWorld world;

        [SetUp]
        public void SetUp()
        {
            world = new();
            solver = new();
            actionsOrder = Instantiate(asset);
        }

        [Test]
        public void GenerateSolverActionsTest()
        {
            actionsOrder.GenerateActions(solver, world);

            var expected = new Dictionary<SolverAction, IReadOnlyList<(MethodInfo, Type)>>
            {
                [SolverAction.OnScheduling] = new[] { action(typeof(FakeSystem1), "Method1"), action(typeof(FakeSystem1), "Method2") },
                [SolverAction.OnJobsCompletion] = new[] { action(typeof(FakeSystem2), "Method1") }
            };
            Assert.That(actionsOrder, Is.EquivalentTo(expected));

            static (MethodInfo, Type) action(Type t, string methodName)
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                return (t.GetMethod(methodName, flags), t);
            }
        }

        [Test]
        public void SubscribeSolverEventsTest()
        {
            world.SystemsRegistry.Add<FakeSystem1>(new());
            world.SystemsRegistry.Add<FakeSystem2>(new());

            actionsOrder.GenerateActions(solver, world);

            Assert.That(solver.OnSchedulingInvocationList(), Has.Length.EqualTo(2));
            Assert.That(solver.OnJobsCompleteInvocationList(), Has.Length.EqualTo(1));
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
            var instance = Instantiate(actionsOrder);
            var so = new UnityEditor.SerializedObject(instance);
            var onScheduling = so.FindProperty("onScheduling");
            onScheduling.arraySize = 0;
            so.ApplyModifiedProperties();

            instance.InvokeUnityCallback().OnValidate();

            so = new UnityEditor.SerializedObject(instance);
            var undefinedMethods = so.FindProperty("undefinedMethods");
            var action = undefinedMethods
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("<Action>k__BackingField");
            action.intValue = 0;
            so.ApplyModifiedProperties();

            instance.InvokeUnityCallback().OnValidate();

            world.SystemsRegistry.Add<FakeSystem1>(new());
            world.SystemsRegistry.Add<FakeSystem2>(new());

            instance.GenerateActions(solver, world);

            Assert.That(solver.OnSchedulingInvocationList(), Has.Length.EqualTo(1));
            Assert.That(solver.OnJobsCompleteInvocationList(), Has.Length.EqualTo(1));
        }
    }
}
