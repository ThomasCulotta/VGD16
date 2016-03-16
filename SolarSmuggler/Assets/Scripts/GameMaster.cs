using UnityEngine;
using System.Collections;

public static class GameMaster {

	public enum GameState
	{
		GAME_START, // Start state of the level, used for initialization if needed
		PLAYER_TURN, // Player's turn. On end will change to Enemy turn.
		ENEMY_TURN, // Enemy's turn. On end will change to Environment turn.
		ENVIRONMENT_TURN, // Environment's turn. On end will change to Player turn.
		GAME_LOSS, // Only entered when Player's health <= 0.
		GAME_WIN, // Only entered when Player enters destination trigger volume.

		GAME_STATE_COUNT
	}

	public static GameState CurrentState = GameState.GAME_START;
}
