using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        Destroy(SoundManager.instance.gameObject);
        SceneManager.LoadScene("SampleScene");
    }
    public void LoadLevel(string levelName)
    {
        Destroy(SoundManager.instance.gameObject);
        SceneManager.LoadScene(levelName);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
