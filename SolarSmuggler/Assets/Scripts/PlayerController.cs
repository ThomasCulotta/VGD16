﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public struct GridSquare
    {
        public Vector3 coordinates;
        public bool hidden;
        public bool cargo;
        public float distance;
        public bool destination;
    }

    ///////////////
    // Ship stats
    ///////////////
    public int max_Health  = 100;
    public int curr_Health = 100;

    // If we ever decide for cargo size
    public int max_Cargo  = 100;
    public int curr_Cargo = 0;

    // Used for determining what happens to cargo
    private int cargoResult = 0;

    ///////////////
    // Player turn
    ///////////////
    public  static int playerMoveCount = 0;
    private bool playerStart = true;

    // public static so GridSquare can color accordingly
    public static bool hyperJumping   = false;

    private bool selectingEnemy = false;
    private bool holdMoves      = false;
    private bool newMove        = false;
    public bool imHidden       = false;
    private bool imCloaked      = false;

    private int hyperCoolDown = 0;
    private int empCoolDown = 0;
    private int cloakingCoolDown = 0;

    ///////////////
    // Movement
    ///////////////
    private Queue BFSQueue;
    private float BFSDelay;

    public  static Vector3 nextPosition;
    private Vector3 curPosition;

    public  static bool[,] BlockedGrid;
    private GridSquare[,]   PlayerGrid;
    private ArrayList      gridPlanes;

    public  GameObject gridPlane;
    private int  MAX_MOVE = 10;

    private ArrayList moveList;
    private ArrayList destList;
    private bool destReached = false;

    private bool lastRight;
    private bool lastUp;

    ///////////////
    // Enemy select
    ///////////////
    private ArrayList  selectableEnemies;
    private GameObject curSelectedEnemy;
    private int        curSelectedIndex;
    private ArrayList  affectedEnemies;

    ///////////////
    // Timer
    ///////////////
    private bool timerInit = false;
    public float timer = 2f;

    ///////////////
    // UI
    ///////////////
    public HUDScript hudScript;

    void Start()
    {
        nextPosition = curPosition = transform.position;
        // Force player to grid
        transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
        gridPlanes = new ArrayList();

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        curr_Cargo = max_Cargo; //set cargo to maximum capacity
        //InvokeRepeating("decreaseHealth", 1f, 1f); just for testing purposes, this decreases health by 2 every second
        //SetCargoBar(curr_Cargo);

        GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
        hudScript = HUD.GetComponent<HUDScript>();
    }

    void Update()
    {
        switch (GameMaster.CurrentState)
        {
            case (GameMaster.GameState.GAME_START):
            {
                // TODO: Some level initialization

                GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN;
            }
            break;

            case (GameMaster.GameState.PLAYER_TURN):
            {
                if (playerStart)
                {
                    // TODO: Start turn with some kind of indicator/turn number/etc.

                    /* NOTE: BFS that determines available player movement.
                     *       Clears arrays at the beginning of each turn.
                     *
                     *       In a coroutine to free current process and allow variable
                     *       speed in populating the grid.
                     */
                    playerStart = false;
                    if (imCloaked)
                    {
                        imCloaked = false;
                        imHidden = false;
                        MAX_MOVE = 5;
                    }
                    else
                    {
                        MAX_MOVE = 10;
                        if (imHidden)
                        {
                            imHidden = false;
                            if (Random.Range(0, 10) >= 3)
                                decreaseHealth();
                        }
                    }
                    newMove = true;
                    playerMoveCount = 2;
                    if (hyperCoolDown > 0)
                        hyperCoolDown--;
                    if (empCoolDown > 0)
                        empCoolDown--;
                    if (cloakingCoolDown > 0)
                        cloakingCoolDown--;
                }
                else if (destReached)
                {
                    GameMaster.CurrentState = GameMaster.GameState.GAME_WIN;
                }
                if (newMove)
                {
                    newMove  = false;
                    destList = new ArrayList();

                    StartCoroutine("GridBFS");
                }
                else if (playerMoveCount > 0)
                {
                    if (selectingEnemy)
                    {
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            curSelectedIndex++;
                            if (curSelectedIndex == selectableEnemies.Count)
                                curSelectedIndex = 0;
                            SelectEnemy();
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            curSelectedIndex--;
                            if (curSelectedIndex == -1)
                                curSelectedIndex = selectableEnemies.Count - 1;
                            SelectEnemy();
                        }
                        else if (Input.GetKeyDown(KeyCode.Space))
                        {
                            if (curSelectedEnemy != null)
                            {
                                EnemyLoS curLos = curSelectedEnemy.GetComponent<EnemyLoS>();
                                curLos.GetEMPed();

                                DeselectEnemy();
                                selectingEnemy = false;
                                playerMoveCount--;
                                if (playerMoveCount > 0)
                                    newMove = true;
                                empCoolDown = 4;
                            }
                        }
                        else if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            EnemyLoS curLos = curSelectedEnemy.GetComponent<EnemyLoS>();
                            curLos.Deselect();
                            selectingEnemy = false;
                        }
                    }
                    else if (!holdMoves)
                    {
                        /*
                        * NOTE: If the player has moves left and are not currently in action,
                        * allow them to either move on the grid or use an ability,
                        * or allow them to end the turn.
                        */
                        if (nextPosition != curPosition)
                        {
                            /*
                            * NOTE: Pathfinds backwards from selected GridSquare to player
                            *       iTweens player to each gridspace
                            */
                            if (hyperJumping)
                            {
                                transform.position = nextPosition;
                                curPosition = nextPosition;
                                newMove = true;
                                hyperJumping = false;
                                if (Random.Range(0, 10) >= 3)
                                    decreaseHealth();
                                hyperCoolDown = 3;
                            }
                            else
                            {
                                holdMoves = true;
                                curPosition = nextPosition;
                                moveList = new ArrayList();
                                Pathfind(nextPosition);
                                MovePlayer();
                            }
                            for (int i = 0; i < gridPlanes.Count; i++)
                                Destroy((GameObject)gridPlanes[i]);
                            gridPlanes.Clear();
                        }
                        else if (Input.GetKeyDown(KeyCode.Alpha1) && hyperCoolDown == 0)
                        {
                            // Hyper jump
                            Debug.Log("HYPER JUMPING");
                            if (!hyperJumping)
                            {
                                hyperJumping = true;
                                newMove      = true;
                                for (int i = 0; i < gridPlanes.Count; i++)
                                    Destroy((GameObject)gridPlanes[i]);
                                gridPlanes.Clear();
                            }
                            else
                            {
                                hyperJumping = false;
                                newMove      = true;
                                for (int i = 0; i < gridPlanes.Count; i++)
                                    Destroy((GameObject)gridPlanes[i]);
                                gridPlanes.Clear();
                            }
                        }
                        else if (Input.GetKeyDown(KeyCode.Alpha2) && empCoolDown == 0)
                        {
                            Debug.Log("EMP SELECTION");
                            selectingEnemy = true;

                            if (selectableEnemies == null)
                                GetEnemyListInArea(ref selectableEnemies, transform.position, 10f);

                            if (selectableEnemies.Count > 0)
                            {
                                Debug.Log("Selectable enemies exist.");
                                curSelectedIndex = 0;
                                SelectEnemy();
                            }
                            else
                                selectingEnemy = false;
                        }
                        else if (Input.GetKeyDown(KeyCode.Alpha3) && cloakingCoolDown == 0)
                        {
                            Debug.Log("CLOAKING");
                            // Cloaking
                            imHidden = true;
                            imCloaked = true;
                            playerMoveCount--;
                            if (playerMoveCount > 0)
                                newMove = true;
                            cloakingCoolDown = 4;
                            for (int i = 0; i < gridPlanes.Count; i++)
                                Destroy((GameObject)gridPlanes[i]);
                            gridPlanes.Clear();
                        }
                        else if (Input.GetKeyDown(KeyCode.Alpha0))
                        {
                            // End turn
                            for (int i = 0; i < gridPlanes.Count; i++)
                                Destroy((GameObject)gridPlanes[i]);
                            gridPlanes.Clear();
                            playerMoveCount = 0;
                            playerStart = true;
                            holdMoves = false;
                            timerInit = true;
                            GameMaster.CurrentState = GameMaster.GameState.ENEMY_TURN;
                        }
                    }
                }
                else
                {
                    // End turn
                    playerStart = true;
                    holdMoves = false;
                    timerInit = true;
                    GameMaster.CurrentState = GameMaster.GameState.ENEMY_TURN;
                }
            }
            break;

            case (GameMaster.GameState.ENVIRONMENT_TURN):
            {
                if (timerInit)
                {
                    timerInit = false;
                    StartCoroutine("Timer");
                }
            }
            break;

            case (GameMaster.GameState.GAME_LOSS):
            {
                // TODO: Some game statistics, then main menu or lose scene etc.
            }
            break;

            case (GameMaster.GameState.GAME_WIN):
            {
                // TODO: Some game statistics, then main menu or win scene etc.
                SceneManager.LoadScene(1);
            }
            break;
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds (timer);
        //      Debug.Log("Evironment_TURN -from Orbit");
        GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN; //starts the player's turn
    }

    IEnumerator GridBFS()
    {
        PlayerGrid  = new GridSquare[MAX_MOVE * 2 + 1, MAX_MOVE * 2 + 1];
        BlockedGrid = new bool[MAX_MOVE * 2 + 1, MAX_MOVE * 2 + 1];

        BFSQueue = new Queue();

        // Set player position to blocked immediately
        BlockedGrid[MAX_MOVE, MAX_MOVE] = true;
        GridSquare playerSpace = new GridSquare { coordinates = transform.position };

        BFSQueue.Enqueue(playerSpace);

        while (BFSQueue.Count != 0)
        {
            GridSquare cur = (GridSquare)BFSQueue.Dequeue();

            // Check the cardinal spaces around cur
            CheckAndAddAvailBFS(cur.coordinates.x, cur.coordinates.z + 1, cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x, cur.coordinates.z - 1, cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z, cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z, cur.distance);

            // Check the diagonal spaces around cur
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z + 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z - 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z - 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z + 1, cur.distance + 0.4f);
        }
        yield return null;
    }

    // Adds to queue if not visited/obstructed and if it's in MAX_MOVE range of player
    void CheckAndAddAvailBFS(float x, float z, float dist)
    {
        // Create array space vector
        Vector2 posOffset = new Vector2(x - transform.position.x + MAX_MOVE, z - transform.position.z + MAX_MOVE);

        if (posOffset.x > MAX_MOVE * 2 || posOffset.x < 0 ||
            posOffset.y > MAX_MOVE * 2 || posOffset.y < 0)
            return;

        if (dist < MAX_MOVE)
        {
            if (!BlockedGrid[(int)posOffset.x, (int)posOffset.y])   // Check for not visited
            {
                BlockedGrid[(int)posOffset.x, (int)posOffset.y] = true;

                // Check for collision
                if (!Physics.CheckBox(new Vector3(x, 0f, z),
                                      new Vector3(0.499f, 10f, 0.499f), Quaternion.identity,
                                      255, QueryTriggerInteraction.Ignore))
                {
                    bool hiddenSpace = false;
                    bool cargoSpace  = false;
                    bool destSpace   = false;

                    // Check for stealth trigger
                    Collider[] triggerArray = Physics.OverlapBox(new Vector3(x, 0f, z),
                                                                 new Vector3(0.499f, 10f, 0.499f), Quaternion.identity,
                                                                 255, QueryTriggerInteraction.Collide);
                    if (triggerArray.Length > 0)
                    {
                        /*
                         * NOTE: Loop will help catch unnecessary triggers.
                         *       Won't really be necessary in "release" build.
                         */
                        for (int i = 0; i < triggerArray.Length; i++)
                        {
                            if (triggerArray[i].tag.Equals("Stealth"))
                                hiddenSpace = true;
                            else if (triggerArray[i].tag.Equals("Cargo"))
                                cargoSpace = true;
                            else if (triggerArray[i].tag.Equals("Destination"))
                            {
                                destSpace = true;
                                destList.Add(new Vector3(x, 0f, z));
                            }
                            else return;
                        }
                    }

                    // Everything is good: make space available for the player, and add it to the queue to be checked
                    GameObject tempGrid = (GameObject)GameObject.Instantiate(gridPlane, new Vector3(x, 0f, z), Quaternion.identity);
                    GridSpace  tempGS = tempGrid.GetComponent<GridSpace>();
                    tempGS.baseColor = new Color(50f / 255f, 150f / 255f, 240f / 255f);
                    if (hyperJumping)
                        tempGS.baseColor = Color.yellow;
                    if (hiddenSpace)
                        tempGS.baseColor = Color.grey;
                    if (cargoSpace)
                        tempGS.baseColor = Color.blue;
                    if (destSpace)
                        tempGS.baseColor = Color.green;
                    
                    gridPlanes.Add(tempGrid);

                    GridSquare newSpace = new GridSquare
                    {
                        coordinates = new Vector3(x, 0f, z),
                        hidden = hiddenSpace,
                        cargo = cargoSpace,
                        distance = dist + 1,
                        destination = destSpace
                    };

                    PlayerGrid[(int)posOffset.x, (int)posOffset.y] = newSpace;
                    if (dist + 1 <= 10)
                        BFSQueue.Enqueue(newSpace);
                }
            }
            else if (PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance > (dist + 1))
            {
                PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance = dist + 1;
            }
        }
    }

    void Pathfind(Vector3 curSpace)
    {
        Vector2 posOffset = new Vector2(curSpace.x - transform.position.x + MAX_MOVE, curSpace.z - transform.position.z + MAX_MOVE);
        moveList.Add(PlayerGrid[(int)posOffset.x, (int)posOffset.y]);

        int hiY = (int)posOffset.y + 1;
        int meY = (int)posOffset.y;
        int loY = (int)posOffset.y - 1;

        int hiX = (int)posOffset.x + 1;
        int meX = (int)posOffset.x;
        int loX = (int)posOffset.x - 1;

        // Clamp to bounds. Yeah, this leads to duplicate checks.
        if (hiY > MAX_MOVE * 2) hiY--;
        if (hiX > MAX_MOVE * 2) hiX--;
        if (loY < 0) loY++;
        if (loX < 0) loX++;

        if (PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance > 1.4f)
        {
            // Check cardinal spaces first
            GridSquare closestSpace = PlayerGrid[meX, loY];

            if (PlayerGrid[meX, hiY].distance <= closestSpace.distance && PlayerGrid[meX, hiY].distance > 0) closestSpace = PlayerGrid[meX, hiY];
            if (PlayerGrid[hiX, meY].distance <= closestSpace.distance && PlayerGrid[hiX, meY].distance > 0) closestSpace = PlayerGrid[hiX, meY];
            if (PlayerGrid[loX, meY].distance <= closestSpace.distance && PlayerGrid[loX, meY].distance > 0) closestSpace = PlayerGrid[loX, meY];

            // Check diagonals
            if (PlayerGrid[hiX, hiY].distance <= closestSpace.distance && PlayerGrid[hiX, hiY].distance > 0) closestSpace = PlayerGrid[hiX, hiY];
            if (PlayerGrid[hiX, loY].distance <= closestSpace.distance && PlayerGrid[hiX, loY].distance > 0) closestSpace = PlayerGrid[hiX, loY];
            if (PlayerGrid[loX, hiY].distance <= closestSpace.distance && PlayerGrid[loX, hiY].distance > 0) closestSpace = PlayerGrid[loX, hiY];
            if (PlayerGrid[loX, loY].distance <= closestSpace.distance && PlayerGrid[loX, loY].distance > 0) closestSpace = PlayerGrid[loX, loY];

            Pathfind(closestSpace.coordinates);
        }
    }

    void MovePlayer()
    {
        // Translate to next GridSquare
        if (moveList.Count > 0)
        {
            int curRight = 0;
            int curUp = 0;
            int spaceCount = 0;
            float curDur = 0f;

            // Set initial space to player pos
            GridSquare curSpace = new GridSquare();
            curSpace.coordinates = transform.position;

            // Check if next space is in same direction if there are still spaces to check
            do
            {
                GridSquare tempSpace = (GridSquare)moveList[moveList.Count - 1];
                int tempRight = (int)(tempSpace.coordinates.x - curSpace.coordinates.x);
                int tempUp = (int)(tempSpace.coordinates.z - curSpace.coordinates.z);

                // Pop last gridspace in list if it's in the same line as prev spaces or if it's the first pass
                if (spaceCount == 0 ||
                   (tempRight == curRight && tempUp == curUp))
                {
                    curSpace = tempSpace;
                    moveList.RemoveAt(moveList.Count - 1);
                    curRight = tempRight;
                    curUp = tempUp;
                    curDur += 0.2f;

                    spaceCount++;
                }
                // Break if temp space isn't in line with previous spaces
                else break;
            }
            while (moveList.Count > 0);

            // Move player to the furthest in-line space
            iTween.MoveTo(gameObject, iTween.Hash("x", curSpace.coordinates.x,
                                                  "z", curSpace.coordinates.z,
                                                  "time", curDur,
                                                  "easetype", "linear",
                                                  "oncomplete", "MovePlayer",
                                                  "oncompletetarget", gameObject));
        }
        // Snap player to grid
        else
        {
            transform.position = new Vector3(curPosition.x, 0f, curPosition.z);
            playerMoveCount--;

            Vector2 posOffset = new Vector2(curPosition.x - transform.position.x + MAX_MOVE, curPosition.z - transform.position.z + MAX_MOVE);
            GridSquare curGrid = PlayerGrid[(int)posOffset.x, (int)posOffset.y];

            destReached = curGrid.destination;
            if (hyperJumping)
            hyperJumping = false;

            if (!destReached && playerMoveCount > 0)
            {
                if (!imHidden)
                    imHidden = true;
                if (curGrid.cargo)
                    Add(Random.Range(5, 15), max_Cargo, true);
                
                if (playerMoveCount > 0)
                {
                    newMove = true;
                    holdMoves = false;
                }
            }
        }
    }

    void GetEnemyListInArea(ref ArrayList curList, Vector3 origin, float radius)
    {
        curList = new ArrayList();
        Collider[] rawColliderArray = Physics.OverlapSphere(origin, radius);

        for (int i = 0; i < rawColliderArray.Length; i++)
        {
            if (rawColliderArray[i].tag == "Enemy")
                curList.Add(rawColliderArray[i].gameObject);
        }
    }

    void SelectEnemy()
    {
        Debug.Log("Select new.");

        if (curSelectedEnemy != null && selectableEnemies.Count == 1)
            return;

        if (curSelectedEnemy != null)
        {
            DeselectEnemy();
        }

        curSelectedEnemy = (GameObject)selectableEnemies[curSelectedIndex];
        EnemyLoS curLos = curSelectedEnemy.GetComponent<EnemyLoS>();
        curLos.Select();
    }

    void DeselectEnemy()
    {
        EnemyLoS curLos = curSelectedEnemy.GetComponent<EnemyLoS>();
        curLos.Deselect();
    }

    public void decreaseHealth()
    {
        curr_Health -= Random.Range(5, 15);

        //need a ratio to from current health & max health to scale the hp bar
        hudScript.HealthUpdate(curr_Health, max_Health);
    }

    public void AdjustCargo()
    {
        System.Random rand = new System.Random();
        int randVal;

        switch (cargoResult)
        {
            case 1: // Good negotiation, pirates are happy
                randVal = rand.Next(1, 5) * 6;
                Sub(randVal, true);
                break;
            case 2: // Decent negotiation, pirates are alright with you
                randVal = rand.Next(2, 6) * 7;
                Sub(randVal, true);
                break;
            case 3: // Bad negotiation, pirates hate you
                randVal = rand.Next(3, 8) * 8;
                Sub(randVal, true);
                break;
            case 4: // You've run into the space police and were caught, but let go; cargo forfeited
                curr_Cargo = 0;
                break;
            case 5: // Run into space police or pirates and tried to run but got shot so lost some cargo
                randVal = rand.Next(2, 5) * 10;
                Sub(randVal, true);
                break;
            case 6: // Attempted recovery of cargo after being shot
                randVal = rand.Next(2, 5) * 5;
                Add(randVal, max_Cargo, true);
                break;
            case 7: // Found random cargo near asteroid or something and went to loot it
                randVal = rand.Next(3, 6) * 6;
                Add(randVal, max_Cargo, true);
                break;
                //Do I need a default case?
        }
        /* Not for now
        GameObject HUD = GameObject.FindWithTag("HUD");
        HUDScript HUDScript = HUD.GetComponent<HUDScript> ();
        HUDScript.CargoUpdate(curr_Cargo, max_Cargo);
        */
    }

    void Sub(int randVal, bool isCargo)
    {
        int min_limit = 0;
        int current;

        if (isCargo)
            current = curr_Cargo;
        else
            current = curr_Health;

        int diff = current - randVal;

        if (diff >= min_limit)
            current = diff;
        else
            current = min_limit;

        if (isCargo)
        {
            curr_Cargo = current;
            hudScript.CargoUpdate(curr_Cargo, max_Cargo);
        }
        else
        {
            curr_Health = current;
            hudScript.HealthUpdate(curr_Health, max_Health);
        }
    }

    void Add(int randVal, int max, bool isCargo)
    {
        int max_limit = max;
        int current;

        if (isCargo)
            current = curr_Cargo;
        else
            current = curr_Health;

        int sum = current + randVal;

        if (sum <= max_limit)
            current = sum;
        else
            current = max_limit;

        if (isCargo)
        {
            curr_Cargo = current;
            hudScript.CargoUpdate(curr_Cargo, max_Cargo);
        }
        else
        {
            curr_Health = current;
            hudScript.HealthUpdate(curr_Health, max_Health);
        }

    }
}
