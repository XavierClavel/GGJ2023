using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ObjectPlacer : MonoBehaviour
{

    [SerializeField] Vector2 posMobile;
    [SerializeField] Vector2 posPC;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (MobileControls.isPlatformAndroid()) rectTransform.anchoredPosition = posMobile;
        else rectTransform.anchoredPosition = posPC;
    }

}
