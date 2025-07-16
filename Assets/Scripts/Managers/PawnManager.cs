using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnManager : MonoBehaviour
{
    public static PawnManager PawnManagerInstance { get; private set; }
    public List<PlayerPawns> PlayerPawns;
    public List<EnemyPawn> EnemyPawns;
    private bool TurnAssign = false;

    public List<EnemyPawn> GetAllEnemies()
    {
        return EnemyPawns;
    }

    public int getPlayerPawnPosition(GameObject Pawn)
    {
        int count = 0;
        foreach(PlayerPawns n in PlayerPawns)
        {
            if(n == Pawn)
            {
                return count;
            }
            count++;
        }

        return -1;
    }

    public int getEnemyPawnPosition(EnemyPawn Pawn)
    {
        int count = 0;
        foreach (EnemyPawn n in EnemyPawns)
        {
            if (n == Pawn)
            {
                return count;
            }
            count++;
        }

        return -1;
    }

    public PlayerPawns getPlayerPawn(int Index)
    {
        return PlayerPawns[Index];
    }

    public EnemyPawn getEnemyPawn(int Index)
    {
        return EnemyPawns[Index];
    }

    public Pawn getPawn(GameObject Pawn)
    {
        foreach(PlayerPawns n in PlayerPawns)
        {
            if(n == Pawn)
            {
              //  return n;
            }
        }
        foreach(EnemyPawn n in EnemyPawns)
        {
            if(n == Pawn)
            {
                return n;
            }    
        }

        return null;
    }    

    public int GetNextPlayerPawnByObject(GameObject Index)
    {
        int count = 0;
        foreach(PlayerPawns n in PlayerPawns)
        {
            if (n == Index)
            {
                if(count == PlayerPawns.Count)
                {
                    return 0;
                }
                else
                {
                    return count;
                }
            }
        }
        return -1;
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (PawnManagerInstance != null && PawnManagerInstance != this)
        {
            Destroy(this);
        }
        else
        {
            PawnManagerInstance = this;
        }
        EnemyPawns = new List<EnemyPawn>();
    }

    public void populateEnemey(List<EnemyPawn> Enemy)
    {
        foreach(EnemyPawn n in Enemy)
        {
            EnemyPawns.Add(n);
        }
        if(TurnAssign)
        {
            TurnManager.Instance.SetTurns();
        }
        else { TurnAssign = true; }
    }

    public void populatePlayer(List<PlayerPawns> Player)
    {
        foreach(PlayerPawns n in Player)
        {
            PlayerPawns.Add(n);
        }
        if (TurnAssign)
        {
            TurnManager.Instance.SetTurns();
        }
        else { TurnAssign = true; }
    }

    public void clearPawns()
    {
        PlayerPawns.Clear();
        EnemyPawns.Clear();
    }
}
