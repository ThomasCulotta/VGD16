using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public GameObject main;
    public GameObject help;
    public GameObject credit;

	// Use this for initialization
	void Start () {
        main.SetActive(true);
        help.SetActive(false);
        credit.SetActive(false);
	}

    public void LoadHelp()
    {
        main.SetActive(false);
        help.SetActive(true);
    }

    public void ExitHelp()
    {
        main.SetActive(true);
        help.SetActive(false);
    }

    public void LoadCredit()
    {
        main.SetActive(false);
        credit.SetActive(true);
    }

	public void ExitCredit()
	{
		main.SetActive(true);
		credit.SetActive(false);
	}

    public void StartLevel()
    {
        SceneManager.LoadScene("Thomas");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
