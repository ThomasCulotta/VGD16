using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    public struct GridSpace
    {
        // Simple x,y coordinates for movement (on x,z plane)
        public Vector3 coordinates;
        // Determines if this space will hide the player
        public bool hidden;

        public float distance;
    };

    private float max_Health = 100f;
    public  float curr_Health = 100f;
    private int max_Cargo = 100;
    //If we ever decide for cargo size
    private int curr_Cargo = 100;
    // Used for determining what happens to cargo
    private int cargoResult = 0;
    private const int MAX_MOVE = 10;


    // Used to track visited/obstructed spaces during player BFS
    public static bool[,] BlockedGrid;
    // List of spaces the player can move to and whether or not that space will hide the player
    private GridSpace[,] PlayerGrid;
    private Queue BFSQueue;
    private float BFSDelay;


    private bool EnvironmentStart = true;
    private bool PlayerStart = true;

    public Slider cargoBar;
    public GameObject healthBar;
    public Text cargoText;

    void Start()
    {
        // Force player to grid
        transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        curr_Cargo = max_Cargo; //set cargo to maximum capacity
        //InvokeRepeating("decreaseHealth", 1f, 1f); just for testing purposes, this decreases health by 2 every second
        //SetCargoBar(curr_Cargo);
    }

    void Update()
    {
        Debug.Log(GameMaster.CurrentState);
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
                if (PlayerStart)
                {
                    // TODO: Start turn with some kind of indicator/turn number/etc.

                    /* NOTE: BFS that determines available player movement.
                     *       Starts with fresh arrays at the beginning of each turn.
                     *
                     *       In a coroutine to free current process and allow variable 
                     *       speed in populating the grid.
                     */


                    StartCoroutine("GridBFS");
                    
                    PlayerStart = false;
                    EnvironmentStart = true;
                }
                // WASD grid movement
                if (Input.GetKeyDown(KeyCode.W)) MovePlayer("z", transform.position.z, 1f);
                if (Input.GetKeyDown(KeyCode.A)) MovePlayer("x", transform.position.x, -1f);
                if (Input.GetKeyDown(KeyCode.S)) MovePlayer("z", transform.position.z, -1f);
                if (Input.GetKeyDown(KeyCode.D)) MovePlayer("x", transform.position.x, 1f);

                // NOTE: We may want to have an automated end turn after player uses up their AP (or whatever).
                if (Input.GetKeyDown(KeyCode.Escape)) GameMaster.CurrentState = GameMaster.GameState.ENEMY_TURN;

                //SetCargoText();//Setting cargo amount text
            }
            break;

            case (GameMaster.GameState.ENVIRONMENT_TURN):
            {
                if (EnvironmentStart)
                {
                    // TODO: Start turn with some kind of indicator maybe
                    EnvironmentStart = false;
                    PlayerStart = true;
                }

                // TODO: Timer to end ENVIRONMENT_TURN
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
            }
            break;
        }
    }

    IEnumerator GridBFS()
    {
        PlayerGrid  = new GridSpace[MAX_MOVE*2 + 1, MAX_MOVE*2 + 1];
        BlockedGrid = new bool[MAX_MOVE*2 + 1, MAX_MOVE*2 + 1];
        BFSQueue    = new Queue();
        BFSDelay    = 0.001f;
        // Set player position to blocked immediately
        BlockedGrid[MAX_MOVE, MAX_MOVE] = true;
        GridSpace playerSpace = new GridSpace {coordinates = transform.position};
        BFSQueue.Enqueue(playerSpace);
        bool toggleBool = true;
        while (BFSQueue.Count != 0)
        {
            float curDelay = BFSDelay * (5.0f / BFSQueue.Count);
            if (toggleBool) yield return new WaitForSeconds(curDelay);
            toggleBool = !toggleBool;

            GridSpace cur = (GridSpace)BFSQueue.Dequeue();
            // Check the cardinal spaces around cur
            CheckAndAddAvailBFS(cur.coordinates.x,     cur.coordinates.z + 1, cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x,     cur.coordinates.z - 1, cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z,     cur.distance);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z,     cur.distance);
            // Check the diagonal spaces around cur
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z + 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z - 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x + 1, cur.coordinates.z - 1, cur.distance + 0.4f);
            CheckAndAddAvailBFS(cur.coordinates.x - 1, cur.coordinates.z + 1, cur.distance + 0.4f);
        }
    }

    // Adds to queue if not visited/obstructed and if it's in MAX_MOVE range of player
    void CheckAndAddAvailBFS(float x, float y, float dist)
    {
        // Create world space and array space vectors
        Vector2 pos = new Vector2(x, y);
        Vector2 posOffset = new Vector2(x - transform.position.x + MAX_MOVE, y - transform.position.z + MAX_MOVE);
        if (dist < MAX_MOVE)
        {
            if (!BlockedGrid[(int)posOffset.x, (int)posOffset.y])   // Check for not visited
            {
                BlockedGrid[(int)posOffset.x, (int)posOffset.y] = true;
                // Check for collision
                if (!Physics.CheckBox(new Vector3(pos.x, 0f, pos.y), new Vector3(0.499f, 1f, 0.499f), Quaternion.identity))
                {
                    bool hiddenSpace = false;
                    // Check for stealth trigger
                    Collider[] triggerArray = Physics.OverlapBox(pos, new Vector3(0.4f, 0f, 0.4f), Quaternion.identity);
                    if (triggerArray.Length < 0)
                    {
                        /* 
                         * NOTE: Loop will help catch unnecessary triggers.
                         *       Won't really be necessary in "release" build.
                         */
                        for (int i = 0; i < triggerArray.Length; i++)
                        {
                            if (triggerArray[i].CompareTag("Stealth")) hiddenSpace = true;
                            else return;
                        }
                    }
                    // Everything is good: make space available for the player, and add it to the queue to be checked
                    GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    debugCube.transform.position = new Vector3(pos.x, -0.45f, pos.y);
                    iTween.MoveBy(debugCube, iTween.Hash("y", 0.45f, "time", 0.1f));
                    GridSpace newSpace = new GridSpace {coordinates = new Vector3(pos.x, 0f, pos.y), 
                                                        hidden = hiddenSpace, 
                                                        distance = dist + 1};
                    PlayerGrid[(int)posOffset.x, (int)posOffset.y] = newSpace;
                    if (dist + 1 <= 10)
                        BFSQueue.Enqueue(newSpace);
                }
                else Debug.Log("Triggered");
            }
            else if (PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance > (dist + 1))
            {
                PlayerGrid[(int)posOffset.x, (int)posOffset.y].distance = dist + 1;
            }
        }
    }

    void MovePlayer(string axis, float position, float ammount)
    {
        iTween.MoveTo(gameObject, iTween.Hash(axis, position + ammount, "time", 0.5f));
        transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
    }

    public void decreaseHealth()
    {
        curr_Health -= 2f; // whatever happens to player we decrease health

        //need a ratio to from current health & max health to scale the hp bar
        float calc_Health = curr_Health / max_Health;
        setHealthBar(calc_Health);
    }

    public void setHealthBar(float healthVal)
    {
        //health value needs to be from 0 to 1
        healthBar.transform.localScale = new Vector3(healthVal, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    public void SetCargoBar(float cargo)
    {
        cargoBar.value = cargo;
    }

    public void AdjustCargo()
    {
        System.Random rand = new System.Random();
        int randVal;

        switch (cargoResult)
        {
            case 1: // Good negotiation, pirates are happy
                randVal = rand.Next(1, 5)*6;
                CargoSub(randVal);
                break;
            case 2: // Decent negotiation, pirates are alright with you
                randVal = rand.Next(2, 6) * 7;
                CargoSub(randVal);
                break;
            case 3: // Bad negotiation, pirates hate you
                randVal = rand.Next(3, 8) * 8;
                CargoSub(randVal);
                break;
            case 4: // You've run into the space police and were caught, but let go; cargo forfeited
                curr_Cargo = 0;
                break;
            case 5: // Run into space police or pirates and tried to run but got shot so lost some cargo
                randVal = rand.Next(2,5)*10;
                CargoSub(randVal);
                break;
            case 6: // Attempted recovery of cargo after being shot
                randVal = rand.Next(2, 5) * 5;
                CargoAdd(randVal);
                break;
            case 7: // Found random cargo near asteroid or something and went to loot it
                randVal = rand.Next(3, 6) * 6;
                CargoAdd(randVal);
                break;
            //Do I need a default case?
        }
    }

    void CargoSub(int randVal)
    {
        int min_limit = 0;
        int cargo_calc = curr_Cargo - randVal;

        if (cargo_calc >= min_limit)
            curr_Cargo = cargo_calc;
        else
            curr_Cargo = min_limit;
    }

    void CargoAdd(int randVal)
    {
        int max_limit = 100;
        int cargo_calc = curr_Cargo + randVal;

        if (cargo_calc <= max_limit)
            curr_Cargo = cargo_calc;
        else
            curr_Cargo = max_limit;
    }

    void SetCargoText()
    {
        cargoText.text = "Cargo: " + curr_Cargo.ToString();
    }
}
