using System;

namespace andywiecko.ECS
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SolverActionAttribute : Attribute { }
}