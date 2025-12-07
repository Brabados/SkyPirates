using System;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
public class ScriptableObjectIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        if (string.IsNullOrEmpty(property.stringValue))
        {
            property.stringValue = Guid.NewGuid().ToString();
        }
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
public class BaseScriptableObject : ScriptableObject
{
    [ScriptableObjectId]
    public string UniqueID;
}

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObject/TileData")]
public class TileDataSO : BaseScriptableObject
{
    [SerializeField]
    public string Name;

    [SerializeField]
    public int MovementCost;

    [SerializeField]
    public GameObject TilePrefab;

    [SerializeField]
    public bool Walkable;

    [SerializeField]
    public Material BaseMat;

}
