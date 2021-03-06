﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
/*
 * @Author: Patrick Blanchard
 *
 */

public class WorldSelectController : MonoBehaviour {

	public void Small()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.SMALL;
        SceneManager.LoadScene(2);
        
    }

    public void Medium()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.MEDIUM;
        SceneManager.LoadScene(2);
    }

    public void Large()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.LARGE;
        SceneManager.LoadScene(2);
    }
}
