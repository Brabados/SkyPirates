using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderHightValue : MonoBehaviour
{
    [SerializeField] private Slider Slide;
    void Start()
    {
        Slide.onValueChanged.AddListener((v) =>
        {
            EventManager.TriggerSliderValueChange(v);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
