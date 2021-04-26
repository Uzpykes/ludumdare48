using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject MainMenuObject;
    public GameObject GameOverObject;
    public GameObject GameUIObject;
    public EventSystem EventSystem;
    private int diamondsToWin = 5;

    private void OnEnable()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(this.EventSystem.gameObject);
        MainMenuObject.SetActive(true);
        GameOverObject.SetActive(false);
        GameUIObject.SetActive(false);
        ResourceManager.onItemChange.AddListener(OnItemChange);
    }

    private void OnItemChange(ItemDetails itemDetails)
    {
        if (itemDetails.type != ResourceType.Diamond)
            return;

        if (itemDetails.count >= diamondsToWin)
            StartCoroutine(DoWin());
    }

    private IEnumerator DoWin()
    {
        diamondsToWin = 999;
        InputManager.InputIsBlocked = true;
        var levelManager = FindObjectOfType<LevelManager>();

        var canvasGroup = GameUIObject.GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;

        while (levelManager.currentTopLayer >= 1)
        {
            yield return new WaitForSeconds(.1f);
            levelManager.OnMouseScroll(1);
        }

        yield return new WaitForSeconds(2f);

        GameOverObject.SetActive(true);

        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;
        InputManager.InputIsBlocked = false;
    }

    public void OnPressPlay()
    {
        ResourceManager.SetUp();
        ScoreManager.Reset();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        MainMenuObject.SetActive(false);
        GameUIObject.SetActive(true);
        diamondsToWin = 5;
    }

    public void OnPressRestart()
    {
        MainMenuObject.SetActive(false);
        GameUIObject.SetActive(true);
        GameOverObject.SetActive(false);
        diamondsToWin = 5;
        this.EventSystem.SetSelectedGameObject(null);

        ResourceManager.SetUp();
        ScoreManager.Reset();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void OnPressQuit()
    {
        Application.Quit();
    }

    public void OnDebugWin()
    {
        ResourceManager.ChangeCount(ResourceType.Diamond, 5);
    }

}
