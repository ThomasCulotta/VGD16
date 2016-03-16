using UnityEngine;
using System.Collections;

public class Test_Movement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 10, 0f, Input.GetAxis("Vertical") * Time.deltaTime * 10);
    }
}
