using UnityEngine;
using System.Collections;

public class ManagerContainer
{
	static ManagerContainer singleton = null;
	
    CameraManager _cameraManager = null;
    InputControllerManager _inputControllerManager = null;

	ManagerContainer()
	{}

	public static ManagerContainer sharedInstance()
	{
		if(singleton == null) {
			singleton = new ManagerContainer();
			singleton.init();
		}

		return singleton;
	}

	void init()
	{
		// Create all the managers
		CameraManager.create (out _cameraManager);

        InputControllerManager.create(out _inputControllerManager);

		DebugUtils.assert (null != _cameraManager, "[PlayerController] hash must not be null");
	}

	public CameraManager getCameraManager()
	{
		return _cameraManager;
	}

    public InputControllerManager getInputControllerManager()
    {
        return _inputControllerManager;
    }
}
