//#define REPORT_REFERENCING_GAMEOBJECTS
#define REPORT_RECT_PVRTC_TEXTURES

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class FindTexturesInUse : MonoBehaviour {
	
	class TextureInfo {
		public Texture2D texture;
		public List<GameObject> referencingGOs;
		public int memorySize;
		
		public TextureInfo(Texture2D texture, GameObject referencingGO) {
			this.texture = texture;
			CalculateMemorySize();
			referencingGOs = new List<GameObject>();
			referencingGOs.Add(referencingGO);
		}
		
		void CalculateMemorySize() {
			int bpp = 0;
			if(texture.format == TextureFormat.Alpha8) {
				bpp = 8;
			} else if(texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.RGBA32) {
				bpp = 32;
			} else if(texture.format == TextureFormat.RGB24) {
				bpp = 24;
			} else if(texture.format == TextureFormat.PVRTC_RGBA4) {
				bpp = 4;
			} else if(texture.format == TextureFormat.PVRTC_RGB4) {
				bpp = 4;
			} else if(texture.format == TextureFormat.ARGB4444) {
				bpp = 16;
			}
			
			memorySize = bpp * texture.width * texture.height;
			
			if(texture.mipmapCount != 1) {
				memorySize += memorySize / 2;
			}
			
			//Finally, convert to bytes from bits
			memorySize /= (8);
			
			if(memorySize == 0) {
				Debug.LogError("Calculated Memory size for texture is zero! " + texture.name + " bpp:" + bpp.ToString() + " width:" + texture.width.ToString() + " height:" + texture.height.ToString(), texture);
			}
		}
		
		public override string ToString ()
		{
			return texture.name + " memorySize: " + string.Format("{0:N0}", memorySize) + " bytes";
		}
	}
	
	[MenuItem("Window/Find Textures In Use")]
	public static void FindTextures2() {
		List<TextureInfo> textures = new List<TextureInfo>();
		Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep);
		foreach(GameObject go in selection) {
			Renderer r = go.renderer;
			if(r != null) {
				Material material = r.sharedMaterial;
				if(material != null) {
					if(material.mainTexture != null) {
						TextureInfo textureInfo = textures.Find(x => x.texture == material.mainTexture);
						if(textureInfo == null) {
							textures.Add(new TextureInfo(material.mainTexture as Texture2D, r.gameObject));
						} else {
							textureInfo.referencingGOs.Add(r.gameObject);
						}
					}
				}
			}
		}
		
		ReportTexturesAsAssets(textures);
	}
	
	static void ReportTexturesAsAssets(List<TextureInfo> textures) {
		//Sort assets by size
		textures.Sort((x,y) => y.memorySize - x.memorySize);
		int totalTextureMemory = 0;
		//Try to find these textures as assets
		foreach(TextureInfo textureInfo in textures) {
#if REPORT_RECT_PVRTC_TEXTURES
			if(textureInfo.texture.width != textureInfo.texture.height && textureInfo.texture.format.ToString().Contains("PVRTC")) {
				Debug.LogError("Compressed Rect Texture!", textureInfo.texture);
			}
#endif
			totalTextureMemory += textureInfo.memorySize;
			string assetPath = AssetDatabase.GetAssetPath(textureInfo.texture);
			if(assetPath != null) {
				Debug.Log("Found texture: " + textureInfo.ToString(), textureInfo.texture);
#if REPORT_REFERENCING_GAMEOBJECTS
				foreach(GameObject go in textureInfo.referencingGOs) {
					Debug.Log("Texture: " + textureInfo.texture.name + " used by GO: " + go.name, go);
				}
#endif
			} else {
				Debug.Log("Found texture: " + textureInfo.texture.name + " but no matching asset path.");
			}
		}
		
		Debug.Log("Total texture memory size in bytes: " + string.Format("{0:N0}", totalTextureMemory));
	}
}
