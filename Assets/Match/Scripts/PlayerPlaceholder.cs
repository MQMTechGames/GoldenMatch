using UnityEngine;
using System.Collections;

public class PlayerPlaceholder : MonoBehaviour
{
	public PlayerType 	playerPosition;
	public TeamType		teamType;

	[SerializeField]
	public PlayerInfo	playerInfo = new PlayerInfo();

	public bool			_isActived = true;

	void Awake ()
	{
		// This class is just for setup the player position so it's no longer needed once the match has started
		if(renderer)
		{
			renderer.enabled = false;
		}
	}
}
