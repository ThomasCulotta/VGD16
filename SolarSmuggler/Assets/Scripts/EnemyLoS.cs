using UnityEngine;
using System.Collections;

/*
 * Author: Patrick Blanchard 
 * 
 * Description: This is a script to detect the presence 
 * 			    of the Player by the Enemy. MAX_DIST controls
 * 			    the distance of the Enemy's Line of Sight. 
 */
public class EnemyLoS : MonoBehaviour 
{
    private const float MAX_DIST = 50;
    private Vector3 playerPos;
    private GameObject player;
    private RaycastHit hit;
    

	void Start () 
	{
        player = GameObject.FindGameObjectWithTag("Player");

		/* Thomas: This will store the player's position when the enemy spawns, 
		 * but it only does so once so it will never update from that position. 
		 * I changed the playerPos in the Update() to just be player.transform.position, 
		 * so the enemy will always have the latest position.
		 */
        //playerPos = player.transform.position;
	}
	
	
	void Update () 
	{
		/*
		 * Thomas: This is perfect, but I'm still going to mention an alternative, either 
		 * because it might make a small performance difference or because I might just be a 
		 * pretentious douche. We might want to change it later to only raycast while the 
		 * player or enemy is moving instead of continuously, but it probably won't matter too much.
		 */
		if(Physics.Raycast(transform.position, player.transform.position, out hit, MAX_DIST))
        {
            Debug.Log("Hit True\n");
            if (hit.collider.tag.Equals("player"))
            {
                Debug.Log("Spotted\n");
				//Enemy reaction script goes here. 
            }
        }
	}
}
