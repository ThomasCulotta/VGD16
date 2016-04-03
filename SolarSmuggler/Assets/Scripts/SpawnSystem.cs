/*
 * @Author: Patrick Blanchard
 * @Description: This Script spawns in Game Objects using
 *      a Blue Noise Procedural Generation.
 *
 *  @Use: Create an empty Game Object and add this script to it. 
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class SpawnSystem : MonoBehaviour {
    //Spawn Area
    public float START_GAME_AREA = 0;
    public float MAX_GAME_AREA   = 300;

    //This number can be tweaked to increase or decrease number of spawns
    public int MAX_PICKUP_CELLS = 5;
    public int MAX_PLANET_CELLS = 3;
    public int MAX_PLANETS = 3;

    //Graphic Util
    public int pixWidth;
    public int pixHeight;
    public float xOrg;
    public float yOrg;
    public float scale;

    //Spots not available for spawning
    private bool[] posAvailable;

    //Enum is used for random Pickup Spawning
    enum Pickups
    {
        CUBE,
        SPHERE,
        CAPSULE,
        COUNT
    }

    //Enum is used for random Planet Spawning
    enum Planets
    {
        FROST,
        DESERT,
        WATER,
        COUNT
    }

	// Use this for initialization
	void Start () {
        int size = (int)MAX_GAME_AREA * (int)MAX_GAME_AREA;
        posAvailable = new bool[size];
        initAvaiableSpots();
        spawnPlanets();
        spawnPickups();
	}
	
	void spawnPickups()
    {
        float cellSize = MAX_GAME_AREA / MAX_PICKUP_CELLS;

        //creating the cells
        for (int i = 0; i < MAX_PICKUP_CELLS;  i++) {
            float spawnMinX = cellSize * i;
            float spawnMaxX = cellSize * (i+1);

            //creating space between cells
            spawnMinX += 5;
            spawnMaxX -= 5;

            for (int j = 0; j < MAX_PICKUP_CELLS; j++)
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
                Vector3 spawnPos = new Vector3(spawnPosX, 0 , spawnPosZ);

                while (!isAvailable(spawnPos))
                {
                    spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                    spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);
                    spawnPos = new Vector3(spawnPosX, 0, spawnPosZ);
                }
                AddToList(spawnPos);

                //Creating the GameObjects at random spots on the map;

                switch (spawnType)
                {
                    case (Pickups.CUBE):
                        {
                            Debug.Log("Making a Cube");
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Instantiate(cube, spawnPos, Quaternion.identity);
                        }
                        break;
                    case (Pickups.SPHERE):
                        {
                            Debug.Log("Making a Sphere");
                            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Instantiate(sphere, spawnPos, Quaternion.identity);
                            
                        }
                        break;
                    case (Pickups.CAPSULE):
                        {
                            Debug.Log("Making a Capsule");
                            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            Instantiate(capsule, spawnPos, Quaternion.identity);
                            
                        }
                        break;
                }
            }
        }
    }

    void spawnPlanets()
    {
        float cellSize = MAX_GAME_AREA / MAX_PLANET_CELLS;

        //creating the cells
        for (int i = 0; i < MAX_PLANET_CELLS; i++)
        {
            float spawnMinX = cellSize * i;
            float spawnMaxX = cellSize * (i + 1);

            //creating space between cells
            spawnMinX += 5;
            spawnMaxX -= 5;

            int planetCount = 0;
            for (int j = 0; j < MAX_PLANET_CELLS; j++)
            {
                float spawnMinZ = cellSize * j;
                float spawnMaxZ = cellSize * (j + 1);

                //creating space between cells
                spawnMinZ += 5;
                spawnMaxZ -= 5;

                //Random Values to plug in
                Planets spawnType = (Planets)Random.Range((int)Planets.FROST, (int)Planets.COUNT);
                int spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                int spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);

                Vector3 spawnPos = new Vector3(spawnPosX, 0, spawnPosZ);

                while (!isAvailable(spawnPos))
                {
                    spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                    spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);
                    spawnPos = new Vector3(spawnPosX, 0, spawnPosZ);
                }

                AddToList(spawnPos);

                int size = Random.Range(3, 8);
                int hasMoon = Random.Range(0, 2);

                switch (spawnType)
                {
                    case (Planets.FROST):
                        {
                            Debug.Log("Making FROST");
                            GameObject frost = (GameObject) Resources.Load("Prefabs/Ice Planet", typeof(GameObject));
                            frost.GetComponentInChildren<Transform>().transform.localScale = new Vector3(size, size, size);
                            Instantiate(frost, spawnPos, Quaternion.identity);
                        }
                        break;
                    case (Planets.DESERT):
                        {
                            Debug.Log("Making DESERT");
                            GameObject desert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            desert.transform.localScale = new Vector3(size, size, size);
                            Instantiate(desert, spawnPos, Quaternion.identity);
                        }
                        break;
                    case (Planets.WATER):
                        {
                            Debug.Log("Making WATER");
                            GameObject water = (GameObject)Resources.Load("Prefabs/Planet 2", typeof(GameObject));
                            water.GetComponentInChildren<Transform>().transform.localScale = new Vector3(size, size, size);
                            Instantiate(water, spawnPos, Quaternion.identity);
                        }
                        break;
                }
                planetCount++;
                if (planetCount == MAX_PLANETS)
                    return;
            }
        }
    }

    void createTexture(GameObject moon)
    {
        //Init texture and color array
        Texture2D moonTerrain = new Texture2D(pixHeight, pixWidth);
        Color32[] color = new Color32[pixWidth * pixHeight];

        int index =  0;
        for (float i = 0; i < pixHeight; i++)
        {
            for (float j = 0; j < pixWidth; j++)
            {
                //looping through to assign color array perlin values
                float xCoord = xOrg + j / moonTerrain.width * scale;
                float yCoord = yOrg + i / moonTerrain.height * scale;
                float result = Mathf.PerlinNoise(xCoord, yCoord);
                byte RGBVal = (byte)(result * 255);
                color[index] = new Color32(RGBVal, RGBVal, RGBVal, 1);
                index++; 
            }
        }

        //applying pattern to the texture and giving the texture to the moon
        moonTerrain.SetPixels32(color);
        moonTerrain.Apply();
        moon.GetComponent<Renderer>().material.mainTexture = moonTerrain;
    }

    void initAvaiableSpots()
    {
        for(int i = 0; i < posAvailable.Length; i++)
        {
            posAvailable[i] = true;
        }
    }

    void AddToList(Vector3 pos)
    {
        int index = (((int)pos.x + 4) % (int)MAX_GAME_AREA) + (((int)pos.z + 3) % (int)MAX_GAME_AREA);
        posAvailable[index] = false;
    }

    bool isAvailable(Vector3 pos)
    {
        int index = (((int)pos.x + 4) % (int)MAX_GAME_AREA) + (((int)pos.z + 3) % (int)MAX_GAME_AREA);
        if (posAvailable[index] == true)
            return true;
        else
            return false;
    }
}
