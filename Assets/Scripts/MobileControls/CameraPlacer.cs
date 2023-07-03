using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlacer : MonoBehaviour
{
    [SerializeField] Vector2 posMobile;
    [SerializeField] Vector2 posPC;

    [SerializeField] float sizeMobile;
    [SerializeField] float sizePC;

    // Start is called before the first frame update
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (MobileControls.isPlatformAndroid())
        {
            transform.position = new Vector3(posMobile.x, posMobile.y, -10);
            camera.orthographicSize = sizeMobile;
        }
        else
        {
            transform.position = new Vector3(posPC.x, posPC.y, -10);
            camera.orthographicSize = sizePC;
        }
    }

}
