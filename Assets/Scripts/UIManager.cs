using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public GameObject MainMenuObject;
    public GameObject GameOverObject;
    public GameObject GameUIObject;
    public EventSystem EventSystem;

    private void OnEnable()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(this.EventSystem.gameObject);
        MainMenuObject.SetActive(true);
        GameOverObject.SetActive(false);
        GameUIObject.SetActive(false);
    }

    public void OnPressPlay()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        MainMenuObject.SetActive(false);
        GameUIObject.SetActive(true);
    }

}
