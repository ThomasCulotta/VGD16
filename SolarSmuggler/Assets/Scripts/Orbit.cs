using UnityEngine;
using System.Collections;

public class Orbit : MonoBehaviour
{
	public float rotationSpeed = 1.0f; //how fast the satellite orbits the primary
	float timer = 2; //the amount of time the environment phase lasts

	void Start()
	{
        /*
         * Took out gameobject reference to reduce mem/calcs. Script can be attached 
         * to the null which rotates around it's own axis.
         */
	}

	void Update()
	{
		if (GameMaster.CurrentState == GameMaster.GameState.ENVIRONMENT_TURN)
		{
			StartCoroutine ("Timer");
        	transform.Rotate(new Vector3(0f, rotationSpeed, 0f)); //orbits the satellite around the primary
			//timer -= Time.deltaTime;
		}
	}

	IEnumerator Timer()
	{
		yield return new WaitForSeconds (timer);
		Debug.Log("Evironment_TURN -from Orbit");
		GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN; //starts the player's turn
	}
}
