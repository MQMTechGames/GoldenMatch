using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BasePlayerAI))]
public class AIBehaviurEditor : EditorWindow
{
	public string _state = "RecoverTheBall";

	BasePlayerAI _basePlayerAI = null;

	public float _yRotation;

	[MenuItem ("Window/AITester editor")]
	static void showEditor()
	{
		AIBehaviurEditor pplaceholder = EditorWindow.GetWindow<AIBehaviurEditor>();

		pplaceholder.init();
	}

	public void init()
	{
		GameObject go = GameObject.FindGameObjectWithTag("Player");
		_basePlayerAI = go.GetComponent<BasePlayerAI>();

		DebugUtils.assert(null != _basePlayerAI, "[AIBehaviurTester]: _basePlayerAI is not found!");
	}
	
	void OnSelectionChange()
	{
		Debug.Log("OnSelectionChange");
		Repaint();
	}
	void OnGUI()
	{
		if(null == _basePlayerAI) {
			GameObject go = GameObject.FindGameObjectWithTag("Player");
			_basePlayerAI = go.GetComponent<BasePlayerAI>();
		}

		// BT State
		GUILayout.Label("BT Settings", EditorStyles.boldLabel);
		_state = EditorGUILayout.TextField ("State", _state);

		if(GUILayout.Button("PushState")) {
			DebugUtils.log("pushState pressed");
			_basePlayerAI.pushState(_state);
		}

		// Rotation
		GUILayout.Label("BT Settings", EditorStyles.boldLabel);
		_yRotation = EditorGUILayout.FloatField ("Rotation", _yRotation);
		if(GUILayout.Button("Rotate")) {
			Quaternion quat = Quaternion.AngleAxis(_yRotation, new Vector3(0,1,0));
			_basePlayerAI.rigidbody.MoveRotation(_basePlayerAI.transform.rotation * quat);
		}
	}

	void OnInspectorGUI()
	{

	}
}
