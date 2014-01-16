using UnityEngine;
using System.Collections;

public class TestPlayerAI
{
	static float kThresholdToApproachBall = 0.20f;

	public bool _isAttackingTeam = true;
	
	public float _runVelocity = 5.5f;
	public float _dampTime = 0.1f;
	public float _rotateVel = 2.0f;
	public float _rotationAccelByDistance = 3.0f;
	
	// private params
	BT _bt = null;
	
	PlayerMovement _playerController = null;
	
	Transform _target = null;
	Animator _animator = null;

	Transform _ballTransform = null;
	
	AnimatorParams _animParams = null;
	
	DoneHashIDs _hash = null;
	
	TestPlayerAI _testPlayerAI = null;

	public static void create(out TestPlayerAI testPlayerAI, Transform transform)
	{
		testPlayerAI = new TestPlayerAI(transform);
	}

	public TestPlayerAI(Transform transform)
	{
		// Get components
		_playerController = new PlayerMovement(transform);
		_target = transform;
		_animator = _target.GetComponent<Animator>();
		_hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
		
		DebugUtils.assert(null != _target, "transform must exist");
		DebugUtils.assert(null != _animator, "animator must exist");
		
		// Init objects
		_animParams = AnimatorParams.sharedInstance();
		
		_ballTransform = GameObject.FindGameObjectWithTag("ball").transform;
	}
	
	public void buildNode(string parentName, BTBuilder builder)
	{
		builder.
			addNode (parentName, "TestPlayerAI", BTNodeType.BTNODE_SEQUENCE)

				// Recover the ball
				/***/.addNode ("TestPlayerAI", "Recover_Start", BTNodeType.BTNODE_SEQUENCE)
				/***//***/.addNode ("TestPlayerAI", "Recover_Start_RotateStep", BTNodeType.BTNODE_LEAF, getRecoverStartNeedRotation, recoverRotateStep)
				/***//***/.addNode ("TestPlayerAI", "Recover_GoToBall", BTNodeType.BTNODE_LEAF, null, getRecoverGoToBall)

				// BallHandle the ball
				;
	}

	// Recover the ball
	bool getRecoverStartNeedRotation()
	{
		return true;
	}

	BTNodeResponse recoverRotateStep()
	{
		float rotatedAngle = TransformUtils.rotateToPointStep(_target, _ballTransform.position, _rotateVel * Time.deltaTime);

		bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;

		return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
	}

	BTNodeResponse getRecoverGoToBall()
	{
		float distance =_playerController.runToPointStep(_ballTransform.position, 5.5f);

		if(Mathf.Abs(distance) > kThresholdToApproachBall) {
			return BTNodeResponse.LEAVE;
		}
		
		return BTNodeResponse.STAY;
	}
}
