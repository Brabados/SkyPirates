using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public ScrollRect ItemDysplay;
    public ItemType SearchItem;
    public Item CurrentEquip;

    void Start()
    {
        // Get the ButtonHighlight component and hook up the listener
        ButtonHighlight button = GetComponent<ButtonHighlight>();
        if (button != null)
        {
            button.onClick.AddListener(buttonPress);
            Debug.Log($"[ItemButton] Added listener to {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[ItemButton] No ButtonHighlight found on {gameObject.name}");
        }
    }

    public void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            // Your selection logic here if needed
        }
    }

    public void buttonPress()
    {
        Debug.Log($"[ItemButton] buttonPress called on {gameObject.name}");

        if (ItemDysplay != null)
        {
            ItemDysplay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[ItemButton] ItemDysplay is null on {gameObject.name}");
        }

        if (CurrentEquip != null)
        {
            EventManager.TriggerItemSelect(CurrentEquip);
        }
        else
        {
            Debug.LogWarning($"[ItemButton] CurrentEquip is null on {gameObject.name}");
        }
    }

    void OnDestroy()
    {
        // Clean up the listener
        ButtonHighlight button = GetComponent<ButtonHighlight>();
        if (button != null)
        {
            button.onClick.RemoveListener(buttonPress);
        }
    }
}
