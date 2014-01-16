using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class FindCompressedTextures : ScriptableObject {

	[MenuItem("Window/Find Compressed Textures")]
	public static void ReportAllCompressedTextures() {
		List<string> allAssetPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
		allAssetPaths.ForEach(
			delegate(string assetPath){
				UnityEngine.Object texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
				if(texture != null) {
					if((texture as Texture2D).format.ToString().Contains("PVRTC")) {
						Debug.Log("PVR texture: " + texture.name, texture);
					}
				}
			}		
		);
	}
}
