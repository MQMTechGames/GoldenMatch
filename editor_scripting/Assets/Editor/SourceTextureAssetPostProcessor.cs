using UnityEngine;
using UnityEditor;
using System.Collections;

public class SourceTextureAssetPostProcessor : AssetPostprocessor {
	
	const string targetFolder = "Assets/SourceTextures/";
	
	public static int processPriority = 2;
	
	public override int GetPostprocessOrder() {
		return processPriority;
	}
	
	void OnPreprocessTexture() {
		if(assetPath.StartsWith(targetFolder)) {
			TextureImporter textureImporter = assetImporter as TextureImporter;
			
			TextureImporterSettings settings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(settings);
			
			settings.ApplyTextureType(TextureImporterType.GUI, true);
			settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			
			textureImporter.SetTextureSettings(settings);
			
			textureImporter.textureType = TextureImporterType.GUI;
			textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		}
	}
}
