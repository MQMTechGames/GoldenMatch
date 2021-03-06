using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasePlayerAI : IPlayer
{
	public bool _isAttackingTeam = true;

	public float _runVelocity = 5.5f;
	public float _dampTime = 0.1f;
	public float _rotateVel = 2.0f;
	public float _rotationAccelByDistance = 3.0f;

    public TrainninTeamId _trainningTeamId = new TrainninTeamId();

    Dictionary<IInputPlayerControllerType, IInputPlayerController> _inputPlayerControllers = null;
    IInputPlayerController _inputPlayerController = null;

	PlayerMovement _playerController = null;

	Animator _animator = null;

	Transform _ballTransform = null;

	AnimatorParams _animParams = null;

	DoneHashIDs _hash = null;

    InputControllerManager _inputControllerManager = null;

    // subtrees
    public TestPlayerAI _testPlayerAI = null;
    public TrainningAI _trainningAI = null;

    // Trainning
    TrainningController _trainningController = null;
    public TrainningTeamController _teamController = null;
    TrainninTeamId trainningTeamId = new TrainninTeamId();

    bool _haveToRecoverPossesion = false;

	void Awake()
	{
		// Get components
		_playerController = new PlayerMovement(transform);
		_animator = transform.GetComponent<Animator>();
		_hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();

        DebugUtils.assert(null != transform, "transform must exist");
		DebugUtils.assert(null != _animator, "animator must exist");

		// Init objects
		_animParams = AnimatorParams.sharedInstance();
        _inputControllerManager = ManagerContainer.sharedInstance().getInputControllerManager();
		_ballTransform = GameObject.FindGameObjectWithTag("ball").transform;

        // trainning
        _trainningController = GameObject.FindObjectOfType<TrainningController>();
        DebugUtils.assert(null != _trainningController, "teamContorller can NOT be NULL");
        _teamController = _trainningController.getTeamById(trainningTeamId);

        // get branches
        _testPlayerAI = gameObject.GetComponent<TestPlayerAI>();
        if(null == _testPlayerAI)
        {
            _testPlayerAI = gameObject.AddComponent<TestPlayerAI>();
        }

        _trainningAI = gameObject.GetComponent<TrainningAI>();
        if (null == _trainningAI)
        {
            _trainningAI = gameObject.AddComponent<TrainningAI>();
        }

        base.Awake();
	}

    public void changePlayerController(IInputPlayerControllerType type)
    {
        _inputPlayerController = _inputPlayerControllers[type];
    }

    // Methods to receive inputs from the the team controller
    public void setDoesHaveToRecoverPossesion(bool haveToRecoverPossesion)
    {
        _haveToRecoverPossesion = haveToRecoverPossesion;
    }

    public bool getDoesHaveToRecoverPossesion()
    {
        return _haveToRecoverPossesion;
    }

    public override BT initStandalone()
	{
        BT bt;
		BTBuilder builder = BTBuilder.create ("BasicPlayerAI").getBT(out bt);

		builder.addNode (null, "BasicPlayerAI", BTNodeType.BTNODE_PRIORITY)
			.addNode ("BasicPlayerAI", "AttackingTeamPlayer", BTNodeType.BTNODE_PRIORITY, attackingTeamPlayerCondition)
			/****/.addNode ("AttackingTeamPlayer", "BallCarrier", BTNodeType.BTNODE_PRIORITY, ballCarrierCondition)
			/****//****/.addNode ("BallCarrier", "Shot", BTNodeType.BTNODE_LEAF, shotCondition, shotCallback)
			/****//****/.addNode ("BallCarrier", "Assist", BTNodeType.BTNODE_LEAF, assistCondition, assistCallback)
			/****//****/.addNode ("BallCarrier", "Creator", BTNodeType.BTNODE_PRIORITY, creatorCondition)
			/****//****//****/.addNode ("Creator", "RiskyPass", BTNodeType.BTNODE_LEAF, riskyPassCondition, riskyPassCallback)
			/****//****//****/.addNode ("Creator", "AdvancePosession", BTNodeType.BTNODE_LEAF, advancePosessionCondition, advancePosessionCallback)
			/****//****//****/.addNode ("Creator", "AttackingDribble", BTNodeType.BTNODE_LEAF, null, attackingDriblleCallback)
			/****//****/.addNode ("BallCarrier", "Conservator", BTNodeType.BTNODE_PRIORITY, null)
			/****//****//****/.addNode ("Conservator", "SecurePass", BTNodeType.BTNODE_LEAF, securePassCondition, securePassCallback)
			/****//****//****/.addNode ("Conservator", "MoveBack", BTNodeType.BTNODE_LEAF, moveBackCondition, moveBackCallback)
			/****//****//****/.addNode ("Conservator", "DefendingDribble", BTNodeType.BTNODE_LEAF, defendingDribbleCondition, defendingDribbleCallback)
			/****//****//****/.addNode ("Conservator", "Clear", BTNodeType.BTNODE_LEAF, null, clearCallback)
			/****/.addNode ("AttackingTeamPlayer", "Attacker", BTNodeType.BTNODE_PRIORITY)
			/****//****/.addNode ("Attacker", "CreateAssistSpace", BTNodeType.BTNODE_LEAF, createAssistSpaceCondition, createAssistSpaceCallback)
			/****//****/.addNode ("Attacker", "CreateSpace", BTNodeType.BTNODE_LEAF, createSpaceCondition, createSpaceCallback)
			/****//****/.addNode ("Attacker", "AttackerPassive", BTNodeType.BTNODE_LEAF, null, attackerPassiveCallback)
			
				.addNode ("BasicPlayerAI", "Neutral", BTNodeType.BTNODE_PRIORITY, ()=>{return true;})
			/****/.addNode ("Neutral", "RecoverTheBall", BTNodeType.BTNODE_SEQUENCE, recoverTheBallCondition)
			/****//****/.addNode ("RecoverTheBall", "RotateToBall", BTNodeType.BTNODE_LEAF, null, rotateToBallCallback)
			/****//****/.addNode ("RecoverTheBall", "GoForwardToBall", BTNodeType.BTNODE_LEAF, ()=>{return true;}, goForwardToBallCallback)
			
			.addNode ("BasicPlayerAI", "UserInput", BTNodeType.BTNODE_LEAF, null, controllMovementCallback)
			
			.addNode("BasicPlayerAI", _testPlayerAI.buildNode)

            .addNode("BasicPlayerAI", "ForeverTrainning", BTNodeType.BTNODE_WHILE, () => { return true; })
            /****/.addNode("ForeverTrainning", _trainningAI.buildNode)
			;

        return bt;
	}

    void Start()
    {
        if (getIsStandalone())
        {
            _teamController.addPlayer(this);
        }
    }

	Vector3 getBallPosition()
	{
		if(null != _ballTransform) {
			return _ballTransform.position;
		}

		return new Vector3(0.0f, 0.5f, 0.0f);
	}

	//Go To Idle Position
	private BTNodeResponse goToIdlePosition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	private BTNodeResponse rotateToBallCallback()
	{
		return rotateToBall(_rotateVel);
	}

	private BTNodeResponse rotateToBall(float rotateVel)
	{
		BTNodeResponse response = BTNodeResponse.STAY;

        Transform target = GameObjectUtils.getChildByTag(transform, "spine");
		DebugUtils.assert(null != target, "[BasePlayerAI->rotateBallCallback]: Tranform can NOT be NULL");
		
		float angle = _playerController.rotateToPoint(getBallPosition(), rotateVel * Time.deltaTime);
		
		if(Mathf.Abs(angle) < MathUtils.kDegreeInRadians) {
			response = BTNodeResponse.LEAVE;
		}
		
		return response;
	}

	private BTNodeResponse goForwardToBallCallback()
	{
        float distance = MathUtils.getDistanceToPoint(transform, getBallPosition());


		float minDistance = 0.25f;
		float maxDistance = 5.5f;

		float speed = (distance - minDistance) / (maxDistance - minDistance);
		speed = MathUtils.clamp(speed);

		// Move forward
		_animator.SetFloat(_animParams.speedFloat, speed, _dampTime, Time.deltaTime);

		// Keep adjusting rotation
		float rotateVel = _rotateVel + ( (1-speed) * _rotateVel * _rotationAccelByDistance);
		rotateToBall(rotateVel);

		if(Mathf.Abs(speed) < 0.1f) {
			_animator.SetFloat(_animParams.speedFloat, 0.0f);
			
			return BTNodeResponse.LEAVE;
		} 

		return BTNodeResponse.STAY;
	}

	// RecoverTheBall
	private bool recoverTheBallCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}

	private BTNodeResponse revocerTheBallCallback()
	{

        float angle = MathUtils.getAngleToPoint(transform, getBallPosition());

        Vector3 delta = getBallPosition() - transform.position;
		float distance = delta.magnitude;

		if(Mathf.Abs(distance) < 0.5f || Mathf.Abs(angle) > 0.1f) {

			float angFactor = 1 - (Mathf.Abs(angle) / (float)Mathf.PI);

			_animator.SetFloat(_animParams.speedFloat, 5.5f * angFactor + 1.0f);
			_animator.SetFloat(_animParams.directionFloat, 0.0f, _dampTime, Time.deltaTime);

			DebugUtils.log("angle is: " + angle);
			if(Mathf.Abs(angle) > 0.1f) {
				float finalRotationAngle = angle * _rotateVel * Time.deltaTime;

				Quaternion quat = Quaternion.AngleAxis(finalRotationAngle, new Vector3(0,1,0));
				rigidbody.MoveRotation(quat);
			}

			return BTNodeResponse.STAY;
		}

		DebugUtils.log("[BasePlayerAI]: Leaving the node");
		_animator.SetFloat(_animParams.speedFloat, 0.0f);

		return BTNodeResponse.LEAVE;
	}

	// AttackingTeamPlayer
	private bool neutralCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}

	// AttackingTeamPlayer
	private bool attackingTeamPlayerCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}


	// BallCarrier
	private bool ballCarrierCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}


	// User Input controller
	private BTNodeResponse controllMovementCallback()
	{
        IInputPlayerController inputPlayerController = _inputControllerManager.getActiveInputPlayerController();
        DebugUtils.assert(null != inputPlayerController, "[BasePlayerAI->controllerMovementCallback]: Player Controller must NOT be NULL");

        if (null != inputPlayerController)
        {
            inputPlayerController.setTarget(transform, _animator);
            inputPlayerController.move();
		}

		return BTNodeResponse.STAY;
	}


	// Shot
	private bool shotCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}

	private BTNodeResponse shotCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	// Assist
	private bool assistCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}

	private BTNodeResponse assistCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}

	// Creator
	private bool creatorCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}


	// RiskyPass
	private bool riskyPassCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse riskyPassCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//AdvancePosession
	private bool advancePosessionCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse advancePosessionCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//AttackingDriblle
//	private bool attackingDriblleCondition()
//	{
//		float r = Random.Range (0.0f,1.0f);
//		if (r > 0.5f) {
//			return true;
//		}
//		
//		return false;
//	}
	
	private BTNodeResponse attackingDriblleCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}
	
	//SecurePass
	private bool securePassCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse securePassCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//MoveBack
	private bool moveBackCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse moveBackCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//DefendingDribble
	private bool defendingDribbleCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse defendingDribbleCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//Clear
//	private bool clearCondition()
//	{
//		float r = Random.Range (0.0f,1.0f);
//		if (r > 0.5f) {
//			return true;
//		}
//		
//		return false;
//	}
	
	private BTNodeResponse clearCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	//CreateAssistSpace
	private bool createAssistSpaceCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse createAssistSpaceCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}

	//CreateSpace
	private bool createSpaceCondition()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return true;
		}
		
		return false;
	}
	
	private BTNodeResponse createSpaceCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}


	// AttackerPassive
//	private bool attackerPassiveCondition()
//	{
//		float r = Random.Range (0.0f,1.0f);
//		if (r > 0.5f) {
//			return true;
//		}
//		
//		return false;
//	}
	
	private BTNodeResponse attackerPassiveCallback()
	{
		float r = Random.Range (0.0f,1.0f);
		if (r > 0.5f) {
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}
}
