using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Tile : MonoBehaviour
{
    public int Column;
    public int Row;

    public int QAxis;
    public int RAxis;
    public int SAxis;
    public float Height { get; private set; }
    public HexRenderer Hex { get; private set; }
    public MeshCollider HexCollider { get; private set; }
    public TileDataSO Data;
    public List<Tile> Neighbours;
    public Material BaseMaterial;
    public Pawn Contents = null;
    public GameObject PawnPosition;


    private CameraController cameraController;

    private void Awake()
    {
        Hex = GetComponent<HexRenderer>();
        HexCollider = GetComponent<MeshCollider>();
        cameraController = Camera.main.GetComponentInParent<CameraController>();
        Neighbours = new List<Tile>();
        PawnPosition = new GameObject("PawnPosition");
        PawnPosition.transform.position = this.transform.position;
        PawnPosition.transform.parent = this.transform;
    }

    private void Start()
    {
        Hex.DrawMesh();
        Hex.GetColliderMesh();
        SetColliderMesh();      
    }

    public void SetColliderMesh()
    {
        HexCollider.sharedMesh = Hex.H_ColiderMesh;
        HexCollider.convex = false;
    }

    public void SetPosition(Vector2Int coords)
    {
        Column = coords.x;
        Row = coords.y;
    }

    public void SetQUSPosition(int q, int r)
    {
        QAxis = q;
        RAxis = r;
        SAxis = -QAxis - RAxis;
    }

    public void SetHeight(float height)
    {
        Height = height;
    }

    public void SetPositionAndHeight(Vector2Int coords, int q, int r, float height)
    {
        SetPosition(coords);
        SetQUSPosition(q, r);
        SetHeight(height);      
        transform.position = new Vector3(transform.position.x, Height / 2f, transform.position.z);
        
    }

    public void SetPawnPos()
    {
        PawnPosition.transform.localPosition = new Vector3(PawnPosition.transform.localPosition.x, PawnPosition.transform.localPosition.y + Height / 2f, PawnPosition.transform.localPosition.z);
    }

    //Might find somewhere else for this code to sit, don't like the idea of every single hex having a copy of the camera.
    public Tile CheckNeighbours(Vector2 direction)
    {
        Vector3 forward = CalculateForwardVector(direction);

        Tile closest = null;
        float minDist = Mathf.Infinity;

        foreach (Tile next in Neighbours)
        {
            float dist = Vector3.Distance(next.transform.position, forward);
            if (dist < minDist)
            {
                closest = next;
                minDist = dist;
            }
        }

        return closest;
    }

    private Vector3 CalculateForwardVector(Vector2 direction)
    {
        Vector3 forward = Vector3.zero;

        if (cameraController == null)
        {
            Debug.LogError("Camera Controller is missing!");
            return forward;
        }

        if (direction.y > 0)
        {
            forward = (cameraController.Forward * Hex.outerSize) + transform.position;
        }
        else if (direction.y < 0)
        {
            forward = (-cameraController.Forward * Hex.outerSize) + transform.position;
        }
        else if (direction.x > 0)
        {
            forward = (cameraController.Right * Hex.outerSize) + transform.position;
        }
        else if (direction.x < 0)
        {
            forward = (-cameraController.Right * Hex.outerSize) + transform.position;
        }

        return forward;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Tile other = (Tile)obj;
        return QAxis == other.QAxis && RAxis == other.RAxis && SAxis == other.SAxis;
    }

    public override string ToString()
    {
        return $"Hex(Q: {QAxis}, R: {RAxis}, S: {SAxis})";
    }

    public void SetupHexRenderer(float innerSize, float outerSize, bool isFlatTopped)
    {
        Hex.H_Mat = Data.BaseMat;
        Hex.innerSize = innerSize;
        Hex.outerSize = outerSize;
        Hex.height = Height;
        Hex.isFlatTopped = isFlatTopped;
        BaseMaterial = Data.BaseMat;
        Hex.H_Mat = Data.BaseMat;
        Hex.meshupdate(Data.BaseMat);
    }

    public void SetNeighbour( Tile neighbour)
    {
        if (!Neighbours.Contains(neighbour))
        {
            Neighbours.Add(neighbour);
        }
    }

    public Vector3Int ReturnSquareCoOrds()
    {
        return new Vector3Int(QAxis, RAxis, SAxis);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
