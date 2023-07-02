using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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

public class Player : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap rootTilemap;
    [SerializeField] Tilemap rootOverlayTilemap;
    [SerializeField] Tilemap rootBridgeOverlayTilemap;
    [Header("UI")]
    [SerializeField] GameObject placeArrow;
    [SerializeField] GameObject UI;
    TextMeshProUGUI nbSeedsDisplay;
    SpriteRenderer placeArrowImage;
    Animator UIAnimator;
    Sprite spriteCanPlace;
    [SerializeField] Sprite spriteCannotPlace;
    [Header("Map")]
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
    //Controls controls;
    //PauseControls pauseControls;
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
    [SerializeField] TileBase herb3;
    [SerializeField] List<GameObject> vegetables_seeds;
    [SerializeField] List<GameObject> vegetables_grown;
    [SerializeField] GameObject winScreen;
    GameObject currentSeed;
    int vegeIndex;
    bool hasWon = false;

    TileAnim lastAnim;
    public static Player instance;
    [SerializeField] PauseMenu winScript;
    [SerializeField] PauseMenu pauseScript;
    WaitForSeconds pressDuration = new WaitForSeconds(0.3f);
    Dictionary<TileBase, TileAnim> tileToTileAnim = new Dictionary<TileBase, TileAnim>();
    List<Vector3Int> rootPositions = new List<Vector3Int>();

    private void Awake()
    {
        instance = this;

        Time.timeScale = 1f;

        Cursor.visible = false;


        //controls = new Controls();
        /*
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
        */

        int minPos = tilemap.cellBounds.xMin;
        int maxPos = tilemap.cellBounds.xMax;

        int size = maxPos - minPos;
        int middlePos = size / 2;


        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = tilemap.GetTile<Tile>(pos);
            if (tile == endPoint)
            {
                nbEndPoints++;
            }
        }

        float xPos = mapSize.x % 2 == 0 ? 0.5f : 0f;  //offset arrow if number of tiles is even to make it line up with tiles
        placeArrow = Instantiate(placeArrow);
        placeArrow.transform.position = new Vector2(xPos, 3.5f);

        UI = Instantiate(UI);
        nbSeedsDisplay = UI.GetComponentInChildren<TextMeshProUGUI>();
        nbSeedsDisplay.text = nbEndPoints + "";
        UI.GetComponent<Canvas>().worldCamera = Camera.main;
        placeArrowImage = placeArrow.GetComponentInChildren<SpriteRenderer>();
        spriteCanPlace = placeArrowImage.sprite;
        UIAnimator = UI.GetComponent<Animator>();

        tileToTileAnim[anims_root_Vdown[0].anim[3]] = anims_root_Vup[1];
        tileToTileAnim[anims_root_Vup[0].anim[3]] = anims_root_Vdown[1];
        tileToTileAnim[anims_root_Hright[0].anim[3]] = anims_root_Hleft[1];
        tileToTileAnim[anims_root_Hleft[0].anim[3]] = anims_root_Hright[1];

        tileToTileAnim[anims_corner_NE_down[0].anim[3]] = anims_corner_NE_left[1];
        tileToTileAnim[anims_corner_NE_left[0].anim[3]] = anims_corner_NE_down[1];
        tileToTileAnim[anims_corner_NW_down[0].anim[3]] = anims_corner_NW_right[1];
        tileToTileAnim[anims_corner_NW_right[0].anim[3]] = anims_corner_NW_down[1];

        tileToTileAnim[anims_corner_SE_left[0].anim[3]] = anims_corner_SE_up[1];
        tileToTileAnim[anims_corner_SE_up[0].anim[3]] = anims_corner_SE_left[1];
        tileToTileAnim[anims_corner_SW_right[0].anim[3]] = anims_corner_SW_up[1];
        tileToTileAnim[anims_corner_SW_up[0].anim[3]] = anims_corner_SW_right[1];

        tileToTileAnim[tunnel_left[0].anim[3]] = tunnel_left[1];
        tileToTileAnim[tunnel_right[0].anim[3]] = tunnel_right[1];
    }


    public void Pause()
    {
        if (hasWon) return;
        if (gamePaused)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            pauseWindow.SetActive(false);
            gamePaused = false;
            //controls.Enable();
            SoundManager.ResumeTime();
        }
        else
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            pauseWindow.SetActive(true);
            pauseScript.SelectNext();
            gamePaused = true;
            //controls.Disable();
            SoundManager.StopTime();
        }
    }

    private void OnDisable()
    {
        //controls.Disable();
        //pauseControls.Disable();
    }

    public void Restart()
    {
        SoundManager.StopRoot();
        //controls.Disable();
        //pauseControls.Disable();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void StartMoveRight()
    {
        if (gameState == state.placing) StartCoroutine(nameof(ContinueMoveRight));
    }

    public void StopMoveRight()
    {
        StopCoroutine(nameof(ContinueMoveRight));
    }

    IEnumerator ContinueMoveRight()
    {
        while (true)
        {
            MoveRightPlace();
            yield return pressDuration;
        }
    }

    public void StartMoveLeft()
    {
        if (gameState == state.placing) StartCoroutine(nameof(ContinueMoveLeft));
    }

    public void StopMoveLeft()
    {
        StopCoroutine(nameof(ContinueMoveLeft));
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
        UpdatePlaceArrowSprite();
    }

    void MoveLeftPlace()
    {
        if (placeArrow.transform.position.x - 1 <= -mapSize.x * 0.5f) return;
        placeArrow.transform.position += Vector3.left;
        UpdatePlaceArrowSprite();
    }

    void UpdatePlaceArrowSprite()
    {
        currentPosition = tilemap.WorldToCell(placeArrow.transform.position);
        if (canMove(Vector3Int.down)) placeArrowImage.sprite = spriteCanPlace;
        else placeArrowImage.sprite = spriteCannotPlace;
    }



    public void MoveRight()
    {
        if (gameState == state.controlling) TryFillDirection(Vector3Int.right);
    }

    public void MoveLeft()
    {
        if (gameState == state.controlling) TryFillDirection(Vector3Int.left);
    }

    public void MoveDown()
    {
        if (gameState == state.placing)
        {
            lastDirection = Vector3Int.zero;
            currentPosition = tilemap.WorldToCell(placeArrow.transform.position);
            TryFillDirection(Vector3Int.down);
        }
        else if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.down);
        }
    }

    public void MoveUp()
    {
        if (gameState == state.controlling)
        {
            TryFillDirection(Vector3Int.up);
        }
    }

    bool canMove(Vector3Int direction)
    {
        TileBase currentTile = tilemap.GetTile(currentPosition);

        if (currentTile == unidirect_NE || currentTile == unidirect_NW ||
        currentTile == unidirect_SE || currentTile == unidirect_SW ||
        currentTile == unidirect_H || currentTile == unidirect_V ||
        currentTile == intersect_empty || currentTile == intersect_V ||
        currentTile == intersect_full
        ) return false;

        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        if ((nextTile == unidirect_H && vertical(direction)) ||
            (nextTile == unidirect_V && !vertical(direction))
            ) return false;
        if (isPlacing && (nextTile == grass || nextTile == herb2))
        {
            currentPosition += direction;
            nextTile = tilemap.GetTile(currentPosition + direction);
        }


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
            ) return false;

        return true;
    }

    void TryFillDirection(Vector3Int direction)
    {
        if (!canMove(direction)) return;
        if (isPlacing)
        {
            tilemap.SetTile(currentPosition, herb2);
            //Plant seed
            vegeIndex = Random.Range(0, vegetables_seeds.Count);
            currentSeed = Instantiate(vegetables_seeds[vegeIndex], placeArrow.transform.position + Vector3.up, Quaternion.identity);
            StopMoveRight();
            StopMoveLeft();
            isPlacing = false;
        }
        StartCoroutine(nameof(FillLine), direction);
    }

    bool vertical(Vector3Int direction)
    {
        return direction == Vector3Int.up || direction == Vector3Int.down;
    }


    IEnumerator FillLine(Vector3Int direction)
    {
        SoundManager.PlayRoot();

        placeArrow.SetActive(false);
        gameState = state.animating;
        TileBase nextTile = tilemap.GetTile(currentPosition + direction);
        TileBase nextRoot = rootTilemap.GetTile(currentPosition + direction);

        if ((lastDirection != Vector3Int.zero) && (!obstacles.Contains(nextRoot) || (nextTile == intersect_empty && !vertical(direction))) && nextTile != unidirect_NE
        && nextTile != unidirect_NW && nextTile != unidirect_SE && nextTile != unidirect_SW
        ) yield return ChangeDirection(direction);

        while (true)
        {
            nextTile = tilemap.GetTile(currentPosition + direction);
            nextRoot = rootTilemap.GetTile(currentPosition + direction);
            if (nextRoot != null) Debug.Log(nextRoot.name);
            else Debug.Log("empty tile");
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
                StopCoroutine(nameof(FillLine));
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
                if (direction == Vector3Int.up || direction == Vector3Int.left)
                {
                    yield return Backoff(currentPosition);
                    break;
                }
                if (lastDirection != Vector3Int.zero && lastDirection != direction) yield return ChangeDirection(direction);
                currentPosition += direction;
                if (direction == Vector3Int.down)
                {
                    direction = Vector3Int.left;
                    yield return PlaceTile(tileType.corner_NW_down, currentPosition);
                }
                else
                {
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
        lastDirection = direction;
        gameState = state.controlling;
        isPlacing = false;
        SoundManager.StopRoot();
    }

    #region tilePlacing

    IEnumerator Backoff(Vector3Int position)
    {
        SoundManager.StopRoot();
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
                return anims_root_Vdown[0];

            case tileType.straight_V_up:
                return anims_root_Vup[0];

            case tileType.straight_H_right:
                return anims_root_Hright[0];

            case tileType.straight_H_left:
                return anims_root_Hleft[0];

            case tileType.corner_NE_down:
                return anims_corner_NE_down[0];

            case tileType.corner_NE_left:
                return anims_corner_NE_left[0];

            case tileType.corner_NW_down:
                return anims_corner_NW_down[0];

            case tileType.corner_NW_right:
                return anims_corner_NW_right[0];

            case tileType.corner_SE_up:
                return anims_corner_SE_up[0];

            case tileType.corner_SE_left:
                return anims_corner_SE_left[0];

            case tileType.corner_SW_up:
                return anims_corner_SW_up[0];

            case tileType.corner_SW_right:
                return anims_corner_SW_right[0];

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
        rootPositions.Add(position);
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
        rootPositions.Add(position);
        lastAnim = TileSwitch(type);
        List<TileBase> tiles = lastAnim.anim;
        for (int i = 0; i < tiles.Count; i++)
        {
            rootTilemap.SetTile(position, tiles[i]);
            yield return frameDuration;
        }
    }

    void DecrementSeedCounter()
    {
        nbEndPoints--;
        nbSeedsDisplay.SetText(nbEndPoints + "");
    }

    void EndPointReached()
    {
        rootPositions.Add(currentPosition);
        SoundManager.StopRoot();
        GameObject obj = Instantiate(vegetables_grown[vegeIndex], currentSeed.transform.position + Vector3.down, Quaternion.identity);
        if (vegeIndex == 2) obj.transform.position += 0.5f * Vector3.up;
        if (vegeIndex == 1) obj.transform.position += Vector3.up;
        Destroy(currentSeed);
        DecrementSeedCounter();
        StartCoroutine(nameof(IterateOverRoot));
        if (nbEndPoints <= 0) Win();
        else
        {
            SoundManager.PlaySfx(transform, sfx.endPoint);
            gameState = state.placing;
            isPlacing = true;
            lastDirection = Vector3Int.zero;
            placeArrowImage.sprite = spriteCannotPlace;
            placeArrow.SetActive(true);
        }
    }

    IEnumerator IterateOverRoot()
    {
        rootPositions.Reverse();
        Vector3Int previousPosition = 99999 * Vector3Int.one;
        foreach (Vector3Int rootPosition in rootPositions)
        {
            TileBase root = rootTilemap.GetTile(rootPosition);
            if (root == null) root = tilemap.GetTile(rootPosition);
            if (!tileToTileAnim.ContainsKey(root))
            {
                previousPosition = rootPosition;
                continue;
            }
            TileAnim greenRootAnim;

            if (!vertical(rootPosition - previousPosition) && (tilemap.GetTile(rootPosition) == tunnel_left[0].anim[3] ||
            tilemap.GetTile(rootPosition) == tunnel_right[0].anim[3]))
            {
                root = tilemap.GetTile(rootPosition);
                greenRootAnim = tileToTileAnim[root];
                foreach (TileBase tile in greenRootAnim.anim)
                {
                    tilemap.SetTile(rootPosition, tile);
                    yield return frameDuration;
                }
            }
            else
            {
                greenRootAnim = tileToTileAnim[root];
                foreach (TileBase tile in greenRootAnim.anim)
                {
                    rootOverlayTilemap.SetTile(rootPosition, tile);
                    yield return frameDuration;
                }
            }
            previousPosition = rootPosition;
        }
        tilemap.SetTile(rootPositions[^1] + Vector3Int.up, herb3);
        rootPositions = new List<Vector3Int>();
    }

    void Win()
    {
        UIAnimator.SetTrigger("kill");
        SoundManager.PlaySfx(transform, sfx.endLevel);
        Cursor.visible = true;
        hasWon = true;
        winScreen.SetActive(true);
        winScript.SelectNext();
    }

    IEnumerator ChangeDirection(Vector3Int direction)
    {
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
    #endregion
}
