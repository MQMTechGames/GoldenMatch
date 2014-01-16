using UnityEngine;
using System.Collections;

public class CameraChanger : MonoBehaviour
{
	CameraManager _cameraManager = null;

	public void Awake()
	{
		_cameraManager = ManagerContainer.sharedInstance().getCameraManager();
	}

	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.K)) {
			DebugUtils.assert(null != _cameraManager, "_cameraManager must be NOT NULL");
			_cameraManager.changeToCamera(CameraNames.ThirdPersonCamera);
		}

		if(Input.GetKeyDown(KeyCode.L)) {
			DebugUtils.assert(null != _cameraManager, "_cameraManager must be NOT NULL");
			_cameraManager.changeToCamera(CameraNames.LateralCamera);
		}
	}
}