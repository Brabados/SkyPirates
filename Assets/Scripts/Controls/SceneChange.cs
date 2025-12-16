using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChange : MonoBehaviour
{
    private BasicControls inputActions;
    public DialogueSystem Test;
    // Start is called before the first frame update
    void Start()
    {
        inputActions = EventManager.EventInstance.inputActions;
        string name = SceneManager.GetActiveScene().name;
        if (name == "CharacterScene")
        {
            inputActions.Battle.Disable();
            inputActions.Menu.Enable();


        }
        if (name == "BattleScene")
        {
            inputActions.Battle.Enable();
            inputActions.Menu.Disable();

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.Battle.SceneSwitch.triggered || inputActions.Menu.SceneSwitch.triggered)
        {
            string name = SceneManager.GetActiveScene().name;
            if (name == "BattleScene")
            {
                SceneLoader.UnloadBattleScene();

            }
            if (name == "CharacterScene")
            {
                inputActions.Battle.Enable();
                inputActions.Menu.Disable();
                SceneManager.LoadScene("BattleScene");
            }
        }
        else if(inputActions.Menu.Dialogue.triggered)
        {
            inputActions.Menu.Disable();
            inputActions.Dialouge.Enable();
            Test.ShowExampleDialogue();
        }
        else if(inputActions.Dialouge.SwitchAction.triggered)
        {
            inputActions.Menu.Enable();
            inputActions.Dialouge.Disable();
        }
    }
}
