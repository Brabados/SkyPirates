using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum Charater
{
    Kai,
    MC,
    Terra,
    Urst,
    Last,
    Manahli,
    Adine
}

[CreateAssetMenu(fileName = "CharaterData", menuName = "ScriptableObject/CharaterData")]
public class CharaterData : BaseScriptableObject
{

    public string CharaterName;

    public Sprite CharaterPortrate;

    public Charater WhoIs;
 
}
