using UnityEngine;
using System.Collections;

public class ManagerContainer
{
	static ManagerContainer singleton = null;
	CameraManager cameraManager = null;

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
		CameraManager.create (out cameraManager);

		DebugUtils.assert (null != cameraManager, "[PlayerController] hash must not be null");
	}

	public CameraManager getCameraManager()
	{
		return cameraManager;
	}
}
