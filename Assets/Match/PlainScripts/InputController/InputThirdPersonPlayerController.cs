using UnityEngine;
using System.Collections;

public class InputThirdPersonPlayerController : IInputPlayerController
{
    Transform _target;
    Animator _anim;
    AnimatorParams _animParams;

    public float movementSpeed = 10.0f;
    public float turnSmoothing = 3f;	// A smoothing value for turning the player.
    public float speedDampTime = 0.1f;	// The damping for the speed parameter

    public static void create(out InputThirdPersonPlayerController inputPlayerController)
    {
        inputPlayerController = new InputThirdPersonPlayerController();
        inputPlayerController.init();
    }

    InputThirdPersonPlayerController()
    {
       
    }

    public void setTarget(Transform target, Animator anim)
    {
        _target = target;
        _anim = anim;
    }

    void init()
    {
        _animParams = AnimatorParams.sharedInstance();
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
            Rotating(horizontal);
            _anim.SetFloat(_animParams.speedFloat, vertical, speedDampTime, Time.deltaTime);
        }
        else
        {
            // Otherwise set the speed parameter to 0.
            _anim.SetFloat(_animParams.speedFloat, 0);
        }
    }

    void Rotating(float horizontal)
    {
        
        // Create a rotation based on this new vector assuming that up is the global y axis.
        Quaternion deltaRotation = Quaternion.AxisAngle(new Vector3(0, 1, 0), horizontal * turnSmoothing * Time.deltaTime );

        // Create a rotation that is an increment closer to the target rotation from the player's rotation.
        Quaternion newRotation = _target.rigidbody.rotation * deltaRotation;

        // Change the players rotation to this new rotation.
        _target.rigidbody.MoveRotation(newRotation);
    }
}