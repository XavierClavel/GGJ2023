using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo("SampleScene");
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
