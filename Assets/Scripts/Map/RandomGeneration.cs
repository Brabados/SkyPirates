using UnityEngine;

public class RandomGeneration : MonoBehaviour, IGenerate
{
    public Board Generate(Map Data)
    {

        Vector3Int zeroCube = HexUtils.OffsetToCube(Vector2Int.zero, Data.isFlatTopped, false);
        Board PlayArea = new Board(Data.MapSize, -zeroCube.x, -zeroCube.y);

        for (int x = 0; x < Data.MapSize.x; x++)
        {
            for (int y = 0; y < Data.MapSize.y; y++)
            {
                GameObject holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                Tile tile = holder.GetComponent<Tile>();
                tile.Data = Data.TileTypes[Random.Range(0, 2)];
                tile.SetPositionAndHeight(new Vector2Int(x, y), x, y, tile.Data == Data.TileTypes[0] ? 5 : 20);
                Vector3 tilePosition = Data.GetHexPositionFromCoordinate(new Vector2Int(x, y));
                tilePosition.y = tilePosition.y + tile.Height / 2;
                holder.transform.position = tilePosition;
                holder.transform.SetParent(transform);
                Instantiate(tile.Data.TilePrefab, holder.transform).transform.position += new Vector3(0, tile.Height / 2 - 1, 0);
                tile.SetupHexRenderer(Data.innerSize, Data.outerSize, Data.isFlatTopped);
                tile.SetPosition(new Vector2Int(x, y));
                tile.SetPawnPos();
                PlayArea.set_Tile(x, y, tile);
            }
        }
        return PlayArea;
    }

}
