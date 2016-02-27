using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public struct GridSpace
	{
		// Simple x,y coordinates for movement (on x,z plane)
		public Vector2  coordinates;
		// Determines if this space will hide the player
		public bool hidden;
	};

	private float max_Health   = 100f;
	public  float curr_Health  = 100f;
    private float max_Cargo    = 100f; //If we ever decide for cargo size
	private float curr_Cargo   = 100f;
	private const int MAX_MOVE = 10;

	// Used to show visited spaces during player BFS and also to determine planet/enemy collision spaces during enemy/environment turns
	public static bool[,] BlockedGrid = new bool[MAX_MOVE, MAX_MOVE];
	// Used to show spaces that will hide the player (determined during environment turn)
	private bool[,] HiddenGrid = new bool[MAX_MOVE, MAX_MOVE];

	// List of spaces the player can move to and whether or not that space will hide the player
	private ArrayList PlayerGrid;

	private Queue BFSQueue = new Queue();

	private bool EnvironmentStart = true;
	private bool PlayerStart      = true;

	public Slider cargoBar;
	public GameObject healthBar;
	public Text cargoText;

	void Start()
	{
		// Force player to grid
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));

        // Initialize player stats...
        curr_Health = max_Health; //set player to maximum health
        curr_Cargo = max_Cargo; //set cargo to maximum capacity
        //InvokeRepeating("decreaseHealth", 1f, 1f); just for testing purposes, this decreases health by 2 every second
        //SetCargoBar(curr_Cargo);
	}

	void Update()
	{
		Debug.Log(GameMaster.CurrentState);
		switch (GameMaster.CurrentState)
		{
			case (GameMaster.GameState.GAME_START):
			{
				// TODO: Some level initialization
				
				GameMaster.CurrentState = GameMaster.GameState.PLAYER_TURN;
			}break;

			case (GameMaster.GameState.PLAYER_TURN):
			{
				if (PlayerStart)
				{
					// TODO: Start turn with some kind of indicator
					// TODO: BFS that determines available player movement
					PlayerGrid = new ArrayList();
					GridBFS(transform.position);
					
					PlayerStart      = false;
					EnvironmentStart = true;
				}
				// WASD grid movement
				if (Input.GetKeyDown(KeyCode.W)) MovePlayer("z", transform.position.z,  1f);
				if (Input.GetKeyDown(KeyCode.A)) MovePlayer("x", transform.position.x, -1f);
				if (Input.GetKeyDown(KeyCode.S)) MovePlayer("z", transform.position.z, -1f);
				if (Input.GetKeyDown(KeyCode.D)) MovePlayer("x", transform.position.x,  1f);

				// NOTE: We may want to have an automated end turn after player uses up their AP (or whatever).
				if (Input.GetKeyDown(KeyCode.Escape)) GameMaster.CurrentState = GameMaster.GameState.ENEMY_TURN;

		        //if (Input.GetKeyDown(KeyCode.M)) DecreaseCargo();For testing
		        //if (Input.GetKeyDown(KeyCode.I)) IncreaseCargo();For testing
		        //SetCargoText();//Setting cargo amount text
			}break;

			case (GameMaster.GameState.ENVIRONMENT_TURN):
			{
				if (EnvironmentStart)
				{
					// TODO: Start turn with some kind of indicator
					EnvironmentStart = false;
					PlayerStart      = true;
				}

				// TODO: Timer to end ENVIRONMENT_TURN
			}break;
			
			case (GameMaster.GameState.GAME_LOSS):
			{
				// TODO: Some game statistics, then main menu or lose scene etc.
			}break;
			
			case (GameMaster.GameState.GAME_WIN):
			{
				// TODO: Some game statistics, then main menu or win scene etc.
			}break;
		}
    }

	/*
	 * NOTE: BFS starts at player position and searches radially 
	 */
	void GridBFS(Vector2 pos)
	{
		// Mark current position as visited but don't add to PlayerGrid
		Vector2 posOffset = OffsetPos(pos);
		BlockedGrid[(int)posOffset.x, (int)posOffset.y] = true;
		BFSQueue.Enqueue(pos);

		while (BFSQueue.Count != 0)
		{
			Vector2 cur = (Vector2)BFSQueue.Dequeue();
			Debug.Log("Checking space: " + cur);
			// Check the eight spaces around cur
			CheckAndAddAvailBFS(cur.x + 1, cur.y - 1);
			CheckAndAddAvailBFS(cur.x + 1, cur.y);
			CheckAndAddAvailBFS(cur.x + 1, cur.y + 1);
			CheckAndAddAvailBFS(cur.x,     cur.y - 1);
			CheckAndAddAvailBFS(cur.x,     cur.y + 1);
			CheckAndAddAvailBFS(cur.x - 1, cur.y - 1);
			CheckAndAddAvailBFS(cur.x - 1, cur.y);
			CheckAndAddAvailBFS(cur.x - 1, cur.y + 1);
		}
	}

	void CheckAndAddAvailBFS(float x, float y)
	{
		Vector2 pos = new Vector2(x, y);
		// Adds to queue if visited/obstructed and if it's in MAX_MOVE range of player
		Vector2 posOffset = OffsetPos(pos);
		if (posOffset.x >= 0 && posOffset.x < 10 && 
		    posOffset.y >= 0 && posOffset.y < 10 && 
		    !BlockedGrid[(int)posOffset.x, (int)posOffset.y])
			{
				BlockedGrid[(int)posOffset.x, (int)posOffset.y] = true;
				BFSQueue.Enqueue(pos);
				GridSpace newSpace;
				newSpace.coordinates = pos;
				newSpace.hidden = HiddenGrid[(int)posOffset.x, (int)posOffset.y];
				PlayerGrid.Add(newSpace);
			}
	}

	// Translates from world space to 2D array space of size [MAX_MOVE, MAX_MOVE] and centered around the player
	Vector2 OffsetPos(Vector2 pos)
	{
		return new Vector2(pos.x - transform.position.x + MAX_MOVE/2, pos.y - transform.position.z + MAX_MOVE/2);
	}

	void MovePlayer(string axis, float position, float ammount)
	{
		iTween.MoveTo(gameObject, iTween.Hash(axis, position + ammount, "time", 0.5f));
		transform.position = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
	}

    public void decreaseHealth()
    {
        curr_Health -= 2f; // whatever happens to player we decrease health

        //need a ratio to from current health & max health to scale the hp bar
        float calc_Health = curr_Health / max_Health;
        setHealthBar(calc_Health);
    }

    public void setHealthBar(float healthVal)
    {
        //health value needs to be from 0 to 1
        healthBar.transform.localScale = new Vector3(healthVal, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    public void SetCargoBar(float cargo)
    {
        cargoBar.value = cargo;
    }

    public void DecreaseCargo()
    {
        if (curr_Cargo != 0)
        {
            curr_Cargo--;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    public void IncreaseCargo()
    {
        if (curr_Cargo == max_Cargo)
        {
            curr_Cargo++;
            float calc_Cargo = curr_Cargo / max_Cargo;
            SetCargoBar(calc_Cargo);
        }
    }

    void SetCargoText()
    {
        cargoText.text = "Cargo: " + curr_Cargo.ToString();
    }
}
