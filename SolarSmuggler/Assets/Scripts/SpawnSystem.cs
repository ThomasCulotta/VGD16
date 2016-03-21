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
    public int MAX_PICKUP_CELLS = 5;
    public int MAX_PLANET_CELLS = 3;
    public int MAX_PLANETS = 3;

    //Graphic Util
    public int pixWidth;
    public int pixHeight;
    public float xOrg;
    public float yOrg;
    public float scale;

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
                int size = Random.Range(3, 8);
                int hasMoon = Random.Range(0, 2);

                switch (spawnType)
                {
                    case (Planets.FROST):
                        {
                            Debug.Log("Making FROST");
                            GameObject frost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            frost.transform.localScale = new Vector3(size, size, size);
                            createTexture(frost);
                            Instantiate(frost, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                    case (Planets.DESERT):
                        {
                            Debug.Log("Making DESERT");
                            GameObject desert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            desert.transform.localScale = new Vector3(size, size, size);
                            createTexture(desert);
                            Instantiate(desert, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                    case (Planets.WATER):
                        {
                            Debug.Log("Making WATER");
                            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            water.transform.localScale = new Vector3(size, size, size);
                            createTexture(water);
                            Instantiate(water, new Vector3(spawnPosX, 0f, spawnPosZ), Quaternion.identity);
                        }
                        break;
                }
                planetCount++;
                if (planetCount == MAX_PLANETS)
                    return;
            }
        }
    }

    void createTexture(GameObject planet)
    {
        
        Texture2D planetTerrain = new Texture2D(pixHeight, pixWidth);
        Color32[] color = new Color32[pixWidth * pixHeight];

        int index =  0;
        for (float i = 0; i < pixHeight; i++)
        {
            for (float j = 0; j < pixWidth; j++)
            {

                float xCoord = xOrg + j / planetTerrain.width * scale;
                float yCoord = yOrg + i / planetTerrain.height * scale;
                float result = Mathf.PerlinNoise(xCoord, yCoord);
                byte RGBVal = (byte)(result * 255);
                color[index] = new Color32(RGBVal, RGBVal, RGBVal, 1);
                index++; 
            }
        }

        planetTerrain.SetPixels32(color);
        planetTerrain.Apply();
        planet.GetComponent<Renderer>().material.mainTexture = planetTerrain;
    }
}
