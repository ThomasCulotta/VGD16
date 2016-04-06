using UnityEngine;
using System.Collections;

public class GridSpace : MonoBehaviour
{
    private bool selected;
    
	void Start ()
    {
        if (PlayerController.hyperJumping)
            transform.GetComponent<Renderer>().material.color = Color.yellow;
	}
	
	void Update ()
    {
        if (selected)
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        else
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
	}
    
    void OnMouseEnter()
    {
        selected = true;

    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            PlayerController.nextPosition = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    void OnMouseExit()
    {
        selected = false;

    }
}
