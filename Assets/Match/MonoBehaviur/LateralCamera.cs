using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class LateralCamera : CameraController
{
	public Transform _cameraTarget = null;
	public float _smoothFactor = 2f;
	public float _hDistance = 2f;
	public float _vDistance = 6f;

	private Vector3 _originalForward;

	void Awake()
	{
		_cameraTarget = null == _cameraTarget ? GameObject.FindGameObjectWithTag(DoneTags.player).transform : _cameraTarget;
		_originalForward = transform.forward;
	}

	void Update()
	{
		lerpFromPreviusCamera();

		updateCameraPosition();
	}

	void updateCameraPosition()
	{
		Vector3 newPosition = _cameraTarget.position 
			- Vector3.forward * _hDistance
			+ Vector3.up * _vDistance;

		transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * _smoothFactor);
		transform.forward = Vector3.Lerp(transform.forward, _originalForward, Time.deltaTime * _smoothFactor);
	}
}
