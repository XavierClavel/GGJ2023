using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

enum state { placing, controlling, animating };

public class Player : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap rootTilemap;
    [SerializeField] GameObject placeArrow;
    [SerializeField] Vector2 mapSize;
    [SerializeField] List<TileBase> obstacles;
    [SerializeField] TileBase rootTile_V;
    [SerializeField] TileBase rootTile_H;
    [SerializeField] TileBase rootTile_NE;
    [SerializeField] TileBase rootTile_NW;
    [SerializeField] TileBase rootTile_SE;
    [SerializeField] TileBase rootTile_SW;
    [SerializeField] TileBase intersect_empty;
    [SerializeField] TileBase intersect_H;
    [SerializeField] TileBase intersect_V;
    [SerializeField] TileBase intersect_full;
    [Header("Unidirectional")]
    [SerializeField] TileBase unidirect_H;
    [SerializeField] TileBase unidirect_V;
    [SerializeField] TileBase unidirect_NE;
    [SerializeField] TileBase unidirect_NW;
    [SerializeField] TileBase unidirect_SE;
    [SerializeField] TileBase unidirect_SW;


    [SerializeField] TileBase endPoint;
    state gameState = state.placing;
    Vector3Int currentPosition;
    Vector3Int lastDirection = Vector3Int.zero;
    Controls controls;
    int nbEndPoints = 0;

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

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = tilemap.GetTile<Tile>(pos);
            if (tile == endPoint)
            {
                nbEndPoints++;
            }
        }
        Debug.Log(nbEndPoints);
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
        Debug.Log(gameState);
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
        if (gameState == state.placing)
        {
            currentPosition = tilemap.WorldToCell(placeArrow.transform.position);
            TryFillDirection(Vector3Int.down);
            placeArrow.SetActive(false);
        }
        else if (gameState == state.controlling)
        {
            Debug.Log("placing down");
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
        Debug.Log("current position : " + currentPosition);
        Debug.Log("next position : " + (currentPosition + direction));
        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        if (obstacles.Contains(nextTile)) return;
        FillLine(direction);
    }

    bool vertical(Vector3Int direction)
    {
        return direction == Vector3Int.up || direction == Vector3Int.down;
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
            else if (nextTile == endPoint)
            {
                currentPosition += direction;
                //tilemap.SetTile(currentPosition, rootTile_H);
                EndPointReached();
                lastDirection = Vector3Int.zero;
                return;
            }
            else if (nextTile == intersect_empty)
            {
                currentPosition += direction;
                if (vertical(direction)) tilemap.SetTile(currentPosition, intersect_H);
                else tilemap.SetTile(currentPosition, intersect_V);
            }
            else if (nextTile == intersect_H)
            {
                if (vertical(direction)) break;
                currentPosition += direction;
                tilemap.SetTile(currentPosition, intersect_full);
            }
            else if (nextTile == intersect_V)
            {
                if (!vertical(direction)) break;
                currentPosition += direction;
                tilemap.SetTile(currentPosition, intersect_full);
            }
            else if (nextTile == unidirect_H)
            {
                if (vertical(direction)) break;
                currentPosition += direction;
                rootTilemap.SetTile(currentPosition, rootTile_H);
            }
            else if (nextTile == unidirect_V)
            {
                if (!vertical(direction)) break;
                currentPosition += direction;
                rootTilemap.SetTile(currentPosition, rootTile_V);
            }
            else if (nextTile == unidirect_NE)
            {
                if (direction == Vector3Int.up || direction == Vector3Int.right) break;
                currentPosition += direction;
                if (direction == Vector3Int.down) direction = Vector3Int.right;
                else direction = Vector3Int.up;
            }
            else if (nextTile == unidirect_SE)
            {
                if (direction == Vector3Int.down || direction == Vector3Int.right) break;
                currentPosition += direction;
                if (direction == Vector3Int.up) direction = Vector3Int.right;
                else direction = Vector3Int.down;
            }
            else if (nextTile == unidirect_NW)
            {
                if (direction == Vector3Int.up || direction == Vector3Int.left) break;
                currentPosition += direction;
                if (direction == Vector3Int.down) direction = Vector3Int.left;
                else direction = Vector3Int.up;
            }
            else if (nextTile == unidirect_SW)
            {
                if (direction == Vector3Int.down || direction == Vector3Int.left) break;
                currentPosition += direction;
                if (direction == Vector3Int.up) direction = Vector3Int.left;
                else direction = Vector3Int.down;
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
        Debug.Log("final position : " + currentPosition);
        lastDirection = direction;
        gameState = state.controlling;
    }

    void EndPointReached()
    {
        Debug.Log("end point reached");
        nbEndPoints--;
        if (nbEndPoints <= 0) Win();
        else
        {
            Debug.Log("placing");
            gameState = state.placing;
            placeArrow.SetActive(true);
        }
    }

    void Win()
    {
        Debug.Log("win");
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
