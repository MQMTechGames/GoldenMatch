using UnityEngine;
using UnityEditor;
using System.Collections;

public class TextDialog : EditorWindow {
	
	string textToShow = string.Empty;

	public static void ShowTextDialog(string text) {
		TextDialog dialog = EditorWindow.GetWindow<TextDialog>() as TextDialog;
		dialog.textToShow = text;
		dialog.InitSize();
		dialog.ShowUtility();
	}
	
	GUIStyle veryLargeLabel;
	
	public void InitSize() {
		LoadStyles();
		this.minSize = veryLargeLabel.CalcSize(new GUIContent(textToShow)) + Vector2.one * 20;
		this.maxSize = this.minSize;
		this.position = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, minSize.x ,minSize.y);
	}
	
	void LoadStyles() {
		veryLargeLabel = new GUIStyle(EditorStyles.largeLabel);
		veryLargeLabel.fontSize = 30;
	}
	
	void OnGUI() {
		if(veryLargeLabel == null) {
			LoadStyles();
		}
		
		GUILayout.Label(textToShow, veryLargeLabel);
	}
}
