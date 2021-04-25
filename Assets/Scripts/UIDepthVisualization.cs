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

    private void OnEnable()
    {
        LevelManager.onMapDraw.AddListener(OnMapDraw);
    }

    public void OnMapDraw(int maxDepth, int currentTopLayer)
    {
        CameraSlider.maxValue = maxDepth-1;
        CameraSlider.value = currentTopLayer;
        DepthLabel.text = $"{maxDepth.ToString()}";
    }


    private void OnDisable()
    {
        LevelManager.onMapDraw.RemoveListener(OnMapDraw);
    }


}
