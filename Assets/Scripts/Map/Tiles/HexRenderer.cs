using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Defining a face to avoid having to keep track of verticies and triangles.
public struct Face
{
    public List<Vector3> Verts { get; private set; }
    public List<int> Tris { get; private set; }
    public List<Vector2> Uvs { get; private set; }

    public Face(List<Vector3> verticies, List<int> triangles, List<Vector2> uvs)
    {
        this.Verts = verticies;
        this.Tris = triangles;
        this.Uvs = uvs;
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    public Mesh H_Mesh { get; private set; }
    private MeshFilter H_Meshfilter;
    private MeshRenderer H_Meshrenderer;
    public Mesh H_ColiderMesh;

    public Material H_Mat;

    public float innerSize, outerSize, height;
    public bool isFlatTopped;

    List<Face> H_Faces;
    public void Awake()
    {
        H_Meshfilter = GetComponent<MeshFilter>();
        H_Meshrenderer = GetComponent<MeshRenderer>();

        H_Mesh = new Mesh();
        H_Mesh.name = "Hex";

        H_Meshfilter.mesh = H_Mesh;
        H_Meshrenderer.material = H_Mat;

    }

    //Command to be used if the hexes need to be redrawn for any reason.
    public void DrawMesh()
    {
        DrawFaces();
        CombineFaces();
    }

    //Builds all of the faces for the mesh
    private void DrawFaces()
    {
        H_Faces = new List<Face>();

        for(int point = 0; point < 6; point++)
        {
            H_Faces.Add(CreateFace(innerSize, outerSize, height / 2f, height / 2f, point));
        }

        for (int point = 0; point < 6; point++)
        {
            H_Faces.Add(CreateFace(innerSize, outerSize, -height / 2f, -height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            H_Faces.Add(CreateFace(outerSize, outerSize, height / 2f, -height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            H_Faces.Add(CreateFace(innerSize, innerSize, height / 2f, -height / 2f, point, false));
        }

    }

    //Puts the faces together into a mesh ready for rendering 
    public void CombineFaces()
    {
        List<Vector3> verticies = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for(int x = 0; x < H_Faces.Count; x++)
        {
            verticies.AddRange(H_Faces[x].Verts);
            uvs.AddRange(H_Faces[x].Uvs);

            int offset = (4 * x);

            foreach(int triangle in H_Faces[x].Tris)
            {
                tris.Add(triangle + offset);
            }
        }

        H_Mesh.vertices = verticies.ToArray();
        H_Mesh.triangles = tris.ToArray();
        H_Mesh.uv = uvs.ToArray();
        H_Mesh.RecalculateNormals();
        
    }

    //builds the faces of the hex
    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point< 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> verticies = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        if(reverse)
        {
            verticies.Reverse();
        }
        return new Face(verticies,triangles,uvs);
    }
    
    //Returns a verticy based on input
    protected Vector3 GetPoint(float size, float height, int index)
    {
        float angle_deg = isFlatTopped ? 60 * index: 60 * index - 30;
        float angle_rad = Mathf.PI / 180f * angle_deg;
        return new Vector3((size * Mathf.Cos(angle_rad)), height, size * Mathf.Sin(angle_rad));
    }

    //Changes the mesh of the renderer.
    public void meshupdate(Material Mat)
    {
        H_Meshrenderer.material = Mat;
    }

    public Material currentMat()
    {
        return H_Meshrenderer.material;
    }

    //Creates a mesh to be used as the colider mesh, which is the same as the drawn mesh but with a flat hex on top.
    public void GetColliderMesh()
    {
        H_ColiderMesh = new Mesh();

        for (int point = 0; point < 6; point++)
        {
            H_Faces.Add(CreateFace(0, outerSize, height / 2f, height / 2f, point));
        }

        List<Vector3> verticies = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x < H_Faces.Count; x++)
        {
            verticies.AddRange(H_Faces[x].Verts);
            uvs.AddRange(H_Faces[x].Uvs);

            int offset = (4 * x);

            foreach (int triangle in H_Faces[x].Tris)
            {
                tris.Add(triangle + offset);
            }
        }

        H_ColiderMesh.vertices = verticies.ToArray();
        H_ColiderMesh.triangles = tris.ToArray();
        H_ColiderMesh.uv = uvs.ToArray();
        H_ColiderMesh.RecalculateNormals();

    }
}
