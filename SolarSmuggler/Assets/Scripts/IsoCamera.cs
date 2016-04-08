using UnityEngine;
using System.Collections;

public class IsoCamera : MonoBehaviour
{
    private const float CAM_SPEED = 6f;
    private const float ROT_SPEED = 50f;
    private const float ZOOM_SPEED = 10f;

    private GameObject player;
    private GameObject cam;

    private float deltaX;
    private float deltaZ;
    private float deltaRotY;
    private float deltaZoom;
    private bool  moveToPlayer;

    private Vector3 prevMousePos = Vector3.zero;
    private Vector3 curMousePos  = Vector3.zero;

    // Use this for initialization
    void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = transform.GetChild(0).gameObject;
        moveToPlayer = false;
	}
	
	void FixedUpdate()
    {
        deltaX = 0f;
        deltaZ = 0f;
        deltaRotY = 0f;
        prevMousePos = curMousePos;
        curMousePos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            moveToPlayer = !moveToPlayer;
        }

        // Translation X and Z
        if (moveToPlayer)
        {
            if (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.25f)
                deltaX = (player.transform.position.x - transform.position.x) / 5f;
            if (Mathf.Abs(transform.position.z - player.transform.position.z) > 0.25f)
                deltaZ = (player.transform.position.z - transform.position.z) / 5f;
        }
        else
        {
            deltaX = Input.GetAxisRaw("Horizontal");
            deltaZ = Input.GetAxisRaw("Vertical");
        }
        
        // Rotation Y
        if (Input.GetMouseButton(0))
        {
            //if mouse0 is pressed down
            deltaRotY = (curMousePos.x - prevMousePos.x);
        }

        deltaZoom = Input.GetAxis("Mouse ScrollWheel");
        if (cam.transform.localPosition.z > -3f && deltaZoom > 0)
            deltaZoom = 0f;
        else if (cam.transform.localPosition.z < -10f && deltaZoom < 0)
            deltaZoom = 0f;
    }

	void Update()
    {
        if (deltaX != 0 || deltaZ != 0)
        {
            if (moveToPlayer)
                transform.position = transform.position + new Vector3(deltaX * CAM_SPEED * Time.deltaTime, 0f, deltaZ * CAM_SPEED * Time.deltaTime);
            else
                transform.Translate(new Vector3(deltaX * CAM_SPEED * Time.deltaTime, 0f, deltaZ * CAM_SPEED * Time.deltaTime));
        }
        
        if (deltaRotY != 0)
            transform.Rotate(0f, deltaRotY * ROT_SPEED * Time.deltaTime, 0f);

        if (deltaZoom != 0)
            cam.transform.Translate(Vector3.forward * deltaZoom * ZOOM_SPEED);
    }
}
