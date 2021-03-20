using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class BuildScript
{
	const string kAssetBundlesOutputPath = "AssetBundles";

	public static string GetAssetBundleBuildPath(){
		return kAssetBundlesOutputPath;
	}

	const string kBuildAssetBundlesMenu = "Patcher/Build AssetBundles";

	//[MenuItem(kBuildAssetBundlesMenu)]
	public static void BuildAssetBundles(BuildTarget buildTarget, List<string> buildingAssets = null)
	{
		Debug.Log("Start Build Assetbundles");

		//ProjectBuilder.SetBuildVersion_fromEnvVariable();
		// Choose the output path according to the build target.
		string outputPath = Path.Combine(kAssetBundlesOutputPath,  BaseLoader.GetPlatformFolderForAssetBundles(buildTarget) );

		if (!Directory.Exists(outputPath) )
			Directory.CreateDirectory (outputPath);

		string[] existingFilesPath = Directory.GetFiles(outputPath);
		List<FileInfo> existingFiles = new List<FileInfo>();
		if (existingFilesPath != null)
			for (int i = 0; i < existingFilesPath.Length; ++i)
			{
				FileInfo eachFile = new FileInfo(existingFilesPath[i]);
				existingFiles.Add(eachFile);
			}


        //total assets
        string[] bundles = AssetDatabase.GetAllAssetBundleNames();

        AssetBundleManifest totalManifest;
        //BuildPipeline.BuildAssetBundles (outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
        if (buildingAssets == null || buildingAssets.Count <= 0)
        {
            totalManifest =
                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, buildTarget);   // hash값 유지?
        }
        else if(bundles != null)
        {
            List<AssetBundleBuild> tempListBundles = new List<AssetBundleBuild>();

            for (int i = 0; i < buildingAssets.Count; ++i)
            {
                bool isRightAssetName = false;
                for (int j = 0; j < bundles.Length; ++j)
                {
                    if (bundles[j].Equals(buildingAssets[i]) == true)
                    {
                        isRightAssetName = true;
                        break;
                    }
                }

                if (isRightAssetName == true)
                {
                    AssetBundleBuild newStructInfo = new AssetBundleBuild();
                    newStructInfo.assetBundleName = buildingAssets[i];
                    newStructInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(buildingAssets[i]);

                    tempListBundles.Add(newStructInfo);
                }
            }

            AssetBundleBuild[] arrBundles = tempListBundles.ToArray();
            totalManifest =
                BuildPipeline.BuildAssetBundles(outputPath, arrBundles, BuildAssetBundleOptions.None, buildTarget);
        }
        else
        {
            Debug.LogError("There is no assetbundle to build");
            return;
        }

        string deviceManifestPath = Path.Combine(outputPath, buildTarget.ToString());
		uint deviceCRC = 0;
		BuildPipeline.GetCRCForAssetBundle(deviceManifestPath, out deviceCRC);
		FileInfo totalManiFile = new FileInfo(deviceManifestPath);

		//total manifest
		stAssetInfo totalAssetManifest = new stAssetInfo();
		totalAssetManifest.Name = buildTarget.ToString();
		totalAssetManifest.CRC = deviceCRC;
		totalAssetManifest.FileSize = totalManiFile.Length;
        //each assets
        string[] BuiltBundles = totalManifest.GetAllAssetBundles();

		List<stAssetInfo> listAssetInfo = new List<stAssetInfo>();
		listAssetInfo.Add(totalAssetManifest);

		if (BuiltBundles == null)
		{
			Debug.LogError("bundles is null");
			return;
		}
        
        int invalidCount = 0;
		for (int i = 0; i < BuiltBundles.Length; ++i)
		{
			//어셋 파일 존재 검증
			string targetPath = Path.Combine(outputPath, BuiltBundles[i]);
			if (File.Exists(targetPath) == false)
			{
				invalidCount++;
				Debug.LogError(BuiltBundles[i] + " is not exist");
			}
		}

		if (invalidCount > 0)
			return;
        
        string buildTargetBundle = buildTarget.ToString();
		string buildTargetManifest = buildTargetBundle + ".manifest";

		IEnumerable<FileInfo> deleteTarget = existingFiles.Where(
			(fileInfo) =>
			{
				if(fileInfo.Name.Equals(buildTargetBundle) || fileInfo.Name.Equals(buildTargetManifest) || fileInfo.Name.Equals("patch.xml"))
					return false;

				IEnumerable<string> foundBundles = from bundleName in bundles
												   where (fileInfo.Name.Equals(bundleName) || fileInfo.Name.Equals(StringUtils.Append(bundleName, ".manifest")))
												   select bundleName;

				if (foundBundles.Count() > 0)
					return false;

				return true;
			}
			);

		Debug.Log("Start Delete non-asset files");
		foreach (FileInfo deleteFile in deleteTarget)
		{
			Debug.Log("Deleting " + deleteFile.Name + " ......");
			deleteFile.Delete();
		}

        for (int i = 0; i < bundles.Length; ++i)
		{
			stAssetInfo newInfo = new stAssetInfo();

			string bundleName = bundles[i];

			string targetPath = Path.Combine(outputPath, bundleName);
			uint crc = 0;
            if (BuildPipeline.GetCRCForAssetBundle(targetPath, out crc) == true)
            {
                FileInfo assetFile = new FileInfo(targetPath);

                newInfo.Name = bundleName;
                newInfo.CRC = crc;
                newInfo.FileSize = assetFile.Length;

                listAssetInfo.Add(newInfo);
            }
		}

		VersionXML.MakeVersionFile(deviceCRC, outputPath, listAssetInfo);

		Debug.Log("End Build Assetbundles");
	}
    /*
    #region Legacy
    public static void BuildPlayer()
	{
		var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
		if (outputPath.Length == 0)
			return;

		string[] levels = GetLevelsFromBuildSettings();
		if (levels.Length == 0)
		{
			Debug.Log("Nothing to build.");
			return;
		}

		string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
		if (targetName == null)
			return;

		// Build and copy AssetBundles.
		BuildScript.BuildAssetBundles();
		BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, kAssetBundlesOutputPath) );

		BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
		BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
	}

	public static string GetBuildTargetName(BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.Android :
			return "/test.apk";
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return "/test.exe";
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
			return "/test.app";
		case BuildTarget.WebPlayer:
		case BuildTarget.WebPlayerStreamed:
			return "";
			// Add more build targets for your own.
		default:
			Debug.Log("Target not implemented.");
			return null;
		}
	}

	public static void CopyAssetBundlesTo(string outputPath)
	{
		// Clear streaming assets folder.
		//FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
		Directory.CreateDirectory(outputPath);

		string outputFolder = BaseLoader.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);

		// Setup the source folder for assetbundles.
		var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, kAssetBundlesOutputPath), outputFolder);
		if (!System.IO.Directory.Exists(source) )
			Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

		// Setup the destination folder for assetbundles. modified. ostype folder was erased. by ww
		if (System.IO.Directory.Exists(outputPath) )
			FileUtil.DeleteFileOrDirectory(outputPath);
		
		FileUtil.CopyFileOrDirectory(source, outputPath);
	}

	static string[] GetLevelsFromBuildSettings()
	{
		List<string> levels = new List<string>();
		for(int i = 0 ; i < EditorBuildSettings.scenes.Length; ++i)
		{
			if (EditorBuildSettings.scenes[i].enabled)
				levels.Add(EditorBuildSettings.scenes[i].path);
		}

		return levels.ToArray();
	}
    #endregion
    */
}
