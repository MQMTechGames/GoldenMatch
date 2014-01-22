using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TeamData;

public class TrainningTeamController : MonoBehaviour 
{
    public TrainninTeamId _trainningTeamId = new TrainninTeamId();

    List<BasePlayerAI> _players = new List<BasePlayerAI>();
    TrainningController _trainningController = null;
    bool isInit = false;

    bool isBallNeutral = true;

    Blackboard _blackboard = new Blackboard();

    public void addPlayer(BasePlayerAI ai)
    {
        if(existsPlayer(ai)) {
            return;
        }

        _players.Add(ai);
    }

    bool existsPlayer(BasePlayerAI ai)
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
        foreach (BasePlayerAI player in _players)
        {
            player.pushState("ForeverTrainning");
        }

        BasePlayerAI rand = getRandomTeammate();
        _blackboard.addAction(ActionId.DO_RECOVER_POSSESION, rand.gameObject.GetInstanceID());
    }

    // Team methods
    public Blackboard getBlackboard()
    {
        return _blackboard;
    }

    // helper methods
    public BasePlayerAI getRandomTeammate()
    {
        int playerSpot = Random.Range(0, _players.Count -1);

        BasePlayerAI teammate = _players[playerSpot];

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
}
