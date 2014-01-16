using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class ThirdPersonCamera : CameraController
{
	public Transform _cameraTarget = null;
	public float _smoothFactor = 2f;
	public float _hDistance = 2f;
	public float _vDistance = 1.8f;
	
	void Awake()
	{
		_cameraTarget = null == _cameraTarget ? GameObject.FindGameObjectWithTag(DoneTags.player).transform : _cameraTarget;
	}
	
	void Update()
	{
		lerpFromPreviusCamera();

		updateCameraPosition();
	}
	
	void updateCameraPosition()
	{
		Vector3 newPosition = _cameraTarget.position 
			- _cameraTarget.forward * _hDistance
			+ _cameraTarget.up * _vDistance;

		Vector3 newForward = _cameraTarget.forward;

		transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * _smoothFactor);
		transform.forward = Vector3.Lerp(transform.forward, newForward, Time.deltaTime * _smoothFactor);
	}
}
