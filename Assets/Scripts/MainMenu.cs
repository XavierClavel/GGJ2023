using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioSource saxo;

    private void Start()
    {
        Cursor.visible = true;
    }

    public void Play()
    {
        Cursor.visible = false;
        DontDestroyOnLoad(saxo);
        saxo.Play();
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo("Level 1");
    }

    public void LoadLevel(string levelName)
    {
        Cursor.visible = false;
        DontDestroyOnLoad(saxo);
        saxo.Play();
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo(levelName);
    }

    public void Quit()
    {
        Cursor.visible = false;
        Application.Quit();
    }

}
