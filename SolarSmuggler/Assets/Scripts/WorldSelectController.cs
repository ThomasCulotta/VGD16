using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WorldSelectController : MonoBehaviour {

	public void Small()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.SMALL;
        SceneManager.LoadScene(3);
        
    }

    public void Medium()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.MEDIUM;
        SceneManager.LoadScene(3);
    }

    public void Large()
    {
        SpawnMaster.CURRENT_STATE = SpawnMaster.SpawnState.LARGE;
        SceneManager.LoadScene(3);
    }
}
