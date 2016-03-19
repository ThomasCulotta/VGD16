/*
 * @Author: Patrick Blanchard
 * @Description: This Script spawns in Game Objects using
 *      a Blue Noise Procedural Generation.
 *
 *  @Use: Create an empty Game Object and add this script to it. 
 *
 */

using UnityEngine;
using System.Collections;

public class SpawnSystem : MonoBehaviour {
    //Spawn Area
    public float START_GAME_AREA = 0;
    public float MAX_GAME_AREA   = 300;

    //This number can be tweaked to increase or decrease number of spawns
    public int MAX_CELLS = 5;

    //Enum is used for random Pickup Spawning
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
        float cellSize = MAX_GAME_AREA / MAX_CELLS;

        //creating the cells
        for (int i = 0; i < MAX_CELLS;  i++) {
            float spawnMinX = cellSize * i;
            float spawnMaxX = cellSize * (i+1);

            //creating space between cells
            spawnMinX += 5;
            spawnMaxX -= 5;

            for (int j = 0; j < MAX_CELLS; j++)
            {
                float spawnMinZ = cellSize * j;
                float spawnMaxZ = cellSize * (j+ 1);

                //creating space between cells
                spawnMinZ += 5;
                spawnMaxZ -= 5;

                //Random Values to plug in
                Pickups spawnType = (Pickups)Random.Range((int)Pickups.CUBE, (int)Pickups.COUNT);
                int spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                int spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);

                //Creating the GameObjects at random spots on the map;
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
