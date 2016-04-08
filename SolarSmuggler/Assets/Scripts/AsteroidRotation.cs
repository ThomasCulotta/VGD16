using UnityEngine;
using System.Collections;

public class AsteroidRotation : MonoBehaviour
{
    private Vector3 myRotation;

    void Start()
    {
        myRotation = new Vector3(Random.Range(13f, 17f), Random.Range(28f, 32f), Random.Range(43f, 47f)) / Random.Range(1.5f, 2.5f);
    }
	
    void Update ()
	{
        transform.Rotate(myRotation * Time.deltaTime);
	}
}
