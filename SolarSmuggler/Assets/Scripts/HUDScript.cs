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

		float health = playerController.curr_Health;
		float max_Health = playerController.max_Health;
		float cargo = playerController.curr_Cargo;
		float max_Cargo = playerController.max_Cargo;

		HealthUpdate(health, max_Health);
		CargoUpdate(cargo, max_Cargo);
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void HealthUpdate (float health, float max_Health) 
	{
		float bars_filled = health / (max_Health/4);
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

		SetHealthText (health);
	}

	public void CargoUpdate (float cargo, float max_Cargo) 
	{
		float bars_filled = cargo / (max_Cargo/3);
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
		SetCargoText (cargo);
	}

	void SetCargoText(float cargo)
	{
		cargoText.text = "Cargo: " + cargo.ToString();
	}

	void SetHealthText(float health)
	{
		healthText.text = "Health: " + health.ToString();
	}
}
