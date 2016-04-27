﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StatsScript : MonoBehaviour {

    public Text health;
    public Text cargo;
    public Text turns;

	// Use this for initialization
	void Start () {
        health.text = "Health: " + PlayerPrefs.GetInt("Health").ToString();
        cargo.text  = "Cargo: " + PlayerPrefs.GetInt("Cargo").ToString();
        turns.text = "Turns Taken: " + PlayerPrefs.GetInt("Turns").ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MenuLoad()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
    }
}
