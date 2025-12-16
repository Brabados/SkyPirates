using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the character stat display and equipment button UI
/// </summary>
public class CharacterUIManager : MonoBehaviour
{
    [Header("Stat Displays")]
    [SerializeField] private TextMeshProUGUI chuzpahText;
    [SerializeField] private TextMeshProUGUI cadishnessText;
    [SerializeField] private TextMeshProUGUI graceText;
    [SerializeField] private TextMeshProUGUI gritText;
    [SerializeField] private TextMeshProUGUI serendipityText;
    [SerializeField] private TextMeshProUGUI swaggerText;
    [SerializeField] private TextMeshProUGUI equipmentText;

    [Header("Equipment Buttons")]
    [SerializeField] private ButtonHighlight headButton;
    [SerializeField] private ButtonHighlight bodyButton;
    [SerializeField] private ButtonHighlight weaponButton;
    [SerializeField] private ButtonHighlight feetButton;
    [SerializeField] private ButtonHighlight accessoryButton;

    private Dictionary<ItemType, ButtonHighlight> equipmentButtons;

    void Awake()
    {
        InitializeDictionaries();
        SetupButtonListeners();
    }

    void Start()
    {
        EventSystem.current.firstSelectedGameObject = headButton.gameObject;

        EventManager.OnCharacterChange += UpdateDisplay;
        EventManager.OnEquipmentChange += SelectEquipmentButton;
    }

    private void InitializeDictionaries()
    {
        equipmentButtons = new Dictionary<ItemType, ButtonHighlight>
        {
            { ItemType.Head, headButton },
            { ItemType.Body, bodyButton },
            { ItemType.Weapon, weaponButton },
            { ItemType.Feet, feetButton },
            { ItemType.Accessory, accessoryButton }
        };
    }

    private void SetupButtonListeners()
    {
        foreach (var kvp in equipmentButtons)
        {
            ButtonHighlight button = kvp.Value;

            ItemButton itemButton = button.GetComponent<ItemButton>();
            if (itemButton == null)
            {
                itemButton = button.gameObject.AddComponent<ItemButton>();
            }
        }
    }

    public void UpdateDisplay(Pawn pawn)
    {
        if (pawn == null) return;

        UpdateStats(pawn);
        UpdateEquipment(pawn);
    }

    private void UpdateStats(Pawn pawn)
    {
        chuzpahText.text = (pawn.Stats.Chutzpah + pawn.Equiped.chuzpah).ToString();
        cadishnessText.text = (pawn.Stats.Cadishness + pawn.Equiped.cadishness).ToString();
        graceText.text = (pawn.Stats.Grace + pawn.Equiped.grace).ToString();
        gritText.text = (pawn.Stats.Grit + pawn.Equiped.grit).ToString();
        serendipityText.text = (pawn.Stats.Serendipity + pawn.Equiped.serindipity).ToString();
        swaggerText.text = (pawn.Stats.Swagger + pawn.Equiped.swagger).ToString();
    }

    private void UpdateEquipment(Pawn pawn)
    {
        equipmentText.text = "";

        foreach (Item item in pawn.Equiped.Equipment)
        {
            equipmentText.text += item.Name + Environment.NewLine;

            if (equipmentButtons.TryGetValue(item.Type, out ButtonHighlight button))
            {
                UpdateEquipmentButton(button, item);
            }
        }
    }

    private void UpdateEquipmentButton(ButtonHighlight button, Item item)
    {
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
            textComponent.text = item.Name;

        ItemButton itemButton = button.GetComponent<ItemButton>();
        if (itemButton != null)
            itemButton.CurrentEquip = item;
    }

    private void SelectEquipmentButton(ItemType type, Item item)
    {
        if (equipmentButtons.TryGetValue(item.Type, out ButtonHighlight button))
        {
            StartCoroutine(SelectButtonDelayed(button.gameObject));
        }
    }

    private IEnumerator SelectButtonDelayed(GameObject buttonObj)
    {
        // Wait for spawned list to clear
        yield return null;

        // Clear selection
        EventSystem.current.SetSelectedGameObject(null);

        // Wait one more frame
        yield return null;

        // Select the equipment button
        EventSystem.current.SetSelectedGameObject(buttonObj);
    }

    void OnDestroy()
    {
        EventManager.OnCharacterChange -= UpdateDisplay;
        EventManager.OnEquipmentChange -= SelectEquipmentButton;
    }
}
