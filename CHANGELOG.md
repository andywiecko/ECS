# Change log

## [0.2.1] – 2022-12-07

### Added

- USS file for ECS custom editors (chore changes, does not effect on the features).

### Fixed

- Fix auto reference the `World` component in `ConfigurationHolder<T>` and `Entity` types.
- Fix `EnityEditor`. Editor implementation has been simplified and bug related with displaying abstract classes is eliminated.
Additionally editor supports the `[DisallowMultipleComponent]` attribute properly.
- Fix component cache utils bug for inheritated types.

## [0.2.0] – 2022-09-08

### Added

- Added `ConfigurationRegistry.GetOrCreate<T>()` method, which resolve issue of missing configurations on scene.

### Fixed

- Component `EntityId` may be invalid due to `BaseComponent.Awake()` may be called before `Entity.Awake()`. It has been fixed.
- Editor glitches and issues related to `EntityEditor` and `SystemsManagerEditor`.

### Changed

- Update `BurstCollections` package version.
- Style: removing redundant usings.

## [0.1.0] – 2022-06-11

### Features

- Initial package upload. The first version contains implementation of the basic package components:
  - `World` with corresponding registries,
  - `Entity`,
  - `BaseComponent`,
  - `ComponentsTuple`,
  - `ConfigurationHolder`,
  - `BaseSystem` (and it's variants),
  - `Solver`,
  - `SystemsManager`,
  - Default implementations of `JobsOrder` and `ActionsOrder`.
