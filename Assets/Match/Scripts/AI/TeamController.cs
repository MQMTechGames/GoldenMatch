using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class will be responsible to interact with a player from external entities
// also this class encapsulates what a team does
public class TeamController : MonoBehaviour
{
	Dictionary<int, BasePlayerAI>	_playersById = null;

	TeamType						_teamType;

	public TeamController()
	{
		_playersById = new Dictionary<int, BasePlayerAI>();
	}

	public void setTeamType(TeamType type)
	{
		_teamType = type;
	}

	public void registerPlayerAI(int id, BasePlayerAI player)
	{
		bool alreadyExistPlayer = getExistPlayer(id);
		DebugUtils.assert(false == alreadyExistPlayer, "[TeamController] player must NOT exist");

		_playersById.Add(id, player);
	}

	bool getExistPlayer(int id)
	{
		return _playersById.ContainsKey(id);
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
