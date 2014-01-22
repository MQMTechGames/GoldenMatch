using UnityEngine;
using System.Collections;

// to clear console
using UnityEditor;
using System;
using System.Reflection;

using TeamData;

public class TrainningAI
{
    static float kThresholdToApproachBall = 0.75f;
    static float kThresholdBallHandlingApproachBall = 0.60f;
    static float kThresholdBallHandlingWaypoint = 1.5f;
    static float kThresholdToApproachBallToShot = 0.75f;

    static float kThresholdToBallPossesion = 2.5f;

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
    
    Vector3 _waypointDir = new Vector3();
    Vector3 _startingPoint = new Vector3();

    public TrainningTeamController _teamController = null;
    bool _ballHandling = false;
    BasePlayerAI _basePlayerAI = null;

    // teammate to pass the ball
    BasePlayerAI _teammateToPass = null;

    public static void create(out TrainningAI testPlayerAI, BasePlayerAI basePlayeAI, Transform transform, TrainningTeamController teamController)
    {
        testPlayerAI = new TrainningAI(basePlayeAI, transform, teamController);
    }

    public TrainningAI(BasePlayerAI basePlayeAI, Transform transform, TrainningTeamController teamController)
    {
        // Assign parameters
        _basePlayerAI = basePlayeAI;
        _transform = transform;
        _teamController = teamController;

        // Get components
        _playerController = new PlayerMovement(transform);
        _animator = _transform.GetComponent<Animator>();
        _hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
        _shootTransform = GameObjectUtils.getComponentByEntityName<Transform>("shotPosition");

        DebugUtils.assert(null != _shootTransform, "_shotPosition must exist");
        DebugUtils.assert(null != _transform, "_transform must exist");
        DebugUtils.assert(null != _animator, "_animator must exist");

        // Init objects
        _animParams = AnimatorParams.sharedInstance();

        _ballTransform = GameObject.FindGameObjectWithTag("ball").transform;
    }

    public void buildNode(string parentName, BTBuilder builder)
    {
        builder
            .addNode(parentName, "trainning", BTNodeType.BTNODE_PRIORITY)

            // Ball
            /**/.addNode("trainning", "trainningWithPossesion", BTNodeType.BTNODE_SEQUENCE, trainningPlayerDoesHavePossesion)
            /**//**/.addNode("trainningWithPossesion", "trainningWithPossesionFindTeammate", BTNodeType.BTNODE_LEAF, null, trainningWithPossesionFindTeammate)
            /**//**/.addNode("trainningWithPossesion", "trainningWithPossesionFaceToTeammate", BTNodeType.BTNODE_LEAF, null, trainningWithPossesionFaceToTeammate)
            /**//**/.addNode("trainningWithPossesion", "trainningWithPossesionPassToTeammate", BTNodeType.BTNODE_LEAF, null, trainningWithPossesionPassToTeammate)

            /**/.addNode("trainning", "trainningRecoverPossesion", BTNodeType.BTNODE_SEQUENCE, trainningPlayerdoesCanRecoverTheBall)
            // Recovering
            /**//**/.addNode("trainningRecoverPossesion", "trainningrecoverBallInitialRotation", BTNodeType.BTNODE_LEAF, null, recoverBallInitialRotation)
            /**//**/.addNode("trainningRecoverPossesion", "trainningrecoverBallLoop", BTNodeType.BTNODE_WHILE, recoverBallLoopCondition)
            /**//**//**/.addNode("trainningrecoverBallLoop", "trainningrecoverBallLoopStep", BTNodeType.BTNODE_SEQUENCE)
            /**//**//**//**/.addNode("trainningrecoverBallLoopStep", "trainningrecoveringRotateToBallStep", BTNodeType.BTNODE_LEAF, null, recoveringRotateToBallStep)
            /**//**//**//**/.addNode("trainningrecoverBallLoopStep", "trainningrecoveringGoToBallStep", BTNodeType.BTNODE_LEAF, null, recoveringGoToBallStep)
            /**//**/.addNode("trainningRecoverPossesion", "trainningResetVel", BTNodeType.BTNODE_LEAF, null, trainningResetVel)
            /**//**/.addNode("trainningRecoverPossesion", "trainningRecoverUnassignAction", BTNodeType.BTNODE_LEAF, null, trainningRecoverUnassignAction)

          // nonPosession
            /**/.addNode("trainning", "trainningWithoutPossesion", BTNodeType.BTNODE_SEQUENCE, null)
            /**//**/.addNode("trainningWithoutPossesion", "trainningWithoutPossesionFaceToBall", BTNodeType.BTNODE_LEAF, null, trainningWithoutPossesionFaceToBall)
            ;
    }

    // Recovering
    BTNodeResponse trainningResetVel()
    {
        _animator.SetFloat(_animParams.speedFloat, 0);

        return  BTNodeResponse.LEAVE;
    }

    bool trainningPlayerdoesCanRecoverTheBall()
    {
        // Check the shared blackboard
        int playerId = _basePlayerAI.gameObject.GetInstanceID();

        bool mustRecoverPossesion =_teamController.getBlackboard().isPlayerAssignedTo(ActionId.DO_RECOVER_POSSESION, playerId, true);
        if (mustRecoverPossesion)
        {
            _teamController.getBlackboard().addAction(ActionId.IS_RECOVERING_POSESSION, playerId);

            return true;
        }

        // I'm not assignet to do the job but... I still will try because I'm worth it
        int numPlayers = _teamController.getBlackboard().getNumPlayersAssignedTo(ActionId.IS_RECOVERING_POSESSION);
        if (0 == numPlayers)
        {
            float distanceToBall = MathUtils.getDistanceToPoint(_transform, _ballTransform.position);
            
            if(distanceToBall < 3f)
            {
                _teamController.getBlackboard().addAction(ActionId.IS_RECOVERING_POSESSION, playerId);
                //_ballHandling = true;

                return true;
            }
        }

        return false;
    }

    BTNodeResponse trainningRecoverUnassignAction()
    {
        int playerId = _basePlayerAI.gameObject.GetInstanceID();

        _teamController.getBlackboard().removePlayerFromAction(playerId, ActionId.IS_RECOVERING_POSESSION);

        return BTNodeResponse.LEAVE;
    }
    
    bool recoverBallLoopCondition()
    {
        Vector3 dirToBall = _ballTransform.position - _transform.position;
        float distanceToBall = dirToBall.magnitude;

        return distanceToBall > kThresholdBallHandlingWaypoint;
    }

    BTNodeResponse recoverBallInitialRotation()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, _rotateVel * Time.deltaTime);

        bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;

        return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
    }

    BTNodeResponse recoveringRotateToBallStep()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, 9999f);

        return BTNodeResponse.LEAVE;
    }

    BTNodeResponse recoveringGoToBallStep()
    {
        float distance = _playerController.runToPointStepSmooth(_ballTransform.position, 5.5f);

        return BTNodeResponse.LEAVE;
    }
    // End Recovering

    bool trainningPlayerDoesHavePossesion()
    {
        float distance = MathUtils.getDistanceToPoint(_transform, _ballTransform.position);

        //bool doesHavePossesion = _ballHandling && distance < kThresholdBallHandlingWaypoint * 2f;
        bool doesHavePossesion = distance < 5f;

        return doesHavePossesion;
    }

    BTNodeResponse trainningWithPossesionFindTeammate()
    {
        _teammateToPass = _teamController.getRandomTeammate();

        DebugUtils.log("trainningWithPossesionFaceToTeammate");
        //if (_basePlayerAI == _teammateToPass)
        //{
        //    return BTNodeResponse.STAY;
        //}

         return BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithPossesionFaceToTeammate()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _teammateToPass.transform.position, _rotateVel * Time.deltaTime);

        DebugUtils.log("trainningWithPossesionFaceToTeammate");

//        bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;

  //      return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
        return BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithPossesionPassToTeammate()
    {
        DebugUtils.log("trainningWithPossesionPassToTeammate");

        Vector3 dir = MathUtils.getDirection(_transform, _teammateToPass.transform);
        float distance = dir.magnitude;

        dir.Normalize();

        float forceMag = 15f * distance; //15f
        Vector3 force = dir * forceMag;

        _ballTransform.rigidbody.AddForce(force, ForceMode.Impulse);

        return BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithoutPossesionFaceToBall()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(_transform, _ballTransform.position, _rotateVel * 10 * Time.deltaTime);

        return BTNodeResponse.LEAVE;
    }
}
