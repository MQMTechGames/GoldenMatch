using UnityEngine;
using System.Collections;

// to clear console
using UnityEditor;
using System;
using System.Reflection;

using TeamData;

class BallHandler
{
    float _lasTimePass = 0;
    public Transform _ball = null;

    public void passTo(Transform point)
    {
        float currTime = Time.timeSinceLevelLoad;
        if (currTime - _lasTimePass < 2f)
        {
            return;
        }

        // do pass
        _lasTimePass = Time.timeSinceLevelLoad;

        Vector3 dir = MathUtils.getDirection(_ball, point);
        float distance = dir.magnitude;

        dir.Normalize();

        float forceMag = 1.35f * distance;

        if(forceMag > 20.0f)
        {
            forceMag = 20.0f;
        }
        Vector3 force = dir * forceMag;

        _ball.rigidbody.AddForce(force, ForceMode.Impulse);
    }
}

public class TrainningAI : IPlayer
{
    // constants
    static float kDistanceBallInControll = 1.0f;

    // Tweakeable variables
    public float _runVelocity = 5.5f;
    public float _dampTime = 0.1f;
    public float _rotateVel = 2.0f;
    public float _rotationAccelByDistance = 3.0f;

    PlayerMovement _playerController = null;
    Animator _animator = null;
    Transform _ballTransform = null;
    Transform _shootTransform = null;
    AnimatorParams _animParams = null;
    DoneHashIDs _hash = null;
    
    public TrainningTeamController _teamController = null;

    IPlayer _teammateToPass = null;

    BallHandler _ballHandler = new BallHandler();

    float _waittingTime = 0.0f;

    public override void Awake()
    {
        // Get components
        _playerController = new PlayerMovement(transform);
        _animator = transform.GetComponent<Animator>();
        _hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
        _shootTransform = GameObjectUtils.getComponentByEntityName<Transform>("shotPosition");

        // Init objects
        _animParams = AnimatorParams.sharedInstance();

        _ballTransform = GameObject.FindGameObjectWithTag("ball").transform;
        Debug.Log(_ballTransform);
        _ballHandler._ball = _ballTransform;

        base.Awake();
    }

    public override BT initStandalone()
    {
        BT bt;

        BTBuilder builder = BTBuilder.create("trainningAI").getBT(out bt);
        builder.addNode(null, "trainningAI", BTNodeType.BTNODE_PRIORITY)
            .addNode("trainningAI", buildNode);

        _teamController.addPlayer(this);

        return bt;
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
            /**//**/.addNode("trainningWithPossesion", "prepareToWait", BTNodeType.BTNODE_LEAF, null, prepareToWait)
            /**//**/.addNode("trainningWithPossesion", "trainningWait", BTNodeType.BTNODE_LEAF, null, trainningWait)

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

        // save behaviur tree
        builder.getBT(out _bt);
    }

    // Recovering
    BTNodeResponse trainningResetVel()
    {
        _animator.SetFloat(_animParams.speedFloat, 0);

        return  BTNodeResponse.LEAVE;
    }

    // Recovering
    BTNodeResponse prepareToWait()
    {
        _waittingTime = 0.0f;

        return BTNodeResponse.LEAVE;
    }

    // Recovering
    BTNodeResponse trainningWait()
    {
        _waittingTime += Time.deltaTime;
        if (_waittingTime > 1.5f)
        {
            return BTNodeResponse.LEAVE;
        }

        return BTNodeResponse.STAY;
    }
    
    bool trainningPlayerdoesCanRecoverTheBall()
    {
        Debug.Log(_teamController);

        int numPlayers = _teamController.getBlackboard().getNumPlayersAssignedTo(ActionId.IS_RECOVERING_POSESSION);
        if (0 == numPlayers)
        {
            IPlayer player = _teamController.getClosesPlayerTo(_ballTransform.position);
            if(GetInstanceID() == player.GetInstanceID())
            {
                _teamController.getBlackboard().addAction(ActionId.IS_RECOVERING_POSESSION, GetInstanceID());
                
                return true;
            }
        }

        return false;
    }

    BTNodeResponse trainningRecoverUnassignAction()
    {
        int playerId = GetInstanceID();

        _teamController.getBlackboard().removePlayerFromAction(playerId, ActionId.IS_RECOVERING_POSESSION);

        return BTNodeResponse.LEAVE;
    }
    
    bool recoverBallLoopCondition()
    {
        Vector3 dirToBall = _ballTransform.position - transform.position;
        float distanceToBall = dirToBall.magnitude;

        return distanceToBall > kDistanceBallInControll;
    }

    BTNodeResponse recoverBallInitialRotation()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(transform, _ballTransform.position, _rotateVel * Time.deltaTime);

        bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;

        return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
    }

    BTNodeResponse recoveringRotateToBallStep()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(transform, _ballTransform.position, 9999f);

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
        float distance = MathUtils.getDistanceToPoint(transform, _ballTransform.position);

        //bool doesHavePossesion = _ballHandling && distance < kThresholdBallHandlingWaypoint * 2f;
        bool doesHavePossesion =  distance < 1.0f;

        return doesHavePossesion;
    }

    BTNodeResponse trainningWithPossesionFindTeammate()
    {
        _teammateToPass = _teamController.getRandomTeammate();

        DebugUtils.log("trainningWithPossesionFaceToTeammate");
        if (GetInstanceID() == _teammateToPass.GetInstanceID())
        {
            return BTNodeResponse.STAY;
        }

         return BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithPossesionFaceToTeammate()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(transform, _teammateToPass.transform.position, _rotateVel * Time.deltaTime);

        DebugUtils.log("trainningWithPossesionFaceToTeammate");

        bool hasRotated = Mathf.Abs(rotatedAngle) <= MathUtils.kEpsilon;

        return hasRotated ? BTNodeResponse.STAY : BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithPossesionPassToTeammate()
    {
        DebugUtils.log("trainningWithPossesionPassToTeammate");

        _ballHandler.passTo(_teammateToPass.transform);

        return BTNodeResponse.LEAVE;
    }

    BTNodeResponse trainningWithoutPossesionFaceToBall()
    {
        float rotatedAngle = TransformUtils.rotateToPointStep(transform, _ballTransform.position, _rotateVel * 10 * Time.deltaTime);

        return BTNodeResponse.LEAVE;
    }
}
