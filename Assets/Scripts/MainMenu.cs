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
        Time.timeScale = 1f;
        DontDestroyOnLoad(saxo);
    }

    public void Play()
    {
        LoadLevel("Level 1");
    }

    public void LoadLevel(string levelName)
    {
        Cursor.visible = false;
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
