# ECS

Custom Entity Component System architecture designed to work with "large" entities.

## Package summary

- Skeleton of the ECS architecture model designed especially for large entities (e.g. entities which contains thousand of triangles etc). 
- Package forces to keep your logic and data separate and maintain the ECS design pattern.
- `Unity.Burst` friendly systems.
- Easily customizable engine. 
- Easily testable architecture.
- Auto-creation of components tuples for selected types.
- Basic implementations of the `World`, `Solver`, `Entity`, `BaseComponent`, `BaseSystem<T>`, `SolverJobsOrder`, `SolverActionsOrder`, and more. 
- *static*less features, all objects used in the engine are not static (except utils/extensions).

**TODO:** table of contents here?

## Introduction

The package implements custom approach to the [ECS design pattern](https://en.wikipedia.org/wiki/Entity_component_system).
ECS stands for **_e_**_ntity_ **_c_**_omponent_ **_s_**_ystem_.
In principle the pattern is rather simple.
Entities contain components, components contain data, and systems modify the data in the components.
The key feature of the pattern is that logic and data are separated, i.e. all data can be found at components, and logics at systems.

This package was build as a core feature of the [PBD2D][pbd2d] engine. **TODO:** ADD FOOTNOTE HERE
It is designed to work with large entities, i.e. entities which holds large amount of data.
For small entities up to a few bytes, I could recommend using the `Unity.Entities`.

## World

The main part of the framework is the `World`.
`World` can be considered as data base, a container for all data injected into the simulation.
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

At `Solver` one can configure execution order of the systems by passing the proper `ScriptableObject` with configuration, namely `SolverJobsOrder` and `SolverActionsOrder`.
There are available default implementations of those, however, 
one can find more complex example at [PBD2D][pbd2d]

```mermaid
%%{init: {"theme": "neutral", "flowchart": { "useMaxwidth": false}}}%%

graph TB

subgraph one[ ]
    direction TB
    w1[World] --> s[Solver] --> w1;
end

c[Jobs Order<br>Actions Order] --> s
```

- `Entity`: `MonoBehaviour` for which one attaches components.
- `BaseComponent`: component where the data related to `Entity` live.
- `BaseSystem<T>`: grabs all the objects of type `T` from the `World` and perform selected operation with this. 
- `IConfiguration`: implement the interface and add "global" configuration of your `World`.
- `ComponentsTuple<T1, T2>`, i.e. component which is automagicaly created/destroyed if certain components of type `T1`, `T2` are added to the `World`


## Entities

Entities are just `MonoBehaviour`s to which one attaches the components.
To derive new entity implement abstract `Entity` class.

```mermaid
%%{init: {"theme": "neutral", "flowchart": { "useMaxwidth": false}}}%%

graph TB

e[Entity] --> c1[Component A];
e --> c2[Component B];
e --> c3[Component C];

```

## Components

## Systems

## Configurations

## Components tuples

## Roadmap

### v1.0.0

- [ ] Defaults impl of the scriptable objects.
- [ ] Editors utilities and editors for base components.

### v2.0.0

- [ ] Scheduling jobs from job.

## Dependencies

[pbd2d]:https://github.com/andywiecko/PBD2D
