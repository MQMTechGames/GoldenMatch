using UnityEngine;
using System.Collections;
using AStar;

/**
 * This class is responsible to init the life cycle of a level
 **/
public class Level1Main : ILevelMain
{	
	bool isSceleLoaded = false;
	TeamBuilder teamBuilder = null;
	StateMachine stateMachine = null;
	MatchController _matchController = null;
	
	public Level1Main()
	{
		stateMachine = new StateMachine ();
		loadStates ();
	}

	void loadStates()
	{
		stateMachine.addStateMethod ("mainScreenState", mainScreenState);
		stateMachine.addStateMethod ("mainSceneState", mainSceneState);
	}
	
	public void startLevel()
	{
		// set the active camera
		CameraManager manager = ManagerContainer.sharedInstance ().getCameraManager ();
		//manager.changeToCamera (CameraNames.Level1ScreenCamera);
		manager.changeToCamera (CameraNames.Level1SceneCamera);
	}

	public void Update()
	{
		stateMachine.Update ();
	}

	void loadScene()
	{
		CameraManager camMng = ManagerContainer.sharedInstance().getCameraManager();
		
		bool camChanged = camMng.changeToCamera(CameraNames.Level1SceneCamera);
		DebugUtils.assert(camChanged, "[PlayerController] hash must not be null");
		
		buildMatch ();
	}

	void buildMatch()
	{
		// create match controller
		_matchController = new MatchController();

		// Create team controllers
		TeamController playerTeamController = _matchController.createTeamController(TeamType.PLAYER_TEAM);
		TeamController rivalTeamController =_matchController.createTeamController(TeamType.RIVAL_TEAM);

		// create players
		TeamBuilder.create (out teamBuilder, playerTeamController, rivalTeamController);
		teamBuilder.buildAllPlayersInScene ();
	}

//	States
    static bool init = false;
	void mainScreenState()
	{
        if (false == init)
        {
            // Test Tile
            TiledAStar tiledAStar;
            TiledAStar.create(out tiledAStar, 10, 10);

            // add the tiles
            tiledAStar.addTile(new Tile(new ID(0,0), new Vector3(10,0,10), true, null));

            AStar.Tile tile = tiledAStar.getTileByID(new AStar.ID(0, 0));
            DebugUtils.log("tile 0,0 is: " + tile._id._x + ", " + tile._id._y);

            init = true;
        }

        //DebugUtils.Log ("mainScreenState");
		if (Input.anyKey && !isSceleLoaded && false)
		{
			stateMachine.changeState("mainSceneState");

			loadScene();
			isSceleLoaded = true;
		}
	}

	void mainSceneState()
	{
        //Debug.Log ("mainSceneState");
	}
}
