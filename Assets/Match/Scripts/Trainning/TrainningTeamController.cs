using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TeamData;

public class TrainningTeamController : MonoBehaviour 
{
    public TrainninTeamId _trainningTeamId = new TrainninTeamId();

    List<IPlayer> _players = new List<IPlayer>();
    TrainningController _trainningController = null;
    bool isInit = false;

    bool isBallNeutral = true;

    Blackboard _blackboard = new Blackboard();

    public void addPlayer(IPlayer ai)
    {
        if(existsPlayer(ai)) {
            return;
        }

        _players.Add(ai);
    }

    bool existsPlayer(IPlayer ai)
    {
        return _players.Contains(ai);
    }

    void Awake()
    {
        _trainningController = GameObject.FindObjectOfType<TrainningController>();

        _trainningController.addTeam(_trainningTeamId, this);
    }

	// Update is called once per frame
	void Update () {
        if (!isInit)
        {
            isInit = true;
            initTrainning();
        }
	}

    public void initTrainning()
    {
        foreach (IPlayer player in _players)
        {
            player.pushState("ForeverTrainning");
        }

        IPlayer rand = getRandomTeammate();
        _blackboard.addAction(ActionId.DO_RECOVER_POSSESION, rand.gameObject.GetInstanceID());
    }

    // Team methods
    public Blackboard getBlackboard()
    {
        return _blackboard;
    }

    // helper methods
    public IPlayer getRandomTeammate()
    {
        int playerSpot = Random.Range(0, _players.Count);

        IPlayer teammate = _players[playerSpot];

        return teammate;
    }

    public bool doesCanRecoverTheBall()
    {
        return isBallNeutral;
    }

    public void setBallRecovered(bool isRecovered)
    {
        isBallNeutral = !isRecovered;
    }

    public bool getHaveBallPossesion()
    {
        return !isBallNeutral;
    }
}
