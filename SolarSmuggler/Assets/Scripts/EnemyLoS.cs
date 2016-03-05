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
    private const int MAX_MOVE = 10;
    private const float MAX_SPOT = (MAX_MOVE*2 + 1);
    private const float MAX_FIRE_DIST = 1f;
    private Vector3 playerPos;
    private GameObject player;
    private RaycastHit hit;
    private Component playerControllerScript;
    private Queue BFSQueue;
    private bool init;

    public struct GridNode
    {
        //Node Posistion
        public Vector3 coords;
        //Shows state of Node
        public bool hasPlayer;
        //Shows Node visited
        public bool visited;
        //Distance from Enemy
        public int dist;
        //Grid X
        public int x;
        //Grid Y
        public int y;
    }
    
    private GridNode[,] EnemySearchPlane;
    private GridNode playerNode;

	void Start () 
	{
        player = GameObject.FindGameObjectWithTag("Player");
        EnemySearchPlane = new GridNode[(int)MAX_SPOT, (int)MAX_SPOT];
        init = true;
    }
	
	
	void Update () 
	{
		/*
		 * Thomas: The logic is perfect, but I'm still going to mention an alternative, either 
		 * because it might make a small performance difference or because I might just be a 
		 * pretentious douche. We might want to change it later to only raycast while the 
		 * player or enemy is moving instead of continuously, but it probably won't matter too much.
		 */
		// Thomas: Added this small debug line so we'll see exactly what the ray is doing when we test this out.
		Debug.DrawLine(transform.position, player.transform.position, Color.cyan, 0.5f);

        /*
         *Patrick:
         *Fancy math that calculates the direction vector
         */

      
       Vector3 heading = player.transform.position - transform.position;
       if(Physics.Raycast(transform.position, heading, out hit, MAX_SPOT))
       {
           Debug.Log("Hit True\n");
           if (hit.collider.tag.Equals("Player"))
           {
               Debug.Log("Spotted\n");
               //Enemy reaction script goes here.
               ShootAtPlayer();
           }
       }
      
        
       StartCoroutine("initESP");
       
	}
    /*
     *Patrick: This will setup the Enemy Search Plane at the start 
     *of the Enemy's turn.
     */
    IEnumerator initESP()
    {
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].coords    = new Vector3(transform.position.x, 0f, transform.position.z);
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].visited   = true;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].hasPlayer = false;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].dist      = 0;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].x         = MAX_MOVE;
        EnemySearchPlane[MAX_MOVE, MAX_MOVE].y         = MAX_MOVE;

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
            //Get current node
            GridNode cur = (GridNode)BFSQueue.Dequeue();
            int nextDist = cur.dist + 1;

            //Player Node found
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
           /* if (yP1 < MAX_SPOT && !EnemySearchPlane[cur.x, yP1].visited) BFSQueue.Enqueue(EnemySearchPlane[cur.x, yP1]); //North
            if (yM1 > 0        && !EnemySearchPlane[cur.x, yM1].visited) BFSQueue.Enqueue(EnemySearchPlane[cur.x, yM1]); //South
            if (xP1 < MAX_SPOT && !EnemySearchPlane[xP1, cur.y].visited) BFSQueue.Enqueue(EnemySearchPlane[xP1, cur.y]); //East
            if (xM1 > 0        && !EnemySearchPlane[xM1, cur.y].visited) BFSQueue.Enqueue(EnemySearchPlane[xM1, cur.y]); //West

            if (xP1 < MAX_SPOT && yP1 < MAX_SPOT && !EnemySearchPlane[xP1, yP1].visited) BFSQueue.Enqueue(EnemySearchPlane[xP1, yP1]); //NorthEast
            if (xM1 > 0        && yM1 > 0        && !EnemySearchPlane[xM1, yM1].visited) BFSQueue.Enqueue(EnemySearchPlane[xM1, yM1]); //SouthWest
            if (xP1 < MAX_SPOT && yM1 > 0        && !EnemySearchPlane[xP1, yM1].visited) BFSQueue.Enqueue(EnemySearchPlane[xP1, yM1]); //SouthEast
            if (xM1 > 0        && yP1 < MAX_SPOT && !EnemySearchPlane[xM1, yP1].visited) BFSQueue.Enqueue(EnemySearchPlane[xM1, yP1]); //NorthWest */

            if (yP1 < MAX_SPOT && !EnemySearchPlane[cur.x, yP1].visited) visit(cur.x, yP1, nextDist); //North
            if (yM1 > 0        && !EnemySearchPlane[cur.x, yM1].visited) visit(cur.x, yM1, nextDist); //South
            if (xP1 < MAX_SPOT && !EnemySearchPlane[xP1, cur.y].visited) visit(xP1, cur.y, nextDist); //East
            if (xM1 > 0        && !EnemySearchPlane[xM1, cur.y].visited) visit(xM1, cur.y, nextDist); //West

            if (xP1 < MAX_SPOT && yP1 < MAX_SPOT && !EnemySearchPlane[xP1, yP1].visited) visit(xP1, yP1, nextDist); //NorthEast
            if (xM1 > 0        && yM1 > 0        && !EnemySearchPlane[xM1, yM1].visited) visit(xM1, yM1, nextDist); //SouthWest
            if (xP1 < MAX_SPOT && yM1 > 0        && !EnemySearchPlane[xP1, yM1].visited) visit(xP1, yM1, nextDist); //SouthEast
            if (xM1 > 0        && yP1 < MAX_SPOT && !EnemySearchPlane[xM1, yP1].visited) visit(xM1, yP1, nextDist); //NorthWest

            //Debug
            Debug.Log("["+cur.x+"," +cur.y+"]" + " Coords: " + cur.coords +  "; Visited: " + cur.visited + "; hasPlayer: " + cur.hasPlayer + "; Distance: " + cur.dist);
        }
        
    }

    void visit(int x, int y, int dist)
    {

        //Gamespace Coordinates
        float xCoord = x + transform.position.x - MAX_MOVE;
        float yCoord = y + transform.position.y - MAX_MOVE;

        //init GridNode
        EnemySearchPlane[x, y].coords    = new Vector3( xCoord, 0f, yCoord);
        EnemySearchPlane[x, y].visited   = true;
        EnemySearchPlane[x, y].hasPlayer = false;
        EnemySearchPlane[x, y].dist      = dist;
        EnemySearchPlane[x, y].x         = x;
        EnemySearchPlane[x, y].y         = y;
        
        //Player Found
        if (EnemySearchPlane[x, y].coords == player.transform.position)
        {
            EnemySearchPlane[x, y].hasPlayer = true;
            playerNode = EnemySearchPlane[x, y];
        }

        BFSQueue.Enqueue(EnemySearchPlane[x, y]);

        //Visual Debugging
        GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.position = new Vector3(xCoord, 0f, yCoord);
        

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
