using UnityEngine;
using System.Collections;

public class GridSpace : MonoBehaviour
{
    public bool currentlyMoving;
    private bool selected;
    
	void Start ()
    {
        currentlyMoving = true;
        iTween.MoveTo(gameObject, iTween.Hash("y", 0f, 
                                              "time", 0.1f, 
                                              "oncomplete", "DoneMoving", 
                                              "oncompletetarget", gameObject));
	}
	
	void Update ()
    {
        if (!currentlyMoving)
        {
            if (selected)
                transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
        else
            currentlyMoving = false;
	}

    void OnMouseDown()
    {
        PlayerController.nextPosition = new Vector3(transform.position.x, 0f, transform.position.y);
    }

    void OnMouseEnter()
    {
        selected = true;
        if (currentlyMoving) 
        {
            iTween.StopByName("MouseExit");
            currentlyMoving = false;
        }
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        currentlyMoving = true;
        iTween.MoveTo(gameObject, iTween.Hash("name", "MouseEnter", 
                                              "y", 0.1f, 
                                              "time", 0.5f, 
                                              "oncomplete", "DoneMoving", 
                                              "oncompletetarget", gameObject));
    }

    void OnMouseExit()
    {
        selected = false;
        if (currentlyMoving) 
        {
            iTween.StopByName("MouseEnter");
            currentlyMoving = false;
        }
        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        currentlyMoving = true;
        iTween.MoveTo(gameObject, iTween.Hash("name", "MouseEnter", 
                                              "y", 0f, 
                                              "time", 0.5f, 
                                              "oncomplete", "DoneMoving", 
                                              "oncompletetarget", gameObject));
    }

    void DoneMoving()
    {
        currentlyMoving = false;
    }
}
