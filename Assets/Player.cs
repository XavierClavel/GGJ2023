using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

enum state { placing, controlling, animating };

public class Player : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject placeArrow;
    [SerializeField] Vector2 mapSize;
    [SerializeField] List<TileBase> obstacles;
    [SerializeField] TileBase rootTile_V;
    [SerializeField] TileBase rootTile_H;
    [SerializeField] TileBase rootTile_NE;
    [SerializeField] TileBase rootTile_NW;
    [SerializeField] TileBase rootTile_SE;
    [SerializeField] TileBase rootTile_SW;
    state gameState = state.placing;
    Vector3Int currentPosition;
    Vector3Int lastDirection = Vector3Int.zero;
    Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Enable();
        controls.Game.MoveRight.performed += ctx => MoveRight();
        controls.Game.MoveLeft.performed += ctx => MoveLeft();
        controls.Game.MoveDown.performed += ctx => MoveDown();
        controls.Game.MoveUp.performed += ctx => MoveUp();

        controls.Game.Restart.performed += ctx => Restart();

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

    void Restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
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

        if (lastDirection != Vector3Int.zero) ChangeDirection(direction);

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
                if (direction == Vector3Int.left || direction == Vector3Int.right)
                {
                    tilemap.SetTile(currentPosition, rootTile_H);
                }
                else
                {
                    tilemap.SetTile(currentPosition, rootTile_V);
                }

            }
        }
        lastDirection = direction;
        gameState = state.controlling;
    }

    void ChangeDirection(Vector3Int direction)
    {
        Vector3Int directionChange = direction - lastDirection;
        if (directionChange == Vector3Int.up + Vector3Int.right)
        {
            tilemap.SetTile(currentPosition, rootTile_NE);
        }
        else if (directionChange == Vector3Int.down + Vector3Int.right)
        {
            tilemap.SetTile(currentPosition, rootTile_SE);
        }
        else if (directionChange == Vector3Int.up + Vector3Int.left)
        {
            tilemap.SetTile(currentPosition, rootTile_NW);
        }
        else if (directionChange == Vector3Int.down + Vector3Int.left)
        {
            tilemap.SetTile(currentPosition, rootTile_SW);
        }
    }
}
