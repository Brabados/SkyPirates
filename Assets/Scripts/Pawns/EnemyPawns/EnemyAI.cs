using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyPawn AI;
    public IAIMoveable Movement;
    // Start is called before the first frame update
    void Start()
    {
        Movement = this.gameObject.GetComponent<IAIMoveable>();
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
