using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Scenes;
using Unity.Entities;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public Scene SpawnScene;
    public GameObject SpawnPrefab;
    
    // Start is called before the first frame update
    void Start()
    {

    }

}
