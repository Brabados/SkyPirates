using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyPawn AI;
    public IAIHeatMap Movement;
    public IAIHeatMap AbilityChoice;
    // Start is called before the first frame update
    void Start()
    {
        Movement = this.gameObject.GetComponent<BasicMovement>();
        AbilityChoice = this.gameObject.GetComponent<BasicAbilityChoice>();
        Debug.Log("");
    }

}
