using UnityEngine;
using System.Collections;
/*
 * @Author: Patrick Blanchard
 *
 */

public class SpawnMaster : MonoBehaviour {
    public static int SMALL_ENEMY  = 5;
    public static int MEDIUM_ENEMY = 10;
    public static int LARGE_ENEMY  = 15;

    public static int SMALL_MAP_SIZE  = 100;
    public static int MEDIUM_MAP_SIZE = 200;
    public static int LARGE_MAP_SIZE  = 300;

    public static int SMALL_CARGO  = 5;
    public static int MEDIUM_CARGO = 10;
    public static int LARGE_CARGO  = 15;

    public static int SMALL_PLANET  = 3;
    public static int MEDIUM_PLANET = 5;
    public static int LARGE_PLANET  = 7;


    public enum SpawnState
    {
        SMALL,
        MEDIUM,
        LARGE,
        SPAWN_STATE_COUNT
    }

    public static SpawnState CURRENT_STATE = SpawnState.LARGE;
}
