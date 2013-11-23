//
//  AssetBundleWindow.cs
//
// Editor window that lets the user decide the settings for the
// Asset Bundles.
//
// The MIT License (MIT)
//
// Copyright (c) 2013 Niklas Borglund
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AssetBundleWindow : EditorWindow 
{
    private AssetBundleContent contentWindow;

    public string assetBundleFolderLocation = "/Data/AssetBundles/";
    public string exportLocation = "/../AssetBundles/";
    public string bundleFileExtension = ".unity3d";

    public bool optionalSettings = false;
    public BuildTarget buildTarget = BuildTarget.WebPlayer;

    //BuildAssetBundleOptions
    public bool buildAssetBundleOptions = true;
    public bool collectDependencies = true;
    public bool completeAssets = true;
    public bool disableWriteTypeTree = false;
    public bool deterministicAssetBundle = false;
    public bool uncompressedAssetBundle = false;
	public bool setLowerCaseName = true;
	
    public Dictionary<string, int> bundleVersions = new Dictionary<string, int>();
    public Dictionary<string, List<string>> bundleContents = new Dictionary<string, List<string>>();
    public Dictionary<string, float> bundleFileSizes = new Dictionary<string, float>();
	
	//The position of the scrollview
	private Vector2 scrollPosition = Vector2.zero;
	
	//The undo manager
	HOEditorUndoManager undoManager;
	
	private void OnEnable()
	{
		// Instantiate undoManager
		undoManager = new HOEditorUndoManager( this, "AssetBundleCreator" );
	}
 

    void OnGUI()
    {
		undoManager.CheckUndo();
		
        GUILayout.Label ("Export Settings", EditorStyles.boldLabel);
        assetBundleFolderLocation = EditorGUILayout.TextField("AssetBundles folder", assetBundleFolderLocation);
		GUILayout.Label ("Application.dataPath			 " + Application.dataPath, EditorStyles.label);
        exportLocation = EditorGUILayout.TextField("Export folder", exportLocation);
        bundleFileExtension = EditorGUILayout.TextField("Bundle file ext.", bundleFileExtension);
		setLowerCaseName = EditorGUILayout.Toggle("Names to lower case", setLowerCaseName);

        buildAssetBundleOptions = EditorGUILayout.BeginToggleGroup("BuildAssetBundleOptions", buildAssetBundleOptions);
        collectDependencies = EditorGUILayout.Toggle("CollectDependencies", collectDependencies);
        completeAssets = EditorGUILayout.Toggle("CompleteAssets", completeAssets);
        disableWriteTypeTree = EditorGUILayout.Toggle("DisableWriteTypeTree", disableWriteTypeTree);
        deterministicAssetBundle = EditorGUILayout.Toggle("DeterministicAssetBundle", deterministicAssetBundle);
        uncompressedAssetBundle = EditorGUILayout.Toggle("UncompressedAssetBundle", uncompressedAssetBundle);
        EditorGUILayout.EndToggleGroup();

        optionalSettings = EditorGUILayout.BeginToggleGroup("Optional Settings", optionalSettings);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        EditorGUILayout.EndToggleGroup();
		
		undoManager.CheckDirty();

        GUILayout.Label("Reset Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset"))
        {
            ClearPreferences(this);
            WriteEditorPrefs(this);
            CreateAssetBundles.ReadBundleControlFile(Application.dataPath + exportLocation + CreateAssetBundles.bundleControlFileName, bundleVersions);
            CreateAssetBundles.ReadBundleContentsFile(Application.dataPath + exportLocation + CreateAssetBundles.bundleContentsFileName, bundleContents);
            ReadBundleFileSizes();
        }

        GUILayout.Label("Build", EditorStyles.boldLabel);
        if (GUILayout.Button("Build Asset Bundles"))
        {
            if (!CreateAssetBundles.ExportAssetBundleFolders(this))
            {
                Debug.LogError("AssetBundle Build Failed! - Please check your settings in the Bundle Creator at Assets->Bundle Creator-> Asset Bundle Creator.");
            }
            else
            {
                //It worked, save the preferences and reload the control file
                WriteEditorPrefs(this);
                bundleVersions.Clear();
                bundleContents.Clear();
                bundleFileSizes.Clear();
                CreateAssetBundles.ReadBundleControlFile(Application.dataPath + exportLocation + CreateAssetBundles.bundleControlFileName, bundleVersions);
                CreateAssetBundles.ReadBundleContentsFile(Application.dataPath + exportLocation + CreateAssetBundles.bundleContentsFileName, bundleContents);
                ReadBundleFileSizes();
            }
        }

        GUILayout.Label("Bundle Versions", EditorStyles.boldLabel);
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (KeyValuePair<string, int> bundleVersion in bundleVersions)
        {
            float bundleFileSize = 0;
            bundleFileSizes.TryGetValue(bundleVersion.Key, out bundleFileSize);
            if (GUILayout.Button(bundleVersion.Key + ", Version:" + bundleVersion.Value + ", Size: " + bundleFileSize + "kb"))
            {
                List<string> assetsInBundle = null;
                bundleContents.TryGetValue(bundleVersion.Key, out assetsInBundle);
                if (assetsInBundle != null)
                {
                    CreateContentWindow();
                    contentWindow.SelectAssetBundle(bundleVersion.Key, assetsInBundle, Application.dataPath + exportLocation, bundleFileSize);
                    contentWindow.ShowTab();
                }
            }
        }
		EditorGUILayout.EndScrollView();
    }

    public void ReadBundleFileSizes()
    {
		bundleFileSizes.Clear();
        if (bundleVersions.Count > 0)
        {
            foreach (KeyValuePair<string, int> bundleVersion in bundleVersions)
            {
                if (File.Exists(Application.dataPath + exportLocation + bundleVersion.Key))
                {
                    FileInfo thisFileInfo = new FileInfo(Application.dataPath + exportLocation + bundleVersion.Key);
                    bundleFileSizes.Add(bundleVersion.Key, (thisFileInfo.Length / 1024));
                }
            }
        }
    }
    private void CreateContentWindow()
    {
        if (contentWindow == null)
        {
            contentWindow = AssetBundleContent.CreateContentWindow();
        }
    }
    private static void ReadEditorPrefs(AssetBundleWindow thisWindow)
    {
        //load editor prefs
        //cws is for "cry wolf studios"
        if (EditorPrefs.HasKey("cws_assetFolder"))
        {
            thisWindow.assetBundleFolderLocation = EditorPrefs.GetString("cws_assetFolder");
        }
        if (EditorPrefs.HasKey("cws_exportFolder"))
        {
            thisWindow.exportLocation = EditorPrefs.GetString("cws_exportFolder");
        }
        if (EditorPrefs.HasKey("cws_bundleExtension"))
        {
            thisWindow.bundleFileExtension = EditorPrefs.GetString("cws_bundleExtension");
        }
        if (EditorPrefs.HasKey("cws_optionalSettings"))
        {
            thisWindow.optionalSettings = EditorPrefs.GetBool("cws_optionalSettings");
        }
        if (EditorPrefs.HasKey("cws_buildTarget"))
        {
            thisWindow.buildTarget = (BuildTarget)EditorPrefs.GetInt("cws_buildTarget");
        }
        if (EditorPrefs.HasKey("cws_buildAssetBundleOptions"))
        {
            thisWindow.buildAssetBundleOptions = EditorPrefs.GetBool("cws_buildAssetBundleOptions");
        }
        if (EditorPrefs.HasKey("cws_collectDependencies"))
        {
            thisWindow.collectDependencies = EditorPrefs.GetBool("cws_collectDependencies");
        }
        if (EditorPrefs.HasKey("cws_completeAssets"))
        {
            thisWindow.completeAssets = EditorPrefs.GetBool("cws_completeAssets");
        }
        if (EditorPrefs.HasKey("cws_disableWriteTypeTree"))
        {
            thisWindow.disableWriteTypeTree = EditorPrefs.GetBool("cws_disableWriteTypeTree");
        }
        if (EditorPrefs.HasKey("cws_deterministicAssetBundle"))
        {
            thisWindow.deterministicAssetBundle = EditorPrefs.GetBool("cws_deterministicAssetBundle");
        }
        if (EditorPrefs.HasKey("cws_uncompressedAssetBundle"))
        {
            thisWindow.uncompressedAssetBundle = EditorPrefs.GetBool("cws_uncompressedAssetBundle");
        }
		if (EditorPrefs.HasKey("cws_setLowerCaseName"))
        {
            thisWindow.setLowerCaseName = EditorPrefs.GetBool("cws_setLowerCaseName");
        }
    }
    private static void WriteEditorPrefs(AssetBundleWindow thisWindow)
    {
        //save editor prefs
        //cws is for "cry wolf studios"
        EditorPrefs.SetString("cws_assetFolder", thisWindow.assetBundleFolderLocation);
        EditorPrefs.SetString("cws_exportFolder", thisWindow.exportLocation);
        EditorPrefs.SetString("cws_bundleExtension", thisWindow.bundleFileExtension);
        EditorPrefs.SetBool("cws_optionalSettings", thisWindow.optionalSettings);
        EditorPrefs.SetInt("cws_buildTarget", (int)thisWindow.buildTarget);
        EditorPrefs.SetBool("cws_buildAssetBundleOptions", thisWindow.buildAssetBundleOptions);
        EditorPrefs.SetBool("cws_collectDependencies", thisWindow.collectDependencies);
        EditorPrefs.SetBool("cws_completeAssets", thisWindow.completeAssets);
        EditorPrefs.SetBool("cws_disableWriteTypeTree", thisWindow.disableWriteTypeTree);
        EditorPrefs.SetBool("cws_deterministicAssetBundle", thisWindow.deterministicAssetBundle);
        EditorPrefs.SetBool("cws_uncompressedAssetBundle", thisWindow.uncompressedAssetBundle);
		EditorPrefs.SetBool("cws_setLowerCaseName", thisWindow.setLowerCaseName);
		
		//If you want the export folder at runtime (for asset bundle loading in editor mode)
		PlayerPrefs.SetString("cws_exportFolder", thisWindow.exportLocation);
    }
    private static void ClearPreferences(AssetBundleWindow thisWindow)
    {
        thisWindow.assetBundleFolderLocation = "/BundleCreator/Data/AssetBundles/";
        thisWindow.exportLocation = "/../AssetBundles/";
        thisWindow.bundleFileExtension = ".unity3d";

        thisWindow.optionalSettings = false;
        thisWindow.buildTarget = BuildTarget.WebPlayer;

        //BuildAssetBundleOptions
        thisWindow.buildAssetBundleOptions = true;
        thisWindow.collectDependencies = true;
        thisWindow.completeAssets = true;
        thisWindow.disableWriteTypeTree = false;
        thisWindow.deterministicAssetBundle = false;
        thisWindow.uncompressedAssetBundle = false;
        thisWindow.bundleVersions.Clear();
        thisWindow.bundleContents.Clear();
        thisWindow.bundleFileSizes.Clear();
    }

    //Show window
    [MenuItem("Assets/Bundle Creator/Asset Bundle Creator")]
    public static void ShowWindow()
    {
        AssetBundleWindow thisWindow = (AssetBundleWindow)EditorWindow.GetWindow(typeof(AssetBundleWindow));
        thisWindow.title = "Bundle Creator";
      
        ReadEditorPrefs(thisWindow);
        CreateAssetBundles.ReadBundleControlFile(Application.dataPath + thisWindow.exportLocation + CreateAssetBundles.bundleControlFileName, thisWindow.bundleVersions);
        CreateAssetBundles.ReadBundleContentsFile(Application.dataPath + thisWindow.exportLocation + CreateAssetBundles.bundleContentsFileName, thisWindow.bundleContents);
        thisWindow.ReadBundleFileSizes();
    }
}
