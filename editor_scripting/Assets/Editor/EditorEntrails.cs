using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

//TODO
//Add live-update by uncommenting ResolveName in ObjectInfo.Name
//OR
//Smartly check for changes in children of visible and expanded ObjectInfo items
//Draw only the visible items in the GUI.ScrollView

public class EditorEntrails : EditorWindow {
	
	const int indentation = 10;
	
	string newTarget = string.Empty;
	
	public class ObjectInfo {
		public System.Type type;
		public System.Object obj;
		public string fieldName;
		
		string name;
		
		public string Name {
			get {
				//ResolveName();
				return name;
			}
		}
		
		ObjectInfo[] children;
		bool childrenResolved = false;
		public ObjectInfo[] Children {
			get {
				if(children == null && !childrenResolved) {
					ResolveChildren();
				}
				return children;
			}
		}
		
		public ObjectInfo parent;
		public ObjectInfo next;
		
		public bool HasChildren {
			get {
				return Children != null && Children.Length > 0;
			}
		}
		
		bool m_isExpanded = false;
		public bool IsExpanded {
			get{ return m_isExpanded;}
			set{
				m_isExpanded = value;
			}
		}
		
		public ObjectInfo(System.Object obj, ObjectInfo parent, string fieldName) {
			this.obj = obj;
			this.parent = parent;
			this.fieldName = fieldName;
			if(this.obj != null) {
				this.type = this.obj.GetType();
			} else {
				//Debug.LogError("Null object passed to ObjectInfo constructor with parent" + (this.parent != null ? this.parent.ToString() : "null"));
			}
			
			ResolveName();
		}
		
		public void ResolveName() {
			if(obj == null) {
				name = fieldName + ": null";
			} else {
				name = fieldName + ": " + obj.ToString();
				if(type.GetField("name", Automator.fullBinding) != null) {
					name += "(name: " + type.GetField("name", Automator.fullBinding).GetValue(obj);
				}
			}
		}
		
		public void ResolveChildren() {
			childrenResolved = true;
			
			if(obj == null || type.IsValueType || type == typeof(string)) {
				//Debug.Log(name + " is value type");
				return;
			}	
			
			if(type.GetInterface("IEnumerable") != null) {
				//Debug.Log(name + " is IEnumerable");
				IEnumerable enumerable = obj as IEnumerable;
				if(enumerable == null) {
					Debug.Log("Could not cast " + name + " to IEnumerable");
					return;
				}
				IEnumerator enumerator = enumerable.GetEnumerator();
				if(enumerator == null) {
					Debug.Log("Could not get enumerator for " + name);
					return;
				}
				
				List<ObjectInfo> childrenList = new List<ObjectInfo>();
				
				int i=0;
				while(enumerator.MoveNext()) {
					System.Object current = enumerator.Current;
					childrenList.Add(new ObjectInfo(current, this, '[' + i++.ToString() + ']'));
				}
				
				children = childrenList.ToArray();
			} else {
				List<MemberInfo> membersList = new List<MemberInfo>(type.GetMembers(Automator.fullBinding));
				
				//Select only fields and properties
				List<ObjectInfo> childrenList = new List<ObjectInfo>();
				
				foreach(MemberInfo member in membersList) {
					if(member.MemberType == MemberTypes.Field) {
						FieldInfo field = type.GetField(member.Name, Automator.fullBinding);
						System.Object fieldObject = field.GetValue(obj);
						if(fieldObject != null) {
							childrenList.Add(new ObjectInfo(fieldObject, this, member.Name));
						}
					} else if(member.MemberType == MemberTypes.Property) {
						PropertyInfo property = type.GetProperty(member.Name, Automator.fullBinding);
						System.Object propertyObject = property.GetValue(obj, null);
						if(propertyObject != null) {
							childrenList.Add(new ObjectInfo(propertyObject, this, member.Name));
						}
					}
				}
				
				children = childrenList.ToArray();
			}
			
			//Link the children together
			if(children != null) {
				for(int i = children.Length - 1; i>-1; i--) {
					if(i != children.Length - 1) {
						children[i].next = children[i+1];
					}
				}
			}
		}
	}
	
	ObjectInfo parentObject;
	
	System.Type containerWindowType;
	
	Vector2 scrollPosition;
	
	[MenuItem("Window/Editor Entrails")]
	public static void ShowWindow() {
		EditorWindow.GetWindow<EditorEntrails>();
	}
	
	void OnEnable() {
		Refresh();
	}

	void OnGUI() {
		ObjectInfo currentObject = parentObject;
		List<ObjectInfo> childrenVisited = new List<ObjectInfo>(currentObject.HasChildren ? currentObject.Children.Length : 0);
		int currentIndentation = 0;
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		while(currentObject != null) {
			
			bool alreadyVisited = childrenVisited.Contains(currentObject);
			
			if(!alreadyVisited) {
				GUILayout.BeginHorizontal();
					GUILayout.Space(currentIndentation);
					if(currentObject.HasChildren) {
						currentObject.IsExpanded = EditorGUILayout.Foldout(currentObject.IsExpanded, currentObject.Name);
					} else {
						GUILayout.Label(currentObject.Name);
					}
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			
			if(currentObject.IsExpanded && currentObject.Children != null && currentObject.Children.Length > 0 && !alreadyVisited) {
				childrenVisited.Add(currentObject);
				currentIndentation += indentation;
				currentObject = currentObject.Children[0];
			} else if(currentObject.next != null) {
				currentObject = currentObject.next;
			} else {
				currentIndentation -= indentation;
				currentObject = currentObject.parent;
			}
		}
		
		GUILayout.EndScrollView();
		
		GUILayout.Space(5);
		
		GUILayout.BeginHorizontal();
			newTarget = GUILayout.TextField(newTarget);
			if(GUILayout.Button("Obtain Target")) {
				ObtainTarget(newTarget);
			}
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("Refresh")) {
			Refresh();
		}
		
	}
	
	void ObtainTarget(string targetString) {
		int dotIndex = targetString.IndexOf('.');
		if(dotIndex == -1) {
			Debug.Log("Could not find period in the target string");
			return;
		}
		string className = targetString.Substring(0, dotIndex);
		string memberName = targetString.Substring(dotIndex + 1, targetString.Length - (dotIndex + 1));
		System.Type classType = Automator.GetTypeFromAllAssemblies(className);
		if(classType == null) {
			Debug.Log("Could not find type " + className);
			return;
		}
		
		MemberInfo memberInfo = classType.GetMember(memberName, Automator.fullBinding)[0];
		if(memberInfo == null) {
			Debug.Log("Could not find member " + memberName + " of type " + className);
			return;
		}
		
		if(memberInfo.MemberType == MemberTypes.Field) {
			System.Object memberObject = classType.GetField(memberName, Automator.fullBinding).GetValue(null);
			if(memberObject == null) {
				Debug.Log("Could not get value for field " + memberName);
				return;
			}
			
			parentObject = new ObjectInfo(memberObject, null, memberName);
		} else if(memberInfo.MemberType == MemberTypes.Property) {
			System.Object memberObject = classType.GetProperty(memberName, Automator.fullBinding).GetValue(null, null);
			if(memberObject == null) {
				Debug.Log("Could not get value for field " + memberName);
				return;
			}
			
			parentObject = new ObjectInfo(memberObject, null, memberName);
		}
	}
	
	void Refresh() {
		parentObject = new ObjectInfo(this, null, "EditorEntrails");
	}
}
