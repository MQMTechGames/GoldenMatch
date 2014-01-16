using UnityEngine;
using System.Collections;

public class PlayerMovement
{
	// public vars
	public Transform _transform = null;

	public float movementSpeed = 10.0f;

	public float turnSmoothing = 15f;	// A smoothing value for turning the player.
	public float speedDampTime = 0.1f;	// The damping for the speed parameter

	public static float kStopRotatingThreshold = MathUtils.kDegreeInRadians * 6.0f;

	// prvate vars
	private Animator _anim = null;
	private DoneHashIDs _hash;
	private AnimatorParams _animParams;

	public PlayerMovement(Transform transform) {
		if (null == _transform) {
			_transform = transform; 
		}

		DebugUtils.assert (_transform, "[PlayerController] hash must not be null");
		DebugUtils.assert (_transform.rigidbody, "[PlayerController] hash must not be null");

		_anim = _transform.GetComponent<Animator>();
		DebugUtils.assert (null != _anim, "[PlayerController] hash must not be null");

		_hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
		DebugUtils.assert (null != _hash, "[PlayerController] hash must not be null");

		_animParams = AnimatorParams.sharedInstance();
	}

	public float rotateToPoint(Vector3 point, float rotateVel)
	{
		float angle = MathUtils.getAngleToPoint(_transform, point);

		float finalRotationAngle = angle * rotateVel;
		if(Mathf.Abs(angle) < Mathf.Abs(finalRotationAngle)) {
			finalRotationAngle = angle;
		}

		// Apply new quat
		Quaternion deltaQuat = Quaternion.AngleAxis(MathUtils.toDegree(finalRotationAngle), new Vector3(0,1,0));
		Quaternion newQuat = _transform.rotation * deltaQuat;
		_transform.rigidbody.MoveRotation(_transform.rotation * deltaQuat);

		// Return the angle smoothed
		return angle;
	}

	public float runToPointStep(Vector3 point, float rotateStep, float dampTime = 0.0f, float attenuationMinDist = 0.25f, float attenuationMaxDist = 5.5f)
	{
		float distance = MathUtils.getDistanceToPoint(_transform, point);
		
		float minDistance = attenuationMinDist;
		float maxDistance = attenuationMaxDist;
		
		float speed = (distance - minDistance) / (maxDistance - minDistance);
		speed = MathUtils.clamp(speed);
		
		// Move forward
		_anim.SetFloat(_animParams.speedFloat, speed, dampTime, Time.deltaTime);
		
		// Keep adjusting rotation
		float angleRotated = rotateToPoint(point, rotateStep);
		
		return distance; 
	}

	public void move()
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		bool sneak = Input.GetButton("Sneak");
		
		MovementManagement(h, v, sneak);
	}

	void MovementManagement (float horizontal, float vertical, bool sneaking)
	{
		// Set the sneaking parameter to the sneak input.
		_anim.SetBool(_hash.sneakingBool, sneaking);
		
		// If there is some axis input...
		if(horizontal != 0f || vertical != 0f)
		{
			// ... set the players rotation and set the speed parameter to 5.5f.
			Rotating(horizontal, vertical);
			_anim.SetFloat(_hash.speedFloat, 5.5f, speedDampTime, Time.deltaTime);
		}
		else
			// Otherwise set the speed parameter to 0.
			_anim.SetFloat(_hash.speedFloat, 0);
	}
	
	
	void Rotating (float horizontal, float vertical)
	{
		// Create a new vector of the horizontal and vertical inputs.
		Vector3 targetDirection = new Vector3(horizontal, 0f, vertical);
		
		// Create a rotation based on this new vector assuming that up is the global y axis.
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		
		// Create a rotation that is an increment closer to the target rotation from the player's rotation.
		Quaternion newRotation = Quaternion.Lerp(_transform.rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);
		
		// Change the players rotation to this new rotation.
		_transform.rigidbody.MoveRotation(newRotation);
	}
}
