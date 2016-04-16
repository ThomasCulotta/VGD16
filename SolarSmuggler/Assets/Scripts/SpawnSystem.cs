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
    private int START_GAME_AREA;
    public int MAX_GAME_AREA;
    private Vector3 center;


    //These number can be tweaked to increase or decrease number of spawns
    public int MAX_PLANETS;
    public int MAX_CARGO;
    public static int MAX_ENEMIES;

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
    private GameObject moon;

    private Transform frostChild;
    private Transform purpleChild;
    private Transform waterChild;

    private Transform frostMoon;
    private Transform purpleMoon;
    private Transform waterMoon;

    private Transform frostMoonChild;
    private Transform purpleMoonChild;
    private Transform waterMoonChild;

    private GameObject cargo;
    private GameObject player;
    private GameObject enemy;
    private GameObject cameraMaster;
    private GameObject sun;
    private GameObject UI;
    private GameObject spaceStation;
    private Transform spaceStationChild;
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
        
        //Planet Children
        frostChild  = frost.transform.FindChild("Planet 0");
        purpleChild = purple.transform.FindChild("Planet 0");
        waterChild  = water.transform.FindChild("Planet 0");

        //Moons
        frostMoon  = frostChild.transform.FindChild("Moon");
        purpleMoon = purpleChild.transform.FindChild("Moon");
        waterMoon  = waterChild.transform.FindChild("Moon");

        //Moon Children
        frostMoonChild  = frostMoon.transform.FindChild("Moon 0");
        purpleMoonChild = purpleMoon.transform.FindChild("Moon 0");
        waterMoonChild  = waterMoon.transform.FindChild("Moon 0");

        //Other Prefabs
        cargo             = (GameObject)Resources.Load("Prefabs/Supplies");
        player            = (GameObject)Resources.Load("Prefabs/Player");
        enemy             = (GameObject)Resources.Load("Prefabs/Enemy");
        cameraMaster      = (GameObject)Resources.Load("Prefabs/Game Camera");
        UI                = (GameObject)Resources.Load("Prefabs/Player UI");
        sun               = (GameObject)Resources.Load("Prefabs/Sun");
        spaceStation      = (GameObject)Resources.Load("Prefabs/Space Station Null");
        AudioController   = (GameObject)Resources.Load("Prefabs/AudioController");

        //Other Prefab Children
        spaceStationChild = spaceStation.transform.FindChild("Space Station");

        /***********************************************************************************/

        switch (SpawnMaster.CURRENT_STATE)
        {
            case (SpawnMaster.SpawnState.SMALL):
                {
                    spawnSmallSolarSystem();
                }
                break;
            case (SpawnMaster.SpawnState.MEDIUM):
                {
                    spawnMediumSolarSystem();
                }
                break;
            case (SpawnMaster.SpawnState.LARGE):
                {
                    spawnLargeSolarSystem();
                }
                break;
        }

    }


    void spawnSmallSolarSystem()
    {
        MAX_GAME_AREA = SpawnMaster.SMALL_MAP_SIZE;
        MAX_PLANETS   = SpawnMaster.SMALL_PLANET;
        MAX_CARGO     = SpawnMaster.SMALL_CARGO;
        MAX_ENEMIES   = SpawnMaster.SMALL_ENEMY;

        center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);
        posAvailable = new bool[(MAX_GAME_AREA * MAX_GAME_AREA) + MAX_GAME_AREA + 1];
        initAvaiableSpots();
        transform.position = center;

        resetPlanetPrefabs();
        spawnSun();
        spawnPlanetsWhite(MAX_PLANETS);
        spawnCargoWhite(MAX_CARGO);
        spawnSpaceStation();
        spawnPlayer();
        spawnEnemiesWhite(MAX_ENEMIES);
        loadPlayerUI();
    }

    void spawnMediumSolarSystem()
    {
        MAX_GAME_AREA = SpawnMaster.MEDIUM_MAP_SIZE;
        MAX_PLANETS   = SpawnMaster.MEDIUM_PLANET;
        MAX_CARGO     = SpawnMaster.MEDIUM_CARGO;
        MAX_ENEMIES   = SpawnMaster.MEDIUM_ENEMY;

        center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);
        posAvailable = new bool[(MAX_GAME_AREA * MAX_GAME_AREA) + MAX_GAME_AREA + 1];
        initAvaiableSpots();
        transform.position = center;

        resetPlanetPrefabs();
        spawnSun();
        spawnPlanetsWhite(MAX_PLANETS);
        spawnCargoWhite(MAX_CARGO);
        spawnSpaceStation();
        spawnPlayer();
        spawnEnemiesWhite(MAX_ENEMIES);
        loadPlayerUI();
    }

    void spawnLargeSolarSystem()
    {
        MAX_GAME_AREA = SpawnMaster.LARGE_MAP_SIZE;
        MAX_PLANETS   = SpawnMaster.LARGE_PLANET;
        MAX_CARGO     = SpawnMaster.LARGE_CARGO;
        MAX_ENEMIES   = SpawnMaster.LARGE_ENEMY;

        center = new Vector3(MAX_GAME_AREA / 2, 0, MAX_GAME_AREA / 2);
        posAvailable = new bool[(MAX_GAME_AREA * MAX_GAME_AREA) + MAX_GAME_AREA + 1];
        initAvaiableSpots();
        transform.position = center;

        resetPlanetPrefabs();
        spawnSun();
        spawnPlanetsWhite(MAX_PLANETS);
        spawnCargoWhite(MAX_CARGO);
        spawnSpaceStation();
        spawnPlayer();
        spawnEnemiesWhite(MAX_ENEMIES);
        loadPlayerUI();
    }

    void spawnPlanetsWhite(int numPlanets)
    {
        for(int i=0; i<numPlanets; i++)
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
            GameObject myPlanet = (GameObject)Instantiate(planet, center, Quaternion.identity);
            int hasMoon = Random.Range(0, 2);
            if(hasMoon == 0)
            {
                //spawnMoon(planet.transform.GetChild(0).transform, spawnPos);
                var children = new System.Collections.Generic.List<GameObject>();
                foreach(Transform child in myPlanet.transform.FindChild("Planet 0"))
                    children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));
            }
        }

    }

    /*
    void spawnMoon(Transform parent, Vector3 spawnPos)
    {
        GameObject myMoon = (GameObject)Instantiate(moon, spawnPos, Quaternion.identity);
        myMoon.transform.parent = parent;
    }
    */


    void spawnCargoWhite(int numCargo)
    {
        for (int i = 0; i < numCargo; i++)
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
            cargo.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Instantiate(cargo, spawnPos, Quaternion.identity);
            cargo.tag = "Cargo";
        }
    }

    void spawnEnemiesWhite(int numEnemies)
    {
        for (int i = 0; i < numEnemies; i++)
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
        spaceStationChild.position = spawnPos - center;
        Instantiate(spaceStation, center, Quaternion.identity);
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
        player.GetComponent<PlayerController>().curr_Cargo = 0;
        Instantiate(player, spawnPos, Quaternion.identity);
        player.tag = "Player";
        initCamera(spawnPos);
    }

    /*
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
    */

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

    void spawnSun()
    {
        Debug.Log("Spawning Sun");
        Instantiate(sun, center, Quaternion.identity);
    }

    void initCamera(Vector3 spawnPos)
    {
        Instantiate(cameraMaster, spawnPos, Quaternion.identity);
        Instantiate(AudioController, spawnPos, Quaternion.identity);
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
        float moonSize = Random.Range(0.1f, 0.5f);
        Vector3 spawnPoint = spawnPos - center;

        switch (spawnType)
        {
            case (Planets.FROST):
                {
                    Debug.Log("Making FROST");
                    frostChild.position = spawnPoint;
                    frostChild.localScale = new Vector3(size, size, size);
                    frostMoon.position = spawnPoint;
                    frostMoonChild.localScale = new Vector3(moonSize, moonSize, moonSize);
                    return frost;

                }
            case (Planets.PURPLE):
                {
                    Debug.Log("Making Purple");
                    purpleChild.position = spawnPoint;
                    purpleChild.localScale = new Vector3(size, size, size);
                    purpleMoon.position = spawnPoint;
                    purpleMoonChild.localScale = new Vector3(moonSize, moonSize, moonSize);
                    return purple;
                }
            case (Planets.WATER):
                {
                    Debug.Log("WATER");
                    waterChild.position = spawnPoint;
                    waterChild.localScale = new Vector3(size, size, size);
                    waterMoon.position = spawnPoint;
                    waterMoonChild.localScale = new Vector3(moonSize, moonSize, moonSize);
                    return water;
                }
        }
        return frost;
    }
}
