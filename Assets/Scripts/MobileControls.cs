using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MobileControls : MonoBehaviour
{
    [SerializeField] ButtonHandler buttonUp;
    [SerializeField] ButtonHandler buttonDown;
    [SerializeField] ButtonHandler buttonLeft;
    [SerializeField] ButtonHandler buttonRight;
    [SerializeField] Button restartButton;
    [SerializeField] Button pauseButton;
    [SerializeField] GameObject mobileUI;
    [SerializeField] GameObject pcUI;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        if (!isPlatformAndroid())
        {
            pcUI.SetActive(true);
            return;
        }
        mobileUI.SetActive(true);

        buttonDown.onClick.AddListener(Player.instance.MoveDown);
        buttonUp.onClick.AddListener(Player.instance.MoveUp);

        buttonRight.onClick.AddListener(Player.instance.MoveRight);
        buttonRight.onPointerDown.AddListener(Player.instance.StartMoveRight);
        buttonRight.onPointerUp.AddListener(Player.instance.StopMoveRight);

        buttonLeft.onClick.AddListener(Player.instance.MoveLeft);
        buttonLeft.onPointerDown.AddListener(Player.instance.StartMoveLeft);
        buttonLeft.onPointerUp.AddListener(Player.instance.StopMoveLeft);

        restartButton.onClick.AddListener(Player.instance.Restart);
        pauseButton.onClick.AddListener(Player.instance.Pause);
    }

    public static bool isPlatformAndroid()
    {
        //return false;
        return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || Application.platform == RuntimePlatform.Android;
    }

}
