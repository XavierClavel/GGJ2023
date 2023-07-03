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
    float fadeTime = 0.7f;
    [SerializeField] Canvas canvas;

    private void Awake()
    {
        instance = this;
        image.color = new Color(0f, 0f, 0f, 1f);
    }

    void Start()
    {
        canvas.worldCamera = Camera.main;
        StartCoroutine(nameof(FadeIn));
    }

    public static void FadeTo(string scene)
    {
        instance.StartCoroutine(nameof(FadeOut), scene);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        yield return null;
        yield return null;

        while (t < fadeTime)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
            float a = curveIn.Evaluate(t / fadeTime);
            image.color = new Color(0f, 0f, 0f, a);
        }
        image.color = new Color(0f, 0f, 0f, 0f);
    }

    IEnumerator FadeOut(string scene)
    {
        float t = 0f;
        yield return null;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = curveOut.Evaluate(t / fadeTime);
            image.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
        yield return null;
        SceneManager.LoadScene(scene);
    }

}
