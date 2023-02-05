using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        Player.instance.Pause();
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        string currentLevel = SceneManager.GetActiveScene().name;
        string levelNumber = currentLevel.Split(" ")[1];
        int lvlInt = int.Parse(levelNumber);
        int buildIndex = SceneUtility.GetBuildIndexByScenePath("Level " + (++lvlInt));
        if (buildIndex == -1) SceneFader.FadeTo("MainMenu");
        else
        {
            string scene = SceneManager.GetSceneByBuildIndex(buildIndex).name;
            SceneFader.FadeTo(scene);
        }



    }

    public void Menu()
    {
        Destroy(SoundManager.instance.gameObject);
        SceneFader.FadeTo("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
