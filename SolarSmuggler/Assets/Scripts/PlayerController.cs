using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	void Start()
	{
		// Force player to grid
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));

		// Initialize player stats...
	}

	void Update()
	{
		// WASD grid movement
		if (Input.GetKeyDown(KeyCode.W)) MovePlayer("z", transform.position.z,  1f);
		if (Input.GetKeyDown(KeyCode.A)) MovePlayer("x", transform.position.x, -1f);
		if (Input.GetKeyDown(KeyCode.S)) MovePlayer("z", transform.position.z, -1f);
		if (Input.GetKeyDown(KeyCode.D)) MovePlayer("x", transform.position.x,  1f);
	}

	void MovePlayer(string axis, float position, float ammount)
	{
		iTween.MoveTo(gameObject, iTween.Hash(axis, position + ammount, "time", 0.5f));
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
	}
}
