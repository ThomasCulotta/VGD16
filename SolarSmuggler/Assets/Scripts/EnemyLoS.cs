using UnityEngine;
using System.Collections;

/*
 * Author: Patrick Blanchard 
 * 
 * Description: This is a script to detect the presence 
 * 			    of the Player, move the enemy within firing
 * 			    distance of the player, and to attack the
 *              player.
 *
 * Use: 1) Attach this script to Enemy
 *      2) Set Player tag to Player
 *      3) Enemy must have a collider
 */
public class EnemyLoS : MonoBehaviour
{

    //Enemy check in
    private static bool init;
    private static ArrayList finishedList = new ArrayList();
    private static Queue turnQueue = new Queue();
    private int finishedListIndex;
    private int id;
    private bool removed;
    private bool scan;
    private int MAX_ENEMIES = SpawnSystem.MAX_ENEMIES; 

    //Debug Variables
    private const bool DEBUG = false;

    //Constant Variables
    private const int MAX_MOVE = 10;
    private const float MAX_SPOT = (MAX_MOVE * 2 + 1);
    private const float MAX_FIRE_DIST = 2f;

    //Utility Variables
    private GameObject player;
    private RaycastHit hit;
    private Component playerControllerScript;
    private Queue BFSQueue;
    private ArrayList moveList;
    private Vector3 curPos;
    private Vector3 playerPos;
    private bool turnInProgress;
    private bool isSelected;
    private bool shot;
    private int shotHit;

    //Laser Variables
    private LineRenderer laser;
    private Material laserMat;
    private Vector3[] linePos;
    private float laserRenderTime;
    private float laserDecayTime;

    //Fog of War Variables
    private Renderer rend;
    private Material hiddenMat;
    private Material spottedMat;

    //Public Variables
    public bool playerFound;
    public bool isShooting;

    public struct GridNode
    {
        //Node Position
        public Vector3 coords;
        //Shows state of Node
        public bool hasPlayer;
        //Shows Node visited
        public bool visited;
        //Distance from Player
        public float dist;
        //Grid X
        public int x;
        //Grid Y
        public int y;
        //Valid
        public bool valid;
    }

    //Grid Variables
    private GridNode[,] EnemySearchPlane;
    private GridNode playerNode;
    private GridNode dest;

    //Select Variables
    public GameObject selectPrefab;
    private GameObject selectInst;

    //Condition Variables
    private bool emp;
    public GameObject empPrefab;
    private GameObject empInst;

    private bool disoriented;
    public GameObject disorientedPrefab;
    private GameObject disorientedInst;

    //Sound Variables
    private AudioSource laserSound;
    private AudioSource alarmSound;
    private AudioSource empSound;
    private static bool alert;

    //Texture
    public Texture2D normTexture;

    void Awake()
    {
        //Instance Init Status
        init = true;
        id = GetInstanceID();
        removed = false;

        //Init Player Status
        player = GameObject.FindGameObjectWithTag("Player");
        playerFound = false;

        //Init Audio
        laserSound = gameObject.AddComponent<AudioSource>();
        alarmSound = gameObject.AddComponent<AudioSource>();
        empSound   = gameObject.AddComponent<AudioSource>();
        laserSound.clip = AudioController.effect[6];
        alarmSound.clip = AudioController.effect[0];
        empSound.clip   = AudioController.effect[2];

        //Init Laser
        laser = gameObject.transform.FindChild("pCube5").GetComponent<LineRenderer>();
        laserMat = (Material)Resources.Load("Prefabs/Materials/laser", typeof(Material));
        laser.material = laserMat;
        laser.SetWidth(0.05f, 0.05f);
        laserRenderTime = 1f;
        laser.enabled = false;
        laserDecayTime = laserRenderTime;
        linePos = new Vector3[2];
        linePos[0] = transform.position;
        linePos[1] = player.transform.position;

        //Init Fog of War 
        hiddenMat  = (Material)Resources.Load("Prefabs/Materials/HiddenEnemy", typeof(Material));
        spottedMat = (Material)Resources.Load("Prefabs/Materials/SpottedEnemy", typeof(Material));

        //Init Attack Status
        isShooting = false;
        shotHit = -1;
        shot = true;
        alert = false;
    }

    void Update()
    {
        Vector3 spotVector = player.transform.position - transform.position + new Vector3(0f, -1f, 0f);
        if (Physics.Raycast(transform.position, spotVector, out hit, MAX_SPOT)) // set white
        {
            gameObject.GetComponentInChildren<Renderer>().material = spottedMat;
            if (alert && !player.GetComponent<PlayerController>().imHidden)
            {
                alarmSound.Play();
                alert = false;
            }
        }
        else // set black
        {
           gameObject.GetComponentInChildren<Renderer>().material = hiddenMat;
        }

        //Sets a timer for the laser
        if (laser.enabled)
        {
            laserDecayTime -= Time.deltaTime;
            laser.material = laserMat;
            linePos[0] = transform.position;
            linePos[1] = player.transform.position;
            laser.useLightProbes = true;
            laser.SetPositions(linePos);

            if (laserDecayTime < 0)
            {
                laser.enabled = false;
            }
        }
        else
        {
            laserDecayTime = laserRenderTime;
        }

        if (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN)
        {
            //Reset Game Board.
            if (init)
            {
                turnInProgress = true;
                playerFound = false;
                shot = true;
                scan = true;
                playerNode = new GridNode();
                playerPos = player.transform.position;
                curPos = transform.position;

                if (!finishedList.Contains(id))
                {
                    EnemySearchPlane = new GridNode[(int)MAX_SPOT, (int)MAX_SPOT];
                    finishedList.Add(id);
                    turnQueue.Enqueue(id);
                    Debug.Log(id + " Checking in, finishedList Count: " + finishedList.Count);
                }

                if (MAX_ENEMIES == finishedList.Count)
                    init = false;
            }
            else
            {
                if (turnQueue.Peek().Equals(id))
                {
                    if(scan)
                    {
                        StartCoroutine("initESP");
                        scan = false;
                    }

                    //Player is hidden
                    if (player.GetComponent<PlayerController>().imHidden)
                    {
                        turnQueue.Clear();
                        finishedList.Clear();
                        GameMaster.CurrentState = GameMaster.GameState.ENVIRONMENT_TURN;
                    }

                    //Player ended enemy turn with emp.
                    else if (emp)
                    {
                        emp = false;
                        Destroy(empInst);
                        init = true;

                        //EMP Sound
                        empSound.Play();

                        turnQueue.Clear();
                        finishedList.Clear();
                        Debug.Log("PLAYER_TURN -from enemyLOS EMPed");
                        GameMaster.CurrentState = GameMaster.GameState.ENVIRONMENT_TURN;
                    }

                    else if (finishedList.Count > 0 && finishedList.Contains(id))
                    {
                        //Movement
                        if (playerFound)
                        {
                            moveList = new ArrayList();
                            PathFinding(MAX_MOVE, MAX_MOVE);
                            if (moveList.Count > 0)
                            {
                                dest = (GridNode)moveList[moveList.Count - 1];
                                moveEnemy();
                            }
                            else
                            {
                                dest.coords = transform.position;
                            }

                            if (moveList.Count == 0)
                            {
                                //Checks if player is obstructed by obstacle. 
                                Vector3 heading = player.transform.position - transform.position + new Vector3(0f, -1f, 0f);
                                if (Physics.Raycast(transform.position, heading, out hit, MAX_SPOT))
                                {
                                    //Combat
                                    if (shot)
                                    {

                                        ShootAtPlayer(dest.coords);
                                        linePos[0] = transform.position;
                                        linePos[1] = player.transform.position;
                                        laser.SetPositions(linePos);
                                        laser.enabled = true;
                                        shot = false;
                                    }
                                }
                            }
                        }
                    }

                    //Change the Game State to PLAYER_TURN.
                    if (finishedList.Count > 0 && finishedList.Contains(id))
                    {
                        finishedList.Remove(id);
                        turnQueue.Dequeue();
                        Debug.Log(id + " Checking out, finishedList Count: " + finishedList.Count);
                    }

                    if (finishedList.Count <= 0)
                    {
                        Debug.Log("PLAYER_TURN -from enemyLOS");
                        init = true;
                        alert = true;
                        GameMaster.CurrentState = GameMaster.GameState.ENVIRONMENT_TURN;
                    }

                    //Recalculating enemy instance Array posistion. 
                    //UpdateIndex();
                }
            }
        }
    }

    /*
     *Patrick: This will setup the Enemy Search Plane at the start 
     *of the Enemy's turn.
     */
    IEnumerator initESP()
    {
        //the Enemy GridNode is primed
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].coords = new Vector3(transform.position.x, 0f, transform.position.z);
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].visited = true;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].hasPlayer = false;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist = Vector3.Distance(transform.position, playerPos);
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].x = MAX_MOVE;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].y = MAX_MOVE;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].valid = false;

        //Init the Queue to begin the Flood Fill
        BFSQueue = new Queue();
        BFSQueue.Enqueue(EnemySearchPlane[MAX_MOVE, MAX_MOVE]);

        initFill();
        yield return null;
    }

    //Flood Fill to obtain the Player Node.
    void initFill()
    {

        while (BFSQueue.Count != 0)
        {
            //Get current node out of Queue
            GridNode cur = (GridNode)BFSQueue.Dequeue();

            //Saving Computations
            int xP1 = cur.x + 1;
            int xM1 = cur.x - 1;
            int yP1 = cur.y + 1;
            int yM1 = cur.y - 1;

            //Checking Boundaries and if visited
            if (yP1 < MAX_SPOT && !EnemySearchPlane[cur.x, yP1].visited) visit(cur.x, yP1); //North
            if (yM1 > -1       && !EnemySearchPlane[cur.x, yM1].visited) visit(cur.x, yM1); //South
            if (xP1 < MAX_SPOT && !EnemySearchPlane[xP1, cur.y].visited) visit(xP1, cur.y); //East
            if (xM1 > -1       && !EnemySearchPlane[xM1, cur.y].visited) visit(xM1, cur.y); //West

            if (xP1 < MAX_SPOT && yP1 < MAX_SPOT && !EnemySearchPlane[xP1, yP1].visited) visit(xP1, yP1); //NorthEast
            if (xM1 > -1       && yM1 > -1       && !EnemySearchPlane[xM1, yM1].visited) visit(xM1, yM1); //SouthWest
            if (xP1 < MAX_SPOT && yM1 > -1       && !EnemySearchPlane[xP1, yM1].visited) visit(xP1, yM1); //SouthEast
            if (xM1 > -1       && yP1 < MAX_SPOT && !EnemySearchPlane[xM1, yP1].visited) visit(xM1, yP1); //NorthWest
        }
    }

    void visit(int x, int y)
    {
        //Gamespace coordinates conversion
        float xCoord = x + transform.position.x - MAX_MOVE;
        float yCoord = y + transform.position.z - MAX_MOVE;

        //init GridNode
        EnemySearchPlane[x, y].coords = new Vector3(xCoord, 0f, yCoord);
        EnemySearchPlane[x, y].visited = true;
        EnemySearchPlane[x, y].hasPlayer = false;
        EnemySearchPlane[x, y].dist = Vector3.Distance(EnemySearchPlane[x, y].coords, playerPos);
        EnemySearchPlane[x, y].x = x;
        EnemySearchPlane[x, y].y = y;
        EnemySearchPlane[x, y].valid = true;

        //Player Found, info is loaded into a new Grid Node struct for easy access
        if (EnemySearchPlane[x, y].coords == player.transform.position)
        {
            EnemySearchPlane[x, y].hasPlayer = true;
            playerNode = EnemySearchPlane[x, y];
            playerFound = true;
            alert = true;
            Debug.Log(id + " Found Player at " + playerNode.x + "," + playerNode.y + " He is " + EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist + " away.");
        }

        Collider[] c = Physics.OverlapBox(EnemySearchPlane[x, y].coords, new Vector3(0.499f, 10f, 0.499f), Quaternion.identity,
                                      255, QueryTriggerInteraction.Ignore);

        foreach (Collider col in c)
        {
            if (col.tag.Equals("Enemy"))
                EnemySearchPlane[x, y].valid = false;
        }

        //Enque so neighbors can be checked.
        BFSQueue.Enqueue(EnemySearchPlane[x, y]);
    }


    /*
     *Forms a path of GridNode objects 
     *for the player to move on.
     */
    void PathFinding(int curX, int curY)
    {
        GridNode closestSpace;
        if (EnemySearchPlane[curX, curY].dist > 1.4f)
        {
            //Saving Computations
            int xP1 = curX + 1;
            int xM1 = curX - 1;
            int yP1 = curY + 1;
            int yM1 = curY - 1;

            //Checking Bounds
            if (yP1 > MAX_SPOT - 1) yP1--;
            if (xP1 > MAX_SPOT - 1) xP1--;
            if (xM1 < 0) xM1++;
            if (yM1 < 0) yM1++;

            //init closestSpace to current Node.
            closestSpace = EnemySearchPlane[curX, curY];

            //Cardinals
            if (EnemySearchPlane[curX, yP1].dist < closestSpace.dist && EnemySearchPlane[curX, yP1].valid) closestSpace = EnemySearchPlane[curX, yP1]; //North
            if (EnemySearchPlane[curX, yM1].dist < closestSpace.dist && EnemySearchPlane[curX, yM1].valid) closestSpace = EnemySearchPlane[curX, yM1]; //South
            if (EnemySearchPlane[xP1, curY].dist < closestSpace.dist && EnemySearchPlane[xP1, curY].valid) closestSpace = EnemySearchPlane[xP1, curY]; //East
            if (EnemySearchPlane[xM1, curY].dist < closestSpace.dist && EnemySearchPlane[xM1, curY].valid) closestSpace = EnemySearchPlane[xM1, curY]; //West

            //Diagonals
            if (EnemySearchPlane[xP1, yP1].dist < closestSpace.dist && EnemySearchPlane[xP1, yP1].valid) closestSpace = EnemySearchPlane[xP1, yP1]; //NorthEast
            if (EnemySearchPlane[xP1, yM1].dist < closestSpace.dist && EnemySearchPlane[xP1, yM1].valid) closestSpace = EnemySearchPlane[xP1, yM1]; //SouthEast
            if (EnemySearchPlane[xM1, yP1].dist < closestSpace.dist && EnemySearchPlane[xM1, yP1].valid) closestSpace = EnemySearchPlane[xM1, yP1]; //NorthWest
            if (EnemySearchPlane[xM1, yM1].dist < closestSpace.dist && EnemySearchPlane[xM1, yM1].valid) closestSpace = EnemySearchPlane[xM1, yM1]; //SouthWest

            //Return if playerNode is selected
            if (closestSpace.x == playerNode.x && closestSpace.y == playerNode.y)
                return;

            //Add closestSpace to list, recur.
            moveList.Add(closestSpace);
            PathFinding(closestSpace.x, closestSpace.y);
        }
    }

    /*
     *This function is a slight modification of
     *the movePlayer function in player Controller
     *that enables the Enemy to move across the grid.
     */
    void moveEnemy()
    {
        if (moveList.Count > 0)
        {
            float curDur = 0f;

            GridNode curNode = new GridNode();
            curNode.coords = transform.position;

            if (Vector3.Distance(curNode.coords, player.transform.position) > 3)
                do
                {
                    //Debug.Log("MoveList Count: " + moveList.Count);
                    curNode = (GridNode)moveList[0];
                    moveList.RemoveAt(0);
                    curDur += 0.2f;

                } while (0 < moveList.Count && Vector3.Distance(curNode.coords, player.transform.position) > 3);

            moveList.Clear();
            //Debug.Log("MoveList Count: " + moveList.Count);
            iTween.MoveTo(gameObject, iTween.Hash("x", curNode.coords.x,
                                                  "z", curNode.coords.z,
                                                  "time", curDur,
                                                  "oncomplete", "moveEnemy",
                                                  "oncompletetarget", gameObject));
        }
    }

    public void Select()
    {
        if (selectInst == null)
            selectInst = (GameObject)GameObject.Instantiate(selectPrefab, new Vector3(transform.position.x, 0.1f, transform.position.z), Quaternion.identity);
    }

    public void Deselect()
    {
        if (selectInst != null)
            Destroy(selectInst);
    }

    public void GetEMPed()
    {
        if (Random.Range(0, 3) == 2)
        {
            GetDisoriented();
            return;
        }
        emp = true;
        if (empInst == null)
        {
            empInst = (GameObject)GameObject.Instantiate(empPrefab, new Vector3(transform.position.x, 0.1f, transform.position.z), Quaternion.identity);
            empInst.transform.parent = transform;
        }

    }

    public void GetDisoriented()
    {
        disoriented = true;
        if (disorientedInst == null)
        {
            disorientedInst = (GameObject)GameObject.Instantiate(disorientedPrefab,
                                                                 new Vector3(transform.position.x, 0.1f, transform.position.z), Quaternion.identity);
            disorientedInst.transform.parent = transform;
        }
    }

    void ShootAtPlayer(Vector3 dest)
    {
        float dist = Vector3.Distance(dest, playerPos);
        if (MAX_FIRE_DIST >= dist)
        {
            shotHit = Random.Range(0, 2);

            // If disoriented, cut shot chance in half
            if (disoriented && shotHit == 1)
            {
                disoriented = false;
                Destroy(disorientedInst);
                shotHit = Random.Range(0, 2);
            }

            if (shotHit == 1)
            { 
                player.GetComponent<PlayerController>().decreaseHealth();
                Debug.Log("Player has been hit, health is " + (player.GetComponent<PlayerController>().curr_Health) + "\n");
                //Laser Sound
                laserSound.Play();
            }
            else
            {
                Debug.Log("Enemy has missed player, health is " + player.GetComponent<PlayerController>().curr_Health + "\n");
                //Laser Sound
                laserSound.Play();
            }
        }

    }

    //Updates Enemy Instance Index in finishedList
    void UpdateIndex()
    {
        int i;
        for (i = 0; i < finishedList.Count; i++)
        {
            if ((int)finishedList[i] == id)
            {
                finishedListIndex = i;
            }
        }
    }

}