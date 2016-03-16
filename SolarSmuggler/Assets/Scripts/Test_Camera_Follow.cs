using UnityEngine;
using System.Collections;

public class Test_Camera_Follow : MonoBehaviour {

    public Transform target;

    [System.Serializable]
    public class PositionSettings
    {
        //distance from our target
        //bools for zooming and smoothfollowing
        //min and max zoom settings
        public float distanceFromTarget = -50;
        public bool allowZoom           = true;
        public float zoomSmooth         = 100;
        public float zoomStep           = 2;
        public float maxZoom            = -30;
        public float minZoom            = -60;
        public bool smoothFollow        = true;
        public float smooth             = 0.05f;

        [HideInInspector]
        public float newDistance = -50; //used for smooth zooming & gives us a "destination" zoom
    }

    [System.Serializable]
    public class OrbitSettings
    {
        //holindg current x and y rotation for camera
        //bool for allowing orbit
        public float xRotation      = -65;
        public float yRotation      = -180;
        public bool allowOribit     = true;
        public float yOrbitSmooth   = 0.5f;
    }

    [System.Serializable]
    public class InputSettings
    {
        public string MOUSE_ORBIT = "MouseOrbit";
        public string ZOOM = "Mouse ScrollWheel";
    }

    public PositionSettings position    = new PositionSettings();
    public OrbitSettings orbit          = new OrbitSettings();
    public InputSettings input          = new InputSettings();

    //target destination
    Vector3 destination             = Vector3.zero;
    Vector3 camVelocity             = Vector3.zero;
    Vector3 currentMousePosition    = Vector3.zero;
    Vector3 previousMousePosition   = Vector3.zero;
    float mouseOrbitInput, zoomInput;

    void Start()
    {
        //setting camera target
        SetCameraTarget(target);

        if (target)
        {
            moveToTarget();//moves camera to player
        }
    }

    public void SetCameraTarget(Transform t)
    {
        //if we want to set a new target at runtime
        target = t;
        if (target == null)
        {
            Debug.LogError("Camera needs a target");
        }
    }

    void getInput()
    {
        //filling values for our input variables
        mouseOrbitInput = Input.GetAxisRaw(input.MOUSE_ORBIT);
        zoomInput = Input.GetAxisRaw(input.ZOOM);
    }

    void Update()
    {
        //getting input and zooming
        getInput();
        if (position.allowZoom)
        {
            zoomInTarget();
        }
    }

    void FixedUpdate()
    {
        if (target)
        {
            moveToTarget();
            lookAtTarget();
            mouseOrbitTarget();
        }
    }

    void moveToTarget()
    {
        //handling getting our camera to its desintation position
        destination = target.position;
        destination += Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0) * -Vector3.forward * position.distanceFromTarget; //quater * vec3 = vec3

        //move camera to dest
        if (position.smoothFollow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVelocity, position.smooth);
        }
        //otherwise just lock onto the target
        else
        {
            transform.position = destination;
        }
    }

    void lookAtTarget()
    {
        //handling getting our camera to look at the target at all times
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = targetRotation;
    }

    //orbiting target
    void mouseOrbitTarget()
    {
        //getting the camera to orbit around our character
        previousMousePosition = currentMousePosition;
        currentMousePosition = Input.mousePosition;

        //if mouse0 is pressed down
        if(mouseOrbitInput > 0)
        {
            orbit.yRotation += (currentMousePosition.x - previousMousePosition.x) * orbit.yOrbitSmooth;
        }
    }

    //zoom target
    void zoomInTarget()
    {
        //modifying the distance from target to be closer/further away from target
        position.newDistance += position.zoomStep * zoomInput * 25;

        position.distanceFromTarget = Mathf.Lerp(position.distanceFromTarget, position.newDistance, position.zoomSmooth * Time.deltaTime);

        //maintain distance threshold
        if(position.distanceFromTarget > position.maxZoom)
        {
            position.distanceFromTarget = position.maxZoom;
            position.newDistance = position.maxZoom;
        }
        if (position.distanceFromTarget < position.minZoom)
        {
            position.distanceFromTarget = position.minZoom;
            position.newDistance = position.minZoom;
        }
    }
}
