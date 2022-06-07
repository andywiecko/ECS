using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class SolverActionsOrder : ScriptableObject
    {
        public abstract void GenerateActions(Solver solver, World world);
    }
}