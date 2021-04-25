using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIDepthVisualization : MonoBehaviour
{
    public Slider CameraSlider;
    public TextMeshProUGUI DepthLabel;
    public TextMeshProUGUI SliderHandleText;

    private void OnEnable()
    {
        LevelManager.onMapDraw.AddListener(OnMapDraw);
    }

    public void OnMapDraw(int maxDepth, int currentTopLayer)
    {
        CameraSlider.maxValue = Mathf.Max(10, maxDepth-1);
        CameraSlider.value = currentTopLayer;
        SliderHandleText.text = $" {currentTopLayer}";
        DepthLabel.text = $"{CameraSlider.maxValue}";
    }


    private void OnDisable()
    {
        LevelManager.onMapDraw.RemoveListener(OnMapDraw);
    }


}
