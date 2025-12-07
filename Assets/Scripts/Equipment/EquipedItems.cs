using System.Collections.Generic;
using UnityEngine;

public class EquipedItems : MonoBehaviour
{
    public HeadItem Head;
    public WeaponItem Weapon;
    public BodyItem Body;
    public AccessoriesItem Accessorie;
    public FeetItem Feet;
    public int chuzpah;
    public int cadishness;
    public int grace;
    public int grit;
    public int serindipity;
    public int swagger;

    public List<Item> Equipment = new List<Item>(5);
    public bool Onscreen;

    void Start()
    {
        EventManager.OnInfoCompare += Compare;
        populateEquipment();
        SetStats();
    }

    public void populateEquipment()
    {
        if (Equipment.Count < 5)
        {
            Equipment.Add(Head);
            Equipment.Add(Body);
            Equipment.Add(Weapon);
            Equipment.Add(Feet);
            Equipment.Add(Accessorie);
        }
    }

    public void UpdateEquipment(ItemType WhatToCahnge, Item newItem)
    {

        switch (WhatToCahnge)
        {
            case ItemType.Head:
                Equipment.Remove(Head);
                Head = (HeadItem)newItem;
                Equipment.Add(Head);
                break;
            case ItemType.Body:
                Equipment.Remove(Body);
                Body = (BodyItem)newItem;
                Equipment.Add(Body);
                break;
            case ItemType.Weapon:
                Equipment.Remove(Weapon);
                Weapon = (WeaponItem)newItem;
                Equipment.Add(Weapon);
                break;
            case ItemType.Feet:
                Equipment.Remove(Feet);
                Feet = (FeetItem)newItem;
                Equipment.Add(Feet);
                break;
            case ItemType.Accessory:
                Equipment.Remove(Accessorie);
                Accessorie = (AccessoriesItem)newItem;
                Equipment.Add(Accessorie);
                break;
        }

        SetStats();
    }

    public void Compare(Item toCompare)
    {
        if (!Onscreen)
        {
            return;
        }

        Item inventoryItem = null;

        switch (toCompare.Type)
        {
            case ItemType.Head:
                inventoryItem = Head;
                break;
            case ItemType.Body:
                inventoryItem = Body;
                break;
            case ItemType.Weapon:
                inventoryItem = Weapon;
                break;
            case ItemType.Feet:
                inventoryItem = Feet;
                break;
            case ItemType.Accessory:
                inventoryItem = Accessorie;
                break;
        }

        if (inventoryItem == null)
        {
            Debug.LogWarning("No matching equipped item found for comparison.");
            return;
        }

        int[] compareStats = new int[6];

        for (int i = 0; i < compareStats.Length; i++)
        {
            if (toCompare.StatChanges[i] > inventoryItem.StatChanges[i])
                compareStats[i] = 1;
            else if (toCompare.StatChanges[i] < inventoryItem.StatChanges[i])
                compareStats[i] = -1;
            // else leave as 0
        }

        EventManager.TriggerInfoCompareChange(toCompare, compareStats);
    }

    public void OnDestroy()
    {
        EventManager.OnInfoCompare -= Compare;
    }

    public void SetStats()
    {
        int[] stats = new int[6];

        Item[] items = { Head, Body, Weapon, Feet, Accessorie };
        foreach (Item item in items)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] += item.StatChanges[i];
            }
        }

        chuzpah = stats[0];
        cadishness = stats[1];
        grace = stats[2];
        grit = stats[3];
        serindipity = stats[4];
        swagger = stats[5];
    }
}