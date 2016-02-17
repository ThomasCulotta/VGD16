using UnityEngine;
using System.Collections;

/**
/*Author: Patrick Blanchard 
/*
/*Description: This is a script to detect the presence 
/*			   of the Player by the Enemy. MAX_DIST controls
/*			   the distance of the Enemy's Line of Sight. 
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
        playerPos = player.transform.position;
	}
	
	
	void Update () 
	{
	    if(Physics.Raycast(transform.position, playerPos, out hit, MAX_DIST))
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
