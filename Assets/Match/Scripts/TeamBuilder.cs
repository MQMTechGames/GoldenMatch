using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamBuilder
{
	private GameObject playerAPrefab = null;
	private GameObject playerBPrefab = null;

	private TeamController _playerTeamController = null;
	private TeamController _rivalTeamController = null;

	private TeamBuilder(TeamController playerTeamController, TeamController rivalTeamController)
	{
		_playerTeamController = playerTeamController;
		_rivalTeamController = rivalTeamController;
	}

	public static void create(out TeamBuilder teamBuilder, TeamController playerTeamController, TeamController rivalTeamController)
	{
		teamBuilder = new TeamBuilder ( playerTeamController, rivalTeamController);
		teamBuilder.init ();
	}

	public void init()
	{
		Debug.Log("TeamBuilder init");
		
		playerAPrefab = (GameObject)Resources.Load(TeamPrefabsPaths.playerA + "TeamAPlayer");
		playerBPrefab = (GameObject)Resources.Load(TeamPrefabsPaths.playerB + "TeamBPlayer");

		DebugUtils.assert (playerAPrefab != null, "[playerAPrefab] playerBPrefab hash must not be null");
		DebugUtils.assert (playerBPrefab != null, "[playerBPrefab] playerBPrefab hash must not be null");
	}

	public void buildAllPlayersInScene()
	{
		Debug.Log("buildAllPlayersInScene");

		GameObject[] gos = GameObject.FindGameObjectsWithTag("playerPlaceholder");
		buildPlayersFromPlaceholders(gos);
	}

	public void buildPlayersFromPlaceholders(GameObject[] placeHolders)
	{
		DebugUtils.assert (null != playerAPrefab, "[TeamBuilder] playerBPrefab hash must not be null");
		DebugUtils.assert (null != playerBPrefab, "[TeamBuilder] playerBPrefab hash must not be null");

		foreach(GameObject go in placeHolders)
		{
			PlayerPlaceholder currPlayerPlaceholder = go.GetComponent<PlayerPlaceholder>();
			if(   false == currPlayerPlaceholder 
			   || false == currPlayerPlaceholder._isActived
			   ) {
				continue;
			}

			// TODO: delete next log
			Debug.Log("Creating player from placeholder: " + currPlayerPlaceholder.gameObject.name);

			Vector3 placeHolderPosition = currPlayerPlaceholder.transform.position;
			Quaternion placeHolderOr = currPlayerPlaceholder.transform.rotation;

			if(currPlayerPlaceholder.teamType == TeamType.PLAYER_TEAM)
			{
				GameObject player = GameObject.Instantiate(playerAPrefab, placeHolderPosition, placeHolderOr) as GameObject;

				BasePlayerAI playerController = player.GetComponent<BasePlayerAI>();
				DebugUtils.assert(null != playerController, "[TeamBuilder] playerController hash must not be null");
				_playerTeamController.registerPlayerAI(player.GetInstanceID(), playerController);
			} else
			{
				GameObject player = GameObject.Instantiate(playerBPrefab, placeHolderPosition, placeHolderOr) as GameObject;
				BasePlayerAI playerController = player.GetComponent<BasePlayerAI>();
				DebugUtils.assert(null != playerController, "[TeamBuilder] playerController hash must not be null");
				_rivalTeamController.registerPlayerAI(player.GetInstanceID(), playerController);
			}
		}
	}
}
