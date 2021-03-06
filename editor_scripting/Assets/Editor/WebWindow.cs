using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class WebWindow : EditorWindow {
	
	static Rect windowRect = new Rect(100,100,800,600);
	static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
	
	object webView;
	Type webViewType;
	MethodInfo doGUIMethod;
	MethodInfo loadURLMethod;
	MethodInfo focusMethod;
	MethodInfo unFocusMethod;
	
	Vector2 resizeStartPos;
	Rect resizeStartWindowSize;
	//MethodInfo dockedGetterMethod;
	
	//string urlText = "file:///Applications/Unity%203.3/Documentation/ScriptReference/index.html";
	string urlText = "http://www.duckduckgo.com";
	

	[MenuItem ("Window/Web Window %#w")]
    public static void Load() {
        WebWindow window = WebWindow.GetWindow<WebWindow>();
		//window.Show();
		window.Init();
    }
	
	void Init() {
		//Set window rect
		this.position = windowRect;
		//Get WebView type
		webViewType = GetTypeFromAllAssemblies("WebView");
		//Init web view
		InitWebView();
		//Get docked property getter MethodInfo
		//dockedGetterMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
	}
	
	//Mostly copies what Asset Store window does, plus caching some MethodInfos
	private void InitWebView() {
		//Create an instance of web view
		webView = ScriptableObject.CreateInstance(webViewType);
		//Init web view with width and height
		MethodInfo initMethod = webViewType.GetMethod("InitWebView");
		initMethod.Invoke(webView, new object[] {(int)position.width,(int)position.height,false});
		//Set web view hide flags
		webViewType.GetMethod("set_hideFlags").Invoke(webView, new object[] {13});
		
		//Get LoadURL method
		loadURLMethod = webViewType.GetMethod("LoadURL");
		//Call LoadURL on webView with urlText
		loadURLMethod.Invoke(webView, new object[] {urlText});
		//Set this WebWindow to be the delegate object for webView
		webViewType.GetMethod("SetDelegateObject").Invoke(webView, new object[] {this});
		
		//Get DoGUI method
		doGUIMethod = webViewType.GetMethod("DoGUI");
		//Get Focus method
		focusMethod = webViewType.GetMethod("Focus");
		//Get UnFocus method
		unFocusMethod = webViewType.GetMethod("UnFocus");
		
		this.wantsMouseMove = true;
	}
	
	void OnGUI() {
		if(GUI.GetNameOfFocusedControl().Equals("urlfield"))
			unFocusMethod.Invoke(webView, null);
		
		//bool isDocked = (bool)(dockedGetterMethod.Invoke(this, null));
		//Rect webViewRect = new Rect(0,20,position.width,position.height - ((isDocked) ? 20 : 40));
		Rect webViewRect = new Rect(0,20,position.width,position.height - 40);
		if(Event.current.isMouse && Event.current.type == EventType.MouseDown && webViewRect.Contains(Event.current.mousePosition)) {
			GUI.FocusControl("hidden");
			focusMethod.Invoke(webView, null);
		}
		
		//Hidden, disabled button for taking focus away from urlfield
		GUI.enabled = false;
		GUI.SetNextControlName("hidden");
		GUI.Button(new Rect(-20,-20,5,5), string.Empty);
		GUI.enabled = true;
		
		//URL Label
		GUI.Label(new Rect(0,0,30,20), "URL:");
		
		//URL text field
		GUI.SetNextControlName("urlfield"); 
		urlText = GUI.TextField(new Rect(30,0, position.width-30, 20), urlText);
		
		//Focus on web view if return is pressed in URL field
		if(Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl().Equals("urlfield")) {
			loadURLMethod.Invoke(webView, new object[] {urlText});
			GUI.FocusControl("hidden");
			focusMethod.Invoke(webView, null);
		}
		
		//Web view
		if(webView != null)
			doGUIMethod.Invoke(webView, new object[] {webViewRect});
	}
	
	private void OnWebViewDirty() {
		this.Repaint();
	}
	
	public static Type GetTypeFromAllAssemblies(string typeName) {
		Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach(Assembly assembly in assemblies) {
			Type[] types = assembly.GetTypes();
			foreach(Type type in types) {
				if(type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
					return type;
			}
		}
		return null;
	}
	
	void OnDestroy() {
		//Destroy web view
		webViewType.GetMethod("DestroyWebView", fullBinding).Invoke(webView, null);
	}
}
