using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public class DefaultSolverActionsOrderEditorTests : UnityEditor.Editor
    {
        [SerializeField]
        private DefaultSolverActionsOrder solverActionsOrder;

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

        private FakeSolver solver;
        private FakeWorld world;

        [Test]
        public void GenerateSolverActionsTest()
        {
            world = new();
            solver = new();

            solverActionsOrder.GenerateActions(solver, world);

            var expected = new Dictionary<SolverAction, IReadOnlyList<(MethodInfo, Type)>>
            {
                [SolverAction.OnScheduling] = new[] { action(typeof(FakeSystem1), "Method1"), action(typeof(FakeSystem1), "Method2") },
                [SolverAction.OnJobsCompletion] = new[] { action(typeof(FakeSystem2), "Method1") }
            };
            Assert.That(solverActionsOrder, Is.EquivalentTo(expected));

            static (MethodInfo, Type) action(Type t, string methodName)
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                return (t.GetMethod(methodName, flags), t);
            }
        }

        [Test]
        public void SubscribeSolverEventsTest()
        {
            world = new();
            world.SystemsRegistry.Add<FakeSystem1>(new());
            solver = new();

            solverActionsOrder.GenerateActions(solver, world);

            Assert.That(solver.OnSchedulingInvocationList(), Has.Length.EqualTo(2));
            Assert.That(solver.OnJobsCompleteInvocationList(), Is.Null);
        }
    }
}
