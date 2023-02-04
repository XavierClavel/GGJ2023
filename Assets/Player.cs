using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum state { placing, controlling, animating };

public class Player : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject placeArrow;
    [SerializeField] Vector2 mapSize;
    [SerializeField] List<TileBase> obstacles;
    [SerializeField] TileBase rootTile;
    state gameState = state.placing;
    Vector3Int currentPosition;
    Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Enable();
        controls.Game.MoveRight.performed += ctx => MoveRight();
        controls.Game.MoveLeft.performed += ctx => MoveLeft();
        controls.Game.MoveDown.performed += ctx => MoveDown();
        controls.Game.MoveUp.performed += ctx => MoveUp();

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
        if (gameState == state.placing)
        {
            if (placeArrow.transform.position.x >= mapSize.x * 0.5f) return;
            placeArrow.transform.position += Vector3.right;
        }
        else if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.right);
        }

    }

    void MoveLeft()
    {
        if (gameState == state.placing)
        {
            if (placeArrow.transform.position.x <= -mapSize.x * 0.5f) return;
            placeArrow.transform.position += Vector3.left;
        }
        else if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.left);
        }
    }

    void MoveDown()
    {
        currentPosition = tilemap.WorldToCell(placeArrow.transform.position);
        if (gameState == state.placing)
        {
            TryFillDirection(Vector3Int.down);
            placeArrow.SetActive(false);
        }
        else if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.down);
        }
    }

    void MoveUp()
    {
        if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.up);
        }
    }

    void TryFillDirection(Vector3Int direction)
    {
        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        if (obstacles.Contains(nextTile)) return;
        FillLine(direction);
    }

    void FillLine(Vector3Int direction)
    {
        gameState = state.animating;
        for (int i = 0; i < 20; i++)
        {
            Debug.Log(currentPosition);
            TileBase nextTile = tilemap.GetTile(currentPosition + direction);
            if (obstacles.Contains(nextTile))
            {
                break;
            }
            else
            {
                currentPosition += direction;
                tilemap.SetTile(currentPosition, rootTile);
            }
        }
        gameState = state.controlling;
    }
}
