using UnityEngine;
using System.Collections;

public class InputLateralPlayerController : IInputPlayerController
{
    Transform _target;
    Animator _anim;
    AnimatorParams _animParams;

    public float movementSpeed = 10.0f;
    public float turnSmoothing = 15f;	// A smoothing value for turning the player.
    public float speedDampTime = 0.1f;	// The damping for the speed parameter

    public static void create(out InputLateralPlayerController inputPlayerController)
    {
        inputPlayerController = new InputLateralPlayerController();
        inputPlayerController.init();
    }

    InputLateralPlayerController()
    {
    }

    void init()
    {
        _animParams = AnimatorParams.sharedInstance();
    }

    public void setTarget(Transform target, Animator anim)
    {
        _target = target;
        _anim = anim;
    }

    public void move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool sneak = Input.GetButton("Sneak");

        MovementManagement(h, v, sneak);
    }

    void MovementManagement(float horizontal, float vertical, bool sneaking)
    {
        // If there is some axis input...
        if (horizontal != 0f || vertical != 0f)
        {
            // ... set the players rotation and set the speed parameter to 5.5f.
            Rotating(horizontal, vertical);
            _anim.SetFloat(_animParams.speedFloat, 5.5f, speedDampTime, Time.deltaTime);
        }
        else
            // Otherwise set the speed parameter to 0.
            _anim.SetFloat(_animParams.speedFloat, 0);
    }

    void Rotating(float horizontal, float vertical)
    {
        // Create a new vector of the horizontal and vertical inputs.
        Vector3 targetDirection = new Vector3(horizontal, 0f, vertical);

        // Create a rotation based on this new vector assuming that up is the global y axis.
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // Create a rotation that is an increment closer to the target rotation from the player's rotation.
        Quaternion newRotation = Quaternion.Lerp(_target.rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);

        // Change the players rotation to this new rotation.
        _target.rigidbody.MoveRotation(newRotation);
    }
}