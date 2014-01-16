using UnityEngine;
using System.Collections;
using UnityEditor;



[CustomEditor(typeof(PlayerPlaceholder))]
public class PlayerPlaceholderEditor : EditorWindow
{
	public const float kPlayerMaxMovementRange = 999.0f;
	public const float kPlayerMinMovementRange = 0.0f;

	// Model props
	PlayerPlaceholder playerPlaceholder = null;

	// Editor props
	bool init = false;

	string myString = "Hello World";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;

	// Model set/getters
	public void setPlayerPlaceholder(PlayerPlaceholder placeholder)
	{
		playerPlaceholder = placeholder;
	}

	[MenuItem ("Window/Player editor")]
	static void showEditor()
	{
		PlayerPlaceholderEditor pplaceholder = EditorWindow.GetWindow<PlayerPlaceholderEditor>();

		pplaceholder.saveCurrentPlayerPlaceholder();
	}

	void OnSelectionChange()
	{
		Debug.Log("OnSelectionChange");
		saveCurrentPlayerPlaceholder();
		Repaint();
	}

	void saveCurrentPlayerPlaceholder()
	{
		Debug.Log("Entering in saveCurrentPlayerPlaceholder");

		GameObject go = Selection.activeGameObject;
		if(!go) {
			playerPlaceholder = null;
			return;
		}

		playerPlaceholder = go.GetComponent<PlayerPlaceholder>();
	}

	void OnGUI()
	{
		saveCurrentPlayerPlaceholder();

		GUILayout.Label("Base Settings", EditorStyles.boldLabel);
		myString = EditorGUILayout.TextField ("Text Field", myString);		

		groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		{
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
		}
		EditorGUILayout.EndToggleGroup ();

		showPlayerPlaceholderProps();
	}

	void showPlayerPlaceholderProps()
	{
		if(!playerPlaceholder) {
			Debug.Log("showPlayerPlaceholderPros: Player Placeholder is: null");
			return;
		}

		Debug.Log("showPlayerPlaceholderPros: Player Placeholder is: " + playerPlaceholder.gameObject.name);

		EditorGUILayout.BeginHorizontal ();
		{
			playerPlaceholder.playerInfo.backwardsMovementRange = EditorGUILayout.Slider("backwardsMovementRange"
                        , playerPlaceholder.playerInfo.backwardsMovementRange, kPlayerMinMovementRange, kPlayerMaxMovementRange );

			playerPlaceholder.playerInfo.forwardMovementRange = EditorGUILayout.Slider("forwardMovementRange"
                    	, playerPlaceholder.playerInfo.forwardMovementRange, kPlayerMinMovementRange, kPlayerMaxMovementRange );

			playerPlaceholder.playerInfo.leftMovementRange = EditorGUILayout.Slider("leftMovementRange"
                        , playerPlaceholder.playerInfo.leftMovementRange, kPlayerMinMovementRange, kPlayerMaxMovementRange );

			playerPlaceholder.playerInfo.rightMovementRange = EditorGUILayout.Slider("rightMovementRange"
                        , playerPlaceholder.playerInfo.rightMovementRange, kPlayerMinMovementRange, kPlayerMaxMovementRange );

		}
		EditorGUILayout.EndHorizontal ();
	}
}
