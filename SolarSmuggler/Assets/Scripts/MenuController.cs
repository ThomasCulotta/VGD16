using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public GameObject main;
    public GameObject help;
    public GameObject help1;
    public GameObject help2;
    public GameObject credit;


	// Use this for initialization
	void Start () {
        main.SetActive(true);
        help.SetActive(false);
        help1.SetActive(false);
        help2.SetActive(false);
        credit.SetActive(false);
	}

    public void LoadHelp()
    {
        main.SetActive(false);
        help.SetActive(true);
        help1.SetActive(true);
        help2.SetActive(true);
    }

    public void ExitHelp()
    {
        main.SetActive(true);
        help.SetActive(false);
        help1.SetActive(false);
        help2.SetActive(false);
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
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
