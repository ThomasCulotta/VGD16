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
    RaycastHit hit;
    GameObject player;
    Component LoS;
   
    
	// Use this for initialization
	void Awake () {
	    laser    = gameObject.AddComponent<LineRenderer>();
        laserMat = (Material)Resources.Load("Prefabs/Materials/laser", typeof(Material));
        LoS = gameObject.GetComponent<EnemyLoS>();
    }
	
	// Update is called once per frame
	void Update () {

        if (isShot() && (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN) && playerFound())
        {
            player = GameObject.FindWithTag("Player");
            Vector3[] linePos = {transform.position, player.transform.position};
            laser.enabled = true;
            laser.material = laserMat;
            laser.SetWidth(0.1f, 0.1f);
            laser.SetPositions(linePos);
        }
        else
        {
            laser.enabled = false;
        }
	}

    bool isShot()
    {
        if (gameObject.GetComponent<EnemyLoS>().shotHit == 1)
            return true;
        return false;
    }

    bool playerFound()
    {
        if (gameObject.GetComponent<EnemyLoS>().playerFound)
            return true;
        return false;
    }
}
