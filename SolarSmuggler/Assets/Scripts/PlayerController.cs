using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public struct GridSpace
    {
        // Simple x,y coordinates for movement (on x,z plane)
        public Vector3 coordinates;
        // Determines if this space will hide the player
        public bool hidden;
    };

    private float max_Health = 100f;
    public  float curr_Health = 100f;
    private float max_Cargo = 100f;
    //If we ever decide for cargo size
    private float curr_Cargo = 100f;
    private const int MAX_MOVE = 10;


    // Used to track visited/obstructed spaces during player BFS
    public static bool[,] BlockedGrid;
    // List of spaces the player can move to and whether or not that space will hide the player
    private ArrayList PlayerGrid;
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
                    PlayerGrid  = new ArrayList();
                    BlockedGrid = new bool[MAX_MOVE*2, MAX_MOVE*2];
                    BFSQueue    = new Queue();
                    BFSDelay    = 0.016f;
                    // Set player position to blocked immediately
                    BlockedGrid[MAX_MOVE, MAX_MOVE] = true;
                    BFSQueue.Enqueue((Vector2)transform.position);

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

                //if (Input.GetKeyDown(KeyCode.M)) DecreaseCargo();For testing
                //if (Input.GetKeyDown(KeyCode.I)) IncreaseCargo();For testing
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
        bool toggleBool = true;
        while (BFSQueue.Count != 0)
        {
            /*
             * TODO: Trying to eliminate variability of grid population speed 
             * when queue is large. Still WIP.
             */
            float curDelay = BFSDelay / BFSQueue.Count;
            if (toggleBool) yield return new WaitForSeconds(curDelay);
            toggleBool = !toggleBool;

            Vector2 cur = (Vector2)BFSQueue.Dequeue();
            // Check the cardinal spaces around cur
            CheckAndAddAvailBFS(cur.x, cur.y + 1);
            CheckAndAddAvailBFS(cur.x, cur.y - 1);
            CheckAndAddAvailBFS(cur.x + 1, cur.y);
            CheckAndAddAvailBFS(cur.x - 1, cur.y);
        }
    }

    // Adds to queue if not visited/obstructed and if it's in MAX_MOVE range of player
    void CheckAndAddAvailBFS(float x, float y)
    {
        // Create world space and array space vectors
        Vector2 pos = new Vector2(x, y);
        Vector2 posOffset = new Vector2(x - transform.position.x + MAX_MOVE, y - transform.position.z + MAX_MOVE);
        if (posOffset.x >= 0 && posOffset.x < MAX_MOVE*2 &&     // In x grid range
            posOffset.y >= 0 && posOffset.y < MAX_MOVE*2 &&     // In y grid range
            !BlockedGrid[(int)posOffset.x, (int)posOffset.y])   // Check for not visited
        {
            BlockedGrid[(int)posOffset.x, (int)posOffset.y] = true;
            // Check for collision
            if (!Physics.CheckBox(pos, new Vector3(0.45f, 0f, 0.45f), Quaternion.identity))
            {
                bool hiddenSpace = false;
                // Check for stealth trigger
                Collider[] triggerArray = Physics.OverlapBox(pos, new Vector3(0.5f, 0f, 0.5f), Quaternion.identity);
                if (triggerArray.Length < 0)
                {
                    /* 
                     * NOTE: Loop will help catch unnecessary triggers.
                     *       Won't really be necessary in "release" build.
                     */
                    for (int i = 0; i < triggerArray.Length; i++)
                    {
                        if (triggerArray[i].CompareTag("Stealth")) hiddenSpace = true;
                        if (i > 1) Debug.Log("Collider " + i + "is:" + triggerArray[i].name);
                    }
                }
                // Everything is good: make space available for the player, and add it to the queue to be checked
                GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugCube.transform.position = new Vector3(pos.x, 0f, pos.y);
                BFSQueue.Enqueue(pos);
                GridSpace newSpace;
                newSpace.coordinates = new Vector3(pos.x, 0f, pos.y);
                newSpace.hidden = hiddenSpace;
                PlayerGrid.Add(newSpace);
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

    public void DecreaseCargo()
    {
        if (curr_Cargo != 0)
        {
            curr_Cargo--;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    public void IncreaseCargo()
    {
        if (curr_Cargo == max_Cargo)
        {
            curr_Cargo++;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    void SetCargoText()
    {
        cargoText.text = "Cargo: " + curr_Cargo.ToString();
    }
}
