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
    private const bool DEBUG = true;
    public GameObject debugPlane;

    //Constant Variables
    private const int MAX_MOVE = 10;
    private const float MAX_SPOT = (MAX_MOVE * 2 + 1);
    private const float MAX_FIRE_DIST = 1f;

    //Utility Variables
    private GameObject player;
    private RaycastHit hit;
    private Component playerControllerScript;
    private Queue BFSQueue;
    private ArrayList moveList;
    private bool init;
    private Vector3 curPos;

    public struct GridNode
    {
        //Node Posistion
        public Vector3 coords;
        //Shows state of Node
        public bool hasPlayer;
        //Shows Node visited
        public bool visited;
        //Distance from Player
        public int dist;
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
        EnemySearchPlane = new GridNode[(int)MAX_SPOT, (int)MAX_SPOT];
        curPos =  transform.position;
        init = true;
    }


    void Update()
    {
        if (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN)
        {
            if (init)
            {
                init = false;
                StartCoroutine("initESP");
            }
           
                Debug.Log(curPos);
                moveList = new ArrayList();
                PathFinding(curPos);
                lastPos = (GridNode)moveList[moveList.Count - 1];
                moveEnemy();
            

            // Thomas: Added this small debug line so we'll see exactly what the ray is doing when we test this out.
            Debug.DrawLine(transform.position, player.transform.position, Color.cyan, 0.5f);

            //Fancy math that calculates the direction vector 
            Vector3 heading = player.transform.position - transform.position;
            if (Physics.Raycast(transform.position, heading, out hit, MAX_SPOT))
            {
                //Debug.Log("Hit True\n");
                if (hit.collider.tag.Equals("Player"))
                {
                    //Debug.Log("Spotted\n");
                    //Enemy reaction script goes here.
                    ShootAtPlayer();
                }
            }

            GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN;
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
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist = 0;
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
            int nextDist = cur.dist + 1;

            //Player Node found, exit function.
            if (EnemySearchPlane[cur.x, cur.y].hasPlayer)
            {
                Debug.Log("Found Player at " + cur.x + "," + cur.y + " He is " + cur.dist + " away.");
                return;
            }

            //Saving Computations
            int xP1 = cur.x + 1;
            int xM1 = cur.x - 1;
            int yP1 = cur.y + 1;
            int yM1 = cur.y - 1;

            //Checking Boundaries and if visited
            if (yP1 < MAX_SPOT && !EnemySearchPlane[cur.x, yP1].visited) visit(cur.x, yP1, nextDist); //North
            if (yM1 > 0        && !EnemySearchPlane[cur.x, yM1].visited) visit(cur.x, yM1, nextDist); //South
            if (xP1 < MAX_SPOT && !EnemySearchPlane[xP1, cur.y].visited) visit(xP1, cur.y, nextDist); //East
            if (xM1 > 0        && !EnemySearchPlane[xM1, cur.y].visited) visit(xM1, cur.y, nextDist); //West

            if (xP1 < MAX_SPOT && yP1 < MAX_SPOT && !EnemySearchPlane[xP1, yP1].visited) visit(xP1, yP1, nextDist); //NorthEast
            if (xM1 > 0        && yM1 > 0        && !EnemySearchPlane[xM1, yM1].visited) visit(xM1, yM1, nextDist); //SouthWest
            if (xP1 < MAX_SPOT && yM1 > 0        && !EnemySearchPlane[xP1, yM1].visited) visit(xP1, yM1, nextDist); //SouthEast
            if (xM1 > 0        && yP1 < MAX_SPOT && !EnemySearchPlane[xM1, yP1].visited) visit(xM1, yP1, nextDist); //NorthWest

            //Debug
            if(DEBUG)
                Debug.Log("[" + cur.x + "," + cur.y + "]" + " Coords: " + cur.coords + "; Visited: " + cur.visited + "; hasPlayer: " + cur.hasPlayer + "; Distance: " + cur.dist);
        }
    }

    void visit(int x, int y, int dist)
    {
        //Gamespace coordinates conversion
        float xCoord = x + transform.position.x - MAX_MOVE;
        float yCoord = y + transform.position.y - MAX_MOVE;

        //init GridNode
        EnemySearchPlane[x, y].coords = new Vector3(xCoord, 0f, yCoord);
        EnemySearchPlane[x, y].visited = true;
        EnemySearchPlane[x, y].hasPlayer = false;
        EnemySearchPlane[x, y].dist = dist;
        EnemySearchPlane[x, y].x = x;
        EnemySearchPlane[x, y].y = y;

        //Player Found, info is loaded into a new Grid Node struct for easy access
        if (EnemySearchPlane[x, y].coords == player.transform.position)
        {
            EnemySearchPlane[x, y].hasPlayer = true;
            playerNode = EnemySearchPlane[x, y];
        }

        //Enque so neighbors can be checked.
        BFSQueue.Enqueue(EnemySearchPlane[x, y]);

        //Visual Debugging
        if (DEBUG == true)
        {
            Instantiate(debugPlane, new Vector3(xCoord, 0.3f, yCoord), Quaternion.identity);
        }
    }


    /*
     *Forms a path of GridNode objects 
     *for the player to move on.
     */
    void PathFinding(Vector3 curPos)
    {
        Debug.Log("Path " + curPos);
        Vector2 posOffset = new Vector2(curPos.x - transform.position.x + MAX_MOVE, curPos.z - transform.position.z + MAX_MOVE);
        moveList.Add(EnemySearchPlane[(int)posOffset.x, (int)posOffset.y]);
        //Debug.Log( playerNode.dist - EnemySearchPlane[(int)posOffset.x, (int)posOffset.y].dist);
        if(Vector3.Distance(EnemySearchPlane[(int)posOffset.x, (int)posOffset.y].coords, playerNode.coords) > 1.4)
        {
            int curX = (int)posOffset.x;
            int curY = (int)posOffset.y;

            int xP1 = curX + 1;
            int xM1 = curX - 1;
            int yP1 = curY + 1;
            int yM1 = curY - 1;

            //Start looking North
            GridNode closestSpace = EnemySearchPlane[curX, yP1];

            if (Vector3.Distance(EnemySearchPlane[curX, yM1].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[curX, yM1]; //South
            if (Vector3.Distance(EnemySearchPlane[xP1, curY].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xP1, curY]; //East
            if (Vector3.Distance(EnemySearchPlane[xM1, curY].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xM1, curY]; //West

            if (Vector3.Distance(EnemySearchPlane[xP1, yP1].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xP1, yP1]; //NorthEast
            if (Vector3.Distance(EnemySearchPlane[xP1, yM1].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xP1, yM1]; //SouthEast
            if (Vector3.Distance(EnemySearchPlane[xM1, yP1].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xM1, yP1]; //NorthWest
            if (Vector3.Distance(EnemySearchPlane[xM1, yM1].coords, playerNode.coords) < Vector3.Distance(closestSpace.coords, playerNode.coords)) closestSpace = EnemySearchPlane[xM1, yM1]; //SouthWest

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
                Debug.Log(tempNode.coords);
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

            iTween.MoveTo(gameObject, iTween.Hash("x", curNode.coords.x,
                                                  "z", curNode.coords.z,
                                                  "time", curDur,
                                                  "oncomplete", "moveEnemy",
                                                  "oncompletetarget", gameObject));
        }

        else {
           
            transform.position = new Vector3(lastPos.coords.x, 0f, lastPos.coords.z);
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
