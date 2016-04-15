using UnityEngine;
using System.Collections;

public class IsoCamera : MonoBehaviour
{
    private const float CAM_SPEED = 6f;
    private const float ROT_SPEED = 35f;
    private const float ZOOM_SPEED = 50f;

    private GameObject player;
    private GameObject cam;

    private Vector3 deltaCam;
    private float deltaRotY;
    private float deltaZoom;
    private bool  moveToPlayer = true;

    private Vector3 prevMousePos = Vector3.zero;
    private Vector3 curMousePos  = Vector3.zero;

    // Use this for initialization
    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = transform.GetChild(0).gameObject;
        cam.transform.LookAt(transform.position);
        moveToPlayer = true;
	}

	void FixedUpdate()
    {
        deltaCam = Vector3.zero;
        deltaRotY = 0f;
        prevMousePos = curMousePos;
        curMousePos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Tab))
            moveToPlayer = !moveToPlayer;

        // Translation X and Z
        if (moveToPlayer)
        {
            deltaCam = player.transform.position - transform.position;
            if (deltaCam.magnitude < 0.05f)
                deltaCam = Vector3.zero;
        }
        else
            deltaCam = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // Rotation Y
        if (Input.GetMouseButton(0))
            deltaRotY = prevMousePos.x - curMousePos.x;

        // Zoom Z
        deltaZoom = Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -1f, 1f);
        if ((cam.transform.localPosition.z > -3f && deltaZoom > 0) ||
            (cam.transform.localPosition.z < -10f && deltaZoom < 0))
            deltaZoom = 0f;
    }

	void Update()
    {
        if (deltaCam.magnitude != 0)
        {
            if (moveToPlayer)
                transform.position = transform.position + deltaCam * CAM_SPEED * Time.deltaTime / 3f;
            else
                transform.Translate(deltaCam * CAM_SPEED * Time.deltaTime);
        }

        if (deltaRotY != 0)
            transform.Rotate(0f, deltaRotY * ROT_SPEED * Time.deltaTime, 0f);

        if (deltaZoom != 0)
            cam.transform.Translate(Vector3.forward * deltaZoom * ZOOM_SPEED * Time.deltaTime);
    }

    public void DamageIndicator()
    {
        iTween.ShakePosition(gameObject, iTween.Hash("amount", new Vector3(0.4f, 0.4f, 0.4f), "time", 0.5f));
    }
}
