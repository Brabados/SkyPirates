using UnityEngine;

public class Attributes : MonoBehaviour
{
    // Base Attributes
    public int Chutzpah;
    public int Cadishness;
    public int Grace;
    public int Grit;
    public int Serendipity;
    public int Swagger;

    //Hidden Stats
    public int speed;

    // Derived Aspects and Skills can be calculated or adjusted here if needed

   
    public int MeleeCombat { get { return Chutzpah * 2; } } 

    public int Defence { get { return Grit * 2; } } 

    public int Movement { get { return Grace / 2;  } }

    public void Attack()
    {
       
        
        
    }

    public void Defend()
    {
        
      
        
    }

}
