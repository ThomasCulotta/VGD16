using UnityEngine;
using System.Collections;

public class GridSpace : MonoBehaviour
{
    private bool selected;
    public Color baseColor = new Color(50f/255f, 150f/255f, 240f/255f, 200f/255f);
    
	void Start ()
    {
        if (PlayerController.hyperJumping)
            baseColor = Color.yellow;
        
        transform.GetComponent<Renderer>().material.color = baseColor;
	}
	
	void Update ()
    {
        if (selected)
        {
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            transform.GetComponent<Renderer>().material.color = new Color(baseColor.r + 0.2f, baseColor.g + 0.2f, baseColor.b + 0.2f, baseColor.a);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.GetComponent<Renderer>().material.color = baseColor;
        }
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
