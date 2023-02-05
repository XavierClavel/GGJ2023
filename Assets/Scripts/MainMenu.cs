using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo("Level 1");
    }

    public void LoadLevel(string levelName)
    {
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo(levelName);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
