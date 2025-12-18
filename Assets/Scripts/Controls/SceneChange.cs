using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChange : MonoBehaviour
{
    private BasicControls inputActions;
    public ConversationManager test;
    public Conversation testConvo;
    public EncyclopediaPanel testClose;
    public DialogueSystem testDiag;
    public GameObject menues;
    // Start is called before the first frame update
    void Start()
    {
        inputActions = EventManager.EventInstance.inputActions;
        string name = SceneManager.GetActiveScene().name;
        if (name == "CharacterScene")
        {
            inputActions.Battle.Disable();
            inputActions.Menu.Enable();
            inputActions.OverWorld.Disable();
            inputActions.Dialouge.Disable();

        }
        if (name == "BattleScene")
        {
            inputActions.Battle.Enable();
            inputActions.Menu.Disable();
            inputActions.OverWorld.Disable();
            inputActions.Dialouge.Disable();
        }
        if (name == "OverWorld")
        {
            inputActions.Battle.Disable();
            inputActions.Menu.Disable();
            inputActions.OverWorld.Enable();
            inputActions.Dialouge.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        string name = SceneManager.GetActiveScene().name;
        if (inputActions.Battle.SceneSwitch.triggered || inputActions.Menu.SceneSwitch.triggered)
        {
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
        else if (inputActions.Menu.Dialogue.triggered || inputActions.Battle.DialogeStart.triggered || inputActions.OverWorld.DioalogeStart.triggered)
        {
            inputActions.Menu.Disable();
            inputActions.Battle.Disable();
            inputActions.OverWorld.Disable();
            inputActions.Dialouge.Enable();
            test.StartConversation(testConvo);
            if (menues != null)
            {
                menues.SetActive(false);
            }
        }
        else if (inputActions.Dialouge.SwitchAction.triggered)
        {
            test.EndConversation();
        }

        if(inputActions.Dialouge.Advance.triggered)
        {
            test.NextSegment();
        }
        if(inputActions.Dialouge.Devance.triggered)
        {
            test.PreviousSegment();
        }
        if(inputActions.Dialouge.SelectTerm.triggered && testDiag.HasSelectableTerms())
        {
            testDiag.OpenCurrentTerm();
        }
        if (inputActions.Dialouge.CloseTerm.triggered)
        {
            testClose.ClosePanel();
        }
        if (inputActions.Dialouge.NextTerm.triggered && testDiag.HasSelectableTerms())
        {
            testDiag.CycleTermForward();
        }
        if (inputActions.Dialouge.PreviousTerm.triggered && testDiag.HasSelectableTerms())
        {
            testDiag.CycleTermBackward();
        }
    }

    public void endconverstaion()
    {
        string name = SceneManager.GetActiveScene().name;
        inputActions.Dialouge.Disable();
        if (menues != null)
        {
            menues.SetActive(true);
        }
        if (name == "BattleScene")
        {
            inputActions.Battle.Enable();
        }
        else if (name == "CharacterScene")
        {
            inputActions.Menu.Enable();
        }
        else if (name == "OverWorld")
        {
            inputActions.OverWorld.Enable();
        }
    }
}
