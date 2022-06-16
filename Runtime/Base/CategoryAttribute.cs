using System;

namespace andywiecko.ECS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CategoryAttribute : Attribute
    {
        public string Name { get; }
        public CategoryAttribute(string name) => Name = name;
    }
}