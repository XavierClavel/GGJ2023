using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image image;
    public static SceneFader instance;
    public AnimationCurve curveIn;
    public AnimationCurve curveOut;
    WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
    float fadeTime = 0.7f;

    private void Awake()
    {
        image.gameObject.SetActive(true);
    }

    void Start()
    {
        instance = this;
        StartCoroutine(FadeIn());
    }

    public static void FadeTo(string scene)
    {
        instance.StartCoroutine("FadeOut", scene);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = curveIn.Evaluate(t / fadeTime);
            image.color = new Color(0f, 0f, 0f, a);
            yield return waitFrame;
        }
        image.color = new Color(0f, 0f, 0f, 0f);
    }

    IEnumerator FadeOut(string scene)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = curveOut.Evaluate(t / fadeTime);
            image.color = new Color(0f, 0f, 0f, a);
            yield return waitFrame;
        }
        SceneManager.LoadScene(scene);
    }
}
