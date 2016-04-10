/*
 * @Author: Patrick Blanchard
 * @Description: This Script spawns in Game Objects using
 *      a Blue Noise Procedural Generation.
 *
 * @Use: Create an empty Game Object and add this script to it. 
 *
 */

using UnityEngine;
using System.Collections;

public class SpawnSystem : MonoBehaviour {
    //Spawn Area
    private int START_GAME_AREA = 0;
    public int MAX_GAME_AREA   = 50;
    private Vector3 center;


    //These number can be tweaked to increase or decrease number of spawns
    public int MAX_PICKUP_CELLS = 5;
    public int MAX_PLANET_CELLS = 3;
    public int MAX_PLANETS = 3;

    //Graphic Util
    public int pixWidth;
    public int pixHeight;
    public float xOrg;
    public float yOrg;
    public float scale;

    //List of valid Spawning positions
    bool[] posAvailable;

    //Prefab Planets
    GameObject frost;
    GameObject purple;
    GameObject water;

    Transform frostChild;
    Transform purpleChild;
    Transform waterChild;

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
        PURPLE,
        WATER,
        COUNT
    }

	// Use this for initialization
	void Start ()
    {
        //Prefab Init
        /***********************************************************************************/

        //Planet Nulls
        frost = (GameObject)Resources.Load("Prefabs/Ice Planet", typeof(GameObject));
        purple = (GameObject)Resources.Load("Prefabs/Purple Planet", typeof(GameObject));
        water = (GameObject)Resources.Load("Prefabs/Planet 2", typeof(GameObject));

        //Planet Childs
        frostChild = frost.transform.FindChild("Planet 0");
        purpleChild = purple.transform.FindChild("Planet 0");
        waterChild = water.transform.FindChild("Planet 0");

        /***********************************************************************************/

        //Spawn Area Init
        center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);
        posAvailable = new bool[MAX_GAME_AREA * MAX_GAME_AREA];
        initAvaiableSpots();

        //Spawning
        resetPlanetPrefabs();
        spawnPlanetsWhite();
        //spawnPlanets();
        //spawnPickups();
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
        Debug.Log("Cellsize " + cellSize);
        //creating the cells
        for (int i = 0; i < MAX_PLANET_CELLS; i++)
        {
            float spawnMinX = cellSize * i;
            float spawnMaxX = cellSize * (i + 1);
            

            //creating space between cells
            spawnMinX += 20;
            spawnMaxX -= 20;

            for (int j = 0; j < MAX_PLANET_CELLS; j++)
            {
                float spawnMinZ = cellSize * j;
                float spawnMaxZ = cellSize * (j + 1);

                //creating space between cells
                spawnMinZ += 20;
                spawnMaxZ -= 20;

                //Random Values to plug in
                int spawnPosX = (int)Random.Range(spawnMinX, spawnMaxX);
                int spawnPosZ = (int)Random.Range(spawnMinZ, spawnMaxZ);
                Vector3 spawnPos = new Vector3(spawnPosX, 0, spawnPosZ);

                Debug.Log("SpawnPos: "+spawnPos);

                AddToList(spawnPos);
                GameObject planet = makePlanet(spawnPos);
                Instantiate(planet, center, Quaternion.identity);
            }
        }
    }


    void spawnPlanetsWhite()
    {
        for(int i=0; i<MAX_PLANETS; i++)
        {
            int spawnX = Random.Range(START_GAME_AREA, MAX_GAME_AREA + 1);
            int spawnY = Random.Range(START_GAME_AREA, MAX_GAME_AREA + 1);
            Vector3 spawnPos = new Vector3(spawnX, 0, spawnY);

            while (!isAvailable(spawnPos))
            {
                spawnX = Random.Range(START_GAME_AREA, MAX_GAME_AREA + 1);
                spawnY = Random.Range(START_GAME_AREA, MAX_GAME_AREA + 1);
                spawnPos = new Vector3(spawnX, 0, spawnY);
            }

            AddToList(spawnPos);
            GameObject planet = makePlanet(spawnPos);
            Instantiate(planet, center, Quaternion.identity);
        }

    }

    void createTexture(GameObject moon)
    {
        
        Texture2D moonTerrain = new Texture2D(pixHeight, pixWidth);
        Color32[] color = new Color32[pixWidth * pixHeight];

        int index =  0;
        for (float i = 0; i < pixHeight; i++)
        {
            for (float j = 0; j < pixWidth; j++)
            {

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


    Vector3 GridPos2PlanetPos(Vector3 pos)
    {
        Vector3 vectorTransform = Vector3.zero;
        if (pos.x > 75) { vectorTransform.x = (int)pos.x + 75; }
        else if (pos.x < 75) { vectorTransform.x = (int)pos.x - 75; }
        else vectorTransform.x = 150;

        if (pos.z > 75) { vectorTransform.z = (int)pos.z + 75; }
        else if (pos.z < 75) { vectorTransform.z = (int)pos.z - 75; }
        else vectorTransform.z = 150;

        return vectorTransform;
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
        int index = ((int)pos.x * MAX_GAME_AREA) + (int)pos.z;
        posAvailable[index] = false;
    }

    bool isAvailable(Vector3 pos)
    {
        int index = ((int)pos.x * MAX_GAME_AREA) + (int)pos.z;
        if (posAvailable[index] == true)
            return true;
        else
            return false;
    }

    void spawnSun(Vector3 pos)
    {
        Debug.Log("Spawning Sun");
        GameObject sun = (GameObject)Resources.Load("Prefabs/");
    }

    void resetPlanetPrefabs()
    {
        Vector3 unitVector = new Vector3(1,1,1);

        frost.transform.localScale = unitVector;
        purple.transform.localScale = unitVector;
        water.transform.localScale = unitVector;

        frostChild.localScale = unitVector;
        purpleChild.localScale = unitVector;
        waterChild.localScale = unitVector;

        frostChild.localPosition = Vector3.zero;
        purpleChild.localPosition = Vector3.zero;
        waterChild.localPosition = Vector3.zero;
    }

    GameObject makePlanet(Vector3 spawnPos)
    {
        Planets spawnType = (Planets)Random.Range((int)Planets.FROST, (int)Planets.COUNT);
        int size = Random.Range(3, 8);
        int hasMoon = Random.Range(0, 2);

        switch (spawnType)
        {
            case (Planets.FROST):
                {
                    Debug.Log("Making FROST");
                    frost.transform.localScale = new Vector3(size, size, size);
                    frostChild.localPosition = frost.transform.InverseTransformPoint(spawnPos) - center/2;
                    Debug.Log("World Posisiton\n" + frost.transform.localToWorldMatrix);
                    return frost;

                }
            case (Planets.PURPLE):
                {
                    Debug.Log("Making Purple");
                    purple.transform.localScale = new Vector3(size, size, size);
                    purpleChild.localPosition = purple.transform.InverseTransformPoint(spawnPos) - center/2;
                    Debug.Log("World Posisiton\n" + purple.transform.localToWorldMatrix);
                    return purple;
                }
            case (Planets.WATER):
                {
                    Debug.Log("WATER");
                    water.transform.localScale = new Vector3(size, size, size);
                    waterChild.localPosition = water.transform.InverseTransformPoint(spawnPos) - center/2;
                    Debug.Log("World Posisiton\n" + water.transform.transform.localToWorldMatrix);
                    return water;
                }
        }
        return frost;
    }
}
