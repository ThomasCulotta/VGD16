using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {

	public Image healthShard1;
	public Image healthShard2;
	public Image healthShard3;
	public Image healthShard4;

	public Image cargoShard1;
	public Image cargoShard2;
	public Image cargoShard3;

	public Text cargoText;
	public Text healthText;

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindWithTag("Player");
		PlayerController playerController = player.GetComponent<PlayerController> ();

		int health = (int)playerController.curr_Health;
		int max_Health = (int)playerController.max_Health;
		int cargo = playerController.curr_Cargo;
		int max_Cargo = playerController.max_Cargo;

		HealthUpdate(health, max_Health);
		CargoUpdate(cargo, max_Cargo);
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void HealthUpdate (int health, int max_Health) 
	{
		float bars_filled = (float)health / ((float)max_Health/4);
		if (bars_filled <= 1) 
		{
			healthShard1.fillAmount = bars_filled;
		} 
		else if (bars_filled <= 2)
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = bars_filled - 1;
		}
		else if (bars_filled <= 3) 
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = 1.0f;
			healthShard3.fillAmount = bars_filled - 2;
		}
		else if (bars_filled <= 4) 
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = 1.0f;
			healthShard3.fillAmount = 1.0f;
			healthShard4.fillAmount = bars_filled - 3;
		}
		SetHealthText (health, max_Health);
	}

	public void CargoUpdate (int cargo, int max_Cargo) 
	{
		float bars_filled = (float)cargo / ((float)max_Cargo/3);
		if (bars_filled <= 1) 
		{
			cargoShard1.fillAmount = bars_filled;
		} 
		else if (bars_filled <= 2)
		{
			cargoShard1.fillAmount = 1.0f;
			cargoShard2.fillAmount = bars_filled - 1;
		}
		else if (bars_filled <= 3) 
		{
			cargoShard1.fillAmount = 1.0f;
			cargoShard2.fillAmount = 1.0f;
			cargoShard3.fillAmount = bars_filled - 2;
		}
		SetCargoText (cargo, max_Cargo);
		Debug.Log ("bars_filled:" + bars_filled);
	}

	void SetCargoText(int cargo, int max_Cargo)
	{
		cargoText.text = "Cargo: " + cargo.ToString() + "/" + max_Cargo.ToString();
	}

	void SetHealthText(int health, int max_Health)
	{
		healthText.text = "Health: " + health.ToString() + "/" + max_Health.ToString();
	}
}
