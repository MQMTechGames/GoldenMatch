using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class will be responsible to interact with a player from external entities
// also this class encapsulates what a team does
public class MatchController : MonoBehaviour
{
	Dictionary<TeamType, TeamController> _teams;
	TeamType							 _posessionTeamType;
	
	public MatchController()
	{
		_teams = new Dictionary<TeamType, TeamController>();
		_posessionTeamType = TeamType.NO_TEAM;
	}

	public bool teamHasThePossesion(TeamType teamName)
	{
		return _posessionTeamType == teamName;
	}

	public TeamController getTeamController(TeamType teamType)
	{
		bool existTeam = getExistTeam(teamType);

		DebugUtils.assert(true == existTeam, "[MatchController] team must exist");

		return _teams[teamType];
	}

	public TeamController createTeamController(TeamType teamType)
	{
		TeamController teamController = new TeamController();
		teamController.setTeamType(teamType);

		DebugUtils.assert(false == getExistTeam(teamType), "[MatchController] team must exist");
		_teams.Add(teamType, teamController);

		return teamController;
	}

	bool getExistTeam(TeamType teamType)
	{
		return _teams.ContainsKey(teamType);
	}

	void trySetPosessionToTeam(TeamType teamType)
	{
		_posessionTeamType = teamType;
	}

	void Awake()
	{
	}
	
	void Start()
	{
	}
	
	void Update()
	{
		
	}
}
