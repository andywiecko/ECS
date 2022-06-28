using andywiecko.BurstCollections;
using Unity.Collections;

namespace andywiecko.ECS.Tests
{
    public class FakeEntity : Entity
    {
        public Ref<NativeArray<int>> Data { get; private set; }

        private void Awake()
        {
            DisposeOnDestroy(
                Data = new(new(1, Allocator.Persistent))
            );
        }
    }
}