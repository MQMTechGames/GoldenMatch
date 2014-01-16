using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class CameraController : MonoBehaviour
{
	Camera _prevCamera = null;
	public float _lerpTime = 3f;

	// original values
	float _orthographicSize;
	float _farClipPlane;
	float _nearClipPlane;
	float _depth;
	float _fieldOfView;

	public virtual void changedCameraFrom(Camera prevCamera)
	{
		DebugUtils.assert(null!=prevCamera, "[CameraController->changedCameraFrom]: camera must NOT be NULL");
		_prevCamera = prevCamera;

		setPreviousCameraValues();
	}

	public virtual void setPreviousCameraValues()
	{
		DebugUtils.log("[CameraController->setPreviousCameraValues]");

		transform.position = _prevCamera.transform.position;
		transform.forward = _prevCamera.transform.forward;

		_orthographicSize = _prevCamera.orthographicSize;
		
		_nearClipPlane = _prevCamera.nearClipPlane;
		_farClipPlane = _prevCamera.farClipPlane;
		
		_fieldOfView = _prevCamera.fieldOfView;
		_depth = _prevCamera.depth;
	}

	public virtual void lerpFromPreviusCamera()
	{
		if(null == _prevCamera) {
			return;
		}

		DebugUtils.log("[CameraController->lerpFromPreviusCamera]");

		DebugUtils.assert(null!=_prevCamera, "[CameraController->lerpFromPreviusCamera]: _prevCamera must NOT be NULL");

		//DebugUtils.assert(null != _prevCamera, "[CameraController]->LerpFromPreviusCamera: " + "_prevCamera must be NOT null");

		camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, _orthographicSize, Time.deltaTime * _lerpTime);
		
		camera.nearClipPlane = Mathf.Lerp(camera.nearClipPlane, _nearClipPlane, Time.deltaTime * _lerpTime);
		camera.farClipPlane = Mathf.Lerp(camera.farClipPlane, _farClipPlane, Time.deltaTime * _lerpTime);
		
		camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, _fieldOfView, Time.deltaTime * _lerpTime);
		camera.depth = Mathf.Lerp(camera.depth, _depth, Time.deltaTime * _lerpTime);
	}

//	public void Update()
//	{
//		if(null != _prevCamera) {
//			LerpFromPreviusCamera();
//		}
//	}
	
}
