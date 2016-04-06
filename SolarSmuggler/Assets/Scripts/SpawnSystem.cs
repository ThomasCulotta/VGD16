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
    public int START_GAME_AREA = 0;
    public int MAX_GAME_AREA   = 300;

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

    //Planet Transform
    bool[] posAvailable;

    //Enemies
    /* NOTE(Thomas): Putting this in for an enemy check in system.
     *               MAX = 1 for single manual enemy.
     *               Enemy spawn function would be nice.
     */
    public const int MAX_ENEMIES = 1;
    private static bool[] enemyCheckList;

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
        posAvailable = new bool[MAX_GAME_AREA *MAX_GAME_AREA];
        initAvaiableSpots();
        spawnPlanets();
        spawnPickups();
        enemyCheckList = new bool[MAX_ENEMIES];
	}

    void LateUpdate()
    {
        if (GameMaster.CurrentState == GameMaster.GameState.ENEMY_TURN)
        {
            if (AllEnemiesCheckedIn())
            {
                GameMaster.CurrentState = GameMaster.GameState.ENVIRONMENT_TURN;
                enemyCheckList = new bool[MAX_ENEMIES];
            }
        }
    }

    private bool AllEnemiesCheckedIn()
    {
        for (int i = 0; i < MAX_ENEMIES; i++)
        {
            if (!enemyCheckList[i])
                return false;
        }
        return true;
    }

    // Enemies call this at the end of their turn with their id
    public static void CheckInEnemy(int i)
    {
        enemyCheckList[i] = true;
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
            Vector3 center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);

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
                            Instantiate(frost, center, Quaternion.identity);
                            frost.transform.FindChild("Planet 0").localPosition = GridPos2PlanetPos(spawnPos);
                        }
                        break;
                    case (Planets.PURPLE):
                        {
                            Debug.Log("Making Purple");
                            GameObject purple = (GameObject)Resources.Load("Prefabs/Purple Planet", typeof(GameObject));
                            purple.transform.localScale = new Vector3(size, size, size);
                            purple.GetComponentInChildren<Transform>().transform.localPosition = GridPos2PlanetPos(spawnPos);
                            Instantiate(purple, center, Quaternion.identity);
                        }
                        break;
                    case (Planets.WATER):
                        {
                            Debug.Log("Making WATER");
                            GameObject water = (GameObject)Resources.Load("Prefabs/Planet 2", typeof(GameObject));
                            water.GetComponentInChildren<Transform>().transform.localScale = new Vector3(size, size, size);
                            Instantiate(water, center, Quaternion.identity);
                            water.transform.FindChild("Planet 0").localPosition = GridPos2PlanetPos(spawnPos);
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
        return new Vector3((int)pos.x -300, 0, (int)pos.z - 300);
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
        int index = ((int)pos.x * (int)pos.x % 300) * (int)pos.z;
        posAvailable[index] = false;
    }

    bool isAvailable(Vector3 pos)
    {
        int index = ((int)pos.x * (int)pos.x % 300) * (int)pos.z;
        if (posAvailable[index] == true)
            return true;
        else
            return false;
    }
}
