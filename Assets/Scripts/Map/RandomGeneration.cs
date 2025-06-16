using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGeneration : MonoBehaviour, IGenerate
{
    public Board Generate(Map Data)
    {
        Board PlayArea = new Board(Data.MapSize);

        for (int x = 0; x < Data.MapSize.x; x++)
        {
            for (int y = 0; y < Data.MapSize.y; y++)
            {
                int centerX = Data.MapSize.x / 2;
                int centerY = Data.MapSize.y / 2;
                Vector2Int offsetCoords = new Vector2Int(x - centerX, y - centerY);
                Vector2Int center = new Vector2Int(Data.MapSize.x / 2, Data.MapSize.y / 2);
                Vector2Int centeredOffset = offsetCoords - center;
                Vector3Int cube = HexUtils.OffsetToCube(centeredOffset, Data.isFlatTopped);

                GameObject holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                Tile tile = holder.GetComponent<Tile>();

                tile.Data = Data.TileTypes[Random.Range(0, 2)];

                // Set all values correctly
                float height = tile.Data == Data.TileTypes[0] ? 5 : 20;
                tile.SetPosition(offsetCoords);
                tile.SetQUSPosition(cube);
                tile.SetHeight(height);

                Vector3 tilePosition = Data.GetHexPositionFromCoordinate(offsetCoords);
                tilePosition.y += height / 2f;
                holder.transform.position = tilePosition;

                holder.transform.SetParent(transform);

                Instantiate(tile.Data.TilePrefab, holder.transform).transform.position += new Vector3(0, height / 2f - 1f, 0);

                tile.SetupHexRenderer(Data.innerSize, Data.outerSize, Data.isFlatTopped);
                tile.SetPawnPos();

                PlayArea.set_Tile(x, y, tile);
            }
        }

        return PlayArea;
    }

}
