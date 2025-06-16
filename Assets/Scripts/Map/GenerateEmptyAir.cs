using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateEmptyAir : MonoBehaviour, IGenerate
{
    public Board Generate(Map Data)
    {
        Board PlayArea = new Board(Data.MapSize);

        for (int x = 0; x < Data.MapSize.x; x++)
        {
            for (int y = 0; y < Data.MapSize.y; y++)
            {
                Vector2Int offsetCoords = new Vector2Int(x, y);
                Vector3Int cube = HexUtils.OffsetToCube(offsetCoords, Data.isFlatTopped);

                GameObject holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                Tile tile = holder.GetComponent<Tile>();
                tile.Data = Data.TileTypes[0]; // Always air

                float height = 5f;

                // Correct position assignment
                tile.SetPosition(offsetCoords);
                tile.SetQUSPosition(cube);
                tile.SetHeight(height);

                Vector3 tilePosition = Data.GetHexPositionFromCoordinate(offsetCoords);
                tilePosition.y += height / 2f;
                holder.transform.position = tilePosition;

                holder.transform.SetParent(transform);

                GameObject visual = Instantiate(tile.Data.TilePrefab, holder.transform);
                visual.transform.position += new Vector3(0, height / 2f - 1f, 0);

                tile.SetupHexRenderer(Data.innerSize, Data.outerSize, Data.isFlatTopped);
                tile.SetPawnPos();

                PlayArea.set_Tile(x, y, tile);
            }
        }

        return PlayArea;
    }
}
