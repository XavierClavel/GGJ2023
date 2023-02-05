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
    under_right, under_left,
}

enum vegetable { carotte, salade }

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
    [SerializeField] List<TileAnim> tunnel_right;
    [SerializeField] List<TileAnim> tunnel_left;
    [SerializeField] TileBase herb2;
    [SerializeField] List<GameObject> vegetables_seeds;
    [SerializeField] List<GameObject> vegetables_grown;
    [SerializeField] GameObject winScreen;
    vegetable currentVegetable;
    GameObject currentSeed;
    int vegeIndex;
    bool hasWon = false;

    TileAnim lastAnim;
    public static Player instance;
    [SerializeField] PauseMenu winScript;
    [SerializeField] PauseMenu pauseScript;
    WaitForSeconds pressDuration = new WaitForSeconds(0.3f);

    private void Start()
    {
        instance = this;

        Cursor.visible = false;

        controls = new Controls();
        controls.Enable();
        controls.Game.MoveRight.performed += ctx => MoveRight();
        controls.Game.MoveRight.started += ctx => StartMoveRight();
        controls.Game.MoveRight.canceled += ctx => StopMoveRight();

        controls.Game.MoveLeft.performed += ctx => MoveLeft();
        controls.Game.MoveLeft.started += ctx => StartMoveLeft();
        controls.Game.MoveLeft.canceled += ctx => StopMoveLeft();
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
        int xPosMax = 0;

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pos.y > yPos) yPos = pos.y;
            if (pos.x > xPosMax) xPosMax = pos.x;
            Tile tile = tilemap.GetTile<Tile>(pos);
            if (tile == endPoint)
            {
                nbEndPoints++;
            }
        }
        float xPos = xPosMax % 1 == 0 ? 0.5f : 0f;
        placeArrow = Instantiate(placeArrow);
        placeArrow.transform.position = new Vector2(xPos, 3.5f);
        Debug.Log(nbEndPoints);
    }

    public void Pause()
    {
        if (hasWon) return;
        if (gamePaused)
        {
            Cursor.visible = false;
            pauseWindow.SetActive(false);
            gamePaused = false;
            controls.Enable();
        }
        else
        {
            Cursor.visible = true;
            pauseWindow.SetActive(true);
            pauseScript.SelectNext();
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
        controls.Disable();
        pauseControls.Disable();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void StartMoveRight()
    {
        if (gameState == state.placing) StartCoroutine("ContinueMoveRight");
    }

    void StopMoveRight()
    {
        StopCoroutine("ContinueMoveRight");
    }

    IEnumerator ContinueMoveRight()
    {
        while (true)
        {
            MoveRightPlace();
            yield return pressDuration;
        }
    }

    void StartMoveLeft()
    {
        if (gameState == state.placing) StartCoroutine("ContinueMoveLeft");
    }

    void StopMoveLeft()
    {
        StopCoroutine("ContinueMoveLeft");
    }

    IEnumerator ContinueMoveLeft()
    {
        while (true)
        {
            MoveLeftPlace();
            yield return pressDuration;
        }
    }

    void MoveRightPlace()
    {
        if (placeArrow.transform.position.x + 1 >= mapSize.x * 0.5f) return;
        placeArrow.transform.position += Vector3.right;
    }

    void MoveLeftPlace()
    {
        if (placeArrow.transform.position.x - 1 <= -mapSize.x * 0.5f) return;
        placeArrow.transform.position += Vector3.left;

    }



    void MoveRight()
    {
        if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.right);
        }

    }

    void MoveLeft()
    {
        if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.left);
        }
    }

    void MoveDown()
    {
        if (gameState == state.placing)
        {
            lastDirection = Vector3Int.zero;
            currentPosition = tilemap.WorldToCell(placeArrow.transform.position);
            TryFillDirection(Vector3Int.down);
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
        TileBase currentTile = tilemap.GetTile(currentPosition);
        if (currentTile == unidirect_NE || currentTile == unidirect_NW ||
        currentTile == unidirect_SE || currentTile == unidirect_SW ||
        currentTile == unidirect_H || currentTile == unidirect_V ||
        currentTile == intersect_empty
        ) return;
        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        if (nextTile == unidirect_H && vertical(direction) ||
        (nextTile == unidirect_V && !vertical(direction))
        ) return;
        bool placeGrass = false;
        if (isPlacing && (nextTile == grass || nextTile == herb2))
        {
            currentPosition += direction;
            placeGrass = true;
        }
        nextTile = tilemap.GetTile(currentPosition + direction);
        TileBase rootTile = rootTilemap.GetTile(currentPosition + direction);
        if (nextTile != null) Debug.Log("adjacent tile : " + nextTile.name);
        if (obstacles.Contains(nextTile) || (obstacles.Contains(rootTile) && nextTile != intersect_empty) ||
        (nextTile == unidirect_NE && direction == Vector3Int.right) ||
        (nextTile == unidirect_NE && direction == Vector3Int.up) ||
        (nextTile == unidirect_NW && direction == Vector3Int.left) ||
        (nextTile == unidirect_NW && direction == Vector3Int.up) ||
        (nextTile == unidirect_SE && direction == Vector3Int.right) ||
        (nextTile == unidirect_SE && direction == Vector3Int.down) ||
        (nextTile == unidirect_SW && direction == Vector3Int.left) ||
        (nextTile == unidirect_SW && direction == Vector3Int.down)
        ) return;
        if (isPlacing)
        {
            tilemap.SetTile(currentPosition, herb2);
            vegeIndex = Random.Range(0, vegetables_seeds.Count);
            currentSeed = Instantiate(vegetables_seeds[vegeIndex], placeArrow.transform.position + Vector3.up, Quaternion.identity);
        }
        StartCoroutine("FillLine", direction);
    }

    bool vertical(Vector3Int direction)
    {
        return direction == Vector3Int.up || direction == Vector3Int.down;
    }


    IEnumerator FillLine(Vector3Int direction)
    {
        SoundManager.PlaySfx(transform, sfx.grow);

        placeArrow.SetActive(false);
        gameState = state.animating;
        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        TileBase nextRoot = rootTilemap.GetTile(currentPosition + direction);
        Debug.Log("last direction : " + lastDirection);

        if ((lastDirection != Vector3Int.zero) && (!obstacles.Contains(nextRoot) || (nextTile == intersect_empty && !vertical(direction))) && nextTile != unidirect_NE
        && nextTile != unidirect_NW && nextTile != unidirect_SE && nextTile != unidirect_SW
        ) yield return ChangeDirection(direction);

        while (true)
        {
            Debug.Log(currentPosition);
            nextTile = tilemap.GetTile(currentPosition + direction);
            nextRoot = rootTilemap.GetTile(currentPosition + direction);
            if (nextTile != null) Debug.Log(nextTile.name);
            if (isPlacing && nextTile == grass) currentPosition += direction;
            if (obstacles.Contains(nextTile) || (obstacles.Contains(nextRoot) && nextTile != intersect_empty))
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
                if (vertical(direction))
                {
                    if (direction == Vector3Int.up) yield return PlaceTile(tileType.straight_V_up, currentPosition);
                    else yield return PlaceTile(tileType.straight_V_down, currentPosition);
                }
                else
                {
                    if (direction == Vector3Int.right) yield return PlaceUnderTile(tileType.under_right, currentPosition);
                    else yield return PlaceUnderTile(tileType.under_left, currentPosition);
                }
            }
            else if (nextTile == intersect_H)
            {
                if (vertical(direction))
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                currentPosition += direction;
                if (direction == Vector3Int.right) yield return PlaceUnderTile(tileType.under_right, currentPosition);
                else yield return PlaceUnderTile(tileType.under_left, currentPosition);
            }
            else if (nextTile == intersect_V)
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
                if (lastDirection != Vector3Int.zero && lastDirection != direction) yield return ChangeDirection(direction);
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
                if (lastDirection != Vector3Int.zero && lastDirection != direction) yield return ChangeDirection(direction);
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
                Debug.Log("UNIDIRECT NW");
                Debug.Log(direction);
                if (direction == Vector3Int.up || direction == Vector3Int.left)
                {
                    Debug.Log("backoff");
                    yield return Backoff(currentPosition);
                    break;
                }
                if (lastDirection != Vector3Int.zero && lastDirection != direction) yield return ChangeDirection(direction);
                currentPosition += direction;
                if (direction == Vector3Int.down)
                {
                    Debug.Log("down");
                    direction = Vector3Int.left;
                    yield return PlaceTile(tileType.corner_NW_down, currentPosition);
                }
                else
                {
                    Debug.Log("right");
                    direction = Vector3Int.up;
                    yield return PlaceTile(tileType.corner_NW_right, currentPosition);
                }
            }
            else if (nextTile == unidirect_SW)
            {
                if (direction == Vector3Int.down || direction == Vector3Int.left)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                if (lastDirection != Vector3Int.zero && lastDirection != direction) yield return ChangeDirection(direction);
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
            lastDirection = direction;
        }
        Debug.Log("final position : " + currentPosition);
        lastDirection = direction;
        gameState = state.controlling;
        isPlacing = false;
    }

    IEnumerator Backoff(Vector3Int position)
    {
        SoundManager.PlaySfx(transform, sfx.wallCollide);
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
                return anims_corner_NW_right[Random.Range(0, anims_corner_NW_right.Count)];

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

            case tileType.under_right:
                return tunnel_right[0];

            case tileType.under_left:
                return tunnel_left[0];


            default:
                return anims_root_Vdown[0];
        }
    }

    IEnumerator PlaceUnderTile(tileType type, Vector3Int position)
    {
        lastAnim = TileSwitch(type);
        List<TileBase> tiles = lastAnim.anim;
        for (int i = 0; i < tiles.Count; i++)
        {
            tilemap.SetTile(position, tiles[i]);
            yield return frameDuration;
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
        GameObject obj = Instantiate(vegetables_grown[vegeIndex], currentSeed.transform.position + Vector3.down, Quaternion.identity);
        if (vegeIndex == 2) obj.transform.position += 0.5f * Vector3.up;
        if (vegeIndex == 1) obj.transform.position += Vector3.up;
        Destroy(currentSeed);
        Debug.Log("end point reached");
        nbEndPoints--;
        if (nbEndPoints <= 0) Win();
        else
        {
            SoundManager.PlaySfx(transform, sfx.endPoint);
            Debug.Log("placing");
            gameState = state.placing;
            isPlacing = true;
            lastDirection = Vector3Int.zero;
            placeArrow.SetActive(true);
        }
    }

    void Win()
    {
        SoundManager.PlaySfx(transform, sfx.endLevel);
        Cursor.visible = true;
        hasWon = true;
        winScreen.SetActive(true);
        winScript.SelectNext();
    }

    IEnumerator ChangeDirection(Vector3Int direction)
    {
        Debug.Log("last direction : " + lastDirection);
        Debug.Log("current direction : " + direction);
        if (direction == Vector3Int.down)
        {
            if (lastDirection == Vector3Int.right) yield return PlaceTile(tileType.corner_SW_right, currentPosition);
            else yield return PlaceTile(tileType.corner_SE_left, currentPosition);
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
