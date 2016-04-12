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
    private int finishedListIndex;
    private int id;
    private bool removed;
    private int MAX_ENEMIES = SpawnSystem.MAX_ENEMIES; 

    //Debug Variables
    private const bool DEBUG = false;

    //Constant Variables
    private const int MAX_MOVE = 7;
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
    private bool playerFound;
    private bool isSelected;
    private bool shot;

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

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        init = true;
        playerFound = false;
        shot = true;
        id = GetInstanceID();
        removed = false;
    }

    void Update()
    {
        if (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN)
        {
            // Thomas: Added this small debug line so we'll see exactly what the ray is doing when we test this out.
            //Debug.DrawLine(transform.position, player.transform.position, Color.cyan, 0.5f);

            //Reset Game Board.
            if (init)
            {
                turnInProgress = true;
                playerFound = false;
                shot = true;
                playerNode = new GridNode();
                playerPos = player.transform.position;
                curPos = transform.position;

                if (!finishedList.Contains(id))
                {
                    EnemySearchPlane = new GridNode[(int)MAX_SPOT, (int)MAX_SPOT];
                    StartCoroutine("initESP");
                    finishedList.Add(id);
                    Debug.Log(id + " Checking in, finishedList Count: " + finishedList.Count);
                }

                if (MAX_ENEMIES == finishedList.Count)
                    init = false;

            }
            else
            {
                //Player ended enemy turn with emp.
                if (emp)
                {
                    emp = false;
                    Destroy(empInst);
                    init = true;
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
                    Debug.Log(id + " Checking out, finishedList Count: " + finishedList.Count);
                }

                if (finishedList.Count <= 0)
                {
                    Debug.Log("PLAYER_TURN -from enemyLOS");
                    init = true;
                    GameMaster.CurrentState = GameMaster.GameState.ENVIRONMENT_TURN;
                }

                //Recalculating enemy instance Array posistion. 
                //UpdateIndex();
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
            if (yM1 > -1 && !EnemySearchPlane[cur.x, yM1].visited) visit(cur.x, yM1); //South
            if (xP1 < MAX_SPOT && !EnemySearchPlane[xP1, cur.y].visited) visit(xP1, cur.y); //East
            if (xM1 > -1 && !EnemySearchPlane[xM1, cur.y].visited) visit(xM1, cur.y); //West

            if (xP1 < MAX_SPOT && yP1 < MAX_SPOT && !EnemySearchPlane[xP1, yP1].visited) visit(xP1, yP1); //NorthEast
            if (xM1 > -1 && yM1 > -1 && !EnemySearchPlane[xM1, yM1].visited) visit(xM1, yM1); //SouthWest
            if (xP1 < MAX_SPOT && yM1 > -1 && !EnemySearchPlane[xP1, yM1].visited) visit(xP1, yM1); //SouthEast
            if (xM1 > -1 && yP1 < MAX_SPOT && !EnemySearchPlane[xM1, yP1].visited) visit(xM1, yP1); //NorthWest
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

        //Player Found, info is loaded into a new Grid Node struct for easy access
        if (EnemySearchPlane[x, y].coords == player.transform.position)
        {
            EnemySearchPlane[x, y].hasPlayer = true;
            playerNode = EnemySearchPlane[x, y];
            playerFound = true;
            Debug.Log(id + " Found Player at " + playerNode.x + "," + playerNode.y + " He is " + EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist + " away.");
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
            if (EnemySearchPlane[curX, yP1].dist < closestSpace.dist && EnemySearchPlane[curX, yP1].visited) closestSpace = EnemySearchPlane[curX, yP1]; //North
            if (EnemySearchPlane[curX, yM1].dist < closestSpace.dist && EnemySearchPlane[curX, yM1].visited) closestSpace = EnemySearchPlane[curX, yM1]; //South
            if (EnemySearchPlane[xP1, curY].dist < closestSpace.dist && EnemySearchPlane[xP1, curY].visited) closestSpace = EnemySearchPlane[xP1, curY]; //East
            if (EnemySearchPlane[xM1, curY].dist < closestSpace.dist && EnemySearchPlane[xM1, curY].visited) closestSpace = EnemySearchPlane[xM1, curY]; //West

            //Diagonals
            if (EnemySearchPlane[xP1, yP1].dist < closestSpace.dist && EnemySearchPlane[xP1, yP1].visited) closestSpace = EnemySearchPlane[xP1, yP1]; //NorthEast
            if (EnemySearchPlane[xP1, yM1].dist < closestSpace.dist && EnemySearchPlane[xP1, yM1].visited) closestSpace = EnemySearchPlane[xP1, yM1]; //SouthEast
            if (EnemySearchPlane[xM1, yP1].dist < closestSpace.dist && EnemySearchPlane[xM1, yP1].visited) closestSpace = EnemySearchPlane[xM1, yP1]; //NorthWest
            if (EnemySearchPlane[xM1, yM1].dist < closestSpace.dist && EnemySearchPlane[xM1, yM1].visited) closestSpace = EnemySearchPlane[xM1, yM1]; //SouthWest

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

            do
            {
                //Debug.Log("MoveList Count: " + moveList.Count);
                curNode = (GridNode)moveList[0];
                moveList.RemoveAt(0);
                curDur += 0.5f;

            } while (0 < moveList.Count);
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
            int shotHit = Random.Range(0, 2);

            // If disoriented, cut shot chance in half
            if (disoriented && shotHit == 1)
            {
                disoriented = false;
                Destroy(disorientedInst);
                shotHit = Random.Range(0, 2);
            }

            if (shotHit == 1)
            {
                /*
                Vector3[] linePos = { transform.position, playerPos };
                LineRenderer laser = new LineRenderer();
                laser.SetColors(Color.red, Color.red);
                laser.SetPositions(linePos);
                */
                player.GetComponent<PlayerController>().decreaseHealth();
                Debug.Log("Player has been hit, health is " + (player.GetComponent<PlayerController>().curr_Health) + "\n");
            }
            else
            {
                Debug.Log("Enemy has missed player, health is " + player.GetComponent<PlayerController>().curr_Health + "\n");
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