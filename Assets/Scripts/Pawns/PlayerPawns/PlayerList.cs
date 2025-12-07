using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
    public static PlayerList ListInstance { get; private set; }

    public List<GameObject> PrefabPawns;

    public List<GameObject> AllPlayerPawns;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (ListInstance != null && ListInstance != this)
        {
            Destroy(this);
        }
        else
        {
            ListInstance = this;
            foreach (GameObject x in PrefabPawns)
            {
                var pawn = Object.Instantiate(x, Vector3.zero, Quaternion.identity) as GameObject;
                if (AllPlayerPawns.Count < PrefabPawns.Count)
                {
                    pawn.GetComponent<Pawn>().Equiped.populateEquipment();
                    AllPlayerPawns.Add(pawn);
                    pawn.SetActive(false);
                    DontDestroyOnLoad(pawn);
                }
            }
        }
        DontDestroyOnLoad(this);
    }


}
