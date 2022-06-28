using andywiecko.BurstCollections;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.ECS.Tests
{
    public interface IFakeComponent1 : IComponent { }

    [RequireComponent(typeof(FakeEntity))]
    public class FakeComponent1 : BaseComponent, IFakeComponent1
    {
        public Ref<NativeArray<int>> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            DisposeOnDestroy(
                Data = new(new(1, Allocator.Persistent))
            );
        }
    }
}