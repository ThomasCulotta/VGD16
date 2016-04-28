using UnityEngine;
using System.Collections;

public class Orbit : MonoBehaviour
{
	public float rotationSpeed; //how fast the satellite orbits the primary

	void Start()
	{
        /*
         * Took out gameobject reference to reduce mem/calcs. Script can be attached 
         * to the null which rotates around it's own axis.
         */
        rotationSpeed = Random.Range(0.1f, 0.3f);
	}

	void Update()
	{
		if (GameMaster.CurrentState == GameMaster.GameState.ENVIRONMENT_TURN)
		{
        	transform.Rotate(new Vector3(0f, rotationSpeed, 0f)); //orbits the satellite around the primary
		}
	}
}
