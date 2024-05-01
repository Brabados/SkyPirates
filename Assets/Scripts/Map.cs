using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private Vector2Int MapSize;

    public float innerSize , outerSize, height;
    public bool isFlatTopped;

    private Board PlayArea;

    public Material Mat;
    // Start is called before the first frame update
    void Start()
    {
        PlayArea = new Board(MapSize);

        for(int x = 0; x < MapSize.x; x++)
        {
            for(int y = 0; y < MapSize.y; y++)
            {
                GameObject Holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                Holder.transform.position = GetHexPositionFromCoordinate(new Vector2Int(x, y));
                Tile ToAdd = Holder.GetComponent<Tile>();
                ToAdd.Hex.H_Mat = Mat;
                ToAdd.Hex.innerSize = innerSize;
                ToAdd.Hex.outerSize = outerSize;
                ToAdd.Hex.height = height;
                ToAdd.Hex.isFlatTopped = isFlatTopped;
                ToAdd.Hex.meshupdate();
                PlayArea.set_Tile(x, y, ToAdd);
            }
        }
    }

    public Vector3 GetHexPositionFromCoordinate(Vector2Int Coordinates)
    {
        int column = Coordinates.x;
        int row = Coordinates.y;
        float width, height, xposition, yposition, horizontalDistance, VerticalDistance, offset;
        float size = outerSize;
        bool shouldOffset;

        if(!isFlatTopped)
        {
            shouldOffset = (row % 2) == 0;
            width = Mathf.Sqrt(3) * size;
            height = 2f * size;

            horizontalDistance = width;
            VerticalDistance = height * (3f / 4f);

            offset = (shouldOffset) ? width / 2 : 0;

            xposition = (column * (horizontalDistance)) + offset;
            yposition = (row * VerticalDistance);

        }
        else
        {
            shouldOffset = (column % 2) == 0;
            height = Mathf.Sqrt(3) * size;
            width = 2f * size;

            VerticalDistance = height;
            horizontalDistance = width * (3f / 4f);

            offset = (shouldOffset) ? height / 2 : 0;

            xposition = (column * horizontalDistance);
            yposition = (row * (VerticalDistance)) - offset;

        }

        return new Vector3(xposition,0,-yposition);
    }

    public void Redraw()
    {
        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                PlayArea.get_Tile(x, y).Hex.innerSize = innerSize;
                PlayArea.get_Tile(x, y).Hex.outerSize = outerSize;
                PlayArea.get_Tile(x, y).Hex.height = height;
                PlayArea.get_Tile(x, y).Hex.isFlatTopped = isFlatTopped;
                PlayArea.get_Tile(x, y).gameObject.transform.position = GetHexPositionFromCoordinate(new Vector2Int(x, y));
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        Redraw();
    }
}
