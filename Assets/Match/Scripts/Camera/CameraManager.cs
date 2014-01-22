using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager
{
	Dictionary<string, GameObject> cameras;

	GameObject currCamera = null;

	private CameraManager()
	{
		cameras = new Dictionary<string, GameObject> ();
	}

	private void init()
	{
//		Debug.Log ("CameraManager.init()");

		Object[] cameras = GameObject.FindObjectsOfType (typeof(Camera));

		foreach(Object cam in cameras )
        {
			Camera castedCam = (Camera) cam;

			registerCamera(castedCam.gameObject);
		}
	}

	public static void create(out CameraManager cameraManager)
	{
		cameraManager = new CameraManager ();
		cameraManager.init ();
	}

	public void registerCamera(GameObject camera)
	{
		//Debug.Log ("CameraManager.registerCamera()");
		DebugUtils.assert (camera != null, "cameras must be not null");

		cameras.Add (camera.name, camera);

		if (null == currCamera) {
			currCamera = camera;
			currCamera.SetActive(true);
		} else {
			camera.SetActive(false);
		}
	}

	public bool changeToCamera(string name)
	{
//		Debug.Log (cameras);

//		foreach (var pair in cameras) {
//			Debug.Log(pair.Key);
//		}

		GameObject camera = null;
		bool exist = cameras.TryGetValue (name, out camera);

		if (!exist) {
			return false;
		}

		currCamera.SetActive(false);

		camera.GetComponent<CameraController>().changedCameraFrom(currCamera.camera);

		camera.SetActive(true);
		currCamera = camera;

		return true;
	}
}
