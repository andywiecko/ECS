using System;
using UnityEngine;

namespace andywiecko.ECS.Tests
{
    [Serializable]
    public class FakeConfiguration : IConfiguration
    {
        [field: SerializeField]
        public int Value { get; private set; } = 0;
    }
}