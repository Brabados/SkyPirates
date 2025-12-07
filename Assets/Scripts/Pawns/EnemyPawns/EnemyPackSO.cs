using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObject/EnemyPack")]
public class EnemyPackSO : ScriptableObject
{

    public List<int> NumberInPack;

    public List<EType> EnemyType;

}
