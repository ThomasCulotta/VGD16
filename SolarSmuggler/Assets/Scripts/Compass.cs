using UnityEngine;
using System.Collections;

public class Compass : MonoBehaviour {

    public Transform target;
	// Use this for initialization
	void Start () {
        target = GameObject.Find("Space Station").transform;
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(target);
	}
}
