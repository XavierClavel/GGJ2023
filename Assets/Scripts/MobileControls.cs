using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileControls : MonoBehaviour
{
    [SerializeField] ButtonHandler buttonUp;
    [SerializeField] ButtonHandler buttonDown;
    [SerializeField] ButtonHandler buttonLeft;
    [SerializeField] ButtonHandler buttonRight;
    [SerializeField] Button restartButton;
    [SerializeField] Button pauseButton;

    private void Start()
    {
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


        /*
        buttonDown.onClick.AddListener(Player.instance.MoveDown);
        buttonUp.onClick.AddListener(Player.instance.MoveUp);

        buttonRight.OnPointerDown Player.instance.StartMoveRight);
        buttonRight.OnPointerUp(Player.instance.StopMoveRight);
        */
    }
}
