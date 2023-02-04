using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

enum state { placing, controlling, animating };
enum tileType
{
    straight_V_down, straight_V_up,
    straight_H_right, straight_H_left,
    corner_NE_down, corner_NE_left,
    corner_NW_down, corner_NW_right,
    corner_SE_up, corner_SE_left,
    corner_SW_up, corner_SW_right,
    endPoint_left, endPoint_right,
    endPoint_up, endPoint_down,
}

public class Player : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap rootTilemap;
    [SerializeField] GameObject placeArrow;
    [SerializeField] Vector2 mapSize;
    [SerializeField] List<TileBase> obstacles;
    [SerializeField] TileBase grass;
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
    PauseControls pauseControls;
    int nbEndPoints = 0;
    WaitForSeconds frameDuration = new WaitForSeconds(0.03f);
    bool gamePaused = false;
    [SerializeField] GameObject pauseWindow;
    [SerializeField] GameObject grid;
    bool isPlacing = true;
    [SerializeField] List<TileAnim> anims_root_Vdown;
    [SerializeField] List<TileAnim> anims_root_Vup;
    [SerializeField] List<TileAnim> anims_root_Hright;
    [SerializeField] List<TileAnim> anims_root_Hleft;

    [SerializeField] List<TileAnim> anims_corner_NE_down;
    [SerializeField] List<TileAnim> anims_corner_NE_left;
    [SerializeField] List<TileAnim> anims_corner_NW_down;
    [SerializeField] List<TileAnim> anims_corner_NW_right;
    [SerializeField] List<TileAnim> anims_corner_SE_up;
    [SerializeField] List<TileAnim> anims_corner_SE_left;
    [SerializeField] List<TileAnim> anims_corner_SW_up;
    [SerializeField] List<TileAnim> anims_corner_SW_right;
    [SerializeField] List<TileAnim> endPoint_right;
    [SerializeField] List<TileAnim> endPoint_left;
    [SerializeField] List<TileAnim> endPoint_up;
    [SerializeField] List<TileAnim> endPoint_down;

    TileAnim lastAnim;

    private void Start()
    {
        controls = new Controls();
        controls.Enable();
        controls.Game.MoveRight.performed += ctx => MoveRight();
        controls.Game.MoveLeft.performed += ctx => MoveLeft();
        controls.Game.MoveDown.performed += ctx => MoveDown();
        controls.Game.MoveUp.performed += ctx => MoveUp();

        pauseControls = new PauseControls();
        pauseControls.Enable();
        pauseControls.Pause.Pause.performed += ctx => Pause();
        pauseControls.Pause.Restart.performed += ctx => Restart();

        int minPos = tilemap.cellBounds.xMin;
        int maxPos = tilemap.cellBounds.xMax;

        Debug.Log("min " + minPos);
        Debug.Log("max " + maxPos);

        int size = maxPos - minPos;
        int middlePos = size / 2;

        int yPos = tilemap.cellBounds.yMax + 1;

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pos.y > yPos) yPos = pos.y;
            Tile tile = tilemap.GetTile<Tile>(pos);
            if (tile == endPoint)
            {
                nbEndPoints++;
            }
        }
        placeArrow.transform.position = new Vector2(0f, (yPos - 1.2f) * grid.transform.localScale.y);
        Debug.Log(nbEndPoints);
    }

    void Pause()
    {
        if (gamePaused)
        {
            pauseWindow.SetActive(false);
            gamePaused = false;
            controls.Enable();
        }
        else
        {
            pauseWindow.SetActive(true);
            gamePaused = true;
            controls.Disable();
        }
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
        if (nextTile == grass) currentPosition += direction;
        nextTile = tilemap.GetTile(currentPosition + direction);
        if (obstacles.Contains(nextTile)) return;
        StartCoroutine("FillLine", direction);
    }

    bool vertical(Vector3Int direction)
    {
        return direction == Vector3Int.up || direction == Vector3Int.down;
    }


    IEnumerator FillLine(Vector3Int direction)
    {
        gameState = state.animating;

        if (lastDirection != Vector3Int.zero) yield return ChangeDirection(direction);

        while (true)
        {
            Debug.Log(currentPosition);
            TileBase nextTile = tilemap.GetTile(currentPosition + direction);
            TileBase nextRoot = rootTilemap.GetTile(currentPosition + direction);
            if (nextTile != null) Debug.Log(nextTile.name);
            if (isPlacing && nextTile == grass) currentPosition += direction;
            if (obstacles.Contains(nextTile) || obstacles.Contains(nextRoot))
            {
                yield return Backoff(currentPosition);
                break;
            }
            else if (nextTile == endPoint)
            {
                currentPosition += direction;
                if (direction == Vector3Int.down) yield return PlaceTile(tileType.endPoint_down, currentPosition);
                else if (direction == Vector3Int.up) yield return PlaceTile(tileType.endPoint_up, currentPosition);
                else if (direction == Vector3Int.right) yield return PlaceTile(tileType.endPoint_right, currentPosition);
                else if (direction == Vector3Int.left) yield return PlaceTile(tileType.endPoint_left, currentPosition);
                EndPointReached();
                lastDirection = Vector3Int.zero;
                StopCoroutine("FillLine");
            }
            else if (nextTile == intersect_empty)
            {
                currentPosition += direction;
                if (vertical(direction)) tilemap.SetTile(currentPosition, intersect_H);
                else tilemap.SetTile(currentPosition, intersect_V);
            }
            else if (nextTile == intersect_H)
            {
                if (vertical(direction))
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                tilemap.SetTile(currentPosition, intersect_full);
            }
            else if (nextTile == intersect_V)
            {
                if (!vertical(direction))
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                tilemap.SetTile(currentPosition, intersect_full);
            }
            else if (nextTile == unidirect_H)
            {
                if (vertical(direction))
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.right) yield return PlaceTile(tileType.straight_H_right, currentPosition);
                else yield return PlaceTile(tileType.straight_H_left, currentPosition);
            }
            else if (nextTile == unidirect_V)
            {
                if (!vertical(direction))
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.up) yield return PlaceTile(tileType.straight_V_up, currentPosition);
                else yield return PlaceTile(tileType.straight_V_down, currentPosition);
            }
            else if (nextTile == unidirect_NE)
            {
                if (direction == Vector3Int.up || direction == Vector3Int.right)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.down)
                {
                    yield return PlaceTile(tileType.corner_NE_down, currentPosition);
                    direction = Vector3Int.right;
                }
                else
                {
                    direction = Vector3Int.up;
                    yield return PlaceTile(tileType.corner_NE_left, currentPosition);
                }
                Debug.Log(direction);
            }
            else if (nextTile == unidirect_SE)
            {
                if (direction == Vector3Int.down || direction == Vector3Int.right)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.up)
                {
                    yield return PlaceTile(tileType.corner_SE_up, currentPosition);
                    direction = Vector3Int.right;
                }
                else
                {
                    direction = Vector3Int.down;
                    yield return PlaceTile(tileType.corner_SE_left, currentPosition);
                }
            }
            else if (nextTile == unidirect_NW)
            {
                if (direction == Vector3Int.up || direction == Vector3Int.left)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.down)
                {
                    direction = Vector3Int.left;
                    yield return PlaceTile(tileType.corner_NW_down, currentPosition);
                }
                else
                {
                    direction = Vector3Int.up;
                    yield return PlaceTile(tileType.corner_SE_left, currentPosition);
                }
            }
            else if (nextTile == unidirect_SW)
            {
                if (direction == Vector3Int.down || direction == Vector3Int.left)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.up)
                {
                    direction = Vector3Int.left;
                    yield return PlaceTile(tileType.corner_SW_up, currentPosition);
                }
                else
                {
                    direction = Vector3Int.down;
                    yield return PlaceTile(tileType.corner_SW_right, currentPosition);
                }
            }
            else
            {
                currentPosition += direction;
                if (direction == Vector3Int.down) yield return PlaceTile(tileType.straight_V_down, currentPosition);
                else if (direction == Vector3Int.up) yield return PlaceTile(tileType.straight_V_up, currentPosition);
                else if (direction == Vector3Int.right) yield return PlaceTile(tileType.straight_H_right, currentPosition);
                else if (direction == Vector3Int.left) yield return PlaceTile(tileType.straight_H_left, currentPosition);

            }
            //yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("final position : " + currentPosition);
        lastDirection = direction;
        gameState = state.controlling;
        isPlacing = false;
    }

    IEnumerator Backoff(Vector3Int position)
    {
        for (int i = lastAnim.anim.Count - 1; i > 0; i--)
        {
            rootTilemap.SetTile(position, lastAnim.anim[i]);
            yield return frameDuration;
        }
    }

    TileAnim TileSwitch(tileType type)
    {
        switch (type)
        {
            case tileType.straight_V_down:
                return anims_root_Vdown[Random.Range(0, anims_root_Vdown.Count)];

            case tileType.straight_V_up:
                return anims_root_Vup[Random.Range(0, anims_root_Vup.Count)];

            case tileType.straight_H_right:
                return anims_root_Hright[Random.Range(0, anims_root_Hright.Count)];

            case tileType.straight_H_left:
                return anims_root_Hleft[Random.Range(0, anims_root_Hleft.Count)];

            case tileType.corner_NE_down:
                return anims_corner_NE_down[Random.Range(0, anims_corner_NE_down.Count)];

            case tileType.corner_NE_left:
                return anims_corner_NE_left[Random.Range(0, anims_corner_NE_left.Count)];

            case tileType.corner_NW_down:
                return anims_corner_NW_down[Random.Range(0, anims_corner_NW_down.Count)];

            case tileType.corner_NW_right:
                return anims_corner_NW_down[Random.Range(0, anims_corner_NW_down.Count)];

            case tileType.corner_SE_up:
                return anims_corner_SE_up[Random.Range(0, anims_corner_SE_up.Count)];

            case tileType.corner_SE_left:
                return anims_corner_SE_left[Random.Range(0, anims_corner_SE_left.Count)];

            case tileType.corner_SW_up:
                return anims_corner_SW_up[Random.Range(0, anims_corner_SW_up.Count)];

            case tileType.corner_SW_right:
                return anims_corner_SW_right[Random.Range(0, anims_corner_SW_right.Count)];

            case tileType.endPoint_up:
                return endPoint_up[0];

            case tileType.endPoint_down:
                return endPoint_down[0];

            case tileType.endPoint_right:
                return endPoint_right[0];

            case tileType.endPoint_left:
                return endPoint_left[0];


            default:
                return anims_root_Vdown[0];
        }
    }

    IEnumerator PlaceTile(tileType type, Vector3Int position)
    {
        lastAnim = TileSwitch(type);
        List<TileBase> tiles = lastAnim.anim;
        for (int i = 0; i < tiles.Count; i++)
        {
            rootTilemap.SetTile(position, tiles[i]);
            yield return frameDuration;
        }
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

    IEnumerator ChangeDirection(Vector3Int direction)
    {
        Vector3Int directionChange = direction - lastDirection;
        if (lastDirection == Vector3Int.down)
        {
            if (direction == Vector3Int.right) yield return PlaceTile(tileType.corner_NE_down, currentPosition);
            else yield return PlaceTile(tileType.corner_NW_down, currentPosition);
        }
        else if (direction == Vector3Int.up)
        {
            if (lastDirection == Vector3Int.right) yield return PlaceTile(tileType.corner_NW_right, currentPosition);
            else yield return PlaceTile(tileType.corner_NE_left, currentPosition);
        }
        else if (direction == Vector3Int.right)
        {
            if (lastDirection == Vector3Int.up) yield return PlaceTile(tileType.corner_SE_up, currentPosition);
            else yield return PlaceTile(tileType.corner_NE_down, currentPosition);
        }
        else if (direction == Vector3Int.left)
        {
            if (lastDirection == Vector3Int.up) yield return PlaceTile(tileType.corner_SW_up, currentPosition);
            else yield return PlaceTile(tileType.corner_NW_down, currentPosition);
        }
    }
}
