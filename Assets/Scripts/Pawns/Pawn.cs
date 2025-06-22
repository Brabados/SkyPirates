using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Attributes))]
public abstract class Pawn : MonoBehaviour
{
    public CapsuleCollider BaseSize;
    public Tile Position;
    public Attributes Stats;
    public EquipedItems Equiped;
    public void Awake()
    {
        BaseSize = GetComponent<CapsuleCollider>();
        Stats = GetComponent<Attributes>();
        Equiped = GetComponent<EquipedItems>();
    }

    public void SetPosition(Tile Set)
    {
        Position = Set;
        float height = Set.transform.position.y + (Set.Height / 2) + ((this.transform.localScale.y * BaseSize.height) / 2);
        transform.position = new Vector3(Set.transform.position.x, height, Set.transform.position.z);
        Set.Contents = this;
    }


}
