using UnityEngine;
using System.Collections;

// to clear console
using UnityEditor;
using System;
using System.Reflection;

public class TestPlayerAI
{
	static float kThresholdToApproachBall = 0.30f;
	static float kThresholdBallHandlingApproachBall = 0.40f;
	static float kThresholdBallHandlingWaypoint = kThresholdBallHandlingApproachBall * 3.0f;

	public bool _isAttackingTeam = true;
	
	public float _runVelocity = 5.5f;
	public float _dampTime = 0.1f;
	public float _rotateVel = 2.0f;
	public float _rotationAccelByDistance = 3.0f;
	
	// private params
	BT _bt = null;
	
	PlayerMovement _playerController = null;
	
	Transform _transform = null;
	Animator _animator = null;
	Transform _ballTransform = null;
	Transform _shootTransform = null;
	AnimatorParams _animParams = null;
	DoneHashIDs _hash = null;
	TestPlayerAI _testPlayerAI = null;

	Vector3 _waypointDir;

	public static void create(out TestPlayerAI testPlayerAI, Transform transform)
	{
		testPlayerAI = new TestPlayerAI(transform);
	}

	public TestPlayerAI(Transform transform)
	{
		// Initialize objects
		_waypointDir = new Vector3();

		// Get components
		_playerController = new PlayerMovement(transform);
		_transform = transform;
		_animator = _transform.GetComponent<Animator>();
		_hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
		_shootTransform = GameObjectUtils.getComponentByEntityName<Transform>("shotPosition");

		DebugUtils.assert(null != _shootTransform, "_shotPosition must exist");
		DebugUtils.assert(null != _transform, "transform must exist");
		DebugUtils.assert(null != _animator, "animator must exist");
		
		// Init objects
		_animParams = AnimatorParams.sharedInstance();
		
		_ballTransform = GameObject.FindGameObjectWithTag("ball").transform;
	}
	
	public void buildNode(string parentName, BTBuilder builder)
	{
		builder.
			addNode (parentName, "TestPlayerAI", BTNodeType.BTNODE_SEQUENCE)

				/**/.addNode ("TestPlayerAI", "clearConsole", BTNodeType.BTNODE_LEAF, null, clearConsole)

				// Recover the ball
				/**/.addNode ("TestPlayerAI", "RecoverBall", BTNodeType.BTNODE_SEQUENCE, null)
				/**//**/.addNode ("RecoverBall", "recoverBallInitialRotation", BTNodeType.BTNODE_LEAF, null, recoverBallInitialRotation)
				/**//**/.addNode ("RecoverBall", "recoverBallLoop", BTNodeType.BTNODE_WHILE, recoverBallLoopCondition)
				/**//**//**/.addNode ("recoverBallLoop", "recoverBallLoopStep", BTNodeType.BTNODE_SEQUENCE)
				/**//**//**//**/.addNode ("recoverBallLoopStep", "recoveringRotateToBallStep", BTNodeType.BTNODE_LEAF, null, recoveringRotateToBallStep)
				/**//**//**//**/.addNode ("recoverBallLoopStep", "recoveringGoToBallStep", BTNodeType.BTNODE_LEAF, null, recoveringGoToBallStep)

				// BallHandle the ball
				/**/.addNode ("TestPlayerAI", "ballHandlingToWaypoint", BTNodeType.BTNODE_PRIORITY, null)
				/**//**/.addNode ("ballHandlingToWaypoint", "ballHandlingToWaypointLoop", BTNodeType.BTNODE_WHILE, ballHandlingToWaypointLoopCondition)
				/**//**//**/.addNode ("ballHandlingToWaypointLoop", "BallHandlingToWaypointStep", BTNodeType.BTNODE_SEQUENCE)
				/**//**//**//**/.addNode ("BallHandlingToWaypointStep", "ballHandlingRotateToBall", BTNodeType.BTNODE_LEAF, null, ballHandlingRotateToBall)
				/**//**//**//**/.addNode ("BallHandlingToWaypointStep", "ballHandlingGoToBallStep", BTNodeType.BTNODE_LEAF, null, ballHandlingGoToBallStep)
				/**//**//**//**/.addNode ("BallHandlingToWaypointStep", "ballHandlingCalculateWaypointDir", BTNodeType.BTNODE_LEAF, null, ballHandlingCalculateWaypointDir)
				/**//**//**//**/.addNode ("BallHandlingToWaypointStep", "ballHandlingPushBallToWaypoint", BTNodeType.BTNODE_LEAF, null, ballHandlingPushBallToWaypoint)

				// Shot the ball
				/**/.addNode ("TestPlayerAI", "shotBallToGoal", BTNodeType.BTNODE_LEAF, null, shotBallToGoal)
				;
	}

	// Clear console
	BTNodeResponse clearConsole()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
		
		Type type = assembly.GetType("UnityEditorInternal.LogEntries");
		MethodInfo method = type.GetMethod("Clear");
		method.Invoke(new object(), null);

		return BTNodeResponse.LEAVE;
	}

	// Recover the ball
	bool recoverBallLoopCondition()
	{
		Vector3 dirToBall = _ballTransform.position - _transform.position;
		float distanceToBall = dirToBall.magnitude;
		
		return distanceToBall > 1f;
	}

	BTNodeResponse recoverBallInitialRotation()
	{
		float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, _rotateVel * Time.deltaTime);
		
		bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;
		
		return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
	}

	BTNodeResponse recoveringRotateToBallStep()
	{
		float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, _rotateVel * 10 * Time.deltaTime);

		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse recoveringGoToBallStep()
	{
		float distance =_playerController.runToPointStepSmooth(_ballTransform.position, 5.5f);

		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse ballHandlingCalculateWaypointDir()
	{
		_waypointDir = _shootTransform.position - _transform.position;
		_waypointDir.Normalize();
		
		return BTNodeResponse.LEAVE;
	}

	// BallHandling

	bool ballHandlingToWaypointLoopCondition()
	{
		Vector3 dirToWaypoint = _shootTransform.position - _transform.position;
		float distanceToWaypoint = dirToWaypoint.magnitude;

		return distanceToWaypoint > kThresholdBallHandlingWaypoint;
	}

	BTNodeResponse ballHandlingCalculateDestinationDir()
	{
		float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, 9999f);
		
		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse ballHandlingRotateToBall()
	{
		float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, 9999f);
		
		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse ballHandlingGoToBallStep()
	{
		float distance =_playerController.runToPointStepStraight(_ballTransform.position, 0.5f);
		
		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse ballHandlingPushBallToWaypoint()
	{
		float distanceToBal = MathUtils.getDistanceToPoint(_transform, _ballTransform.position);

		if(distanceToBal > kThresholdBallHandlingApproachBall) {
			return BTNodeResponse.LEAVE;
		}

		float forceMag = 6f;
		Vector3 force = _waypointDir * forceMag;

		_ballTransform.rigidbody.AddForce(force, ForceMode.Impulse);

		return BTNodeResponse.LEAVE;
	}

	BTNodeResponse shotBallToGoal()
	{
		float forceMag = 15f;
		Vector3 force = _waypointDir * forceMag;
		
		_ballTransform.rigidbody.AddForce(force, ForceMode.Impulse);
		
		return BTNodeResponse.LEAVE;
	}
}
