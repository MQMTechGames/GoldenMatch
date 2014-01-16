using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class AdvancedEditorScripting : EditorWindow {
	
	const string level004Path = "Assets/004.unity";
	
	public static AdvancedEditorScripting instance;
	
	public class Slide {
		public string[] textLines;
		public string sceneToOpen;
		
		public Slide (string[] textLines, string sceneToOpen) {
			this.textLines = textLines;
			this.sceneToOpen = sceneToOpen;
		}
	}
	
	Slide[] slides;
	int currentSlide = 0;
	
	GUIStyle veryLargeLabel;
	GUIStyle largeLabel;
	
	static double startTime;
	
	[MenuItem ("Window/Advanced Editor Scripting")]
	public static void ShowMe() {
		GetWindow<AdvancedEditorScripting>();
	}
	
	void OnEnable() {
		LoadSlides();
		instance = this;
	}
	
	void OnDisable() {
		if(instance == this) {
			instance = null;
		}
	}
	
	void OnFocus() {
		LoadSlides();
	}
	
	void LoadSlides() {
		string[] lines = new System.IO.StreamReader(GetSlidesFilePath()).ReadToEnd().Split(new string[] {"*slide"}, System.StringSplitOptions.RemoveEmptyEntries);
		slides = new Slide[lines.Length];
		for(int i=0; i<slides.Length; i++) {
			string[] tokens = lines[i].Split(";"[0]);
			slides[i] = new Slide(tokens[0].Split("\n"[0]), tokens.Length > 1 ? tokens[1].Replace("\n", "") : string.Empty);
		}
	}
	
	string GetSlidesFilePath() {
		string scriptAssetPath = new List<string>(AssetDatabase.GetAllAssetPaths()).Find(x => x.EndsWith(this.GetType().ToString() + ".cs"));
		int lastSlashIndex = scriptAssetPath.LastIndexOf('/');
		if(lastSlashIndex == -1) {
			lastSlashIndex = scriptAssetPath.LastIndexOf('\\');
		}
		scriptAssetPath = scriptAssetPath.Substring(0, lastSlashIndex + 1);
		
		string projectPath = Application.dataPath;
		lastSlashIndex = projectPath.LastIndexOf('/');
		if(lastSlashIndex == -1) {
			lastSlashIndex = projectPath.LastIndexOf('\\');
		}
		projectPath = projectPath.Substring(0, lastSlashIndex + 1);;
		
		return projectPath + scriptAssetPath + "Slides.txt";
	}
	
	int largeFontSize = 48;
	
	void LoadStyles() {
		veryLargeLabel = new GUIStyle(EditorStyles.largeLabel);
		veryLargeLabel.fontSize = largeFontSize;
		largeLabel = new GUIStyle(veryLargeLabel);
		largeLabel.fontSize = Mathf.RoundToInt(largeLabel.fontSize * 0.8f);
	}
	
	void OnGUI() {
		if(slides == null) {
			return;
		}
		
		if(veryLargeLabel == null) {
			LoadStyles();
		}
		
		if(currentSlide > -1 && currentSlide < slides.Length) {
			DrawSlide(slides[currentSlide]);
		}
		
		if(Event.current.type == EventType.KeyDown) {
			if(Event.current.keyCode == KeyCode.LeftBracket) {
				ChangeFontSize(-1);
				Event.current.Use();
			} else if(Event.current.keyCode == KeyCode.RightBracket) {
				ChangeFontSize(1);
				Event.current.Use();
			} else if(Event.current.keyCode == KeyCode.Comma) {
				ChangeSlide(-1);
				Event.current.Use();
			} else if(Event.current.keyCode == KeyCode.Period) {
				ChangeSlide(1);
				Event.current.Use();
			} else if(Event.current.keyCode == KeyCode.T) {
				ShowTimeLeft();
				Event.current.Use();
			}
		}
	}
	
	void ShowTimeLeft() {
		int elapsedMinutes = Mathf.RoundToInt((float) ((EditorApplication.timeSinceStartup - startTime) / 60d));
		ShowTextNotification((60 - elapsedMinutes).ToString() + " minutes left");
	}
	
	void ChangeFontSize(int direction) {
		largeFontSize += direction * 4;
		LoadStyles();
		Repaint();
		ShowTextNotification((direction == -1 ? "Decrease" : "Increase") + " Font Size");
	}
	
	void ShowTextNotification(string text) {
		this.ShowNotification(new GUIContent(text));
	}
	
	void ChangeSlide(int direction) {
		currentSlide = Mathf.Clamp(currentSlide + direction, 0, slides.Length - 1);
		Repaint();
	}
	
	void DrawSlide(Slide slide) {
		EditorGUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginVertical();
		for(int i=0; i<slide.textLines.Length; i++) {
			GUIStyle styleToUse = veryLargeLabel;
			
			string textLine = slide.textLines[i];
			if(textLine.IndexOf(':') != -1) {
				string modifier = textLine.Substring(0, textLine.IndexOf(':'));
				textLine = textLine.Substring(textLine.IndexOf(':') + 1, textLine.Length - (textLine.IndexOf(':') + 1));
				if(modifier == "s") {
					styleToUse = largeLabel;
				}
			}
			GUILayout.Label(textLine, styleToUse);
		}
		EditorGUILayout.EndVertical();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
		
		string label = string.Empty;
		Vector2 labelSize = Vector2.zero;
		
		//Draw slide number at the bottom-left
		label = string.Format("Slide {0}/{1}", currentSlide + 1, slides.Length);
		labelSize = EditorStyles.label.CalcSize(new GUIContent(label));
		GUI.Label(new Rect(5, this.position.height - (5 + labelSize.y), labelSize.x, labelSize.y), label);
		
		//If there's a method to call, show the button to call it
		if(!string.IsNullOrEmpty(slides[currentSlide].sceneToOpen)) {
			label = "Call Method";
			labelSize = EditorStyles.toolbarButton.CalcSize(new GUIContent(label));
			if(GUI.Button(new Rect(this.position.width - (labelSize.x + 5), this.position.height - (5 + labelSize.y), labelSize.x, labelSize.y), label, EditorStyles.toolbarButton)) {
				CallMethod(slides[currentSlide].sceneToOpen);
			}
		}
	}
	
	void CallMethod(string methodName) {
		MethodInfo method = this.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		if(method != null) {
			method.Invoke(null, new System.Object[] {});
		} else {
			Debug.LogError("Could not find method " + methodName);
		}
	}
	
	public static void StartTimer() {
		startTime = EditorApplication.timeSinceStartup;
		if(instance != null) {
			instance.ShowTextNotification("Started Timer");
		}
	}
	
	public static void LoadBoxPlotsLevel004() {
		EditorApplication.OpenScene(level004Path);
	}
	
	public static void BringUpOpenProjectDialog() {
		TextDialog.ShowTextDialog("Open the BoxPlots project you fool!");
	}
	
	public static void SelectModelsFolder() {
		Object obj = AssetDatabase.LoadAssetAtPath("Assets/Models", typeof(Object));
		Selection.activeObject = obj;
		EditorGUIUtility.PingObject(obj);
	}
	
	public static void SelectSourceTexturesFolder() {
		Object obj = AssetDatabase.LoadAssetAtPath("Assets/SourceTextures", typeof(Object));
		Selection.activeObject = obj;
		EditorGUIUtility.PingObject(obj);
	}
	
	public static void ShowBuildManager() {
		BuildManager.ShowWindow();
	}
	
	public static void ShowResourceChecker() {
		EditorApplication.OpenScene(level004Path);
//		ResourceChecker.Init();
	}
	
	public static void SelectPVRTexture() {
		Object obj = AssetDatabase.LoadAssetAtPath("Assets/Textures/grass_darkgreen.jpg", typeof(Object));
		Selection.activeObject = obj;
		EditorGUIUtility.PingObject(obj);
	}
	
	public static void ShowStressBall() {
//		StressBall.Init();
	}
	
	public static void ShowWebWindow() {
		WebWindow.Load();
	}
}
