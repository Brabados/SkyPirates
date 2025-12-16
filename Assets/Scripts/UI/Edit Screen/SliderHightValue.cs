using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Broadcasts slider value changes through the EventManager
/// </summary>
public class SliderHeightValue : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Start()
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        EventManager.TriggerSliderValueChange(value);
    }

    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
