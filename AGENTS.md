
# Guidelines for Automated Contributions

This repository is a Unity project targeting **Unity 2020.3.14f1**. The project
uses [Git LFS](https://git-lfs.github.com/) to track most Unity assets and has
continuous integration configured through GitHub Actions.

## Coding style
- C# scripts live under `Assets/Scripts`. Indent using four spaces.
- Public methods and classes use **PascalCase**. Private fields use `camelCase`.
- Include summary XML comments where possible. Tests contain explanatory
  comments to help new contributors.

## Tests
- Unit tests reside in `Assets/Tests` and are split into *EditMode* and
  *PlayMode* assemblies. Each has an `.asmdef` file referencing `GameCore`.
- When adding new tests, ensure the corresponding `.asmdef` references remain
  valid and that the `.meta` files are committed.
- Tests use NUnit's `[Test]` attribute. See `RangeCalculatorTests.cs` for
  examples.
- The GitHub workflow `unity-ci.yml` builds the project and runs play mode
  tests on pull requests.

## Assets
- The `.gitattributes` file configures Git LFS for Unity asset types such as
  `*.prefab`, `*.unity`, `*.png`, `*.exr` and `*.fbx`.
- Commit the `.meta` file whenever an asset is added or moved.

## Commit messages
- Keep messages short and descriptive (e.g. "Extract merge layout logic").
- Include a brief summary of the change; additional details can be placed in the
  body if needed.

## Recommendations
- Run the Unity Test Runner before submitting changes. The CI workflow mirrors
  this process.
- Avoid large, unnecessary scene or prefab changes to keep diffs readable.
- If you modify assembly definitions or introduce new assemblies, update the
  corresponding `.asmdef` files so tests continue to compile.

# SkyPirates Coding Agent Guide

This repository contains a Unity 2020.3.14f1 project.  The project now includes
unit tests and CI which means even small changes can break the build.  Follow
these conventions when contributing with an AI coding agent.

## General Guidelines

* **Unity version**: use Unity **2020.3.14f1**.  The CI workflow assumes this
  version.
* **.meta files**: Every asset added or moved must include the accompanying
  `.meta` file.  Unity will not recognise assets without them.
* **Binary assets**: Large Unity assets (`*.png`, `*.prefab`, `*.unity`, etc.)
  are tracked with Git LFS via `.gitattributes`.  Do not attempt to diff these
  manually.
* **Assembly definitions**: Scripts are organised using `.asmdef` files.
  When adding new folders of scripts or tests, ensure they are part of the
  correct assembly and commit the `.asmdef` and `.asmdef.meta` files.
* **Commit messages**: keep them short and imperative (e.g. "Add unit tests"),
  mirroring existing history.

## C# Style

* Indent with four spaces.  Opening braces go on a new line.
* Classes and methods use PascalCase.  Private fields commonly start with an
  underscore (`_gameBoard`).
* Use XML documentation comments where helpful; recent commits added extensive
  comments to make behaviour clear for agents.
* Prefer pure static methods for algorithmic code (see `RangeCalculator`) so
  they can be tested without scenes.

## Tests

* Tests live under `Assets/Tests` and are split into `EditMode` and `PlayMode`
  directories.  Use NUnit's `[Test]` or `[UnityTest]` attributes and follow the
  style of existing tests such as
  `Assets/Tests/PlayMode/MapGetHexPositionTests.cs`.
* When adding tests, also update or create any required `.asmdef` files so the
  test assemblies compile.  A missing `.asmdef` caused build errors which were
  fixed in commit `a99ef7a`.
* The GitHub Actions workflow (`.github/workflows/unity-ci.yml`) builds the
  project and runs the tests for every pull request.  Ensure new tests pass
  locally before committing if possible.

## Additional Notes

* Keep changes to Unity scene files small.  Large diffs in `.unity` files are
  difficult to review and may be rejected.
* Refer to `README.md` for details on running the project and tests.


