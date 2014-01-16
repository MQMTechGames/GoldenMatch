using UnityEngine;
using UnityEditor;
using System.Collections;

public class iOSEmulationToggle : EditorWindow {
	
	[MenuItem ("Window/iOS Emulation Toggle Window")]
	public static void Init() {
		iOSEmulationToggle myWindow = EditorWindow.GetWindow<iOSEmulationToggle>();
		myWindow.minSize = new Vector2(160, 20);
	}
	
	bool m_iOSEmulation;
	
	bool syncedWithEditorPref = false;
	
	bool iOSEmulation {
		get {
			if(!syncedWithEditorPref) {
				m_iOSEmulation = EditorPrefs.GetBool("iOSEmulation");
				syncedWithEditorPref = true;
			}
			return m_iOSEmulation;
		}
		set {
			m_iOSEmulation = value;
			EditorPrefs.SetBool("iOSEmulation", m_iOSEmulation);
		}
	}
	
	void OnDisable() {
		syncedWithEditorPref = false;
	}
	
	void OnGUI() {
		bool temp_iOSEmulation = EditorGUILayout.Toggle("Emulate iOS", iOSEmulation);
		if(GUI.changed) {
			iOSEmulation = temp_iOSEmulation;
		}
	}
}
