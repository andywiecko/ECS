using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class ActionsOrder : ScriptableObject
    {
        public abstract void GenerateActions(ISolver solver, IWorld world);
    }
}