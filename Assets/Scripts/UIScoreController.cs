using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UIScoreController : MonoBehaviour
{
    public TextMeshProUGUI Score;
    public TextMeshProUGUI ScorePopup;
    public Transform startPosition;
    public Transform endPosition;

    bool popUpIsMoving = false;
    private int popupVal = 0;


    private void OnEnable()
    {
        ScoreManager.onScoreChanged.AddListener(OnScoreChanged);       
    }

    private void OnScoreChanged(int delta)
    {

        if (!popUpIsMoving)
        {
            popupVal = delta;
            StartCoroutine(DoPopup());
        }
        else
            popupVal += delta;
        ScorePopup.text = $"{popupVal}";
        Score.text = ScoreManager.TotalScore.ToString();
    }

    private IEnumerator DoPopup()
    {
        popUpIsMoving = true;
        ScorePopup.gameObject.SetActive(true);
        var t = 0f;
        while (t < 1f)
        {
            ScorePopup.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, t);
            t += Time.deltaTime;
            yield return null;
        }
        ScorePopup.gameObject.SetActive(false);
        popUpIsMoving = false;
    }

    private void OnDestroy()
    {
        ScoreManager.onScoreChanged.RemoveListener(OnScoreChanged);
    }
}
