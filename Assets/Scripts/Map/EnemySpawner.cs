using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public EnemyPackSO ToSpawn;

    public void Awake()
    {
        ToSpawn = PawnManager.PawnManagerInstance.Enimies;
    }

    // Start is called before the first frame update
    public void Start()
    {
        SpawnEnemies();

    }

    private void SpawnEnemies()
    {
        List<EnemyPawn> Enemies = new List<EnemyPawn>();
        int count = 0;
        if (ToSpawn == null)
        {
            return;
        }
        foreach (EType n in ToSpawn.EnemyType)
        {

            for (int x = 0; x < ToSpawn.NumberInPack[count]; x++)
            {
                GameObject Holder = Instantiate(EnemyList.ListInstance.AllEnemies[((int)n)]);
                EnemyPawn ToAdd = Holder.GetComponent<EnemyPawn>();
                Enemies.Add(ToAdd);
            }
            count++;
        }
        PawnManager.PawnManagerInstance.populateEnemey(Enemies);
    }
}
