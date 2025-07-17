using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnToken : IComparable
{
    public Pawn Owner;
    private float graceWeight = 1.2f;
    private float cadishWeight = 0.7f;
    private float speedWeight = 1.5f;
    private int _Grace, _Speed, _Cadishness, _Serindipity;
    public float TurnCounter;
    public bool ActionTaken = false;
    public int MovementLeft;
    public int FullMovement;

    public TurnToken(Pawn owner, int grace, int speed, int cadishness, int serindipity)
    {
        Owner = owner;
        _Grace = grace;
        _Speed = speed;
        _Cadishness = cadishness;
        _Serindipity = serindipity;
        FullMovement = Owner.Stats.Movement;
        MovementLeft = FullMovement;
    }

    public float InitativeChange()
    {
        // Grace: uses a sigmoid-like curve to model plateau and diminishing returns
        float graceCurve = (float)(graceWeight * (_Grace / (5.0f + Mathf.Abs(_Grace)))); // S-shaped

        // Cadishness: mild, consistent growth without plateau
        float cadishCurve = (float)(cadishWeight * Mathf.Sqrt(_Cadishness));

        // Speed: additive, hidden from players
        float speedEffect = speedWeight * _Speed;

        // Add some obfuscation (non-linearity & randomness)
        float noise = UnityEngine.Random.Range(0.85f, 1.15f);  // minor unpredictability
        float complexityMix = (float)(Mathf.Sin(_Grace + _Cadishness) + 2.5f); // non-linear mix

        float luck = 0;

        if (UnityEngine.Random.value < _Serindipity / 1000f) { luck = _Serindipity * 0.8f; }

        float rawInitiative = (graceCurve + cadishCurve + speedEffect + luck) * noise * complexityMix;

        return rawInitiative;
    }

    public int CompareTo(object obj)
    {
       if(obj == null)
        {
            return 1;
        }
        TurnToken ToCompare = obj as TurnToken;

        if (this.TurnCounter > ToCompare.TurnCounter)
        {
            return -1;
        }
        else if(this.TurnCounter < ToCompare.TurnCounter)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
