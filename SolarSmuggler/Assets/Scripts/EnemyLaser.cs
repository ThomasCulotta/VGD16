using UnityEngine;
using System.Collections;
/*
 * @Author: Patrick Blanchard
 *
 */

public class EnemyLaser : MonoBehaviour {
    //Util Vars
    LineRenderer laser;
    Material laserMat;
    //RaycastHit hit;
    GameObject player;
    Component LoS;

    
   
    
	// Use this for initialization
	void Awake () {
	    laser    = gameObject.AddComponent<LineRenderer>();
        laserMat = (Material)Resources.Load("Prefabs/Materials/laser", typeof(Material));
        LoS = gameObject.GetComponent<EnemyLoS>();
        player = GameObject.FindWithTag("Player");
        laser.material = laserMat;
        laser.SetWidth(0.1f, 0.1f);
    }
	
	// timeToLive determines how long to laser will be in
    // veiw to the player.
	public void Shootlaser(float timeToLive)
    {
        float timeRemaining = timeToLive;
        
        if(timeRemaining > 0)
        {
            Debug.Log("Laser is Active");
            Debug.Log("TTL: " + timeRemaining);
            Vector3[] linePos = {transform.position, player.transform.position};
            laser.enabled = true;
            laser.SetPositions(linePos);
            timeRemaining -= Time.deltaTime;
        }

        laser.enabled = false;
	}
}
