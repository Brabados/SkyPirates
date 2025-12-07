using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static bool isLoading;

    private static List<System.Type> DOTSSystems = new() { typeof(OptimizedFlockingBehaviorSystem) };

    public static void LoadBattleScene()
    {
        if (isLoading) return;
        isLoading = true;

        // Start async additive load
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive)
            .completed += (op) =>
            {
                // Set the newly loaded scene active
                Scene battleScene = SceneManager.GetSceneByName("BattleScene");
                SceneManager.SetActiveScene(battleScene);

                // Disable overworld root objects
                PauseScene("OverWorld", true);
                ToggleSystems(DOTSSystems, false);

                // Hide boids + pause flocking
                BoidSceneController.HideBoidsAndPause();
                EventManager.EventInstance.inputActions.Battle.Enable();
                EventManager.EventInstance.inputActions.OverWorld.Disable();
                isLoading = false;
            };
    }

    public static void UnloadBattleScene()
    {
        SceneManager.UnloadSceneAsync("BattleScene")
            .completed += (op) =>
            {
                // Reactivate overworld
                PauseScene("OverWorld", false);
                ToggleSystems(DOTSSystems, true);

                // Restore boids + resume flocking
                BoidSceneController.ShowBoidsAndResume();

                Scene overworld = SceneManager.GetSceneByName("OverWorld");
                SceneManager.SetActiveScene(overworld);

                // Reset input
                EventManager.EventInstance.inputActions.Battle.Disable();
                EventManager.EventInstance.inputActions.OverWorld.Enable();
            };
    }

    private static void PauseScene(string sceneName, bool pause)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded) return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            root.SetActive(!pause);
        }
    }

    private static void ToggleSystems(List<System.Type> systems, bool enable)
    {
        World world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        foreach (var systemType in systems)
        {
            var system = world.GetExistingSystemManaged(systemType);
            if (system != null)
                system.Enabled = enable;
        }
    }
}
