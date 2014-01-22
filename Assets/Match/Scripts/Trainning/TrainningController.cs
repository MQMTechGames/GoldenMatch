using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainningController : MonoBehaviour 
{
    Dictionary<TrainninTeamId, TrainningTeamController> _teamControllers = new Dictionary<TrainninTeamId,TrainningTeamController>();

    bool isInit = false;

    public TrainningTeamController getTeamById(TrainninTeamId id)
    {
        TrainningTeamController team = null;
        _teamControllers.TryGetValue(id, out team);

        return team;
    }

    public void addTeam(TrainninTeamId id, TrainningTeamController team)
    {
        _teamControllers.Add(id, team);
    }

	// Update is called once per frame
	void Update ()
    {
        if (false == isInit)
        {
            initTrainning();
            isInit = true;
        }
	}

    void initTrainning()
    {
        foreach (KeyValuePair<TrainninTeamId,TrainningTeamController> pair in _teamControllers)
        {
            pair.Value.initTrainning();
        }
    }
}
