using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BuildManager : EditorWindow {
	
	public class BuildConfiguration {
		public string name;
		public List<string> scenes;
		public int firstScene;
		
		public bool verified = false;
		
		public BuildConfiguration() {
			name = "unset";
			scenes = new List<string>();
		}
		
		public void AddScene(string sceneGUID) {
			verified = false;
			scenes.Add(sceneGUID);
		}
		
		public void SwapSceneIndices(int moveSceneIndex, int moveSceneDirection) {
			verified = false;
			string sceneGUID = scenes[moveSceneIndex];
			scenes.RemoveAt(moveSceneIndex);
			scenes.Insert(moveSceneIndex + moveSceneDirection, sceneGUID);
		}
		
		public void RemoveSceneAtIndex(int index) {
			verified = false;
			scenes.RemoveAt(index);
		}
	}
	
	const string buildConfigurationNodeName = "BuildConfiguration";
	
	List<BuildConfiguration> buildConfigurations;
	
	List<Object> scenesInProject;
	string[] scenePaths;
	
	public MonoScript scriptAsset;
	
	Vector2 scrollPosition;
	int sceneSelectionIndex = 0;
	
	//Add and remove button textures and styles
	public Texture2D addTextureUp;
	public Texture2D addTextureDown;
	public Texture2D removeTextureUp;
	public Texture2D removeTextureDown;
	public Texture2D moveItemAboveUp;
	public Texture2D moveItemAboveDown;
	public Texture2D moveItemBelowUp;
	public Texture2D moveItemBelowDown;
	
	[SerializeField] GUIStyle addButtonStyle;
	[SerializeField] GUIStyle removeButtonStyle;
	[SerializeField] GUIStyle moveItemAboveStyle;
	[SerializeField] GUIStyle moveItemBelowStyle;
	
	int moveSceneIndex = -1;
	int moveSceneDirection = 0;
	
	int sceneToRemoveIndex = -1;

	void OnEnable() {
		addButtonStyle = new GUIStyle();
		addButtonStyle.normal.background = addTextureUp;
		addButtonStyle.active.background = addTextureDown;
		addButtonStyle.fixedWidth = addTextureUp.width;
		addButtonStyle.fixedHeight = addTextureUp.height;
		addButtonStyle.margin = new RectOffset(4,4,4,4);
		
		removeButtonStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		removeButtonStyle.normal.background = removeTextureUp;
		removeButtonStyle.active.background = removeTextureDown;
		
		moveItemAboveStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		moveItemAboveStyle.normal.background = moveItemAboveUp;
		moveItemAboveStyle.active.background = moveItemAboveDown;
		
		moveItemBelowStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		moveItemBelowStyle.normal.background = moveItemBelowUp;
		moveItemBelowStyle.active.background = moveItemBelowDown;
		
		LoadConfigurations();
		LoadListOfScenesInProject();
	}
	
	[MenuItem ("Window/Build Manager #&b")]
	public static void ShowWindow() {
		EditorWindow.GetWindow<BuildManager>();
	}
	
	void OnGUI() {
		int removeAtIndex = -1;
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		for(int i=0; i<buildConfigurations.Count; i++) {
			BuildConfiguration buildConfiguration = buildConfigurations[i];
			
			GUILayout.BeginHorizontal();
			GUI.changed = false;
			buildConfiguration.name = EditorGUILayout.TextField(buildConfiguration.name, EditorStyles.boldLabel);
			if(GUI.changed) {
				SaveConfigurations();
			}
			GUILayout.FlexibleSpace();
			sceneSelectionIndex = EditorGUILayout.Popup(sceneSelectionIndex, scenePaths);
			if(sceneSelectionIndex != 0) {
				buildConfiguration.AddScene(AssetDatabase.AssetPathToGUID(scenePaths[sceneSelectionIndex]));
				sceneSelectionIndex = 0;
				SaveConfigurations();
			}
			
			if(GUILayout.Button("Verify")) {
				VerifyBuildConfiguration(buildConfiguration);
			}
			if(GUILayout.Button("Build")) {
				DoBuildConfiguration(buildConfiguration);
				EditorGUIUtility.ExitGUI();
			}
			if(GUILayout.Button("Remove")) {
				removeAtIndex = i;
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				GUILayout.BeginVertical();
					for(int s=0; s<buildConfiguration.scenes.Count; s++) {
						GUILayout.BeginHorizontal();
							GUILayout.Label(AssetDatabase.GUIDToAssetPath(buildConfiguration.scenes[s]));
							GUI.enabled = s != 0;
							if(GUILayout.Button(string.Empty, moveItemAboveStyle)) {
								moveSceneIndex = s;
								moveSceneDirection = -1;
							}
							GUI.enabled = true;
							GUI.enabled = s < buildConfiguration.scenes.Count - 1;
							if(GUILayout.Button(string.Empty, moveItemBelowStyle)) {
								moveSceneIndex = s;
								moveSceneDirection = 1;
							}
							GUI.enabled = true;
							if(GUILayout.Button(string.Empty, removeButtonStyle)) {
								sceneToRemoveIndex = s;
							}
						GUILayout.EndHorizontal();
					}
			
					if(moveSceneIndex != -1) {
						buildConfiguration.SwapSceneIndices(moveSceneIndex, moveSceneDirection);
						moveSceneIndex = -1;
						moveSceneDirection = 0;
						SaveConfigurations();
					}
			
					if(sceneToRemoveIndex != -1) {
						buildConfiguration.RemoveSceneAtIndex(sceneToRemoveIndex);
						sceneToRemoveIndex = -1;
						SaveConfigurations();
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		if(removeAtIndex > -1 && removeAtIndex < buildConfigurations.Count) {
			RemoveBuildConfiguration(removeAtIndex);
		}
		
		GUILayout.EndScrollView();
		
		if(GUILayout.Button("Add Build Configuration")) {
			AddBuildConfiguration();
		}
		
		if(GUILayout.Button("Refresh Project Scenes")) {
			LoadListOfScenesInProject();
		}
	}
	
	void VerifyBuildConfiguration(BuildConfiguration buildConfiguration) {
		/* TEMP */
		buildConfiguration.verified = true;
		return;
		/* END TEMP */
		
		bool verificationError = false;
		
		//Make sure the build name in Resources/buildName is the same as this build configuration
		if(buildConfiguration.name != (Resources.Load("buildName", typeof(TextAsset)) as TextAsset).text) {
			Debug.LogError("The build name in Resources/buildName TextAsset is not " + buildConfiguration.name + ". Please fix the name.");
			verificationError = true;
		}
		
		string coloringPagesInResourcesGUID = AssetDatabase.AssetPathToGUID("Assets/Resources/Coloring Pages");
		string sceneBuilderInResourcesGUID = AssetDatabase.AssetPathToGUID("Assets/Resources/Scene Builder");
		string sceneBuilderNarrativesInResourcesGUID = AssetDatabase.AssetPathToGUID("Assets/Resources/Scene Builder Narratives");
		string sceneBuilderStampGUID = AssetDatabase.AssetPathToGUID("Assets/Resources/SceneBuilderStamp.prefab");
		
		if(buildConfiguration.name == "Full App") {
			//Make sure Coloring Pages and Scene Builder prefabs are in Resources
			if(coloringPagesInResourcesGUID == string.Empty || sceneBuilderInResourcesGUID == string.Empty || sceneBuilderNarrativesInResourcesGUID == string.Empty || sceneBuilderStampGUID == string.Empty) {
				Debug.LogError("Coloring Pages, SceneBuilder, or Scene Builder Narratives folders are outside of Resources. Please move them in.");
				verificationError = true;
			}
			
			//Make sure intro video is in StreamingAssets
			string introVideoGUID = AssetDatabase.AssetPathToGUID("Assets/StreamingAssets/introAni_video2.m4v");
			if(introVideoGUID == string.Empty) {
				Debug.LogError("Intro video asset is outside of StreamingAssets. Please move it in.");
				verificationError = true;
			}
			
			//Make sure the icons are set properly
			if(!VerifyIcons(new string[] {"fullApp_icon144", "fullApp_icon114", "fullApp_icon72", "fullApp_icon57"})) {
				Debug.LogError("App icons aren't set properly. Please move it in.");
				verificationError = true;
			}
			
			if(!verificationError) {
				buildConfiguration.verified = true;
				Debug.Log(buildConfiguration.name + " build configuration successfully verified.");
			}
		} else if(buildConfiguration.name == "Costume Chest") {
			//Make sure Coloring Pages and Scene Builder prefabs are out of Resources
			if(coloringPagesInResourcesGUID != string.Empty || sceneBuilderInResourcesGUID != string.Empty || sceneBuilderNarrativesInResourcesGUID != string.Empty || sceneBuilderStampGUID != string.Empty) {
				Debug.LogError("Coloring Pages, SceneBuilder, or Scene Builder Narratives folders are still in Resources. Please move them out.");
				verificationError = true;
			}
			
			//Make sure intro video is outside of StreamingAssets
			string introVideoGUID = AssetDatabase.AssetPathToGUID("Assets/StreamingAssets/introAni_video2.m4v");
			if(introVideoGUID != string.Empty) {
				Debug.LogError("Intro video asset is in StreamingAssets. Please move it out.");
				verificationError = true;
			}
			
			//Make sure the icons are set properly
			if(!VerifyIcons(new string[] {"costumeChest_icon_144", "costumeChest_icon_114", "costumeChest_icon_72", "costumeChest_icon_57"})) {
				Debug.LogError("App icons aren't set properly. Please move it in.");
				verificationError = true;
			}
			
			if(!verificationError) {
				buildConfiguration.verified = true;
				Debug.Log(buildConfiguration.name + " build configuration successfully verified.");
			}
		} else if(buildConfiguration.name == "Storybook") {
			//Make sure Coloring Pages and Scene Builder prefabs are out of Resources
			if(coloringPagesInResourcesGUID != string.Empty || sceneBuilderInResourcesGUID != string.Empty || sceneBuilderNarrativesInResourcesGUID != string.Empty || sceneBuilderStampGUID != string.Empty) {
				Debug.LogError("Coloring Pages, SceneBuilder, or Scene Builder Narratives folders are still in Resources. Please move them out.");
				verificationError = true;
			}
			
			//Make sure intro video is outside of StreamingAssets
			string introVideoGUID = AssetDatabase.AssetPathToGUID("Assets/StreamingAssets/introAni_video2.m4v");
			if(introVideoGUID != string.Empty) {
				Debug.LogError("Intro video asset is in StreamingAssets. Please move it out.");
				verificationError = true;
			}
			
			//Make sure the icons are set properly
			if(!VerifyIcons(new string[] {"onceUponAYoodle_icon_144", "onceUponAYoodle_icon_114", "onceUponAYoodle_icon_72", "onceUponAYoodle_icon_57"})) {
				Debug.LogError("App icons aren't set properly. Please move it in.");
				verificationError = true;
			}
			
			if(!verificationError) {
				buildConfiguration.verified = true;
				Debug.Log(buildConfiguration.name + " build configuration successfully verified.");
			}
		} else if(buildConfiguration.name == "Coloring Pages") {
			//Make sure Coloring Pages prefabs are in Resources and Scene Builder prefabs are out of Resources
			if(coloringPagesInResourcesGUID == string.Empty) {
				Debug.LogError("Coloring Pages folder is not in Resources. Please move it in.");
				verificationError = true;
			}
			
			if(sceneBuilderInResourcesGUID != string.Empty || sceneBuilderNarrativesInResourcesGUID != string.Empty || sceneBuilderStampGUID != string.Empty) {
				Debug.LogError("Scene Builder or Scene Builder Narratives folders are still in Resources. Please move them out.");
				verificationError = true;
			}
			
			//Make sure intro video is outside of StreamingAssets
			string introVideoGUID = AssetDatabase.AssetPathToGUID("Assets/StreamingAssets/introAni_video2.m4v");
			if(introVideoGUID != string.Empty) {
				Debug.LogError("Intro video asset is in StreamingAssets. Please move it out.");
				verificationError = true;
			}
			
			//Make sure the icons are set properly
			if(!VerifyIcons(new string[] {"crayonMagic_icon_144", "crayonMagic_icon_114", "crayonMagic_icon_72", "crayonMagic_icon_57"})) {
				Debug.LogError("App icons aren't set properly. Please move it in.");
				verificationError = true;
			}
			
			if(!verificationError) {
				buildConfiguration.verified = true;
				Debug.Log(buildConfiguration.name + " build configuration successfully verified.");
			}
		} else if(buildConfiguration.name == "Scene Builder") {
			//Make sure Scene Builder prefabs are in Resources and Coloring Pages prefabs are out of Resources
			if(sceneBuilderInResourcesGUID == string.Empty || sceneBuilderNarrativesInResourcesGUID == string.Empty || sceneBuilderStampGUID == string.Empty) {
				Debug.LogError("Scene Builder elements are not in Resources. Please move them in.");
				verificationError = true;
			}
			
			if(coloringPagesInResourcesGUID != string.Empty) {
				Debug.LogError("Coloring Pages folder is still in Resources. Please move it out.");
				verificationError = true;
			}
			
			//Make sure intro video is outside of StreamingAssets
			string introVideoGUID = AssetDatabase.AssetPathToGUID("Assets/StreamingAssets/introAni_video2.m4v");
			if(introVideoGUID != string.Empty) {
				Debug.LogError("Intro video asset is in StreamingAssets. Please move it out.");
				verificationError = true;
			}
			
			//Make sure the icons are set properly
			if(!VerifyIcons(new string[] {"yoodleDoodle_icon_144", "yoodleDoodle_icon_114", "yoodleDoodle_icon_72", "yoodleDoodle_icon_57"})) {
				Debug.LogError("App icons aren't set properly. Please move it in.");
				verificationError = true;
			}
			
			if(!verificationError) {
				buildConfiguration.verified = true;
				Debug.Log(buildConfiguration.name + " build configuration successfully verified.");
			}
		} else {
			Debug.LogError("Unknown build configuration " + buildConfiguration.name + " in VerifyBuildConfiguration");
		}
	}
	
	bool VerifyIcons(string[] iconTextureNames) {
		Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iPhone);
		if(icons.Length != iconTextureNames.Length) {
			Debug.LogError("Number of icons assigned doesn't match the number of icon texture names given.");
			Debug.Log("Number of icons assigned is " + icons.Length.ToString());
			Debug.Log("Number of icon texture names given is " + iconTextureNames.Length.ToString());
			return false;
		}
		
		bool iconsMatch = true;
		for(int i=0; i<icons.Length; i++) {
			if(icons[i] == null || icons[i].name != iconTextureNames[i]) {
				iconsMatch = false;
				Debug.LogError(i.ToString() + "th icon doesn't match.");
				break;
			}
		}
		
		return iconsMatch;
	}
	
	void DoBuildConfiguration(BuildConfiguration buildConfiguration) {
		VerifyBuildConfiguration(buildConfiguration);
		
		if(!buildConfiguration.verified) {
			Debug.LogError(buildConfiguration.name + " build configuration has not been verified. Please verify it before building.");
			return;
		}
		
		/* TEMP */
		List<string> buildScenePaths2 = new List<string>(buildConfiguration.scenes.Count);
		buildConfiguration.scenes.ForEach(delegate(string sceneGUID) { buildScenePaths2.Add(AssetDatabase.GUIDToAssetPath(sceneGUID));});
		string buildPath2 = GetBuildDirectory().FullName + System.IO.Path.DirectorySeparatorChar + "UNITE12 " + System.DateTime.Now.ToString("d-MMM-yyyy") + ".app";
		BuildTarget target2 = BuildTarget.StandaloneOSXIntel;
		string error = BuildPipeline.BuildPlayer(buildScenePaths2.ToArray(), buildPath2, target2, BuildOptions.None);
		
		if(error != string.Empty) {
			Debug.Log(error);
		} else {
			PostProcessBuild(buildPath2);
		}
		
		buildConfiguration.verified = false;
		
		return;
		/* END TEMP */
			
		if(buildConfiguration.name != "Full App"&&
			buildConfiguration.name != "Costume Chest" &&
			buildConfiguration.name != "Storybook" &&
			buildConfiguration.name != "Coloring Pages" &&
			buildConfiguration.name != "Scene Builder") {
			Debug.LogError("Unknown build configuration " + buildConfiguration.name + " in DoBuildConfiguration");
			return;
		}
		
		string buildPath = GetBuildDirectory().FullName + System.IO.Path.DirectorySeparatorChar;
		BuildTarget target = BuildTarget.iPhone;
	
		if(buildConfiguration.name == "Full App") {
			buildPath += "Yoomerang " + System.DateTime.Now.ToString("d-MMM-yyyy");
			PlayerSettings.productName = "Yoomerang";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
//			PlayerSettings.iOS.targetPlatform = iOSTargetPlatform.ARMv7;
			PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;
			PlayerSettings.bundleIdentifier = "com.yoomerang.yoomerangiPad";
			PlayerSettings.iOS.applicationDisplayName = "Yoomerang";
		} else if(buildConfiguration.name == "Costume Chest") {
			buildPath += "Costume Chest " + System.DateTime.Now.ToString("d-MMM-yyyy");
			PlayerSettings.productName = "Yoodle Apps - Costume Chest";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
//			PlayerSettings.iOS.targetPlatform = iOSTargetPlatform.ARMv7;
			PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;
			PlayerSettings.bundleIdentifier = "com.yoomerang.costumechest";
			PlayerSettings.iOS.applicationDisplayName = "Costume Chest";
		} else if(buildConfiguration.name == "Storybook") {
			buildPath += "Storybook " + System.DateTime.Now.ToString("d-MMM-yyyy");
			PlayerSettings.productName = "Yoodle Apps - Once Upon A Yoodle";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
//			PlayerSettings.iOS.targetPlatform = iOSTargetPlatform.ARMv7;
			PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;
			PlayerSettings.bundleIdentifier = "com.yoomerang.yoodleStorybook";
			PlayerSettings.iOS.applicationDisplayName = "Upon a Yoodle";
		} else if(buildConfiguration.name == "Coloring Pages") {
			buildPath += "Coloring Pages " + System.DateTime.Now.ToString("d-MMM-yyyy");
			PlayerSettings.productName = "Yoodle Apps - Crayon Magic";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
//			PlayerSettings.iOS.targetPlatform = iOSTargetPlatform.ARMv7;
			PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;
			PlayerSettings.bundleIdentifier = "com.yoomerang.yoodleColoringPages";
			PlayerSettings.iOS.applicationDisplayName = "Crayon Magic";
		} else if(buildConfiguration.name == "Scene Builder") {
			buildPath += "Scene Builder " + System.DateTime.Now.ToString("d-MMM-yyyy");
			PlayerSettings.productName = "Yoodle Apps - Yoodle Doodle";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
//			PlayerSettings.iOS.targetPlatform = iOSTargetPlatform.ARMv7;
			PlayerSettings.iOS.targetResolution = iOSTargetResolution.Native;
			PlayerSettings.bundleIdentifier = "com.yoomerang.yoodleSceneBuilder";
			PlayerSettings.iOS.applicationDisplayName = "Yoodle Doodle";
		}
		
		PreProcessBuild();
		
		List<string> buildScenePaths = new List<string>(buildConfiguration.scenes.Count);
		buildConfiguration.scenes.ForEach(delegate(string sceneGUID) { buildScenePaths.Add(AssetDatabase.GUIDToAssetPath(sceneGUID));});
		
		string err = BuildPipeline.BuildPlayer(buildScenePaths.ToArray(), buildPath, target, BuildOptions.None);
		
		if(err != string.Empty) {
			Debug.Log(err);
		} else {
			PostProcessBuild(buildPath);
		}
		
		buildConfiguration.verified = false;
	}
	
	public static void PreProcessBuild() {
		
	}
	
	public static void PostProcessBuild(string buildPath) {
		//Copy the Editor output log to the build directory
		string editorLogFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Library/Logs/Unity/Editor.log";
		FileInfo editorLogFile = new FileInfo(editorLogFilePath);
		if(editorLogFile.Exists) {
			StreamReader reader = new StreamReader(editorLogFile.OpenRead());
			string logFileContents = reader.ReadToEnd();
			//Find the part of the log file we're interested in, namely the build size stuff
			int chunkStart = logFileContents.LastIndexOf("Textures ");
			int chunkEndStart = logFileContents.LastIndexOf("*** Completed ");
			int chunkEndEnd = logFileContents.IndexOf('\n', chunkEndStart);
			if(chunkStart != -1 && chunkEndEnd != -1) {
				logFileContents = logFileContents.Substring(chunkStart, chunkEndEnd - chunkStart);
			} else {
				Debug.LogError("Could not find the proper start and end indices for the chunk we were looking for. ChunkStart: " + chunkStart.ToString() + " ChunkEnd: " + chunkEndEnd.ToString());
			}
			FileInfo buildLogFile = new FileInfo(buildPath + "/build.log");
			StreamWriter writer = buildLogFile.CreateText();
			if(writer == null) {
				Debug.LogError("Build log file could not be created for writing at: " + buildLogFile.FullName);
			} else {
				writer.Write(logFileContents);
				writer.Close();
			}
		} else {
			Debug.LogWarning("Editor log file could not be found at: " + editorLogFilePath);
		}
	}
	
	public static DirectoryInfo GetBuildDirectory() {
		DirectoryInfo assetsDirectory = new DirectoryInfo(Application.dataPath);
		DirectoryInfo projectDirectory = assetsDirectory.Parent;
		DirectoryInfo[] buildsDirectories = projectDirectory.GetDirectories("Builds");
		if(buildsDirectories == null || buildsDirectories.Length == 0) {
			//Try to create the Builds directory
			DirectoryInfo buildsDirectory = projectDirectory.CreateSubdirectory("Builds");
			return buildsDirectory == null ? projectDirectory : buildsDirectory;
		} else {
			return buildsDirectories[0];
		}
	}
	
	void AddBuildConfiguration() {
		buildConfigurations.Add(new BuildConfiguration());
		SaveConfigurations();
	}
	
	void RemoveBuildConfiguration(int removeAtIndex) {
		buildConfigurations.RemoveAt(removeAtIndex);
		SaveConfigurations();
	}

	void LoadListOfScenesInProject() {
		scenesInProject = new List<Object>();
		
		List<string> allAssetPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
		
		allAssetPaths.RemoveAll(x => !(x.EndsWith(".unity")));
		allAssetPaths.ForEach(delegate(string sceneAssetPath) { scenesInProject.Add(AssetDatabase.LoadAssetAtPath(sceneAssetPath, typeof(Object)));} );
		scenesInProject.Sort((x,y) => string.Compare(x.name, y.name));
		
		List<string> scenePathsList = new List<string>(scenesInProject.Count);
		scenesInProject.ForEach(delegate(Object scene) { scenePathsList.Add(AssetDatabase.GetAssetPath(scene)); } );
		//Add "Add Scene" string to the beginning of the list since we'll want to use that as the label for the dropdown
		scenePathsList.Insert(0, "Add Scene");
		
		scenePaths = scenePathsList.ToArray();
	}
	
	void LoadConfigurations() {
		FileInfo buildConfigurationsFile = new FileInfo(GetConfigurationFilePath());
		
		StreamReader reader = new StreamReader(buildConfigurationsFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read));
		
		buildConfigurations = new List<BuildConfiguration>();
	
		string[] lines = reader.ReadToEnd().Split('\n');
		reader.Close();
	
		int i = 0;
		while(i < lines.Length) {
			string line = lines[i];
			if(line.Contains(VerySimpleXml.StartNode(buildConfigurationNodeName))) {
				i = ReadBuildConfiguration(i, lines, buildConfigurations);
			}
			i++;
		}
	}
	
	int ReadBuildConfiguration(int lineIndex, string[] lines, List<BuildConfiguration> buildConfigurationsList) {
		BuildConfiguration buildConfiguration = new BuildConfiguration();
		while(lineIndex < lines.Length) {
			string line = lines[lineIndex];
			if(line.Contains(VerySimpleXml.EndNode(buildConfigurationNodeName))) {
				buildConfigurationsList.Add(buildConfiguration);
				return lineIndex;
			} else if(line.Contains(VerySimpleXml.StartNode("Name"))) {
				buildConfiguration.name = VerySimpleXml.NodeValue(line, "Name");
			} else if(line.Contains(VerySimpleXml.StartNode("Scene"))) {
				string sceneGUID = VerySimpleXml.NodeValue(line, "Scene");
//				string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
//				Debug.Log("scene GUID: " + sceneGUID + " scene Path: " + scenePath);
				buildConfiguration.scenes.Add(sceneGUID);
			}
			lineIndex++;
		}
		return lineIndex;
	}
	
	void SaveConfigurations() {
		FileInfo buildConfigurationsFile = new FileInfo(GetConfigurationFilePath());
		StreamWriter writer = new StreamWriter(buildConfigurationsFile.Open(FileMode.Truncate, FileAccess.Write, FileShare.Read));
		
		foreach(BuildConfiguration buildConfiguration in buildConfigurations) {
			writer.WriteLine(VerySimpleXml.StartNode(buildConfigurationNodeName));
			writer.WriteLine(VerySimpleXml.StartNode("Name", 1) + buildConfiguration.name + VerySimpleXml.EndNode("Name"));
			foreach(string scene in buildConfiguration.scenes) {
				writer.WriteLine(VerySimpleXml.StartNode("Scene", 2) + scene + VerySimpleXml.EndNode("Scene"));
			}
			writer.WriteLine(VerySimpleXml.EndNode(buildConfigurationNodeName));
		}
		
		writer.Close();
	}
	
	string GetConfigurationFilePath() {
		string scriptAssetPath = AssetDatabase.GetAssetPath(scriptAsset);
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
		
		string buildConfigurationsFilePath = projectPath + scriptAssetPath + "BuildConfigurations.xml";
		return buildConfigurationsFilePath;
	}
}
