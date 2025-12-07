using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LoadBoardFromJsonTests
{
    private class FakeFileLoader : IFileLoader
    {
        private readonly string _content;
        public FakeFileLoader(string content) { _content = content; }
        public string ReadAllText(string path) => _content;
        public void WriteAllText(string path, string content) { }
    }

    [Test]
    public void LoadBoardFromJson_DeserializesWithoutFileSystem()
    {
        // Arrange sample JSON with two tiles
        string json = "{\n" +
            "  \"TileTypeIDs\": [\"A\", \"B\"],\n" +
            "  \"Board\": {\n" +
            "    \"x_Height\": 2,\n" +
            "    \"y_Width\": 1,\n" +
            "    \"Tiles\": [[{ \"TileTypeID\": \"A\", \"Height\": 0 }, { \"TileTypeID\": \"B\", \"Height\": 1 }]]\n" +
            "  }\n" +
            "}";

        var mapGO = new GameObject("Map");
        var map = mapGO.AddComponent<Map>();
        map.MapSize = new Vector2Int(2, 1);
        map.innerSize = 0.5f;
        map.outerSize = 1f;
        map.isFlatTopped = true;
        map.TileTypes = new List<TileDataSO>();

        var typeA = ScriptableObject.CreateInstance<TileDataSO>();
        typeA.UniqueID = "A";
        typeA.TilePrefab = new GameObject("A_prefab");
        typeA.BaseMat = new Material(Shader.Find("Standard"));
        map.TileTypes.Add(typeA);

        var typeB = ScriptableObject.CreateInstance<TileDataSO>();
        typeB.UniqueID = "B";
        typeB.TilePrefab = new GameObject("B_prefab");
        typeB.BaseMat = typeA.BaseMat;
        map.TileTypes.Add(typeB);

        var slmGO = new GameObject("SaveLoad");
        var slm = slmGO.AddComponent<SaveLoadManager>();
        slm.FileLoader = new FakeFileLoader(json);

        // Act
        Board board = SaveLoadManager.LoadBoardFromJson("dummy", map, map.transform);

        // Assert
        Assert.IsNotNull(board);
        Assert.AreEqual(2, board._size_X);
        Assert.AreEqual(1, board._size_Y);
        Assert.AreEqual(typeA, board.get_Tile(0, 0).Data);
        Assert.AreEqual(0f, board.get_Tile(0, 0).Height);
        Assert.AreEqual(typeB, board.get_Tile(1, 0).Data);
        Assert.AreEqual(1f, board.get_Tile(1, 0).Height);
    }
}
