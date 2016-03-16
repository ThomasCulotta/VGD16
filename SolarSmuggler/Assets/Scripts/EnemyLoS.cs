using UnityEngine;
using System.Collections;

/*
 * Author: Patrick Blanchard 
 * 
 * Description: This is a script to detect the presence 
 * 			    of the Player by the Enemy. MAX_SPOT controls
 * 			    the distance of the Enemy's Line of Sight. 
 */
public class EnemyLoS : MonoBehaviour
{
    //Debug Variables
    private const bool DEBUG = false;
<<<<<<< Updated upstream
    
=======
    public GameObject debugPlane;
>>>>>>> Stashed changes

    //Constant Variables
    private const int   MAX_MOVE = 10;
    private const float MAX_SPOT = (MAX_MOVE * 2 + 1);
    private const float MAX_FIRE_DIST = 1f;

    //Utility Variables
    private GameObject player;
    private RaycastHit hit;
    private Component  playerControllerScript;
    private Queue      BFSQueue;
    private ArrayList  moveList;
    private Vector3    curPos;
<<<<<<< Updated upstream
    private Vector3    playerPos;
=======
>>>>>>> Stashed changes

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
    GridNode lastPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
<<<<<<< Updated upstream
=======
        curPos = transform.position;
>>>>>>> Stashed changes
    }


    void Update()
    {
        if (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN)
        {
<<<<<<< Updated upstream
            //Reset Game Board
            playerPos = player.transform.position;
            curPos = transform.position;
=======
>>>>>>> Stashed changes
            EnemySearchPlane = new GridNode[(int)MAX_SPOT, (int)MAX_SPOT];
            StartCoroutine("initESP");    
           
            // Thomas: Added this small debug line so we'll see exactly what the ray is doing when we test this out.
            Debug.DrawLine(transform.position, player.transform.position, Color.cyan, 0.5f);

            //Checks if player is obstructed by obstacle 
            Vector3 heading = player.transform.position - transform.position;
            if (Physics.Raycast(transform.position, heading, out hit, MAX_SPOT))
            {
<<<<<<< Updated upstream
                //Movement
                moveList = new ArrayList();
                PathFinding(curPos);
                moveEnemy();

                //Combat
=======
                moveList = new ArrayList();
                PathFinding(curPos);
                lastPos = (GridNode)moveList[moveList.Count - 1];
                moveEnemy();
                curPos = lastPos.coords;

>>>>>>> Stashed changes
                if (hit.collider.tag.Equals("Player"))
                {
                    ShootAtPlayer();
                }
            }

            //Change the Game State to PLAYER_TURN
            GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN;
            Debug.Log("PLAYER_TURN -from enemyLOS");
        }
    }

    /*
     *Patrick: This will setup the Enemy Search Plane at the start 
     *of the Enemy's turn.
     */
    IEnumerator initESP()
    {
        //the Enemy GridNode is primed
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].coords    = new Vector3(transform.position.x, 0f, transform.position.z);
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].visited   = true;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].hasPlayer = false;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist      = Vector3.Distance(transform.position, playerPos);
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].x         = MAX_MOVE;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].y         = MAX_MOVE;

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

            //Player Node found
            if (EnemySearchPlane[cur.x, cur.y].hasPlayer)
                Debug.Log("Found Player at " + playerNode.x + "," + playerNode.y + " He is " + playerNode.dist + " away.");
            

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

            //Debug
            if(DEBUG)
                Debug.Log("[" + cur.x + "," + cur.y + "]" + " Coords: " + cur.coords + "; Visited: " + cur.visited + "; hasPlayer: " + cur.hasPlayer + "; Distance: " + cur.dist);
        }
    }

    void visit(int x, int y)
    {
        //Gamespace coordinates conversion
        float xCoord = x + transform.position.x - MAX_MOVE;
        float yCoord = y + transform.position.y - MAX_MOVE;

        //init GridNode
        EnemySearchPlane[x, y].coords    = new Vector3(xCoord, 0f, yCoord);
        EnemySearchPlane[x, y].visited   = true;
        EnemySearchPlane[x, y].hasPlayer = false;
        EnemySearchPlane[x, y].dist      = Vector3.Distance(EnemySearchPlane[x, y].coords, playerPos);
        EnemySearchPlane[x, y].x         = x;
        EnemySearchPlane[x, y].y         = y;

        //Player Found, info is loaded into a new Grid Node struct for easy access
        if (EnemySearchPlane[x, y].coords == player.transform.position)
        {
            EnemySearchPlane[x, y].hasPlayer = true;
            playerNode = EnemySearchPlane[x, y];
        }

        //Enque so neighbors can be checked.
        BFSQueue.Enqueue(EnemySearchPlane[x, y]);
    }


    /*
     *Forms a path of GridNode objects 
     *for the player to move on.
     */
    void PathFinding(Vector3 curP)
    {
<<<<<<< Updated upstream
        GridNode closestSpace;
        Vector2 posOffset = new Vector2(curP.x - transform.position.x + MAX_MOVE, curP.z - transform.position.z + MAX_MOVE);
        int curX = (int)posOffset.x;
        int curY = (int)posOffset.y;
        Debug.Log(curP.x + " - " + transform.position.x + " + " + MAX_MOVE + " = " + posOffset.x + "; " + curP.z + " - " + transform.position.z + " + " + MAX_MOVE + " = " + posOffset.y);
        moveList.Add(EnemySearchPlane[curX, curY]);

        if(EnemySearchPlane[curX, curY].dist > 1f)
=======
        Debug.Log("Path " + curPos);
        Vector2 posOffset = new Vector2(curPos.x - transform.position.x + MAX_MOVE, curPos.z - transform.position.z + MAX_MOVE);
        moveList.Add(EnemySearchPlane[(int)posOffset.x, (int)posOffset.y]);
        if(Vector3.Distance(EnemySearchPlane[(int)posOffset.x, (int)posOffset.y].coords, playerNode.coords) > 1.4)
>>>>>>> Stashed changes
        {
            int xP1 = curX + 1;
            int xM1 = curX - 1;
            int yP1 = curY + 1;
            int yM1 = curY - 1;

            if (yP1 > MAX_SPOT-1) yP1 = yP1--;
            if (xP1 > MAX_SPOT-1) xP1 = xP1--;
            if (xM1 < 0) xM1 = xM1++;
            if (yM1 < 0) yM1 = yM1++;

            closestSpace = EnemySearchPlane[curX, curY];
           
            if (EnemySearchPlane[curX, yP1].dist < closestSpace.dist && EnemySearchPlane[curX, yP1].visited) closestSpace = EnemySearchPlane[curX, yP1]; //North
            if (EnemySearchPlane[curX, yM1].dist < closestSpace.dist && EnemySearchPlane[curX, yM1].visited) closestSpace = EnemySearchPlane[curX, yM1]; //South
            if (EnemySearchPlane[xP1, curY].dist < closestSpace.dist && EnemySearchPlane[xP1, curY].visited) closestSpace = EnemySearchPlane[xP1, curY]; //East
            if (EnemySearchPlane[xM1, curY].dist < closestSpace.dist && EnemySearchPlane[xM1, curY].visited) closestSpace = EnemySearchPlane[xM1, curY]; //West
            
            if (EnemySearchPlane[xP1, yP1].dist < closestSpace.dist && EnemySearchPlane[xP1, yP1].visited) closestSpace = EnemySearchPlane[xP1, yP1]; //NorthEast
            if (EnemySearchPlane[xP1, yM1].dist < closestSpace.dist && EnemySearchPlane[xP1, yM1].visited) closestSpace = EnemySearchPlane[xP1, yM1]; //SouthEast
            if (EnemySearchPlane[xM1, yP1].dist < closestSpace.dist && EnemySearchPlane[xM1, yP1].visited) closestSpace = EnemySearchPlane[xM1, yP1]; //NorthWest
            if (EnemySearchPlane[xM1, yM1].dist < closestSpace.dist && EnemySearchPlane[xM1, yM1].visited) closestSpace = EnemySearchPlane[xM1, yM1]; //SouthWest

            PathFinding(closestSpace.coords);
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

            bool curRight = false;
            bool curUp = false;
            int spaceCount = 0;
            float curDur = 0f;
            int i = 0;
            GridNode curNode = new GridNode();
            curNode.coords = transform.position;

            do
            {

                GridNode tempNode = (GridNode)moveList[i];
                i++;
                bool tempRight = tempNode.coords.x > curNode.coords.x;
                bool tempUp = tempNode.coords.z > curNode.coords.z;
                if (spaceCount == 0 || (tempRight == curRight && tempUp == curUp))
                {
                    curNode = tempNode;
                    moveList.RemoveAt(0);
                    curRight = tempRight;
                    curUp = tempUp;
                    curDur += 0.5f;
                    spaceCount++;
                }
                else break;

            } while (i < moveList.Count);
            Debug.Log(curNode.coords.x + ", " + curNode.coords.z);
            iTween.MoveTo(gameObject, iTween.Hash("x", curNode.coords.x,
                                                  "z", curNode.coords.z,
                                                  "time", curDur,
                                                  "oncomplete", "moveEnemy",
                                                  "oncompletetarget", gameObject));
        }
    }

    void ShootAtPlayer()
    {
        Vector3 heading  = player.transform.position - transform.position;
        if (MAX_FIRE_DIST >= heading.magnitude)
        {
            //Debug.Log(heading.magnitude);
            int shotHit = Random.Range(0, 2);
            if(shotHit == 1)
            {
                player.GetComponent<PlayerController>().decreaseHealth();
                Debug.Log("Player has been hit, health is " + player.GetComponent<PlayerController>().curr_Health + "\n");
            } else {
                Debug.Log("Enemy has missed player, health is " + player.GetComponent<PlayerController>().curr_Health + "\n");
            }
        }
    }
}
