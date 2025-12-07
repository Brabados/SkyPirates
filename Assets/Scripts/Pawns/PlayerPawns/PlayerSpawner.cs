using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        SpawnPlayers();
    }
    private void SpawnPlayers()
    {
        List<PlayerPawns> Players = new List<PlayerPawns>();
        foreach (GameObject n in PlayerList.ListInstance.AllPlayerPawns)
        {
            GameObject Holder = Instantiate(n);
            PlayerPawns ToAdd = Holder.GetComponentInChildren<PlayerPawns>();
            Holder.SetActive(true);
            Players.Add(ToAdd);
        }
        PawnManager.PawnManagerInstance.populatePlayer(Players);
    }
}
