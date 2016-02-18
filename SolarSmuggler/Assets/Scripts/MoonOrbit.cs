using UnityEngine;
using System.Collections;

public class MoonOrbit : MonoBehaviour
{
	GameObject primary; //primary is an object that another object is orbitting around

	public Transform center;
	public Vector3 axis = new Vector3(0,180,0); //sets the axis for the orbit, 180 in the y sets it to orbit on the y axis around the primary
	public float radius = 25.0f; //the radius of the orbit, this overrides the position of the satellite. Position is relative to the primary
	public float rotationSpeed = 80.0f; //how fast the satellite orbits the primary

	void Start ()
	{
		primary = GameObject.FindGameObjectWithTag ("Planet");
		center = primary.transform; //sets the position of the primary, in this case the sun
	}
	void Update ()
	{
		transform.RotateAround (center.position, axis, rotationSpeed * Time.deltaTime); //orbits the satellite around the primary
	}


}
