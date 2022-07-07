using UnityEditor;
using UnityEngine;

namespace andywiecko.ECS.Editor
{
    public static class DefaultComponentsEditor
    {
        private const string path = "GameObject/ECS/";

        [MenuItem(path + nameof(World))]
        public static void CreateWorld() => Create<World>();

        [MenuItem(path + nameof(Solver))]
        public static void CreateSolver() => Create<Solver>();

        [MenuItem(path + nameof(SystemsManager))]
        public static void CreateSystemsManager() => Create<SystemsManager>();

        private static void Create<T>() where T : MonoBehaviour
        {
            Selection.activeObject = new GameObject(typeof(T).Name).AddComponent<T>();
            EditorApplication.delayCall += subscribe;

            static void subscribe()
            {
                EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
                EditorApplication.delayCall -= subscribe;
            }
        }
    }
}