using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Buttons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Text text;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = Color.cyan;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = Color.white; 
    }

	public void ColorChange()
	{
		text.color = Color.white;
	}
}
