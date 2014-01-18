using UnityEngine;
using System.Collections;

public class CameraChanger : MonoBehaviour
{
	CameraManager _cameraManager = null;
    InputControllerManager _inputControllerManager = null;

	public void Awake()
	{
		_cameraManager = ManagerContainer.sharedInstance().getCameraManager();
        _inputControllerManager = ManagerContainer.sharedInstance().getInputControllerManager();
	}

	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.K)) {
			DebugUtils.assert(null != _cameraManager, "_cameraManager must be NOT NULL");
			_cameraManager.changeToCamera(CameraNames.ThirdPersonCamera);
            
            _inputControllerManager.changeToInputPlayerController(IInputPlayerControllerType.THIRD_PERSON_PLAYER_CONTROLLER);
		}

		if(Input.GetKeyDown(KeyCode.L)) {
			DebugUtils.assert(null != _cameraManager, "_cameraManager must be NOT NULL");
			_cameraManager.changeToCamera(CameraNames.LateralCamera);

            _inputControllerManager.changeToInputPlayerController(IInputPlayerControllerType.LATERAL_PLAYER_CONTROLLER);
		}
	}
}