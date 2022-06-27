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

            public void Awake() => Invoke("Awake");
            public void Start() => Invoke("Start");
            public void Update() => Invoke("Update");
            public void OnDestroy() => Invoke("OnDestroy");

            public void Invoke(string callback) => monoBehaviour
                .GetType()
                .GetMethod(callback, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Invoke(monoBehaviour, null);
        }
    }
}