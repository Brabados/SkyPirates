using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public List<TurnToken> TurnOrder = new List<TurnToken>();
    public int TurnTime = 200;
    public TurnToken currentTurn;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public void SetTurns()
    {
        foreach (Pawn a in PawnManager.PawnManagerInstance.PlayerPawns)
        {
            TurnOrder.Add(a.Turn);
        }
        foreach (Pawn b in PawnManager.PawnManagerInstance.EnemyPawns)
        {
            TurnOrder.Add(b.Turn);
        }
        Debug.Log(TurnOrder.Count);
    }

    public Pawn UpdateTurn()
    {

        TurnOrder.Sort();

        foreach(TurnToken a in TurnOrder)
        {
            if(a.TurnCounter >=  TurnTime)
            {
                a.TurnCounter = 0;
                currentTurn = a;
                return a.Owner;
            }

            a.TurnCounter += a.InitativeChange();
        }
        return null;
    }
}
