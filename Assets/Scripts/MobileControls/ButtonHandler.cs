using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Button button;
    [HideInInspector] public UnityEvent onPointerDown;
    [HideInInspector] public UnityEvent onPointerUp;
    [HideInInspector] public UnityEvent onClick;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ignore if button not interactable
        if (!button.interactable) return;

        onPointerDown?.Invoke();
        onClick?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke();
    }
}
