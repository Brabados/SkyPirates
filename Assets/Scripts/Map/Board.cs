using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public int _size_X { get; private set; }
    public int _size_Y { get; private set; }
    public int qOffset { get; private set; }
    public int rOffset { get; private set; }

    private Tile[,] _board_Contents;

    private Dictionary<Vector3Int, Tile> cubeLookup;


    //Additive vectors for assigning neighbours 
    private Vector3Int[] directions = new Vector3Int[6] { new Vector3Int(1, -1, 0), new Vector3Int(1, 0, -1), new Vector3Int(0, 1, -1),new Vector3Int(-1, 1, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, -1, 1) };
   

    public Board(Vector2Int coordinates, int qOffset = int.MinValue, int rOffset = int.MinValue)
    {
        _size_Y = coordinates.y;
        _size_X = coordinates.x;
        this.qOffset = qOffset == int.MinValue ? _size_X / 2 : qOffset;
        this.rOffset = rOffset == int.MinValue ? _size_Y / 2 : rOffset;
        _board_Contents = new Tile[_size_X, _size_Y];
        cubeLookup = new Dictionary<Vector3Int, Tile>();

    }

    public Tile get_Tile(int x, int y)
    {
        if ((x >= 0 && x < _size_X) && (y >= 0 && y < _size_Y))
        {
            return _board_Contents[x, y];
        }
        else
        {
            return null;
        }
    }

    public void set_Tile(int x, int y, Tile toset)
    {
        // Remove old cube key if one exists
        Tile existing = _board_Contents[x, y];
        if (existing != null)
        {
            Vector3Int oldCube = new Vector3Int(existing.QAxis, existing.RAxis, existing.SAxis);
            if (cubeLookup.ContainsKey(oldCube) && cubeLookup[oldCube] == existing)
            {
                cubeLookup.Remove(oldCube);
            }
        }

        // Set new tile
        _board_Contents[x, y] = toset;

        Vector3Int newCube = new Vector3Int(toset.QAxis, toset.RAxis, toset.SAxis);
        cubeLookup[newCube] = toset;

        toset.Column = x;
        toset.Row = y;

#if UNITY_EDITOR
        Debug.Log($"set_Tile: offset=({x},{y}) → cube={newCube}");
#endif
    }


    public void swap_Tiles(Vector2Int Tile1, Vector2Int Tile2)
    {
        Tile placeholder = get_Tile(Tile1.x, Tile1.y);
        set_Tile(Tile1.x, Tile1.y, get_Tile(Tile2.x, Tile2.y));
        set_Tile(Tile2.x, Tile2.y, placeholder);
    }

    //Sets the 6 neighbours of each tile on the board
    public List<Tile> GetNeighbours(Vector2Int centerTile)
    {
        List<Tile> Neighbours = new List<Tile>();

        Tile tile = get_Tile(centerTile.x, centerTile.y);
        if (tile != null)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3Int neighborPos = new Vector3Int(tile.QAxis + directions[i].x, tile.RAxis + directions[i].y, tile.SAxis + directions[i].z);
                Tile neighbour = SearchTileByCubeCoordinates(neighborPos.x, neighborPos.y, neighborPos.z);
                if (neighbour != null)
                {
                    Neighbours.Add(neighbour);
                }
            }
        }

        return Neighbours;
    }
    public Tile SearchTileByCubeCoordinates(int q, int r, int s)
    {
        // Convert cube coordinates (q, r, s) to array indices (x, y)
        // using the stored offsets so boards with different origins work

        int x = q + qOffset;
        int y = r + rOffset;

        // Check if the calculated indices are within bounds
        if (x >= 0 && x < _board_Contents.GetLength(0) && y >= 0 && y < _board_Contents.GetLength(1))
        {
            return _board_Contents[x, y];
        }
        else
        {
            // If the indices are out of bounds, return null
            return null;
        }
    }


    public Tile GetTileByCube(Vector3Int cubeCoords)
    {
        cubeLookup.TryGetValue(cubeCoords, out Tile tile);
        return tile;
    }


    // Return all tiles in the board (implement if you don't have it)
    public IEnumerable<Tile> GetAllTiles()
    {
        for (int x = 0; x < _size_X; x++)
        {
            for (int y = 0; y < _size_Y; y++)
            {
                Tile tile = get_Tile(x, y);
                if (tile != null) yield return tile;
            }
        }
    }


    public void Destroy()
    {
        foreach(Tile x in _board_Contents)
        {
            if (x != null)
            {
                var cube = new Vector3Int(x.QAxis, x.RAxis, x.SAxis);
                cubeLookup.Remove(cube);
                MonoBehaviour.Destroy(x.gameObject);
            }
        }
        cubeLookup.Clear();
    }

    public Tile FirstNonNullTile()
    {
        for (int x = 0; x < _size_X; x++)
            for (int y = 0; y < _size_Y; y++)
            {
                Tile t = get_Tile(x, y);
                if (t != null)
                    return t;
            }
        return null;
    }

    public int CubeDistance(Tile a, Tile b)
    {
        return (Mathf.Abs(a.QAxis - b.QAxis) + Mathf.Abs(a.RAxis - b.RAxis) + Mathf.Abs(a.SAxis - b.SAxis)) / 2;
    }

    public Vector3 CubeLerp(Tile a, Tile b, float t)
    {
        return new Vector3(
            Mathf.Lerp(a.QAxis, b.QAxis, t),
            Mathf.Lerp(a.RAxis, b.RAxis, t),
            Mathf.Lerp(a.SAxis, b.SAxis, t)
        );
    }

    public Vector3Int CubeRound(Vector3 cube)
    {
        int q = Mathf.RoundToInt(cube.x);
        int r = Mathf.RoundToInt(cube.y);
        int s = Mathf.RoundToInt(cube.z);

        float q_diff = Mathf.Abs(q - cube.x);
        float r_diff = Mathf.Abs(r - cube.y);
        float s_diff = Mathf.Abs(s - cube.z);

        if (q_diff > r_diff && q_diff > s_diff)
        {
            q = -r - s;
        }
        else if (r_diff > s_diff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }

        return new Vector3Int(q, r, s);
    }

    public Vector3Int GetDirectionVector(Tile from, Tile to)
    {
        Vector3Int delta = new Vector3Int(to.QAxis - from.QAxis, to.RAxis - from.RAxis, to.SAxis - from.SAxis);
        // Normalize to closest hex direction
        float max = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
        Vector3 lerp = new Vector3(delta.x / max, delta.y / max, delta.z / max);
        Vector3Int rounded = CubeRound(lerp);

        foreach (Vector3Int dir in directions)
        {
            if (dir == rounded) return dir;
        }
        return Vector3Int.zero;
    }
}
