using UnityEngine;
using System.Collections;

public class SpawnSystem : MonoBehaviour {
    float MIN_GAME_AREA_X = 0;
    float MIN_GAME_AREA_Z = 0;
    float MAX_GAME_AREA_X = 300;
    float MAX_GAME_AREA_Z = 300;

    int PICKUP_COUNT = 100;

    enum Pickups
    {
        CUBE,
        SPHERE,
        CAPSULE,
        COUNT
    }

	// Use this for initialization
	void Start () {
        spawnPickups();
	}
	
	void spawnPickups()
    {
        for (int i = 0; i < PICKUP_COUNT;  i++) {
            float spawnX = Random.Range(0, 301);
            float spawnZ = Random.Range(0, 301);
            Pickups spawnType = (Pickups)Random.Range((int)Pickups.CUBE, (int)Pickups.COUNT);

            switch (spawnType)
            {
                case (Pickups.CUBE):
                    {
                        Debug.Log("Making a Cube");
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Instantiate(cube, new Vector3(spawnX, 0f, spawnZ), Quaternion.identity);
                    }
                    break;
                case (Pickups.SPHERE):
                    {
                        Debug.Log("Making a Sphere");
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Instantiate(sphere, new Vector3(spawnX, 0f, spawnZ), Quaternion.identity);
                    }
                    break;
                case (Pickups.CAPSULE):
                    {
                        Debug.Log("Making a Capsule");
                        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        Instantiate(capsule, new Vector3(spawnX, 0f, spawnZ), Quaternion.identity);
                    }
                    break;
            }

        }
    }
}
