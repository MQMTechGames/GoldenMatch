using UnityEngine;
using UnityEditor;
using System.Collections;

public class ModelAssetPostProcessor : AssetPostprocessor {
	
	private char delimiter = '_';
	private string colliderPrefix = "_col";
	private string rigidbodyPrefix = "_rb";
	private string nomeshPrefix = "_nomesh";
	
	public static int processPriority = 1;
	
	public override int GetPostprocessOrder() {
		return processPriority;
	}
	
	void OnPostprocessModel(GameObject go) {
		Process(go.transform);
	}
	
	void Process(Transform t) {
		string name = t.name;
		if(name.Contains(colliderPrefix)) {
			int startingIdx = name.IndexOf(colliderPrefix);
			int nextDelimiterIdx = name.IndexOf(delimiter, startingIdx + 1);
			if(nextDelimiterIdx == -1) nextDelimiterIdx = name.Length - 1;
			int colliderTypeStartIndex = startingIdx + colliderPrefix.Length;
			string colliderType = name.Substring(colliderTypeStartIndex, nextDelimiterIdx - colliderTypeStartIndex);
			AddCollider(t, colliderType);
		}
		if(name.Contains(rigidbodyPrefix)) {
			AddRigidbody(t);
		}
		if(name.Contains(nomeshPrefix)) {
			RemoveMeshComponents(t);
		}
		
		foreach(Transform child in t)
			Process(child);
	}
	
	void AddCollider(Transform t, string colliderType) {
		//Remove existing collider if there is one
		Collider c = t.collider;
		if(c != null) Object.DestroyImmediate(c);
		//Get mesh bounds if a mesh filter exists
		Vector3 size = new Vector3(1,1,1);
		MeshFilter mf = t.GetComponent<MeshFilter>();
		if(mf != null) size = mf.sharedMesh.bounds.size;
		if(colliderType.Equals("Box")) {
			BoxCollider bc = t.gameObject.AddComponent<BoxCollider>();
			bc.size = size;
		} else if(colliderType.Equals("Sphere")) {
			SphereCollider sc = t.gameObject.AddComponent<SphereCollider>();
			sc.radius = size.z / 2;
		} else if(colliderType.Equals("Capsule")) {
			CapsuleCollider cc = t.gameObject.AddComponent<CapsuleCollider>();
			cc.height = size.y;
			cc.radius = size.z / 2;
		} else if(colliderType.Equals("Mesh")) {
			MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
			if(mf != null) mc.sharedMesh = mf.sharedMesh;
		} else if(colliderType.Equals("MeshConvex")) {
			MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
			if(mf != null) mc.sharedMesh = mf.sharedMesh;
			mc.convex = true;
		} else {
			Debug.LogError(colliderType + " is not a valid collider type.");
		}
	}
	
	void AddRigidbody(Transform t) {
		t.gameObject.AddComponent<Rigidbody>();
	}
	
	void RemoveMeshComponents(Transform t) {
		MeshFilter mf = t.GetComponent<MeshFilter>();
		MeshRenderer mr = t.GetComponent<MeshRenderer>();
		if(mf != null) Object.DestroyImmediate(mf);
		if(mr != null) Object.DestroyImmediate(mr);
	}
}
