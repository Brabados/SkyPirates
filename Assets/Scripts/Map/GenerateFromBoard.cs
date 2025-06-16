using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFromBoard : MonoBehaviour, IGenerate
{
    public Board Generate(Map Data)
    {
        Board playArea = Data.PlayArea;
        int width = Data.MapSize.x;
        int height = Data.MapSize.y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = playArea.get_Tile(x, y);
                if (tile == null)
                    continue;

                Vector2Int offsetCoords = new Vector2Int(x, y);
                Vector3Int cube = HexUtils.OffsetToCube(offsetCoords, Data.isFlatTopped);

                float heightVal = tile.Data == Data.TileTypes[0] ? 5 : 20;

                tile.SetPosition(offsetCoords);
                tile.SetQUSPosition(cube);
                tile.SetHeight(heightVal);

                Vector3 tilePosition = Data.GetHexPositionFromCoordinate(offsetCoords);
                tilePosition.y += heightVal / 2f;
                tile.transform.position = tilePosition;

                if (tile.transform.childCount == 0 && tile.Data.TilePrefab != null)
                {
                    GameObject visual = Instantiate(tile.Data.TilePrefab, tile.transform);
                    visual.transform.position += new Vector3(0, heightVal / 2f - 1f, 0);
                }

                tile.SetupHexRenderer(Data.innerSize, Data.outerSize, Data.isFlatTopped);
                tile.SetPawnPos();
            }
        }

        return playArea;
    }
}
