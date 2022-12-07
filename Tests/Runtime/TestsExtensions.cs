using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public static class TestsExtensions
    {
        public static UnityCallbacks InvokeUnityCallback(this Object @object) => new(@object);

        public class UnityCallbacks
        {
            private readonly Object @object;

            public UnityCallbacks(Object @object) => this.@object = @object;

            public void Awake() => Invoke(nameof(Awake));
            public void Start() => Invoke(nameof(Start));
            public void Update() => Invoke(nameof(Update));
            public void OnDestroy() => Invoke(nameof(OnDestroy));
            public void OnValidate() => Invoke(nameof(OnValidate));
            public void OnEnable() => Invoke(nameof(OnEnable));
            public void OnDisable() => Invoke(nameof(OnDisable));
            public void Reset() => Invoke(nameof(Reset));

            public void Invoke(string callback) => @object
                .GetType()
                .GetMethod(callback, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Invoke(@object, null);
        }
    }
}