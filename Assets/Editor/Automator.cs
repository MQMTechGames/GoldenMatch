using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

/*
 * Initial release goals:
	 * -Be able to set values for most Component and UnityScript fields.
	 * -Be able to change import settings across many assets at once.
	 * -Do everything you can do with TextureImportSettings
	 * -Do everything you can do with MultiPanel
	 * -Set transform position with position.x and also with position 10 10 10 (implemented)
	 * -Select all gameObjects that have X component in the Scene
 * /

/*
 * Todo:
 	 * -Parse Vector3s and similar commonly-used structs
	 * -Support multiple parameters for method invokes
	 * -Show suggestions as user is typing
	 * -Shake box when entered command couldn't be parsed
	 * -Can't set layer because it expects a string to be parsed into an int
 */

 //Should be able to use grabbing to say, grab the TextureImportSettings for one texture asset and apply it to a bunch of others
 //Also, ACTIONS, like in Photoshop, holy shit! Record commands, save and apply again and again

/*Types of "words":
	 * -Actions (add, remove, set, get, select, deselect, toggle)
	 * -Values (numerical values (4, 2.8), structs (v3(1,0,1)), bool (t,f) etc.)
 */

public class AutomatorCommand {
	public string command;
	public string commandMethod;
	public int minArguments;
	public int maxArguments;
	public int minSelectedObjects;
	
	public AutomatorCommand(string _command, string _commandMethod, int _minArguments, int _maxArguments, int _minSelectedObjects) {
		command = _command;
		commandMethod = _commandMethod;
		minArguments = _minArguments;
		maxArguments = _maxArguments;
		minSelectedObjects = _minSelectedObjects;
	}
	
	public override string ToString() {
		return command;
	}
}

public class Automator : EditorWindow {
	public static StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
	public static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static string objectReferenceString = "ref";
	static Vector2 windowSize = new Vector2(300,40);
	static int commandHistoryLength = 20;
	
	static Assembly UnityEditorAssembly;
	
	List<AutomatorCommand> automatorCommands;
	
	List<string> commandHistory;
	int currentHistoryIndex;
	
	string userCommand = "";
	char[] tokenSeparators = {' '};
	char[] commandSeparators = {';'};
	string[] commandTokens;
	
	string[] gameObjectFields = {"name", "active", "isStatic", "layer", "tag"};
	
	object objectReference;
	
	bool gainedFocus = false;
	
	[MenuItem ("Window/Automator %#a")] //Place the Automator menu item in the Tools menu with shortcut Cmd+Shift+A (on Windows, Ctrl+Shift+A)
    static void Load() {
        Automator window = Automator.GetWindowWithRect<Automator>(new Rect(100,100,windowSize.x,windowSize.y));
		window.Init();
//        window.Show();
		window.OnFocus();
    }
	
	void Init() {
		//Set window size
		minSize = windowSize;
		maxSize = windowSize;
		//Load Automator commands
		LoadAutomatorCommands();
		//Initialize command history
		InitializeCommandHistory();
		//Get UnityEditor Assembly
		if(UnityEditorAssembly == null) UnityEditorAssembly = GetAssembly("UnityEditor");
	}
	
	void InitializeCommandHistory() {
		//We set initial capacity to historyLength+1 so List doesn't increase actual capacity once we've reached 20 commands in the list
		if(commandHistory == null) {
			commandHistory = new List<string>(commandHistoryLength+1);
			currentHistoryIndex = 0;
		}
	}
	
	void LoadAutomatorCommands() {
		if(automatorCommands == null)
			automatorCommands = new List<AutomatorCommand>(5);
		else
			automatorCommands.Clear();
		automatorCommands.Add(new AutomatorCommand("add", "Add", 1, 1, 1));
		automatorCommands.Add(new AutomatorCommand("remove", "Remove", 1, 1, 1));
		automatorCommands.Add(new AutomatorCommand("set", "Set", 1, 10, 1));
		automatorCommands.Add(new AutomatorCommand("get", "Get", 0, 2, 1));
		automatorCommands.Add(new AutomatorCommand("toggle", "Toggle", 2, 2, 1));
		automatorCommands.Add(new AutomatorCommand("find", "Find", 2, 2, 0));
		automatorCommands.Add(new AutomatorCommand("select", "Select", 1, 3, 0));
		automatorCommands.Add(new AutomatorCommand("show", "ShowWindow", 1, 1, 0));
		automatorCommands.Add(new AutomatorCommand("temp1", "Temp1", 0, 0, 0));
		automatorCommands.Add(new AutomatorCommand("temp2", "Temp2", 1, 1, 1));
		automatorCommands.Add(new AutomatorCommand("temp3", "Temp3", 1, 1, 1));
		automatorCommands.Add(new AutomatorCommand("temp4", "Temp4", 0, 2, 1));
		automatorCommands.Add(new AutomatorCommand("print", "Print", 1, 1, 0));
	}
	
	public void Temp1(string[] parameters) {
//		foreach(Editor e in ActiveEditorTracker.sharedTracker.activeEditors) {
//			System.Object guiBlock = new System.Object();
//			e.GetType().GetMethod("GetOptimizedGUIBlock", fullBinding).Invoke(e, new object[] {false, false, guiBlock, System.Single.MaxValue});
//			Debug.Log(guiBlock.ToString());
//		}
		EditorWindow ew = EditorWindow.GetWindow(GetTypeFromAssembly("HierarchyWindow", UnityEditorAssembly));
		
		System.Object filteredHierarchy = GetTypeFromAssembly("BaseProjectWindow", UnityEditorAssembly).GetField("m_FilteredHierarchy", fullBinding).GetValue(ew);
		MethodInfo setFilterMethod = filteredHierarchy.GetType().GetProperty("filter", fullBinding).GetSetMethod();
		setFilterMethod.Invoke(filteredHierarchy, new object[] {"gameobject"});
		System.Array results = (System.Array) GetTypeFromAssembly("FilteredHierarchy", UnityEditorAssembly).GetField("m_Results", fullBinding).GetValue(filteredHierarchy);
		
		Type frType = GetTypeFromAssembly("FilterResult", UnityEditorAssembly);
		if(frType == null) Debug.Log("Couldn't find filter result type");
		FieldInfo instanceIDField = frType.GetField("instanceID", fullBinding);
		Debug.Log("Number of results: " + results.Length.ToString());	
		for(int i=0; i<results.Length; i++) {
			System.Object result = results.GetValue(i);
			if(result == null) continue;
			int id = (int) instanceIDField.GetValue(result);
			UnityEngine.Object obj = EditorUtility.InstanceIDToObject(id);
			if(obj == null) continue;
			Debug.Log(obj.name);
		}
	}
	
	public void Temp2(string[] parameters) {
		string[] childrenToRescue = parameters[0].Split(","[0]);
		
		UnityEngine.Object[] topLevelSelection = Selection.GetFiltered(typeof(Transform), SelectionMode.TopLevel);
		foreach(Transform parent in topLevelSelection) {
			foreach(string childToRescueName in childrenToRescue) {
				Transform child = parent.FindChild(childToRescueName);
				if(child != null) {
					child.parent = parent;
				}
			}
		}
	}
	
	/*
	public void Temp3(string[] parameters) {
		//Count number of tk2dSpriteCollections in assets
		List<string> allAssetPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
		int numberOfSpriteCollections = 0;
		int numberOfLoadedAssets = 0;
		List<string> prefabAssetPaths = allAssetPaths.FindAll(x => x.EndsWith(".prefab"));
		foreach(string assetPath in prefabAssetPaths) {
			if(assetPath.EndsWith(".prefab")) {
				GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
				numberOfLoadedAssets++;
				if(EditorUtility.DisplayCancelableProgressBar("Looking for tk2d Sprite Collections...", string.Format("Processed {0} out of {1} assets.", numberOfLoadedAssets, prefabAssetPaths.Count), numberOfLoadedAssets / (float) prefabAssetPaths.Count)) {
					break;
				}
				tk2dSpriteCollection spriteCollection = prefab.GetComponent<tk2dSpriteCollection>();
				if(spriteCollection != null) {
					if(spriteCollection.pixelPerfectPointSampled) {
						Debug.Log("Found pixel-perfect point sampled sprite collection: " + spriteCollection.name, spriteCollection.gameObject);
					} else {
						Debug.Log(spriteCollection.name);
					}
					numberOfSpriteCollections++;
				}
				if(numberOfLoadedAssets % 5 == 0) {
					EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
				}
			}
		}
		Debug.Log(numberOfSpriteCollections.ToString() + " sprite collections found in Asset Database");
		EditorUtility.ClearProgressBar();
	}
	*/
	
	public void Temp4(string[] parameters) {
//		UnityEngine.Object[] topLevelSelection = Selection.GetFiltered(typeof(Transform), SelectionMode.TopLevel);
//		foreach(Transform parent in topLevelSelection) {
//			s4dCrayonButton crayonButton = parent.GetComponent<s4dCrayonButton>();
//			crayonButton.selectionBoxRenderer = crayonButton.transform.FindChild("selectionBox").renderer;
//			UIRadioBtn radioButton = parent.GetComponent<UIRadioBtn>();
//			radioButton.scriptWithMethodToInvoke = crayonButton;
//		}
		
//		List<UnityEngine.Object> topLevelSelection = new List<UnityEngine.Object>(Selection.GetFiltered(typeof(Transform), SelectionMode.TopLevel));
//		iTweenPath path = (topLevelSelection.Find(x => x.name == "EnvelopeDirector") as Transform).GetComponent<iTweenPath>();
//		Vector3 offset = path.transform.parent.position;
//		for(int i=0; i<path.nodeCount; i++) {
//			Vector3 nodePosition = path.nodes[i];
//			nodePosition.z = -200;
//			path.nodes[i] = nodePosition;
//		}
		
//		EditorUtility.ClearProgressBar();
	
//		UnityEngine.Object[] selectedMaterials = Selection.GetFiltered(typeof(Material), SelectionMode.Assets);
//		foreach(Material mat in selectedMaterials) {
//			if(mat.mainTexture != null) {
//				string assetPath = AssetDatabase.GetAssetPath(mat.mainTexture);
//				if(assetPath.EndsWith(".pvr")) {
//					Debug.Log(mat.mainTexture.name + " is a PVR texture. Referencing material is " + mat.name, mat);
//				}
//			}
//		}
		
//		UnityEngine.Object[] selectedPrefabs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
//		foreach(GameObject prefab in selectedPrefabs) {
//			tk2dSpriteCollection collection = prefab.GetComponent<tk2dSpriteCollection>();
//			if(collection != null) {
//				for(int i=0; i<collection.textureRefs.Length; i++) {
//					if(collection.textureRefs[i] == null) {
//						Debug.Log("Null texture ref at index: " + i.ToString() + " in collection " + prefab.name, prefab);
//					}
//					Debug.Log(AssetDatabase.GetAssetPath(collection.textureRefs[i]), collection.textureRefs[i]);
//				}
//			}
//		}
	}
	
	static Color HexToRGB (int pColor) {
		Color color = new Color();
		
		color.r = ((pColor & 0xFF0000) >> 16) / 255.0f;
		color.g = ((pColor & 0x00FF00) >> 8) / 255.0f;
		color.b = (pColor & 0x0000FF) / 255.0f;
		color.a = 1.0f;
		
		return color;
	}
	
	/*
	public void OnEnable() {
		EditorUtility.ClearProgressBar();
	}
	*/
	
	
	/*
	public void Temp3(string[] parameters) {
		int[] charValues = new int[100];
		float[] kernValues = new float[] {14.20f,21.70f,29.60f,50.20f,43.80f,55.50f,46.10f,14.90f,25.10f,25.10f,22.50f,42.70f,13.50f,23.60f,14.80f,31.70f,42.90f,30.50f,40.20f,42.50f,40.00f,43.00f,43.40f,32.70f,42.80f,43.40f,16.20f,16.20f,42.70f,42.70f,42.70f,42.00f,62.00f,42.50f,44.20f,44.30f,44.30f,33.30f,31.90f,44.20f,44.50f,23.10f,26.50f,43.00f,30.50f,57.40f,43.40f,43.80f,40.20f,43.80f,43.10f,41.40f,36.90f,43.80f,41.90f,65.20f,38.60f,37.90f,31.80f,22.60f,31.70f,22.60f,38.70f,44.20f,26.70f,40.40f,41.60f,39.60f,41.60f,40.90f,23.20f,41.50f,42.00f,22.00f,22.40f,38.30f,22.00f,61.70f,41.90f,40.90f,41.60f,41.50f,28.70f,37.70f,24.40f,41.80f,35.00f,53.60f,34.70f,35.90f,29.40f,29.60f,21.70f,29.60f,42.00f,42.00f,42.00f,42.00f,42.00f,42.00f};
		for(int i=0; i<charValues.Length; i++)
			charValues[i] = i+32; //Starting from ascii code 32
		
		UnityEngine.Object[] objs = GetUnfilteredSelection();
		foreach(UnityEngine.Object obj in objs) {
			if(AssetDatabase.IsMainAsset(obj)) {
				if(obj.GetType().Name == "Font") {
					Font font = obj as Font;
					SerializedObject serializedObj = new SerializedObject(font);
					
					SerializedProperty sp = serializedObj.FindProperty("m_PerCharacterKerning");
					
					bool hasChildren = sp.hasChildren; // This may not be necessary
					int arrayLength = 0;
					int count = 0;
					int arrayElementCount = 0;
					int characterCount = 0;
					
					while(hasChildren || count == 0) {
						// Iteration 0 & 1 do not contain the data we need. This was verified by Debug.Log(sp.propertyType) through every iteration to find where the array length and starting indices were stored.
						
						if(count == 2) { // Iteration 2: This should be the size of the array.
						
							arrayLength = sp.intValue * 3; // get array size
							Debug.Log("Array length : " + arrayLength.ToString());
						
						} else if(count > 2) { // Iteration 2+: The remaining should be the array elements
							
							// Do something wonderful with the array here... in my case, create a GUI.IntField
							Debug.Log(arrayElementCount.ToString() + ": " + sp.name + " - type: " + sp.type.ToString());
							
							if(sp.type == "int") {
								sp.intValue = charValues[characterCount];
//								Debug.Log("set int value to: " + sp.intValue.ToString());
							} else if(sp.type == "float") {
								sp.floatValue = kernValues[characterCount];
//								Debug.Log("set float value to: " + sp.floatValue.ToString());
								characterCount++;
							}
							
							arrayElementCount++; // Increment array element count so we know when to stop. sp.CountRemaining() will not help us because it gives the count of all remaining properties in the entire SO, not the children of this SP.
							
							if(arrayElementCount == arrayLength) { // we got the last array element
								hasChildren = false;
								break; // stop iterating
							}
						}
						if(hasChildren) hasChildren = sp.Next(true); // Step to the next property in the SerializedObject including children
						count++;
					}
					
					serializedObj.ApplyModifiedPropertiesMultiSelect();
				}
			}
		}
	}
	*/
	
	public void ShowWindow (string[] parameters) {
		string windowName = parameters[0];
		EditorWindow ew = EditorWindow.GetWindow(GetTypeFromAllAssemblies(windowName));
		ew.Show();
	}
	
	void OnGUI() {
		//Name and call TextField
		GUI.SetNextControlName("commandField");
		userCommand = EditorGUILayout.TextField(userCommand);
		
		if(objectReference != null)
			EditorGUILayout.LabelField("Ref: ", objectReference.ToString());
		
		GUI.SetNextControlName("hiddenField");
		EditorGUILayout.LabelField("","");
		
		//Automatically transfer focus to the textfield when Automator gets focus
		if(gainedFocus) {
			GUI.FocusControl("commandField");
			gainedFocus = false;
		}
		
		//Evaluate current event
		Event e = Event.current;
		if(e.type == EventType.KeyUp) {
			if(e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
				//parse the commands
				string[] commands = userCommand.Split(commandSeparators);
				foreach(string command in commands){
					userCommand = command;
					DoCommand();
				}
				gainedFocus = true; //Re-focus on the textfield since hitting return switches focus elsewhere
			} else if(e.keyCode == KeyCode.UpArrow) {
				GetCommandFromHistory(1); //Go towards older commands in history
			} else if(e.keyCode == KeyCode.DownArrow) {
				GetCommandFromHistory(-1); //Go towards recent commands in history
			}
		}
	}
	
	void DoCommand(){
		ParseCommand(); //Parse the command entered by user into the commandTokens array
		ExecuteCommand(); //Try executing the command specified by the tokens in commandTokens
		SaveCommandToHistory(); //Save the entered command into command history
	}
	
	void ParseCommand() {
		commandTokens = userCommand.Split(tokenSeparators);
	}
	
	void ExecuteCommand() {
		if(commandTokens.Length == 0) return;
		
		string firstWord = commandTokens[0];
		
		//Find command
		AutomatorCommand command = FindAutomatorCommand(firstWord);
		if(command == null) {
			Debug.LogError(firstWord + " isn't a valid command.");
			return;
		}
		
		//Get parameters
		string[] parameters = new string[commandTokens.Length-1];
		Array.Copy(commandTokens, 1, parameters, 0, commandTokens.Length -1);
		
		//Check if given number of parameters matches what's required by the command
		if(parameters.Length < command.minArguments || parameters.Length > command.maxArguments) {
			StringBuilder sb = new StringBuilder();
			sb.Append(firstWord + " command requires between " + command.minArguments.ToString() + " and " + command.maxArguments.ToString() +
				" parameters. The following " + parameters.Length.ToString() + " were given: ");
			for(int i=0; i<parameters.Length; i++) {
				sb.Append(parameters[i]);
				if(i != parameters.Length - 1)
					sb.Append(",");
			}
			Debug.LogError(sb.ToString());
			sb = null;
			return;
		}
		
		//Check if number of selected objects matches what's required by the command
		if(GetUnfilteredSelection().Length < command.minSelectedObjects) {
			Debug.LogError(command.ToString() + " requires at least " + command.minSelectedObjects.ToString() + " selected objects.");
			return;
		}
		
		//Get method
		MethodInfo commandMethod = this.GetType().GetMethod(command.commandMethod);
		if(commandMethod == null) {
			Debug.LogError(command.commandMethod + " isn't a valid method.");
			return;
		}
		
		//Invoke method
		commandMethod.Invoke(this, new object[] {parameters});
	}
	
	void GetCommandFromHistory(int direction) {
		if(commandHistory == null)
			InitializeCommandHistory();
		
		//If there is no command to move to in the direction specified
		if(currentHistoryIndex + direction > commandHistory.Count || currentHistoryIndex + direction < 0) {
			EditorApplication.Beep(); //Let the user know with a short beep
		//If history index is at the oldest command and user hits up arrow
		} else if(currentHistoryIndex + direction == commandHistory.Count) {
			//If the userCommand text is the same as the oldest command
			if(userCommand.Equals(commandHistory[currentHistoryIndex])) {
				EditorApplication.Beep(); //Let the user know there's no previous command to go to
			} else { //If the userCommand is different than the oldest command
				userCommand = commandHistory[currentHistoryIndex]; //Go back to the oldest command
				ForceRepaint(); //Repaint Automator window
			}
		} else { //There is a command to go to in the specified direction
			currentHistoryIndex += direction; //Move history index
			userCommand = commandHistory[currentHistoryIndex]; //Go to the command in the direction specified
			ForceRepaint(); //Repaint Automator window
		}
	}
	
	void SaveCommandToHistory() {
		if(commandHistory == null)
			InitializeCommandHistory();
		if(userCommand.Equals("")) //Don't save an empty command
			return;
		if(commandHistory.Count == commandHistoryLength) {
//			Debug.Log("Removed the last command in the history: " + commandHistory[commandHistory.Count-1]);
			commandHistory.RemoveAt(commandHistory.Count - 1);
		}
		//Don't save the command if it's the same as the one before it
		if(commandHistory.Count > 0 && commandHistory[0].Equals(userCommand)) {
			return;
		}
		commandHistory.Insert(0, userCommand);
		currentHistoryIndex = 0;
//		Debug.Log("Saved command to history: " + userCommand);
//		Debug.Log("New command history count is: " + commandHistory.Count.ToString());
	}
	
	public void Add(string[] parameters) {
//		string componentName = GetCapitalizedName(parameters[0]);
		string componentName = parameters[0];
		foreach(GameObject go in Selection.gameObjects) {
			go.AddComponent(componentName);
			EditorUtility.SetDirty(go);
		}
	}
	
	public void Remove(string[] parameters) {
//		string componentName = GetCapitalizedName(parameters[0]);
		string componentName = parameters[0];
		foreach(GameObject go in Selection.gameObjects) {
			Component c = go.GetComponent(componentName);
			if(c)
				DestroyImmediate(c, true);
			EditorUtility.SetDirty(go);
		}
	}
	
	public void Get(string[] parameters) {
		UnityEngine.Object[] unfilteredSelection = GetUnfilteredSelection();
		if(parameters.Length == 0) {
			if(unfilteredSelection.Length == 0) {
				Debug.Log("No selection to get from.");
			} else if(unfilteredSelection.Length == 1) {
				objectReference = unfilteredSelection[0];
			} else {
				objectReference = unfilteredSelection;
			}
			return;
		}
		
		if(parameters[0] == "*importSettings") {
			GetImportSettings();
			Repaint();
			return;
		} else if(parameters[0] == "*type") {
			GetObjectType();
			Repaint();
			return;
		}
		
		//Check to see if we're applying Set on an asset instead of a gameobject
		
		if(unfilteredSelection != null && unfilteredSelection.Length > 0 && AssetDatabase.IsMainAsset(unfilteredSelection[0])) {
			GetAsset(parameters);
			return;
		}
		
//		string componentName = GetCapitalizedName(parameters[0]);
		string componentName = parameters[0];
		Component component = null;
		foreach(GameObject goForType in Selection.gameObjects) {
			component = goForType.GetComponent(componentName);
			if(component != null)
				break;
		}
		if(component == null) {
			Debug.LogError("None of the selected gameObjects have a " + componentName + " component.");
			return;
		}
		
		if(parameters.Length == 1) {
			objectReference = component;
		} else if(parameters.Length == 2) {
			string propertyName = parameters[1];
			PropertyInfo property = component.GetType().GetProperty(propertyName);
			if(property == null) { // No such property found, it might exist as a field
				FieldInfo field = component.GetType().GetField(propertyName);
				if(field == null) { //No such field exists either, exit
					Debug.Log(propertyName + " is not a property or field of " + componentName);
					return;
				}
				objectReference = field.GetValue(component);
				Repaint();
				return;
			}
			objectReference = property.GetValue(component, null);
			Repaint();
		}
	}
	
	void GetAsset(string[] parameters) {
		string propertyName = parameters[0];
		
		UnityEngine.Object obj = GetUnfilteredSelection()[0];
		string assetPath = AssetDatabase.GetAssetPath(obj);
		AssetImporter importer = AssetImporter.GetAtPath(assetPath);
		
		PropertyInfo property = importer.GetType().GetProperty(propertyName);
		if(property == null) { // No such property found
			Debug.Log(propertyName + " is not a property of " + importer.GetType().Name);
			return;
		}
		
		MethodInfo getMethod = property.GetGetMethod();
		if(getMethod == null) { //No setter found
			Debug.Log("No getter found for property " + propertyName + " of " + importer.name);
			return;
		}
		
		objectReference = getMethod.Invoke(importer, null);
		Repaint();
	}
	
	void GetImportSettings() {
		UnityEngine.Object[] selection = GetUnfilteredSelection();
		if(selection.Length > 1)
			Debug.Log("There's more than 1 object in the current unfiltered selection. Automator cannot guarantee which object's import settings will be read.");
			
		foreach(UnityEngine.Object obj in selection) {
			if(AssetDatabase.IsMainAsset(obj)) {
				string assetPath = AssetDatabase.GetAssetPath(obj);
				AssetImporter importer = AssetImporter.GetAtPath(assetPath);
				if(obj.GetType() == typeof(Texture2D)) {
					TextureImporter textureImporter = importer as TextureImporter;
//					TextureImporterSettings settings = new TextureImporterSettings();
//					textureImporter.ReadTextureSettings(settings);
					objectReference = textureImporter;
					break;
				} else if(obj.GetType() == typeof(AudioClip)) {
					AudioImporter audioImporter = importer as AudioImporter;
					objectReference = audioImporter;
				}
			}
		}
	}
	
	void SetImportSettings() {
		UnityEngine.Object[] unfilteredSelection = GetUnfilteredSelection();
//		AssetDatabase.StartAssetEditing();
		for(int i=0; i<unfilteredSelection.Length; i++) {
			UnityEngine.Object obj = unfilteredSelection[i];
			
			if(AssetDatabase.IsMainAsset(obj)) {
				string assetPath = AssetDatabase.GetAssetPath(obj);
				AssetImporter importer = AssetImporter.GetAtPath(assetPath);
				if(obj.GetType() == typeof(Texture2D)) {
//					if(objectReference.GetType() != typeof(TextureImporterSettings)) {
					if(objectReference.GetType() != typeof(TextureImporter)) {
						Debug.Log(objectReference.GetType() + " cannot be applied as importer settings to a Texture2D asset.");
						continue;
					}
					TextureImporter textureImporter = importer as TextureImporter;
					ApplyTextureImporterSettings(objectReference as TextureImporter, textureImporter);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj), ImportAssetOptions.ForceUpdate);
				} else if(obj.GetType() == typeof(AudioClip)) {
					if(objectReference.GetType() != typeof(AudioImporter)) {
						Debug.Log(objectReference.GetType() + " cannot be applied as importer settings to a AudioClip asset.");
						continue;
					}
					AudioImporter audioImporter = importer as AudioImporter;
					ApplyAudioClipImporterSettings(objectReference as AudioImporter, audioImporter);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj), ImportAssetOptions.ForceUpdate);
				}
			}
		}
//		AssetDatabase.StopAssetEditing();
		AssetDatabase.Refresh();
	}
	
	void ApplyTextureImporterSettings(TextureImporter sourceImporter, TextureImporter destinationImporter) {
		destinationImporter.anisoLevel = sourceImporter.anisoLevel;
		destinationImporter.borderMipmap = sourceImporter.borderMipmap;
		destinationImporter.convertToNormalmap = sourceImporter.convertToNormalmap;
		//Disabled due to compiler warning in 3.5
		//destinationImporter.correctGamma = sourceImporter.correctGamma;
		destinationImporter.fadeout = sourceImporter.fadeout;
		destinationImporter.filterMode = sourceImporter.filterMode;
		destinationImporter.generateCubemap = sourceImporter.generateCubemap;
		destinationImporter.grayscaleToAlpha = sourceImporter.grayscaleToAlpha;
		destinationImporter.heightmapScale = sourceImporter.heightmapScale;
		destinationImporter.isReadable = sourceImporter.isReadable;
		destinationImporter.lightmap = sourceImporter.lightmap;
		destinationImporter.maxTextureSize = sourceImporter.maxTextureSize;
		destinationImporter.mipMapBias = sourceImporter.mipMapBias;
		destinationImporter.mipmapEnabled = sourceImporter.mipmapEnabled;
		destinationImporter.mipmapFadeDistanceStart = sourceImporter.mipmapFadeDistanceStart;
		destinationImporter.mipmapFadeDistanceEnd = sourceImporter.mipmapFadeDistanceEnd;
		destinationImporter.mipmapFilter = sourceImporter.mipmapFilter;
		destinationImporter.normalmap = sourceImporter.normalmap;
		destinationImporter.normalmapFilter = sourceImporter.normalmapFilter;
		destinationImporter.npotScale = sourceImporter.npotScale;
		destinationImporter.textureFormat = sourceImporter.textureFormat;
		destinationImporter.textureType = sourceImporter.textureType;
		destinationImporter.wrapMode = sourceImporter.wrapMode;
		
		TextureImporterSettings textureSettings = new TextureImporterSettings();
		sourceImporter.ReadTextureSettings(textureSettings);
		destinationImporter.SetTextureSettings(textureSettings);
	}
	
	void ApplyAudioClipImporterSettings(AudioImporter sourceImporter, AudioImporter destinationImporter) {
		destinationImporter.compressionBitrate = sourceImporter.compressionBitrate;
		destinationImporter.forceToMono = sourceImporter.forceToMono;
		destinationImporter.format = sourceImporter.format;
		destinationImporter.hardware = sourceImporter.hardware;
		destinationImporter.loadType = sourceImporter.loadType;
		destinationImporter.loopable = sourceImporter.loopable;
		destinationImporter.threeD = sourceImporter.threeD;
	}
	
	void GetObjectType() {
		UnityEngine.Object[] selection = GetUnfilteredSelection();
		if(selection.Length > 1)
			Debug.Log("There's more than 1 object in the current unfiltered selection. Automator cannot guarantee which object's import settings will be read.");
		
		if(selection.Length > 0) {
			objectReference = selection[0].GetType();
		}
	}
	
	public void Set(string[] parameters) {
		//Check to see if we're applying Set on an asset instead of a gameobject
		UnityEngine.Object[] unfilteredSelection = GetUnfilteredSelection();
		if(unfilteredSelection != null && unfilteredSelection.Length > 0 && AssetDatabase.IsMainAsset(unfilteredSelection[0])) {
			SetAsset(parameters);
			return;
		}
		
//		string componentName = GetCapitalizedName(parameters[0]);
		string componentName = parameters[0];
		string propertyName = parameters[1];
		string value = string.Empty;
		if(parameters.Length > 2)
			value = parameters[2];
		
		//Check to see if we're setting a gameobject field such as name, layer, tag etc.
		bool setGameObjectField = false;
		for(int i=0; i<gameObjectFields.Length; i++) {
			if(componentName.Equals(gameObjectFields[i], ignoreCase)) {
				setGameObjectField = true;
				//override the property name and value
				propertyName = parameters[0];
				value = parameters[1];
				break;
			}
		}
		
		if(propertyName.Contains(".")) { //If property name contains a period, use the SerializedProperty method
			SetUsingSerializedProperty(GetAlternatePropertyName(propertyName), componentName, value); //Set using serialized property
			return;
		}
		
		Type type = null;
		
		if(setGameObjectField) {
			type = typeof(GameObject);
		} else {
			foreach(GameObject goForType in Selection.gameObjects) {
				Component compForType = goForType.GetComponent(componentName);
				if(compForType != null) {
					type = compForType.GetType();
					break;
				}
			}
			if(type == null) {
				Debug.LogError("None of the selected gameObjects have a " + componentName + " component.");
				return;
			}
		}
		
		PropertyInfo property = type.GetProperty(propertyName);
		if(property == null) { // No such property found, it might exist as a field, use SetUsingSerializedProperty
			SetUsingSerializedProperty(propertyName, componentName, value);
			return;
		}
		
		MethodInfo setMethod = property.GetSetMethod();
		if(setMethod == null) { //No setter found
			Debug.Log("Setter doesn't exist for " + propertyName + " property of " + componentName + ", using SerializedProperty check instead.");
			SetUsingSerializedProperty(propertyName, componentName, value); //Set using serialized property
			return;
		}
		//Setter exists
		ParameterInfo[] setMethodParameters = setMethod.GetParameters();
		if(setMethodParameters == null || setMethodParameters.Length == 0) {
			Debug.LogError("Method " + setMethod.Name + " for " + componentName + " takes no parameters");
			return;
		}
		Type parameterType = setMethodParameters[0].ParameterType;
		
		object[] valueArray = new object[1]; //Array to place the parsed value in
		if(value.Equals(objectReferenceString, ignoreCase)) {
			valueArray[0] = objectReference;
		} else {
			if(!ParseValueToParameter(valueArray, parameterType, value)) { //Parse value
				Debug.Log("Value couldn't be parsed! - " + value);
				return; //value couldn't be parsed
			}
		}
		
		foreach(GameObject go in Selection.gameObjects) { //For each transform selected
			if(setGameObjectField) {
				setMethod.Invoke(go, valueArray);
				EditorUtility.SetDirty(go);
			} else {
				Component comp = go.GetComponent(componentName); //Get component
				if(comp == null) continue; //If component wasn't found on this object, move to the next one
				setMethod.Invoke(comp, valueArray); //Invoke the setter method on the component
				EditorUtility.SetDirty(comp);
			}
		}
	}
	
	void SetUsingSerializedProperty(string varName, string componentName, string value) {
		foreach(GameObject go in Selection.gameObjects) {
			SerializedObject componentObj = new SerializedObject(go.GetComponent(componentName));
			SerializedProperty property = componentObj.FindProperty(varName);
			if(property == null) {
				Debug.Log(varName + " is not a property of " + componentName);
				break;
			}
			SerializedPropertyType propertyType = property.propertyType;
			if(propertyType == SerializedPropertyType.String) {
				property.stringValue = value;
			} else if(propertyType == SerializedPropertyType.Integer) {
				property.intValue = Int32.Parse(value);
			} else if(propertyType == SerializedPropertyType.Float) {
				property.floatValue = Single.Parse(value);
			} else if(propertyType == SerializedPropertyType.Boolean) {
				if(value.Equals("true", ignoreCase) || value.Equals("t", ignoreCase)) //'true' or 't'
					property.boolValue = true;
				else if(value.Equals("false", ignoreCase) || value.Equals("f", ignoreCase)) //'false' or 'f'
					property.boolValue = false;
				else
					Debug.LogError(value + " needs to be 'true', 'false', 't', or 'f' to be parsed into a boolean.");
			} else if(propertyType == SerializedPropertyType.Enum) {
				string[] enumNames = property.enumNames;
				for(int i=0; i<enumNames.Length; i++) {
					if(enumNames[i].Equals(value)) {
						property.enumValueIndex = i;
						break;
					}
				}
			} else if(propertyType == SerializedPropertyType.ObjectReference) {
				if(value.Equals("null", ignoreCase)) {
					property.objectReferenceValue = null;
				} else if(value.Equals(objectReferenceString, ignoreCase)) {
					property.objectReferenceValue = (UnityEngine.Object) objectReference;
				} else {
					Debug.Log("Object references can only be set to null or to a reference obtained using the get command.");
				}
			} else if(propertyType == SerializedPropertyType.AnimationCurve) {
				if(value.Equals(objectReferenceString, ignoreCase)) {
					property.animationCurveValue = new AnimationCurve((objectReference as AnimationCurve).keys);
				}
			} else if(propertyType == SerializedPropertyType.Color) {
				property.colorValue = ParseColor(value);
			} else {
				Debug.LogError(propertyType.ToString() + " is not a supported PropertyType.");
			}
			componentObj.ApplyModifiedProperties();
		}
	}
	
	public void SetAsset(string[] parameters) {
		string propertyName = parameters[0];
		
		if(propertyName == "*importSettings") {
			SetImportSettings();
			return;
		}
		
		string value = parameters[1];
		
		if(value.Equals(objectReferenceString, ignoreCase))
			value = objectReference.ToString();
		
		bool editorFieldSet = false;
		
		UnityEngine.Object[] unfilteredSelection = GetUnfilteredSelection();

		for(int i=0; i<unfilteredSelection.Length; i++) {
			UnityEngine.Object obj = unfilteredSelection[i];
			
			if(AssetDatabase.IsMainAsset(obj)) {
				string assetPath = AssetDatabase.GetAssetPath(obj);
				AssetImporter importer = AssetImporter.GetAtPath(assetPath);
				
				PropertyInfo property = importer.GetType().GetProperty(propertyName);
				if(property == null) { // No such property found
					Debug.Log(propertyName + " is not a property of " + importer.GetType().Name);
					continue;
				}
				
				MethodInfo setMethod = property.GetSetMethod();
				if(setMethod == null) { //No setter found
					Debug.Log("No setter found for property " + propertyName + " of " + importer.name);
					continue;
				}
				
				ParameterInfo[] methodParameters = setMethod.GetParameters();
				if(methodParameters == null || methodParameters.Length == 0) {
					Debug.LogError("Method " + setMethod.Name + " for " + importer.name + " takes no parameters");
					continue;
				}
				Type parameterType = methodParameters[0].ParameterType;
				
				object[] valueArray = new object[1]; //Array to place the parsed value in
				if(!ParseValueToParameter(valueArray, parameterType, value)) //Parse value
					continue; //value couldn't be parsed
				
				setMethod.Invoke(importer, valueArray); //Invoke the setter method on the AssetImporter
				AssetDatabase.ImportAsset(assetPath);
				
				//Hack for the following issue:
				//When you set a value of the importer, the Editor that's showing that importer in the Inspector still has
				//the old value set in its GUI. Then Editor asks you whether you want to Revert or Apply the change because
				//it thinks the old value set in the GUI is a new value the user entered by hand after the recent re-import
				//done through code by setting the value of the importer.
				if(!editorFieldSet) {
					foreach(Editor e in ActiveEditorTracker.sharedTracker.activeEditors) {
						FieldInfo field = e.GetType().GetField(propertyName, fullBinding);
						if(field != null) {
							field.SetValue(e, valueArray[0]);
							editorFieldSet = true;
							e.Repaint();
						}
					}
				}
			} else {
				Debug.Log(obj.name + " is not a main asset");
			}
		}
		AssetDatabase.Refresh();
	}
	
	public void Select(string[] parameters) {
		if(parameters[0].Equals("asset", ignoreCase) && parameters.Length > 1) {
			string[] constrainedParameters = new string[parameters.Length - 1];
			Array.Copy(parameters, 1, constrainedParameters, 0, constrainedParameters.Length);
			SelectAsset(constrainedParameters);
			return;
		}
		//Initially assume that we're selecting by name
//		string selectBy = "name";
//		string name = parameters[0];
//		if(parameters.Length > 1) { //If we have more than 1 parameter, take first parameter as selectBy and second parameter as name
//			selectBy = parameters[0];
//			name = parameters[1];
//		}
		
		
	}
	
	public void SelectAsset(string[] parameters) {
		//naganna work
		GameObject[] objs = (GameObject[]) FindObjectsOfType(typeof(GameObject));
		StringBuilder sb = new StringBuilder();
		foreach(GameObject obj in objs) {
			sb.AppendLine(obj.name);
		}
		Debug.Log(sb.ToString());
	}
	
	public void Toggle(string[] parameters) {
		string componentName = GetCapitalizedName(parameters[0]);
		string propertyName = parameters[1];
		
		if(componentName.Equals("asset", ignoreCase)) { //Toggling properties for assets
			ToggleAsset(propertyName); //Set asset
			return;
		}
		
		Component compForType = null;
		foreach(GameObject goForType in Selection.gameObjects) {
			compForType = goForType.GetComponent(componentName);
			if(compForType != null)
				break;
		}
		if(compForType == null) {
			Debug.LogError("None of the selected gameObjects have a " + componentName + " component.");
			return;
		}
		
		PropertyInfo property = compForType.GetType().GetProperty(propertyName);
		if(property == null) { // No such property found
			Debug.Log("Property " + propertyName + " doesn't exist for " + componentName);
			return;
		}
		if(property.GetType() != typeof(System.Boolean)) {
			Debug.Log("Toggle command only works for properties of type Boolean.");
			return;
		}
		
		MethodInfo setMethod = property.GetSetMethod();
		if(setMethod == null) { //No setter found
			Debug.Log("Setter doesn't exist for " + propertyName + " property of " + componentName + ", using SerializedProperty check instead.");
			ToggleUsingSerializedProperty(propertyName, componentName); //Toggle using serialized property
			return;
		}
		
		MethodInfo getMethod = property.GetGetMethod();
		if(getMethod == null) { //No getter found
			Debug.Log("Getter doesn't exist for " + propertyName + " property of " + componentName);
			return;
		}
		
		object[] valueArray = new object[1]; //Array to place the parsed value in
		
		foreach(GameObject go in Selection.gameObjects) { //For each transform selected
			Component comp = go.GetComponent(componentName); //Get component
			if(comp == null) continue; //If component wasn't found on this object, move to the next one
			valueArray[0] = !((bool)getMethod.Invoke(comp, null));
			setMethod.Invoke(comp, valueArray); //Invoke the setter method on the component
			EditorUtility.SetDirty(comp);
		}
	}
	
	void ToggleUsingSerializedProperty(string propertyName, string componentName) {
		foreach(GameObject go in Selection.gameObjects) {
			SerializedObject componentObj = new SerializedObject(go.GetComponent(componentName));
			SerializedProperty property = componentObj.FindProperty(propertyName);
			if(property == null) {
				Debug.Log(propertyName + " is not a property of " + componentName);
				break;
			}
			SerializedPropertyType propertyType = property.propertyType;
			if(propertyType == SerializedPropertyType.Boolean) {
				property.boolValue = !property.boolValue;
			} else {
				Debug.LogError(propertyType.ToString() + " is not a boolean.");
			}
			componentObj.ApplyModifiedProperties();
		}
	}
	
	void ToggleAsset(string propertyName) {
		bool editorFieldSet = false;
		
		AssetDatabase.StartAssetEditing();
		foreach(UnityEngine.Object obj in GetUnfilteredSelection()) {
			if(AssetDatabase.IsMainAsset(obj)) {
				string assetPath = AssetDatabase.GetAssetPath(obj);
				AssetImporter importer = AssetImporter.GetAtPath(assetPath);
				
				PropertyInfo property = importer.GetType().GetProperty(propertyName);
				if(property == null) // No such property found
					continue;
				
				MethodInfo setMethod = property.GetSetMethod();
				if(setMethod == null) { //No setter found
					Debug.Log("No setter found for property " + propertyName + " of " + importer.name);
					continue;
				}
				
				MethodInfo getMethod = property.GetGetMethod();
				if(getMethod == null) { //No getter found
					Debug.Log("No getter found for property " + propertyName + " of " + importer.name);
					return;
				}
				
				object[] valueArray = new object[] {!((bool)getMethod.Invoke(importer, null))};
				
				setMethod.Invoke(importer, valueArray); //Invoke the setter method on the AssetImporter
				AssetDatabase.ImportAsset(assetPath);
				
				//Hack for the following issue:
				//When you set a value of the importer, the Editor that's showing that importer in the Inspector still has
				//the old value set in its GUI. Then Editor asks you whether you want to Revert or Apply the change because
				//it thinks the old value set in the GUI is a new value the user entered by hand after the recent re-import
				//done through code by setting the value of the importer.
				if(!editorFieldSet) {
					foreach(Editor e in ActiveEditorTracker.sharedTracker.activeEditors) {
						FieldInfo field = e.GetType().GetField(propertyName, fullBinding);
						if(field != null) {
							field.SetValue(e, valueArray[0]);
							editorFieldSet = true;
							e.Repaint();
						}
					}
				}
			} else {
				Debug.Log(obj.name + " is not a main asset");
			}
		}
		AssetDatabase.StopAssetEditing();
	}
	
	public void Find(string[] parameters) {
		string toFind = parameters[0];
		string toFindAlternate = ToggleFirstLetterCapitalization(toFind);
		string findIn = parameters[1];
		Assembly assembly = GetAssembly(findIn);
		if(assembly == null) {
			Debug.Log("Assembly " + findIn + " not found in current domain.");
		}
		StringBuilder sb = new StringBuilder();
		foreach(Type type in assembly.GetTypes()) {
			//Get type hierarchy
			StringBuilder typeHierarchySB = new StringBuilder();
			
			Type fullBaseType = type;
			typeHierarchySB.Append(fullBaseType.ToString() + "->");
			for(int i=0; i<10; i++) {
				fullBaseType = fullBaseType.BaseType;
				if(fullBaseType == null)
					break;
				typeHierarchySB.Append(fullBaseType.ToString());
				if(fullBaseType == typeof(System.Object))
					break;
				else
					typeHierarchySB.Append("->");
			}
			string typeHierarchy = typeHierarchySB.ToString();
			if(typeHierarchy.Contains(toFind) || typeHierarchy.Contains(toFindAlternate))
				sb.AppendLine("		Found " + toFind + " as type of " + type.Name);
			
			FieldInfo[] fields = type.GetFields(fullBinding);
			foreach(FieldInfo field in fields) {
				if(field.ToString().Contains(toFind) || field.ToString().Contains(toFindAlternate)) {
					sb.AppendLine("Found " + toFind + " as field " + field.Name + " with reflected type " + field.ToString() + " in type " + type.Name);
				}
			}
			PropertyInfo[] properties = type.GetProperties(fullBinding);
			foreach(PropertyInfo property in properties) {
				if(property.ToString().Contains(toFind) || property.ToString().Contains(toFindAlternate)) {
					sb.AppendLine("Found " + toFind + " as property " + property.Name + " with reflected type " + property.ToString() + " in type " + type.Name);
				}
			}
			MethodInfo[] methods = type.GetMethods(fullBinding);
			foreach(MethodInfo method in methods) {
				if(method.ToString().Contains(toFind) || method.ToString().Contains(toFindAlternate)) {
					sb.AppendLine("Found " + toFind + " as method " + method.Name + " with reflected type " + method.ToString() + " in type " + type.Name);
				}
			}
		}
		Debug.Log(sb.ToString());
	}
	
	public void Print(string[] parameters) {
		string toFind = parameters[0];
		if(toFind.Equals(objectReferenceString, ignoreCase)) {
			if(objectReference == null) {
				Debug.Log("Object reference is null. Nothing to print.");
			} else {
				//Handle known types
				if(objectReference.GetType() == typeof(Vector3)) {
					Vector3 vec = (Vector3) objectReference;
					Debug.Log('(' + vec.x.ToString() + ',' + vec.y.ToString() + ',' + vec.z.ToString() + ')');
				} else {
					Debug.Log(objectReference.ToString());
				}
			}
		}
	}
	
	public static Type GetTypeFromAssembly(string typeName, Assembly assembly) {
		if(assembly == null)
			return null;
		Type[] types = assembly.GetTypes();
		foreach(Type type in types) {
			if(type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
				return type;
		}
		return null;
	}
	
	public static Type GetTypeFromAllAssemblies(string typeName) {
		return GetTypeFromAllAssemblies(typeName, false);
	}
	
	public static Type GetTypeFromAllAssemblies(string typeName, bool exactNameMatch) {
		Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach(Assembly assembly in assemblies) {
			Type[] types = assembly.GetTypes();
			foreach(Type type in types) {
				if(exactNameMatch && type.FullName == typeName)
					return type;
				
				if(type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
					return type;
			}
		}
		return null;
	}
	
	public static Assembly GetAssembly(string assemblyName) {
		Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach(Assembly assembly in assemblies) {
			if(assembly.GetName().Name.Equals(assemblyName, ignoreCase))
				return assembly;
		}
		return null;
	}
	
	public static bool ParseValueToParameter(object[] valueArray, Type parameterType, string value) {
		if(parameterType == typeof(String)) {
			valueArray[0] = value;
			return true;
		} else if(parameterType.IsValueType) {
			if(parameterType.IsEnum) { //Enum
				string[] names = Enum.GetNames(parameterType);
				bool nameFound = false;
				foreach(string name in names) {
					if(name.Equals(value, ignoreCase)) {
						nameFound = true;
						break;
					}
				}
				if(nameFound) {
					valueArray[0] = Enum.Parse(parameterType, value, true);
					return true;
				} else {
					return false;
				}
			} else if(IsStruct(parameterType)) { //Struct, hand-parse frequently-used types
				if(parameterType == typeof(Vector2) || parameterType == typeof(Vector3) || parameterType == typeof(Vector4)) { //Vectors
					object vector = ParseVector(value);
					if(vector == null)
						return false;
					if(parameterType != vector.GetType()) {
						Debug.Log("Looking for " + parameterType.ToString() + " but supplied " + vector.GetType().ToString());
						return false;
					}
					valueArray[0] = vector;
					return true;
				} else if(parameterType == typeof(Color)) {
					valueArray[0] = ParseColor(value);
					return true;
				} else { //There are a bajillion struct types, we can't parse all of them :O
					return false;
				}
			} else { //All other value types
				//Get the Parse method for the value type and invoke it
				MethodInfo parseMethod = parameterType.GetMethod("Parse", new Type[] {typeof(String)});
				if(parseMethod == null) {
					Debug.LogError("Parameter type " + parameterType.ToString() + " does not have a Parse method.");
					return false;
				}
				valueArray[0] = parseMethod.Invoke(null, new object[] {value});
				return true;
			}
		} else { //Not a value type so all we can do now is check for a Parse method and use it if it exists
			MethodInfo parseMethod = parameterType.GetMethod("Parse", new Type[] {typeof(String)});
			if(parseMethod == null) {
				Debug.LogError("Parameter type " + parameterType.ToString() + " does not have a Parse method.");
				return false;
			}
			valueArray[0] = parseMethod.Invoke(null, new object[] {value});
			return true;
		}
	}
	
	public static string GetAlternatePropertyName(string varName) {
		char[] varNameCharArray = varName.ToCharArray();
		varNameCharArray[0] = Char.ToUpper(varNameCharArray[0]);
		varName = new string(varNameCharArray);
		varName = varName.Insert(0, "m_");
		return varName;
	}
	
	public static string GetCapitalizedName(string name) {
		char[] nameCharArray = name.ToCharArray();
		nameCharArray[0] = Char.ToUpper(nameCharArray[0]);
		return new string(nameCharArray);
	}
	
	public static string ToggleFirstLetterCapitalization(string name) {
		char[] nameCharArray = name.ToCharArray();
		if(Char.IsUpper(nameCharArray[0])) {
			nameCharArray[0] = Char.ToLower(nameCharArray[0]);
		} else {
			nameCharArray[0] = Char.ToUpper(nameCharArray[0]);
		}
		return new string(nameCharArray);
	}
	
	AutomatorCommand FindAutomatorCommand(string commandName) {
		if(automatorCommands == null)
			LoadAutomatorCommands();
		
		foreach(AutomatorCommand automatorCommand in automatorCommands) {
			if(automatorCommand.command.Equals(commandName, ignoreCase))
				return automatorCommand;
		}
		return null;
	}
	
	public static UnityEngine.Object[] GetUnfilteredSelection() {
		return Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Unfiltered);
	}
	
	public static bool IsStruct(Type type) {
		return type.IsValueType && !type.IsPrimitive && type != typeof(System.Decimal);
	}
	
	public static object ParseVector(string value) {
		if(value[0].Equals('(') && value[value.Length-1].Equals(')')) {
			string[] commaDelimitedValues = value.Substring(1, value.Length-2).Replace(" ", "").Split(new char[] {','});
			if(commaDelimitedValues.Length < 2 || commaDelimitedValues.Length > 4) {
				Debug.Log("There needs to be 2,3, or 4 comma delimited values inside the parens for " + value + " to be parsed into a Vector.");
				return null;
			} else {
				if(commaDelimitedValues.Length == 2) { //Vector2
					return new Vector2(Single.Parse(commaDelimitedValues[0]), Single.Parse(commaDelimitedValues[1]));
				} else if(commaDelimitedValues.Length == 3) { //Vector3
					return new Vector3(Single.Parse(commaDelimitedValues[0]), Single.Parse(commaDelimitedValues[1]),
										Single.Parse(commaDelimitedValues[2]));	
				} else { //Vector4
					return new Vector4(Single.Parse(commaDelimitedValues[0]), Single.Parse(commaDelimitedValues[1]),
										Single.Parse(commaDelimitedValues[2]), Single.Parse(commaDelimitedValues[3]));
				}
			}
			
		} else {
			Debug.Log(value + " cannot be parsed into a vector. It needs to start with an open-paren and end with a close-paren.");
			return null;
		}
	}
	
	public static Color ParseColor(string value) {
		if(value[0].Equals('(') && value[value.Length-1].Equals(')')) {
			string[] commaDelimitedValues = value.Substring(1, value.Length-2).Replace(" ", "").Split(new char[] {','});
			if(commaDelimitedValues.Length < 3 || commaDelimitedValues.Length > 4) {
				Debug.Log("There needs to be 3 or 4 comma delimited values inside the parens for " + value + " to be parsed into a Color. Returning White.");
				return Color.white;
			} else {
				if(commaDelimitedValues.Length == 3) { //Color3, assume 1 alpha
					return new Color(Single.Parse(commaDelimitedValues[0]), Single.Parse(commaDelimitedValues[1]),
										Single.Parse(commaDelimitedValues[2]), 1f);	
				} else { //Color 4
					return new Color(Single.Parse(commaDelimitedValues[0]), Single.Parse(commaDelimitedValues[1]),
										Single.Parse(commaDelimitedValues[2]), Single.Parse(commaDelimitedValues[3]));
				}
			}
			
		} else {
			Debug.Log(value + " cannot be parsed into a color. It needs to start with an open-paren and end with a close-paren. Returning White.");
			return Color.white;
		}
	}
	
	void OnFocus() {
		gainedFocus = true;
	}
	
	void OnLostFocus() {
		gainedFocus = false;
	}
	
	//Hack for the following issue:
	//When the string of an EditorGUI Textfield is changed through code and not through user-input, the Textfield
	//shows the old string unless its focus is taken away. That's why we focus on a hidden field, and then set
	//gainedFocus to true to set focus back to the commandField
	void ForceRepaint() {
		GUI.FocusControl("hiddenField");
		Repaint();
		gainedFocus = true;
	}
}
