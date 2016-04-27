using UnityEngine;
using System.Collections;

public class StatsScript : MonoBehaviour {

    public GUIText health;
    public GUIText cargo;
    public GUIText turns;

	// Use this for initialization
	void Start () {
        GameObject player = GameObject.FindWithTag("Player");
        PlayerController playerController = player.GetComponent<PlayerController>();
        health.text = playerController.curr_Health.ToString();
        cargo.text  = playerController.curr_Cargo.ToString();
        turns.text = playerController.turnCount.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
