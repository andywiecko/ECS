using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS.Editor.Tests
{
    public static class TestsExtensions
    {
        public static UnityCallbacks InvokeUnityCallback(this MonoBehaviour monoBehaviour)
        {
            return new UnityCallbacks(monoBehaviour);
        }

        public class UnityCallbacks
        {
            private readonly MonoBehaviour monoBehaviour;

            public UnityCallbacks(MonoBehaviour monoBehaviour) => this.monoBehaviour = monoBehaviour;

            public void Awake() => Invoke(nameof(Awake));
            public void Start() => Invoke(nameof(Start));
            public void Update() => Invoke(nameof(Update));
            public void OnDestroy() => Invoke(nameof(OnDestroy));
            public void OnValidate() => Invoke(nameof(OnValidate));
            public void OnEnable() => Invoke(nameof(OnEnable));
            public void OnDisable() => Invoke(nameof(OnDisable));

            public void Invoke(string callback) => monoBehaviour
                .GetType()
                .GetMethod(callback, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Invoke(monoBehaviour, null);
        }
    }
}