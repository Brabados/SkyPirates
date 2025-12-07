using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MovementLine : MonoBehaviour
{

    private Board _Playarea;
    private LineRenderer Movement;
    List<Transform> Wpoint;
    // Start is called before the first frame update
    public void Start()
    {
        EventManager.OnMovementChange += SetPoints;
        Wpoint = new List<Transform>();
        Movement = GetComponent<LineRenderer>();
    }

    public void SetMap(Board board)
    {
        _Playarea = board;
    }

    public void SetPoints(List<Vector3Int> Points)
    {
        Tile Tpoint = null;
        Wpoint.Clear();
        if (Points == null)
        {
            Movement.positionCount = 1;
            Movement.SetPosition(0, new Vector3(0, 0, 0));
        }
        else
        {
            foreach (Vector3Int a in Points)
            {
                Tpoint = _Playarea.SearchTileByCubeCoordinates(a.x, a.y, a.z);
                if (Tpoint != null)
                {
                    Wpoint.Add(Tpoint.gameObject.transform);
                }
            }
            Movement.positionCount = Wpoint.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < Wpoint.Count; x++)
        {
            Movement.SetPosition(x, (Wpoint[x].position + new Vector3(0, 15, 0)));
        }
    }

    public void OnDestroy()
    {
        EventManager.OnMovementChange -= SetPoints;
    }
}
