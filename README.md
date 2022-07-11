# ECS

Custom Entity Component System architecture designed to work with "large" entities.

> **Warning**
>
> Package is in preview state.
> The API may change without advance notice.
> Production usage is not recommended.

**Package summary:**

- Skeleton of the ECS architecture model designed especially for large entities (e.g. entities which contain thousand of triangles etc). 
- Package forces to keep your logic and data separate and maintain the ECS design pattern.
- `Unity.Burst` friendly systems.
- Easily customizable engine. 
- Easily testable architecture.
- Auto-creation of components tuples for selected types.
- Basic implementations of the `World`, `Solver`, `Entity`, `BaseComponent`, `BaseSystem<T>`, `SolverJobsOrder`, `SolverActionsOrder`, and more. 
- *static*less features, all objects used in the engine are not static (except utils/extensions).

## Table of Contents

- [ECS](#ecs)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
  - [World](#world)
  - [Solver](#solver)
  - [Systems Manager](#systems-manager)
  - [Entities](#entities)
  - [Components](#components)
  - [Systems](#systems)
  - [Configurations](#configurations)
  - [Components tuples](#components-tuples)
  - [Roadmap v1.0.0](#roadmap-v100)
  - [Dependencies](#dependencies)

## Introduction

The package implements a custom approach to the [ECS design pattern](https://en.wikipedia.org/wiki/Entity_component_system).
ECS stands for **_e_**_ntity_ **_c_**_omponent_ **_s_**_ystem_.
In principle, the pattern is rather simple.
Entities contain components, components contain data, and systems modify the data in the components.
The key feature of the pattern is that logic and data are separated, i.e. all data can be found in components, and logic in systems.

This package was built as a core feature of the [PBD2D][pbd2d] engine.
It is designed to work with large entities, i.e. entities that hold a large amount of data.
For small entities up to a few bytes, I could recommend using the [`Unity.Entities`](https://docs.unity3d.com/Packages/com.unity.entities@0.50/manual/index.html).

## World

The main part of the framework is the `World`.
`World` can be considered as database, a container for all data injected into the simulation.
It contains information about registered components, set configurations, and enabled systems. 

```mermaid
%%{init: {"theme": "neutral", "flowchart": {"curve": "stepBefore", "useMaxwidth": false}}}%%

graph BT
subgraph w[World]
    a; b; c;
end

a[Components Registry]
b[Systems Registry]
c[Configurations Registry]
```

## Solver

At `Solver` one can configure the execution order of the systems by passing the proper `ScriptableObject` with configuration, namely `JobsOrder` and `ActionsOrder`.

```mermaid
%%{init: {"theme": "neutral", "flowchart": { "useMaxwidth": false}}}%%

graph TB

subgraph one[ ]
    direction TB
    w1[World] --> s[Solver] --> w1;
end

c[Jobs Order<br>Actions Order] --> s
```

`Solver` contains `OnScheduling` event, list of jobs[^jobs], and `OnJobsComplete` event, which are invoked every `Solver.Update()` in order, respectively

```mermaid
%%{init: {"theme": "neutral", "flowchart": { "useMaxwidth": false}}}%%

graph TB

subgraph one["Update()"]
    direction TB
    e1[OnScheduling] --> jobs[Jobs] --> e2[OnJobsComplete]
end
```

One can introduce custom hierarchy of the solver lifecycle by implementing `ActionsOrder` and `JobsOrder`.
The package contains default basic implementations:

- `DefaultActionsOrder` supporting solver action on scheduling/on jobs complete events selection.
- `DefaultJobsOrder` supporting linear ordering of the jobs.

Below there is a figure of `DefaultActionOrder` and `DefaultJobsOrder` editors.
Selected assets are directly fetched from test assembly.

![jobs-actions-order](Documentation~/jobs-actions-order-editors.png)

> **Note**
>
> More complex `JobsOrder` implementation can be found at [PBD2D][pbd2d].

## Systems Manager

System Manager is responsible for populating the `World.SystemRegistry` with all target system instances.
Default active status of given system type can be set there, as well as, given system status can be changed during runtime.

Below there is a figure of `SystemsManager` editor. Selected asset is directly fetched from test assembly.

![systems-manager](Documentation~/system-manager-editor.png)

## Entities

Entities are just `MonoBehaviour`s to which one attaches the components.
To derive new entity implement the abstract `Entity` class.

```csharp
public class MyEntity : Entity
{

}
```

```mermaid
%%{init: {"theme": "neutral", "flowchart": { "useMaxwidth": false}}}%%

graph TB
subgraph Entity
    c1[<b>C1</b> component];
    c2[<b>C2</b> component];
    c3[<b>C3</b> component];
end
```

Below there is a figure of `SystemsManager` editor. Selected asset is directly fetched from test assembly.

![entity](Documentation~/entity-editor.png)

> **Note**
> 
> `[RequireComponent(typeof(MyEntity))]` attribute can be used for marking the given component to be visible at given `Entity` editor.

## Components

The components shouldn't contain any logic by design.
Components should be treated as pure objects for holding the data and/or configuration.
The given components can be attached to selected entity types only.

To create new a component it is crucial to add a new component contract.
Introducing the contract is essential since system does not know any information about component implementations and it is required that system works on selected interface.

```csharp
public interface IMyComponent : IComponent
{

}
```

Then one has to implement the introduced interface.
`BaseComponent` class can be helpful, however, it is not necessary to use the class and the contract can be implemented on _pure C#_ (non-`MonoBehaviour`) class.

```csharp
[RequiredComponent(typeof(MyEntity))]
public class MyComponent : BaseComponent, IMyComponent
{

}
```

## Systems

All logic related to components data should be included in systems.
It is recommended to implement the abstract class `BaseSystem<T>` or `BaseSystemWithConfiguration<T, V>`, however,
one can implement a custom system by implementing the general `BaseSystem` class.

When one does not require some configuration, use `BaseSystem<T>` where `T` corresponds to the interface assigned from `IComponent`.
In the following snippet, the system schedules selected jobs on all  `IMyComponent`s objects from the `World` for which the system is attached to

```csharp
public class MySystem : BaseSystem<IMyComponent>
{
    public override JobHandle Schedule(JobHandle dependencies)
    {
        foreach(var component in References)
        {
            // ...
        }

        return dependencies
    }
}
```

Sometimes one needs to provide some global configurations to the system (e.g. gravity).
More information related to configurations can be found at [#Configurations](#configurations).
Assuming that `SimulationConfiguration` is defined,
the system can implement `BaseSystemWithConfiguration`

```csharp
public class MySystemWithConfiguration : BaseSystemWithConfiguration<IMyComponent, SimulationConfiguration>
{
    public override JobHandle Schedule(JobHandle dependencies)
    {
        foreach(var component in References)
        {
            var c = Configuration;
            // ...
        }

        return dependencies
    }
}
```

> **Note**
>
> Using `BaseSystemWithConfiguration` class is not required for accesing the configurations.
> One can find them through `World.ConfigurationsRegistry`.

```mermaid
%%{init: {"theme": "neutral", "flowchart": {"curve": "basis", "useMaxwidth": false}}}%%

graph LR
subgraph World
subgraph Entity
    c1[<b>C1</b> component];
    c2[<b>C2</b> component];
    c3[<b>C3</b> component];
end

    g[<b>G</b> configuration]
    s["System<<b>C, G</b>>"];
end

c2 --> s --> c2;
g --> s;
```

Except jobs, one can define actions inside systems as well, just by decorating the system's method with `SolverAction` attribute

```csharp
public class MySystem : BaseSystem
{
    [SolverAction]
    private void MyMethod()
    {

    } 

    // ...
}
```

## Configurations

Configurations are similar to components, but with restriction that only one instance of the given type of configuration can be present at `World.ConfigurationsRegistry`.
Configurations can be used for setting the global values related to the `World`, e.g. global gravity vector. 
Configuration must implements the `IConfiguration` interface, example

```csharp
[Serializable]
public class MyConfiguration : IConfiguration
{
    [field: SerializeField]
    public int Value { get; private set; } = 0;
}
```

Additionally, to instantiate configuration at scene, implement the corresponding holder


```csharp
public class MyConfigurationHolder : ConfigurationHolder<MyConfiguration> { }
```

Below one can find an example `ConfigurationHolder` editor.

![configuration-holder](Documentation~/configuration-holder-editor.png)

## Components tuples

Components can be matched into pairs by using tuples.
A component tuple is a virtual component that can be created automatically and does not live on the scene. 
It is useful in the cases when one needs to introduce some kind of interaction e.g. collisions.

```csharp
public IMyTuple : IComponent
{

}
```

```csharp
public MyTuple : ComponentsTuple<IMyComponent, IMyComponent>, IMyTuple
{
     protected override bool InstantiateWhen(IMyComponent c1, IMyComponent c2) => c1.Id != c2.Id;
}
```

Currently, the package supports only two argument tuples.

## Roadmap v1.0.0

- [X] ~~A few tests...~~
- [ ] `EntitiesRegistry`
- [ ] Actions only system impl.
- [ ] Basic CI config (add sample project first)
  - [ ] Tests
  - [ ] Coverage
  - [ ] Build
- [ ] Scheduling jobs from job.
- [ ] Jobs caching mechanism.
- [ ] (Optional:) Unify custom editors using USS sheets.

## Dependencies

- [`Unity.Collections`](https://docs.unity3d.com/Packages/com.unity.collections@1.2/manual/index.html)
- [`andywiecko.BurstCollections`](https://github.com/andywiecko/BurstCollections)
- [`Unity.Nuget.Newtonsoft-Json`](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@2.0/manual/index.html)

[pbd2d]:https://github.com/andywiecko/PBD2D

[^jobs]:More precisely list of type `List<Func<JobHandle, JobHandle>>`.