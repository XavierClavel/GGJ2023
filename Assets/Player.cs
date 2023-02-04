using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum state { placing, controlling };

public class Player : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject placeArrow;
    [SerializeField] Vector2 mapSize;
    state gameState = state.placing;
    Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Enable();
        controls.Game.MoveRight.performed += ctx => MoveRight();
        controls.Game.MoveLeft.performed += ctx => MoveLeft();
        controls.Game.MoveDown.performed += ctx => MoveDown();

        int minPos = tilemap.cellBounds.xMin;
        int maxPos = tilemap.cellBounds.xMax;

        Debug.Log("min " + minPos);
        Debug.Log("max " + maxPos);

        int size = maxPos - minPos;
        int middlePos = size / 2;

        int yPos = tilemap.cellBounds.yMax + 1;

        placeArrow.transform.position = new Vector2(0f, 4.5f);
    }

    private void OnDisable()
    {
        controls.Disable();
    }



    void MoveRight()
    {
        if (placeArrow.transform.position.x >= mapSize.x * 0.5f) return;
        placeArrow.transform.position += Vector3.right;
    }

    void MoveLeft()
    {
        if (placeArrow.transform.position.x <= -mapSize.x * 0.5f) return;
        placeArrow.transform.position += Vector3.left;
    }

    void MoveDown()
    {
        RaycastHit2D hit = Physics2D.Raycast(placeArrow.transform.position, 99 * Vector2.down);
        Debug.Log(hit.transform.position);
    }
}
