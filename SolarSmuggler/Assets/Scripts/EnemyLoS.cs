using UnityEngine;
using System.Collections;

public class EnemyLoS : MonoBehaviour {

    private const float MAX_DIST = 50;
    private Vector3 playerPos;
    private GameObject player;
    private RaycastHit hit;
    

	void Start () {
        player = GameObject.FindGameObjectWithTag("player");
        playerPos = player.transform.position;
	}
	
	
	void Update () {
	    if(Physics.Raycast(transform.position, playerPos, out hit, MAX_DIST))
        {
            Debug.Log("Raycast True\n");
            if (hit.collider.tag.Equals("player"))
            {
                Debug.Log("spotted\n");
            }
        }
	}
}
