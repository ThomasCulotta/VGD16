using UnityEngine;
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
    public int turnCount = 0;
    private IsoCamera camNullScript;
    public  static int playerMoveCount = 0;
    private bool playerStart = true;

    // public static so GridSquare can color accordingly
    public static bool hyperJumping   = false;

    private bool selectingEnemy = false;
    private bool holdMoves      = false;
    private bool newMove        = false;
    public  bool imHidden       = false;
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
    private GridSquare[,]  PlayerGrid;
    private GridSquare     moveEndSpace;
    private ArrayList      gridPlanes;

    public  GameObject gridPlane;
    private int  MAX_MOVE = 10;

    private ArrayList moveList;
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

    ///////////////
    // Sound
    //////////////
    private new AudioSource audio;

    void Start()
    {
        camNullScript = Camera.main.gameObject.transform.parent.GetComponent<IsoCamera>();
        nextPosition = curPosition = transform.position;
        // Force player to grid
        transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
        gridPlanes = new ArrayList();

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        //curr_Cargo = max_Cargo; //set cargo to maximum capacity
        //InvokeRepeating("decreaseHealth", 1f, 1f); just for testing purposes, this decreases health by 2 every second
        //SetCargoBar(curr_Cargo);

        GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
        hudScript = HUD.GetComponent<HUDScript>();
        audio = gameObject.AddComponent<AudioSource>();

        switch (SpawnMaster.CURRENT_STATE)
        {
            case (SpawnMaster.SpawnState.SMALL):
                {
                    max_Cargo = SpawnMaster.SMALL_CARGO;
                }
                break;
            case (SpawnMaster.SpawnState.MEDIUM):
                {
                    max_Cargo = SpawnMaster.MEDIUM_CARGO;
                }
                break;
            case (SpawnMaster.SpawnState.LARGE):
                {
                    max_Cargo = SpawnMaster.LARGE_CARGO;
                }
                break;
        }

    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward);
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
                    turnCount++;
                    hudScript.SetTurnText(turnCount);
                    playerStart = false;
                    if (imCloaked)
                    {
                        imCloaked = false;
                        MAX_MOVE = 5;
                    }
                    else
                    {
                        MAX_MOVE = 10;
                        if (imHidden)
                        {
                            if (Random.Range(0, 10) >= 3)
                                decreaseHealth();
                        }
                    }
                    imHidden = false;
                    newMove = true;
                    playerMoveCount = 2;
                    if (hyperCoolDown > 0)
                        hudScript.IconUpdate(1, --hyperCoolDown);
                    if (empCoolDown > 0)
                        hudScript.IconUpdate(2, --empCoolDown);
                    if (cloakingCoolDown > 0)
                        hudScript.IconUpdate(3, --cloakingCoolDown);
                }
                else if (destReached)
                {
                    GameMaster.CurrentState = GameMaster.GameState.GAME_WIN;
                    break;
                }
                if (newMove)
                {
                    newMove  = false;

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
                                hudScript.IconUpdate(2, empCoolDown);
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
                                hyperCoolDown = 4;
                                hudScript.IconUpdate(1, hyperCoolDown);
                                for (int i = 0; i < gridPlanes.Count; i++)
                                    Destroy((GameObject)gridPlanes[i]);
                                gridPlanes.Clear();
                            }
                            else
                            {
                                holdMoves = true;
                                curPosition = nextPosition;
                                moveList = new ArrayList();
                                Pathfind(nextPosition);
                                MovePlayer();
                                audio.clip = AudioController.effect[3];
                                audio.Play();
                            }
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
                            hudScript.IconUpdate(3, cloakingCoolDown);
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
                Debug.LogWarning("game win");
                GameMaster.CurrentState = GameMaster.GameState.GAME_START;
                SceneManager.LoadScene(0);
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
                            if (triggerArray[i].tag.Equals("Cargo"))
                                cargoSpace = true;
                            if (triggerArray[i].tag.Equals("Destination"))
                                destSpace = true;
                        }
                    }

                    // Everything is good: make space available for the player, and add it to the queue to be checked
                    GameObject tempGrid = (GameObject)GameObject.Instantiate(gridPlane, new Vector3(x, 0f, z), Quaternion.identity);
                    GridSpace  tempGS = tempGrid.GetComponent<GridSpace>();

                    Color gridColor = new Color(50f / 255f, 150f / 255f, 240f / 255f);

                    if (hyperJumping)
                        gridColor = Color.yellow;
                    if (hiddenSpace)
                        gridColor = Color.grey;
                    if (cargoSpace)
                        gridColor = Color.white;
                    if (destSpace)
                        gridColor = Color.green;

                    tempGS.baseColor = gridColor;
                        
                    gridPlanes.Add(tempGrid);

                    GridSquare newSpace = new GridSquare
                    {
                        coordinates = new Vector3(x, 0f, z),
                        hidden = hiddenSpace,
                        cargo = cargoSpace,
                        distance = dist + 1,
                        destination = destSpace
                    };

                    float gridMod = Mathf.Pow(newSpace.distance, 0.1f);

                    tempGrid.transform.localScale /= gridMod;
                    BoxCollider col = tempGrid.GetComponent<BoxCollider>();
                    col.size = new Vector3(col.size.x * gridMod, col.size.y, col.size.z * gridMod);

                    PlayerGrid[(int)posOffset.x, (int)posOffset.y] = newSpace;
                    if (dist + 1 <= 10)
                        BFSQueue.Enqueue(newSpace);
                }
                else
                {
                    GridSquare newSpace = new GridSquare
                    {
                        coordinates = new Vector3(x, 0f, z),
                        hidden = false,
                        cargo = false,
                        distance = 20,
                        destination = false
                    };

                    PlayerGrid[(int)posOffset.x, (int)posOffset.y] = newSpace;
                }
            }
            else if (PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance > (dist + 1))
            {
                PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance = dist + 1;
            }
        }
        else if (posOffset.x <= MAX_MOVE * 2 &&
                posOffset.y <= MAX_MOVE * 2)
        {
            GridSquare newSpace = new GridSquare
            {
                coordinates = new Vector3(x, 0f, z),
                hidden = false,
                cargo = false,
                distance = 20,
                destination = false
            };

            PlayerGrid[(int)posOffset.x, (int)posOffset.y] = newSpace;
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
        if (hiY > MAX_MOVE * 2) hiY = MAX_MOVE * 2;
        if (hiX > MAX_MOVE * 2) hiX = MAX_MOVE * 2;
        if (loY < 0) loY = 0;
        if (loX < 0) loX = 0;

        if (PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance > 1.4f)
        {
            GridSquare closestSpace = PlayerGrid[meX, meY];

            // Check cardinal spaces first
            if (PlayerGrid[meX, loY].distance <= closestSpace.distance && PlayerGrid[meX, loY].distance > 0) closestSpace = PlayerGrid[meX, loY];
            if (PlayerGrid[meX, hiY].distance <= closestSpace.distance && PlayerGrid[meX, hiY].distance > 0) closestSpace = PlayerGrid[meX, hiY];
            if (PlayerGrid[hiX, meY].distance <= closestSpace.distance && PlayerGrid[hiX, meY].distance > 0) closestSpace = PlayerGrid[hiX, meY];
            if (PlayerGrid[loX, meY].distance <= closestSpace.distance && PlayerGrid[loX, meY].distance > 0) closestSpace = PlayerGrid[loX, meY];

            // Check diagonals
            if (PlayerGrid[hiX, hiY].distance <= closestSpace.distance && PlayerGrid[hiX, hiY].distance > 0) closestSpace = PlayerGrid[hiX, hiY];
            if (PlayerGrid[hiX, loY].distance <= closestSpace.distance && PlayerGrid[hiX, loY].distance > 0) closestSpace = PlayerGrid[hiX, loY];
            if (PlayerGrid[loX, hiY].distance <= closestSpace.distance && PlayerGrid[loX, hiY].distance > 0) closestSpace = PlayerGrid[loX, hiY];
            if (PlayerGrid[loX, loY].distance <= closestSpace.distance && PlayerGrid[loX, loY].distance > 0) closestSpace = PlayerGrid[loX, loY];

            if (closestSpace.distance < PlayerGrid[meX, meY].distance)
                Pathfind(closestSpace.coordinates);
        }
    }

    void MovePlayer()
    {
        // Translate to next GridSquare
        if (moveList.Count > 0)
        {
            GridSquare finalSquare = (GridSquare)moveList[0];
            GridSquare curSpace = new GridSquare();
            curSpace.coordinates = transform.position;
            Vector3 curDir = Vector3.zero;
            float curDur = 0f;
            RaycastHit hit;
            if (!Physics.SphereCast(transform.position, 0.5f, finalSquare.coordinates - transform.position, 
                                    out hit, Vector3.Distance(finalSquare.coordinates, transform.position), 255, QueryTriggerInteraction.Ignore))
            {
                curDir = finalSquare.coordinates - curSpace.coordinates;
                curDur = Vector3.Distance(finalSquare.coordinates, curSpace.coordinates) * 0.2f;
                curSpace = finalSquare;
                moveEndSpace = curSpace;
                moveList.Clear();
                // 0.2 seconds per meter
            }
            else
            {
                int spaceCount = 0;

                // Set initial space to player pos

                // Check if next space is in same direction if there are still spaces to check
                do
                {
                    GridSquare tempSpace = (GridSquare)moveList[moveList.Count - 1];
                    Vector3 tempDir = tempSpace.coordinates - curSpace.coordinates;
                    // Duration based on cardinal or diagonal direction
                    // 0.2 seconds per meter
                    float durDiff = tempDir.magnitude * 0.2f;

                    // Pop last gridspace in list if it's in the same line as prev spaces or if it's the first pass
                    if (spaceCount == 0 || tempDir == curDir)
                    {
                        curSpace = tempSpace;
                        moveList.RemoveAt(moveList.Count - 1);
                        curDir = tempDir;
                        curDur += durDiff;

                        spaceCount++;
                    }
                    // Break if temp space isn't in line with previous spaces
                    else break;
                }
                while (moveList.Count > 0);

                if (moveList.Count == 0)
                    moveEndSpace = curSpace;
            }

            ArrayList spaceAndDur = new ArrayList();
            spaceAndDur.Add(curSpace);
            spaceAndDur.Add(curDur);
            float yRotationNeeded = Mathf.Acos(Vector3.Dot(transform.forward, curDir.normalized));
            float testX = transform.forward.x * Mathf.Cos(yRotationNeeded) - transform.forward.z * Mathf.Sin(yRotationNeeded);
            float testZ = transform.forward.x * Mathf.Sin(yRotationNeeded) + transform.forward.z * Mathf.Cos(yRotationNeeded);
            Vector3 testVector = new Vector3(testX, transform.forward.y, testZ);
            if (testVector.normalized == curDir.normalized)
                yRotationNeeded = -yRotationNeeded;
            Debug.Log(yRotationNeeded);
            // Move player to the furthest in-line space
            RotatePlayer(yRotationNeeded * 180f / Mathf.PI, spaceAndDur);
        }
        // Snap player to grid
        else
        {
            transform.position = new Vector3(curPosition.x, 0f, curPosition.z);
            playerMoveCount--;

            GridSquare curGrid = moveEndSpace;

            destReached = curGrid.destination;

            if (hyperJumping)
                hyperJumping = false;
            
            if (curGrid.cargo)
            {
                Debug.Log("CARGO GET");

                Collider[] cargoArray = Physics.OverlapBox(new Vector3(curGrid.coordinates.x, 0f, curGrid.coordinates.z),
                                                             new Vector3(0.1f, 10f, 0.1f), Quaternion.identity,
                                                             255, QueryTriggerInteraction.Collide);
                
                for (int i = 0; i < cargoArray.Length; i++)
                    if (cargoArray[i].tag.Equals("Cargo"))
                    {
                        GameObject.Destroy(cargoArray[i].gameObject);
                        break;
                    }
                curr_Cargo++;
                hudScript.CargoUpdate(curr_Cargo, max_Cargo);
                //Add(Random.Range(5, 15), max_Cargo, true);
            }

            imHidden = curGrid.hidden;
            if (imHidden)
                Debug.Log("YAY");

            if (!destReached && playerMoveCount > 0)
            {
                newMove = true;
                holdMoves = false;
            }

            for (int i = 0; i < gridPlanes.Count; i++)
                Destroy((GameObject)gridPlanes[i]);
            gridPlanes.Clear();
        }
    }

    void RotatePlayer(float yRotationNeeded, ArrayList spaceAndDur)
    {
        iTween.RotateAdd(gameObject, iTween.Hash("y", yRotationNeeded, 
                                                 "time", 0.2f, 
                                                 "oncomplete", "TranslatePlayer", 
                                                 "oncompletetarget", gameObject, 
                                                 "oncompleteparams", spaceAndDur));
    }

    void TranslatePlayer(ArrayList spaceAndDur)
    {
        GridSquare curSpace = (GridSquare)spaceAndDur[0];
        float curDur = (float)spaceAndDur[1];
        iTween.MoveTo(gameObject, iTween.Hash("x", curSpace.coordinates.x,
                                              "z", curSpace.coordinates.z,
                                              "time", curDur,
                                              "easetype", "linear",
                                              "oncomplete", "MovePlayer",
                                              "oncompletetarget", gameObject));
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
        curr_Health -= Random.Range(15, 25);
        camNullScript.DamageIndicator();

        //need a ratio to from current health & max health to scale the hp bar
        hudScript.HealthUpdate(curr_Health, max_Health);
        audio.clip = AudioController.effect[2];
        audio.Play();
    }

    public void AdjustCargo()
    {
        /*
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
