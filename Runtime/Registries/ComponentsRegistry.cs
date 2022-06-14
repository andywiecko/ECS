using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace andywiecko.ECS
{
    public class ComponentsRegistry
    {
        public IdCounter<IComponent> Counter { get; } = new();

        private readonly Dictionary<Type, IList> components = new();
        private readonly Dictionary<Type, Action<object>> onAddActions = new();
        private readonly Dictionary<Type, Action<object>> onRemoveActions = new();
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Type>> typeToInterfaces;

        public ComponentsRegistry(IEnumerable<Type> types)
        {
            typeToInterfaces = GetTypeInterfacesMapping(types)
                .ToDictionary(i => i.type, i => i.interfaces);

            var componentInterfaces = typeToInterfaces.Values.SelectMany(i => i).Distinct();
            components = componentInterfaces.ToDictionary(i => i, i => CreateListOf(i));
            onAddActions = componentInterfaces.ToDictionary(i => i, i => default(Action<object>));
            onRemoveActions = componentInterfaces.ToDictionary(i => i, i => default(Action<object>));

            var tupleTypes = GetTuple2Types(types);
            foreach (var (t, t1, t2) in tupleTypes)
            {
                SubscribeOnAdd(t1, (object i) => OnAddComponentItem1(i, t, t2));
                SubscribeOnAdd(t2, (object i) => OnAddComponentItem2(i, t, t1));
            }

            static IList CreateListOf(Type t) => Activator.CreateInstance(typeof(List<>).MakeGenericType(t)) as IList;

            static IEnumerable<(Type type, IReadOnlyList<Type> interfaces)> GetTypeInterfacesMapping(IEnumerable<Type> types) =>
                types.Select(t => (t, t
                    .GetInterfaces()
                    .Where(i => typeof(IComponent).IsAssignableFrom(i) && i.ContainsGenericParameters == false)
                    .ToArray() as IReadOnlyList<Type>)
            );

            static IEnumerable<(Type t, Type t1, Type t2)> GetTuple2Types(IEnumerable<Type> types) =>
                types.Where(i => typeof(ComponentsTuple).IsAssignableFrom(i) && i.IsAbstract == false)
                    .Select(i =>
                    {
                        var args = i.BaseType.GetGenericArguments();
                        return (i, args[0], args[1]);
                    });
        }

        public ComponentsRegistry(IEnumerable<Assembly> assemblies) : this(types: assemblies.SelectMany(i => i.GetTypes())) { }
        public ComponentsRegistry() : this(assemblies: AppDomain.CurrentDomain.GetAssemblies()) { }

        public IReadOnlyList<T> GetComponents<T>() where T : IComponent => components[typeof(T)] as List<T>;
        public IEnumerable GetComponents(Type type) => components[type];

        public void Add<T>(T instance) where T : IComponent
        {
            var type = instance.GetType();
            foreach (var @interface in typeToInterfaces[type])
            {
                components[@interface].Add(instance);
                onAddActions[@interface]?.Invoke(instance);
            }
        }

        public void Remove<T>(T instance) where T : IComponent
        {
            var type = instance.GetType();
            foreach (var @interface in typeToInterfaces[type])
            {
                components[@interface].Remove(instance);
                onRemoveActions[@interface]?.Invoke(instance);
            }
        }

        public void SubscribeOnAdd<T>(Action<object> fun) where T : IComponent => SubscribeOnAdd(typeof(T), fun);
        public void UnsubscribeOnAdd<T>(Action<object> fun) where T : IComponent => UnsubscribeOnAdd(typeof(T), fun);
        public void SubscribeOnAdd(Type type, Action<object> fun) => onAddActions[type] += fun;
        public void UnsubscribeOnAdd(Type type, Action<object> fun) => onAddActions[type] -= fun;
        public void SubscribeOnRemove<T>(Action<object> fun) where T : IComponent => SubscribeOnRemove(typeof(T), fun);
        public void UnsubscribeOnRemove<T>(Action<object> fun) where T : IComponent => UnsubscribeOnRemove(typeof(T), fun);
        public void SubscribeOnRemove(Type type, Action<object> fun) => onRemoveActions[type] += fun;
        public void UnsubscribeOnRemove(Type type, Action<object> fun) => onRemoveActions[type] -= fun;

        private void OnAddComponentItem1(object item1, Type t, Type t2)
        {
            foreach (var item2 in GetComponents(t2))
            {
                Activator.CreateInstance(t, item1, item2, this);
            }
        }

        private void OnAddComponentItem2(object item2, Type t, Type t1)
        {
            foreach (var item1 in GetComponents(t1))
            {
                Activator.CreateInstance(t, item1, item2, this);
            }
        }
    }
}