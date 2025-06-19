using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class AbilitySelectStateTests
{
    private class DummySelection : MonoBehaviour, ISelectionResponce
    {
        public GameObject Selection { get; private set; }
        public void Select(GameObject selection) { Selection = selection; }
        public void Deselect() { }
        public GameObject CurrentSelection() => Selection;
    }

    [Test]
    public void EnterState_ConfiguresMenuComponents()
    {
        var managerGO = new GameObject("manager");
        managerGO.AddComponent<RangeFinder>();
        var manager = managerGO.AddComponent<HexSelectManager>();
        var menuSelect = managerGO.AddComponent<MenuSelect>();
        var abilityHighlight = managerGO.AddComponent<AbilityHighlight>();
        var dummy = managerGO.AddComponent<DummySelection>();

        var selected = new GameObject("selected");
        dummy.Select(selected);
        manager.Responce = dummy;

        var canvasGO = new GameObject("canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        typeof(HexSelectManager).GetProperty("UI", BindingFlags.Instance | BindingFlags.Public)
            .SetValue(manager, canvas);

        var state = new AbilitySelectState();
        Assert.DoesNotThrow(() => state.EnterState(manager));
        Assert.AreSame(menuSelect, manager.Responce);
        Assert.AreSame(abilityHighlight, manager.Highlight);
        Assert.IsTrue(manager.UI.enabled);
    }
}
