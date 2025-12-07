using System.Collections.Generic;
using UnityEngine;

public enum EType
{
    Corrsair,
    Cannoneir,
    Rifleman
}

public class EnemyList : MonoBehaviour
{

    public static EnemyList ListInstance { get; private set; }

    public List<GameObject> AllEnemies;

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
        }
        DontDestroyOnLoad(this);
    }
}
