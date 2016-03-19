using UnityEngine;
using System.Collections;

public class SpawnSystem : MonoBehaviour {
    float MIN_GAME_AREA_X = 0;
    float MIN_GAME_AREA_Z = 0;
    float MAX_GAME_AREA_X = 300;
    float MAX_GAME_AREA_Z = 300;

    int MAX_CELLS = 5;
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
        float cellSize = MAX_GAME_AREA_X / MAX_CELLS;

        for (int i = 0; i < MAX_CELLS;  i++) {
            float spawnMinX = cellSize * i;
            float spawnMaxX = cellSize * (i+1);

            spawnMinX += 5;
            spawnMaxX -= 5;

            for (int j = 0; j < MAX_CELLS; j++)
            {
                float spawnMinZ = cellSize * j;
                float spawnMaxZ = cellSize * (j+ 1);

                spawnMinZ += 5;
                spawnMaxZ -= 5;

                Pickups spawnType = (Pickups)Random.Range((int)Pickups.CUBE, (int)Pickups.COUNT);

                int spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                int spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);

                switch (spawnType)
                {
                    case (Pickups.CUBE):
                        {
                            Debug.Log("Making a Cube");
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Instantiate(cube, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                    case (Pickups.SPHERE):
                        {
                            Debug.Log("Making a Sphere");
                            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Instantiate(sphere, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                    case (Pickups.CAPSULE):
                        {
                            Debug.Log("Making a Capsule");
                            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            Instantiate(capsule, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                }
            }
        }
    }
}
