using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager CanvasInstance { get; private set; }

    [Header("Exclusive Swap Menus (one at a time)")]
    public List<Canvas> SwapMenus = new List<Canvas>();
    public List<Transform> SwapMenuCameraPositions = new List<Transform>();

    [Header("Layered BattleMode Menus (multiple allowed)")]
    public List<Canvas> LayeredMenus = new List<Canvas>();

    public BasicControls inputActions;
    public int position;

    private bool transition = false;

    private void Awake()
    {
        if (CanvasInstance != null && CanvasInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            CanvasInstance = this;
        }
    }

    private void Start()
    {
        position = 0;
        inputActions = EventManager.EventInstance.inputActions;

        if (inputActions.Menu.enabled == true)
        {
            SwapMenus[position].gameObject.SetActive(true);
        }

        foreach (Canvas a in LayeredMenus)
        {
            a.gameObject.SetActive(false);
        }

        EventManager.OnHideCanvas += CloseMenu;
        EventManager.OnShowCanvas += SetMenu;

        inputActions.Menu.MenuSwap.performed += OnMenuSwap;
    }

    private void OnDestroy()
    {
        inputActions.Menu.MenuSwap.performed -= OnMenuSwap;
        EventManager.OnHideCanvas -= CloseMenu;
        EventManager.OnShowCanvas -= SetMenu;
    }


    // Swaps between exclusive menus (one active at a time).

    private void OnMenuSwap(InputAction.CallbackContext context)
    {
        if (transition)
            return;

        float inputValue = context.ReadValue<float>();
        if (inputValue == 0)
            return;

        // Disable current swap menu
        SwapMenus[position].gameObject.SetActive(false);

        // Move to next menu
        position += (int)inputValue;

        if (position >= SwapMenus.Count)
        {
            position = 0;
        }
        else if (position < 0)
        {
            position = SwapMenus.Count - 1;
        }

        // Enable the new swap menu
        SwapMenus[position].gameObject.SetActive(true);

        // Start camera transition if available
        if (position < SwapMenuCameraPositions.Count && SwapMenuCameraPositions[position] != null)
        {
            StartCoroutine(CameraMove(Camera.main.transform.position, SwapMenuCameraPositions[position].position));
            transition = true;
        }
    }


    // Moves the camera between positions.

    private IEnumerator CameraMove(Vector3 start, Vector3 end)
    {
        float time = 0;
        Vector3 currentLocation = start;

        while (Vector3.Distance(currentLocation, end) > 0.01f)
        {
            time += Time.deltaTime;
            currentLocation = Vector3.Lerp(start, end, time);
            Camera.main.transform.position = currentLocation;
            yield return null;
        }

        Camera.main.transform.position = end;
        transition = false;
    }


    // Enable a BattleMode menu without disabling others (layered).

    public void SetMenu(int index)
    {
        if (index < 0 || index >= LayeredMenus.Count)
            return;

        LayeredMenus[index].gameObject.SetActive(true);
    }


    // Disable a specific BattleMode menu.

    public void CloseMenu(int index)
    {
        if (index < 0 || index >= LayeredMenus.Count)
            return;

        LayeredMenus[index].gameObject.SetActive(false);
    }


    // Disable all Layered BattleMode menus.

    public void CloseAllLayeredMenus()
    {
        foreach (Canvas menu in LayeredMenus)
        {
            menu.gameObject.SetActive(false);
        }
    }


    // Disable all Swap menus.

    public void CloseAllSwapMenus()
    {
        foreach (Canvas menu in SwapMenus)
        {
            menu.gameObject.SetActive(false);
        }
    }
}
