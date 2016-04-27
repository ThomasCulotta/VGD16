using UnityEngine;
using System.Collections;

public class MouseCursor : MonoBehaviour {
    public Texture2D cursorTexture;
    public Texture2D cursorTextureHighlight;

	// Use this for initialization
	void Start () {
        Cursor.SetCursor(this.cursorTexture, Vector2.zero, CursorMode.Auto);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            //change cursor on left click
            Cursor.SetCursor(this.cursorTextureHighlight, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            //set original cursor
            Cursor.SetCursor(this.cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }

    void OnGUI()
    {
        if (Event.current.button == 1)
        {
            Debug.Log("Pressed Right click");
            Cursor.SetCursor(this.cursorTextureHighlight, Vector2.zero, CursorMode.Auto);
        }
    }
}
