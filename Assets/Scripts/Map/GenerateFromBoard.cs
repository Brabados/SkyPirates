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

                int q = x - playArea.qOffset;
                int r = y - playArea.rOffset;

                tile.SetPositionAndHeight(new Vector2Int(x, y), q, r, tile.Data == Data.TileTypes[0] ? 5 : 20);
                Vector3 tilePosition = Data.GetHexPositionFromCoordinate(new Vector2Int(x, y));
                tilePosition.y += tile.Height / 2;
                tile.transform.position = tilePosition;

                if (tile.transform.childCount == 0 && tile.Data.TilePrefab != null)
                {
                    Instantiate(tile.Data.TilePrefab, tile.transform).transform.position += new Vector3(0, tile.Height / 2 - 1, 0);
                }

                tile.SetupHexRenderer(Data.innerSize, Data.outerSize, Data.isFlatTopped);
                tile.SetPosition(new Vector2Int(x, y));
                tile.SetPawnPos();
            }
        }

        return playArea;
    }

}
