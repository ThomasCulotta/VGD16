using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float max_Health = 100f;
    public float curr_Health = 0f;
    public GameObject healthBar;

	void Start()
	{
		// Force player to grid
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        InvokeRepeating("decreaseHealth", 1f, 1f); // just for testing purposes, this decreases health by 2 every second
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

    void decreaseHealth()
    {
        curr_Health -= 2f; // whatever happens to player we decrease health

        //need a ratio to from current health & max health to scale the hp bar
        float calc_Health = curr_Health / max_Health;
        setHealthBar(calc_Health);
    }

    public void setHealthBar(float healthVal)
    {
        //health value needs to be from 0 to 1
        healthBar.transform.localScale = new Vector3(healthVal, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }
}
