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
    public int MAX_CARGO = 10;
    public int MAX_ENEMIES = 5;

    //Graphic Util
    public int pixWidth;
    public int pixHeight;
    public float xOrg;
    public float yOrg;
    public float scale;

    //Check Valid Spawning positions
    RaycastHit hit;
    bool[] posAvailable;

    //Prefabs
    private GameObject frost;
    private GameObject purple;
    private GameObject water;

    private Transform frostChild;
    private Transform purpleChild;
    private Transform waterChild;

    private GameObject cargo;
    private GameObject player;
    private GameObject enemy;
    private GameObject cameraMaster;
    private GameObject sun;
    private GameObject UI;
    private GameObject spaceStation;
    private GameObject AudioController;
    


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
    void Start()
    {

        //Prefab Init
        /***********************************************************************************/

        //Planet Nulls
        frost  = (GameObject)Resources.Load("Prefabs/Ice Planet", typeof(GameObject));
        purple = (GameObject)Resources.Load("Prefabs/Purple Planet", typeof(GameObject));
        water  = (GameObject)Resources.Load("Prefabs/Planet 2", typeof(GameObject));

        //Planet Childs
        frostChild  = frost.transform.FindChild("Planet 0");
        purpleChild = purple.transform.FindChild("Planet 0");
        waterChild  = water.transform.FindChild("Planet 0");

        //Other Prefabs
        cargo           = (GameObject)Resources.Load("Prefabs/Supplies");
        player          = (GameObject)Resources.Load("Prefabs/Player");
        enemy           = (GameObject)Resources.Load("Prefabs/Enemy");
        cameraMaster    = (GameObject)Resources.Load("Prefabs/Game Camera");
        UI              = (GameObject)Resources.Load("Prefabs/Player UI");
        sun             = (GameObject)Resources.Load("Prefabs/Sun");
        spaceStation    = (GameObject)Resources.Load("Prefabs/Space Station");
        AudioController = (GameObject)Resources.Load("Prefabs/AudioController");

        /***********************************************************************************/

        //Spawn Area Init
        center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);
        posAvailable = new bool[(MAX_GAME_AREA * MAX_GAME_AREA) + 1];
        initAvaiableSpots();
        transform.position = center;

        //Spawning
        resetPlanetPrefabs();
        spawnSun();
        spawnPlanetsWhite();
        spawnCargoWhite();
        spawnSpaceStation();
        spawnPlayer();
        spawnEnemiesWhite();
        loadPlayerUI();

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

    void spawnMoon(GameObject moon)
    {

    }

    void spawnCargoWhite()
    {
        for (int i = 0; i < MAX_CARGO; i++)
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
            Instantiate(cargo, spawnPos, Quaternion.identity);
            cargo.tag = "Cargo";
        }
    }

    void spawnEnemiesWhite()
    {
        for (int i = 0; i < MAX_ENEMIES; i++)
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
            Instantiate(enemy, spawnPos, Quaternion.identity);
            cargo.tag = "Enemy";
        }
    }

    void spawnSpaceStation()
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
        spaceStation.transform.localScale = new Vector3(1, 1, 1);
        Instantiate(spaceStation, spawnPos, Quaternion.identity);
    }

    void spawnPlayer()
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
        Instantiate(player, spawnPos, Quaternion.identity);
        //Debug.Log("SpawnPos " + spawnPos);
        //Debug.Log("PlayerPo " + player.transform.position);
        player.tag = "Player";
        initCamera(spawnPos);
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
        //Debug.Log("Index: " + index);
        if (posAvailable[index] == true)
            return true;
        else
            return false;
    }

    void spawnSun()
    {
        Debug.Log("Spawning Sun");
        Instantiate(sun, center, Quaternion.identity);
    }

    void initCamera(Vector3 spawnPos)
    {
        Instantiate(cameraMaster, spawnPos, Quaternion.identity);
        Instantiate(AudioController, spawnPos, Quaternion.identity);

        AudioController.transform.parent = cameraMaster.transform;
        
    }

    void loadPlayerUI( )
    {
        Instantiate(UI, Vector3.zero, Quaternion.identity);
    }

    void resetPlanetPrefabs()
    {
        Vector3 unitVector = new Vector3(1,1,1);

        frost.transform.localScale = unitVector;
        purple.transform.localScale = unitVector;
        water.transform.localScale = unitVector;

        frost.transform.position = Vector3.zero;
        purple.transform.position = Vector3.zero;
        water.transform.position = Vector3.zero;

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
                    frostChild.position = spawnPos - center;
                    frostChild.localScale= new Vector3(size, size, size);
                    //Debug.Log("Child.Posisiton " + frostChild.position +"\nSpawnPos " + spawnPos);
                    return frost;

                }
            case (Planets.PURPLE):
                {
                    Debug.Log("Making Purple");
                    purpleChild.position = spawnPos - center;
                    purpleChild.localScale = new Vector3(size, size, size);
                    //Debug.Log("Child.Posisiton " + purpleChild.position + "\nSpawnPos " + spawnPos);
                    return purple;
                }
            case (Planets.WATER):
                {
                    Debug.Log("WATER");
                    waterChild.position = spawnPos - center;
                    waterChild.localScale = new Vector3(size, size, size);
                    //Debug.Log("Child.Posisiton " + waterChild.position + "\nSpawnPos " + spawnPos);
                    return water;
                }
        }
        return frost;
    }

    int getMaxEnemies()
    {
        return MAX_ENEMIES;
    }
}
