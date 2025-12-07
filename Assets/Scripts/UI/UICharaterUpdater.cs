using UnityEngine;
using UnityEngine.UI;

public class UICharaterUpdater : MonoBehaviour
{
    public Canvas UI;
    public Image CharaterProt;
    public Text CharaterName;
    public Text CharaterHealthNO;
    public Slider CharaterHelthVisual;
    public AttackButton attack;


    public void Start()
    {
        EventManager.OnUIUpdate += UpdateUI;
    }

    public void UpdateUI(Pawn Selected)
    {
        UI.gameObject.SetActive(true);
        PlayerPawns Player;
        if (Selected is PlayerPawns)
        {
            Player = (PlayerPawns)Selected;

            CharaterProt.sprite = Player.Data.CharaterPortrate;
            CharaterName.text = Player.Data.CharaterName;
            attack.BasicAttack = Player.Equiped.Weapon.BaseAttack;
        }
    }

    public void OnDestroy()
    {
        EventManager.OnUIUpdate -= UpdateUI;
    }
}
