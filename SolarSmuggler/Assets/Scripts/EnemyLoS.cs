﻿using UnityEngine;
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
    private const float MAX_DIST = 50f;
    private const float MAX_FIRE_DIST = 25f;
    private Vector3 playerPos;
    private GameObject player;
    private RaycastHit hit;
    private Component playerControllerScript;
    

	void Start () 
	{
        player = GameObject.FindGameObjectWithTag("Player");
        
        
        /* Thomas: This will store the player's position when the enemy spawns, 
		 * but it only does so once so it will never update from that position. 
		 * If Update() uses player.transform.position, the enemy will always 
		 * have the latest position.
		 */
        //playerPos = player.transform.position;
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
		if(Physics.Raycast(transform.position, heading, out hit, MAX_DIST))
        {
            Debug.Log("Hit True\n");
            if (hit.collider.tag.Equals("Player"))
            {
                Debug.Log("Spotted\n");
                //Enemy reaction script goes here.
                ShootAtPlayer();
            }
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
