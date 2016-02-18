using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float max_Health = 100f;
    public float curr_Health = 100f;
    public float max_Cargo = 100f; //If we ever decide for cargo size
    public float curr_Cargo = 0f;
    public Slider cargoBar;
    public GameObject healthBar;
    public Text cargoText;

	void Start()
	{
		// Force player to grid
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        curr_Cargo = max_Cargo; //set cargo to maximum capacity
        //InvokeRepeating("decreaseHealth", 1f, 1f); just for testing purposes, this decreases health by 2 every second
        SetCargoBar(curr_Cargo);
	}

	void Update()
	{
		// WASD grid movement
		if (Input.GetKeyDown(KeyCode.W)) MovePlayer("z", transform.position.z,  1f);
		if (Input.GetKeyDown(KeyCode.A)) MovePlayer("x", transform.position.x, -1f);
		if (Input.GetKeyDown(KeyCode.S)) MovePlayer("z", transform.position.z, -1f);
		if (Input.GetKeyDown(KeyCode.D)) MovePlayer("x", transform.position.x,  1f);

        //if (Input.GetKeyDown(KeyCode.M)) DecreaseCargo();For testing
        //if (Input.GetKeyDown(KeyCode.I)) IncreaseCargo();For testing
        SetCargoText();//Setting cargo amount text
    }

	void MovePlayer(string axis, float position, float ammount)
	{
		iTween.MoveTo(gameObject, iTween.Hash(axis, position + ammount, "time", 0.5f));
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
	}

    public void decreaseHealth()
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

    public void SetCargoBar(float cargo)
    {
        cargoBar.value = cargo;
    }

    public void DecreaseCargo()
    {
        if (curr_Cargo != 0)
        {
            curr_Cargo--;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    public void IncreaseCargo()
    {
        if (curr_Cargo == max_Cargo)
        {
            curr_Cargo++;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    void SetCargoText()
    {
        cargoText.text = "Cargo: " + curr_Cargo.ToString();
    }
}
