# Change log

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
