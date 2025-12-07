# SkyPirates

SkyPirates is a Unity project for a tactical, hex-grid based game where you build and command sky ships.

## Project Goals
- Provide a ship building interface so players can design their own sky ships.
- Engage in hex-based tactical battles between ships.
- Allow editing and management of characters and ships between battles.

## Getting Started
1. Install **Unity 2022.3.62f1**. The project was created with this LTS version and should be opened with the same editor version for best compatibility.
2. Clone or download this repository.
3. Open the project folder in the Unity Hub and launch it with `2022.3.62f1`.

### Dependencies
The project relies on Unity packages listed in `Packages/manifest.json`. Notable dependencies include:
- `com.unity.inputsystem` for handling player input.
- `com.unity.textmeshpro` for UI text.
- `com.unity.entities` for ECS text.
- `com.unity.burst' for Unity's DOTS and burst compiling
- `com.unity.nuget.newtonsoft-json` (Newtonsoft JSON) for save/load serialization.
- `com.unity.splines' for quick and simple slines
Unity will automatically install these packages when the project is opened.
 

## Running the Game
After opening the project in the Unity editor, load one of the available scenes from the **Assets/Scenes** folder:
- **BattleScene** – the main tactical combat scene.
- **ShipBuildScreen** – scene used for designing ships.
- **CharaterScene** – an additional scene for character-related features.
Press **Play** in the Unity editor to run the selected scene.

## Building
To create a standalone build:
1. Open `File > Build Settings…` in Unity.
2. Add the scenes you want included in the build (e.g., `BattleScene`, `ShipBuildScreen`).
3. Choose your target platform and click **Build**.

## Testing
This project uses Unity's built-in Test Runner. You can run the Edit Mode tests
from the Unity editor:

1. Open the **Test Runner** window via **Window > General > Test Runner**.
2. Select **EditMode** and click **Run All** to execute the unit tests located
   under `Assets/Tests`.

These tests cover parts of the hex range calculation logic and help verify that
the algorithms work as expected. Running them regularly prevents subtle bugs
from creeping in as you make changes.

Tests are written just like regular C# code using NUnit's `[Test]` attribute.
Good tests document how a method should behave, guard against regressions and,
when possible, act as small examples for new contributors. When a bug is found
it's helpful to first create a failing test that demonstrates the issue – once
the bug is fixed the failing assertion serves as proof and can be removed or
updated.
