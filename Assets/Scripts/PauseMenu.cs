using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject nextButton;
    [SerializeField] Canvas canvas;
    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }

    public void SelectNext()
    {
        EventSystem.current.SetSelectedGameObject(nextButton);
    }

    public void Resume()
    {
        Player.instance.Pause();
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        SceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        string currentLevel = SceneManager.GetActiveScene().name;
        string levelNumber = currentLevel.Split(" ")[1];
        int lvlInt = int.Parse(levelNumber);
        int buildIndex = SceneUtility.GetBuildIndexByScenePath("Level " + (++lvlInt));
        Debug.Log(buildIndex);
        if (buildIndex == -1) SceneFader.FadeTo("MainMenu");
        else
        {
            string scene = NameFromIndex(buildIndex);
            Debug.Log(scene);
            SceneFader.FadeTo(scene);
        }



    }

    private static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo("MainMenu");
    }

    public void Quit()
    {
        Cursor.visible = false;
        Application.Quit();
    }
}
