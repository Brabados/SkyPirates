using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Abstract base class for spawning scrollable button lists with consistent behavior.
/// </summary>
public abstract class ScrollListGenerator<T> : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected RectTransform contentRect;
    [SerializeField] protected Button buttonPrefab;

    [Header("Layout Settings")]
    [SerializeField] protected float buttonHeight = 100f;
    [SerializeField] protected float buttonSpacing = 10f;
    [SerializeField] protected bool autoSelectFirstButton = false;

    protected List<Button> spawnedButtons = new List<Button>();

    protected abstract List<T> GetListData();
    protected abstract string GetItemDisplayText(T item);
    protected abstract void ConfigureButton(Button button, T item);

    public void RefreshList()
    {
        ClearList();

        List<T> data = GetListData();
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] No data to display");
            return;
        }

        UpdateContentSize(data.Count);

        for (int i = 0; i < data.Count; i++)
        {
            Button button = CreateButton(data[i], i);
            spawnedButtons.Add(button);
        }

        if (autoSelectFirstButton)
        {
            SelectFirstButton();
        }
    }

    public void ClearList()
    {
        foreach (Button button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        spawnedButtons.Clear();
    }

    protected virtual void UpdateContentSize(int itemCount)
    {
        if (contentRect == null)
        {
            Debug.LogError($"[{GetType().Name}] contentRect is not assigned!");
            return;
        }

        float totalHeight = (itemCount * buttonHeight) + ((itemCount + 1) * buttonSpacing);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    protected virtual Button CreateButton(T item, int index)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError($"[{GetType().Name}] buttonPrefab is not assigned!");
            return null;
        }

        Button button = Instantiate(buttonPrefab, contentRect);
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        SetupButtonTransform(rectTransform, index);
        SetButtonText(button, GetItemDisplayText(item));
        ConfigureButton(button, item);

        return button;
    }

    protected virtual void SetupButtonTransform(RectTransform rectTransform, int index)
    {
        // Anchor to top, stretch horizontally
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Set size: width stretches, height is fixed
        rectTransform.sizeDelta = new Vector2(0, buttonHeight);

        // Position from top
        float yPosition = -(buttonSpacing + (index * (buttonHeight + buttonSpacing)));
        rectTransform.anchoredPosition = new Vector2(0, yPosition);
    }

    protected virtual void SetButtonText(Button button, string text)
    {
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
            textComponent.text = text;
        else
            Debug.LogWarning($"[{GetType().Name}] No TextMeshProUGUI found in button!");
    }

    protected virtual void SelectFirstButton()
    {
        if (spawnedButtons.Count > 0 && spawnedButtons[0] != null)
            EventSystem.current.SetSelectedGameObject(spawnedButtons[0].gameObject);
    }

    protected virtual void OnDestroy()
    {
        ClearList();
    }
}
