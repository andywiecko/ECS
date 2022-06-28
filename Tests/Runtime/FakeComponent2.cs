using UnityEngine;

namespace andywiecko.ECS.Tests
{
    public interface IFakeComponent2 : IComponent { }

    [RequireComponent(typeof(FakeEntity))]
    public class FakeComponent2 : BaseComponent, IFakeComponent2
    {

    }
}