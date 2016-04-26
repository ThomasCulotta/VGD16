using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDScript : MonoBehaviour {

	public Image healthShard1;
	public Image healthShard2;
	public Image healthShard3;
	public Image healthShard4;

	public Image cargoShard1;
	public Image cargoShard2;
	public Image cargoShard3;

    public Image iconHyper;
    public Image iconEMP;
    public Image iconCloak;

    public Image redBorder;

	public Text cargoText;
	public Text healthText;
    public Text turnText;

    public GameObject menu;
    public GameObject control;
    public GameObject powerUp;
    private bool menuOpen;

	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindWithTag("Player");
		PlayerController playerController = player.GetComponent<PlayerController> ();
        menu.SetActive(false);
        control.SetActive(false);
        powerUp.SetActive(false);
        redBorder.enabled = false;
        menuOpen = false;

		int health = (int)playerController.curr_Health;
		int max_Health = (int)playerController.max_Health;
		int cargo = playerController.curr_Cargo;
		int max_Cargo = playerController.max_Cargo;
        int turns = playerController.turnCount;

		HealthUpdate(health, max_Health);
		CargoUpdate(cargo, max_Cargo);
	}

	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Escape)) {
            if (!menuOpen)
            {
                menu.SetActive(true);
                menuOpen = true;
            }
            //else {
            //    menu.SetActive(false);
            //    menuOpen = false;
            //}
        }
    }

    public void HealthUpdate (int health, int max_Health) 
	{
		float bars_filled = (float)health / ((float)max_Health/4.0f);
		if (bars_filled <= 1.0f) 
		{
			healthShard1.fillAmount = bars_filled;
			healthShard2.fillAmount = 0f;
			healthShard3.fillAmount = 0f;
			healthShard4.fillAmount = 0f;
		} 
		else if (bars_filled <= 2.0f)
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = bars_filled - 1.0f;
			healthShard3.fillAmount = 0f;
			healthShard4.fillAmount = 0f;
		}
		else if (bars_filled <= 3.0f) 
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = 1.0f;
			healthShard3.fillAmount = bars_filled - 2.0f;
			healthShard4.fillAmount = 0f;
		}
		else if (bars_filled <= 4.0f) 
		{
			healthShard1.fillAmount = 1.0f;
			healthShard2.fillAmount = 1.0f;
			healthShard3.fillAmount = 1.0f;
			healthShard4.fillAmount = bars_filled - 3.0f;
		}
		SetHealthText (health, max_Health);
	}

	public void CargoUpdate (int cargo, int max_Cargo) 
	{
		float bars_filled = (float)cargo / ((float)max_Cargo/3.0f);
		if (bars_filled <= 1.0f) 
		{
			cargoShard1.fillAmount = bars_filled;
			cargoShard2.fillAmount = 0f;
			cargoShard3.fillAmount = 0f;
		} 
		else if (bars_filled <= 2.0f)
		{
			cargoShard1.fillAmount = 1.0f;
			cargoShard2.fillAmount = bars_filled - 1.0f;
			cargoShard3.fillAmount = 0f;
		}
		else if (bars_filled <= 3.0f) 
		{
			cargoShard1.fillAmount = 1.0f;
			cargoShard2.fillAmount = 1.0f;
			cargoShard3.fillAmount = bars_filled - 2.0f;
		}
		SetCargoText (cargo, max_Cargo);
		Debug.Log ("bars_filled:" + bars_filled);
	}

	void SetCargoText(int cargo, int max_Cargo)
	{
		cargoText.text = cargo.ToString() + "/" + max_Cargo.ToString();
	}

	void SetHealthText(int health, int max_Health)
	{
		healthText.text = health.ToString() + "/" + max_Health.ToString();
	}

    public void SetTurnText(int turns)
    {
        turnText.text = "Turn: " + turns.ToString();
    }

    public void IconUpdate(int icon, int coolDown)
    {
        Color cd = Color.green;
        switch (coolDown)
        {
            case 4:
            {
                cd = Color.black;
            }
            break;

            case 3:
            {
                cd = Color.red / 3f;
            }
            break;

            case 2:
            {
                cd = Color.red / 3f * 2f;
            }
            break;

            case 1:
            {
                cd = Color.red;
            }
            break;

            case 0:
            {
                cd = Color.white;
            }
            break;

            default:
            {
                Debug.LogWarning("invalid IconUpdate color.");
            }
            break;
        }

        switch (icon)
        {
            case 1:
            {
                iconHyper.color = cd;
            }
            break;

            case 2:
            {
                iconEMP.color = cd;
            }
            break;

            case 3:
            {
                iconCloak.color = cd;
            }
            break;

            default:
            {
                Debug.LogWarning("invalid IconUpdate.");
            }
            break;
        }
    }

    public void Resume()
    {
        menu.SetActive(false);
        menuOpen = false;
    }

    public void MenuLoad()
    {
        SceneManager.LoadScene(0);
    }

    public void ControlMenu()
    {
        menu.SetActive(false);
        control.SetActive(true);
    }

    public void ControlExit()
    {
        menu.SetActive(true);
        control.SetActive(false);
    }

    public void PowerUpMenu()
    {
        menu.SetActive(false);
        powerUp.SetActive(true);
    }

    public void PowerUpExit()
    {
        menu.SetActive(true);
        powerUp.SetActive(false);
    }
}
