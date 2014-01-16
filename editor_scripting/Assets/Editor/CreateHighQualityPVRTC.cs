using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CreateHighQualityPVRTC {
	
	const string Mac_PVRToolFolderPath = "/Users/Yilmaz/Desktop/PVRTC/PVRTexTool/PVRTexToolCL/MacOS_x86/";
	const string Win_PVRToolFolderPath = @"C:\Users\Vox\Desktop\Utilities\PVRTexTool\PVRTexToolCL\Windows_x86_64\";
	const string Mac_PVRToolExecutableName = "PVRTexTool";
	const string Win_PVRToolExecutableName = "PVRTexTool.exe";
	const string PVRTextureSuffix = "_PVR";
	const string PVRTextureExtension = ".pvr";
	
	const string processArgumentsString = "-fPVRTC4 -pvrtcbest -yflip 1 {0} -i{1}";
	// PVRTexTool 
	
	[MenuItem ("Window/Create High Quality PVRTC")]
	public static void CreatePVRTC() {
		bool isMac = Application.platform == RuntimePlatform.OSXEditor;
		
		Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
		
		if(textures.Length == 0) {
			EditorUtility.DisplayDialog("Error", "There aren't any Texture2D assets selected", "OK");
			EditorUtility.ClearProgressBar();
			return;
		}
		
		for(int i=0; i<textures.Length; i++) {
			Texture2D texture = textures[i] as Texture2D;
			
			if(texture == null) {
				continue;
			}
			
			if(EditorUtility.DisplayCancelableProgressBar("Compressing textures...", string.Format("Compressing {0} - {1}/{2}", texture.name, i, textures.Length), i/(float)textures.Length)) {
				break;
			}
			
			string texturePath = AssetDatabase.GetAssetPath(texture);
			string fullTexturePath = ProjectBasedPathToFullPath(texturePath);
			//Get rid of texture filename at the end
			string textureFolderPath = texturePath.Substring(0, texturePath.LastIndexOf('/') + 1);
			textureFolderPath = ProjectBasedPathToFullPath(textureFolderPath);
			
			string PVRToolFolderPath = isMac ? Mac_PVRToolFolderPath : Win_PVRToolFolderPath;
			
			//Copy texture to executable folder
			string textureFileName = texturePath.Substring(texturePath.LastIndexOf('/') + 1, texturePath.Length - (texturePath.LastIndexOf('/') + 1));
			string textureCopyPath = PVRToolFolderPath + textureFileName;
			FileInfo textureFile = new FileInfo(fullTexturePath);
			FileInfo textureCopyFile = textureFile.CopyTo(textureCopyPath, true);
			
			string PVRToolExecutableName = isMac ? Mac_PVRToolExecutableName : Win_PVRToolExecutableName;
			
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = PVRToolFolderPath + PVRToolExecutableName;
			process.StartInfo.WorkingDirectory = PVRToolFolderPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.Arguments = string.Format(processArgumentsString, texture.width == texture.height ? "-square" : string.Empty, textureFileName);
			process.Start();
			Debug.Log("Process launched with arguments: " + process.StartInfo.Arguments);
			Debug.Log(PVRToolExecutableName + " StdError: " + process.StandardError.ReadToEnd());
			Debug.Log(PVRToolExecutableName + " StdOutput: " + process.StandardOutput.ReadToEnd());
			process.WaitForExit();
			process.Dispose();
			
			//Delete the copied texture
			textureCopyFile.Delete();
			
			//Copy the result next to the texture
			FileInfo pvrtcTextureFile = new FileInfo(PVRToolFolderPath + texture.name + PVRTextureExtension);
			if(pvrtcTextureFile.Exists) {
				pvrtcTextureFile.CopyTo(textureFolderPath + texture.name + PVRTextureSuffix + PVRTextureExtension, true);
				pvrtcTextureFile.Delete();
			} else {
				Debug.LogError("Couldn't find the PVR texture to copy next to the original texture at path: " + pvrtcTextureFile.FullName);
			}
		}
		EditorUtility.ClearProgressBar();
		EditorUtility.DisplayProgressBar("Importing compressed textures...", string.Empty, 1f);
		
		AssetDatabase.Refresh();
		
		EditorUtility.ClearProgressBar();
	}
	
	public static string ProjectBasedPathToFullPath(string path) {
		//Get rid of Assets at the beginning
		path = path.Substring(path.IndexOf('/'), path.Length - path.IndexOf('/'));
		//Add full path of Assets/ folder to the beginning
		return Application.dataPath + path;
	}
}
