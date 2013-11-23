//
//  CreateAssetBundles.cs
//
// The class that does all the work in Bundle Creator. 
// It creates the asset bundles in the specified folder and
// stores the information in a control file and a contents file.
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
using System.IO;
using System.Collections.Generic;

public class CreateAssetBundles
{
    public static string bundleControlFileName = "bundleControlFile.txt";
    public static string bundleContentsFileName = "bundleContents.txt";

    public static void ReadBundleControlFile(string path, Dictionary<string, int> bundleVersions)
    {
        if (bundleVersions == null)
        {
            bundleVersions = new Dictionary<string, int>();
        }
		else
		{
			bundleVersions.Clear();	
		}

        if (File.Exists(path))
        {
            StreamReader streamReader = new StreamReader(path);
            string currentLine;
            while ((currentLine = streamReader.ReadLine()) != null)
            {
                if (currentLine.StartsWith("BundleName:"))
                {
                    string bundleName = currentLine.Substring("BundleName:".Length);
                    int bundleVersion = 0;

                    //if the bundlename is there, the versionNumber should be on the next line
                    //otherwise there's an error
                    string nextLine = streamReader.ReadLine();
                    if (nextLine != null && nextLine.StartsWith("VersionNumber:"))
                    {
                        bundleVersion = System.Convert.ToInt32(nextLine.Substring("VersionNumber:".Length));
                    }
                    else
                    {
                        Debug.LogError("CreateAssetBundles.cs: Error reading bundle control file! - Delete the current control file to start over");
                    }

                    bundleVersions.Add(bundleName, bundleVersion);
                }
            }

            streamReader.Close();
        }
    }
    public static void ReadBundleContentsFile(string path, Dictionary<string, List<string>> bundleContents)
    {
        if (bundleContents == null)
        {
            bundleContents = new Dictionary<string, List<string>>();
        }
		else
		{
			bundleContents.Clear();	
		}
        if (File.Exists(path))
        {
            StreamReader streamReader = new StreamReader(path);
            string currentLine;
            while ((currentLine = streamReader.ReadLine()) != null)
            {
                if (currentLine.StartsWith("BundleName:"))
                {
                    string bundleName = currentLine.Substring("BundleName:".Length);

                    int numberOfAssets = 0;
                    string nextLine = streamReader.ReadLine();
                    if (nextLine != null && nextLine.StartsWith("NumberOfAssets:"))
                    {
                        numberOfAssets = System.Convert.ToInt32(nextLine.Substring("NumberOfAssets:".Length));
                    }
                    else
                    {
                        Debug.LogError("CreateAssetBundles.cs: Error reading bundle contents file! - Delete the current control file to start over");
                        break;
                    }
                    List<string> assetsInBundle = new List<string>();
                    for (int i = 0; i < numberOfAssets; i++)
                    {
                        assetsInBundle.Add(streamReader.ReadLine());
                    }

                    bundleContents.Add(bundleName, assetsInBundle);
                }
            }

            streamReader.Close();
        }
    }
    static void WriteBundleControlFile(string path, Dictionary<string, int> bundleVersions, string exportPath)
    {
        StreamWriter streamWriter = new StreamWriter(path);
        foreach (KeyValuePair<string, int> bundleVersion in bundleVersions)
        {
            if (File.Exists(exportPath + bundleVersion.Key))
            {
                streamWriter.WriteLine("BundleName:" + bundleVersion.Key);
                streamWriter.WriteLine("VersionNumber:" + bundleVersion.Value.ToString());

                //For readability in the txt file, add an empty line
                streamWriter.WriteLine();
            }
        }
        streamWriter.Close();
    }
    static void WriteBundleContentsFile(string path, Dictionary<string, List<string>> bundleContents, string exportPath)
    {
        StreamWriter streamWriter = new StreamWriter(path);
        foreach (KeyValuePair<string, List<string>> asset in bundleContents)
        {
            if(File.Exists(exportPath + asset.Key))
            {
                //For readability in the txt file
                streamWriter.WriteLine("BundleName:" + asset.Key);
                streamWriter.WriteLine("NumberOfAssets:" + asset.Value.Count);
                foreach (string assetName in asset.Value)
                {
                    streamWriter.WriteLine(assetName);
                }
            }
        }
        streamWriter.Close();
    }

    public static bool ExportAssetBundleFolders(AssetBundleWindow thisWindow)
    {
        //To keep track of the versions with the control file
        Dictionary<string, int> bundleVersions = new Dictionary<string, int>();

        //A list of all the assets in each bundle
        //This will be saved into the file bundleContents.txt;
        Dictionary<string, List<string>> bundleContents = new Dictionary<string, List<string>>();

        //The AssetBundle folder
        string path = Application.dataPath + thisWindow.exportLocation;

        //The folder location in the editor
        string assetBundleFolderLocation = thisWindow.assetBundleFolderLocation;

        //Create directory if it does not exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //Read and parse the control file
        ReadBundleControlFile(path + bundleControlFileName, bundleVersions);
        //Read and parse the contents file
        ReadBundleContentsFile(path + bundleContentsFileName, bundleContents);

		//Check if the directory exist
        if (!Directory.Exists(Application.dataPath + assetBundleFolderLocation))
        {
            Debug.LogError("Specified 'AssetBundles folder' does not exist! Open Assets->Bundle Creator->Asset Bundle Creator to correct it");
            return false;
        }

        int createdBundles = 0;
        string[] directoryNames = Directory.GetDirectories(Application.dataPath + assetBundleFolderLocation);
        foreach (string folderPath in directoryNames)
        {
            //Generate the name of this asset bundle
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            //if the user has specified a build target, add a prefix to the bundle name
            string bundleName = "";
			string directoryName = dirInfo.Name;
			if(thisWindow.setLowerCaseName)
			{
				directoryName = directoryName.ToLower();
			}
			
            if (!thisWindow.optionalSettings)
            {
                bundleName = directoryName + thisWindow.bundleFileExtension;
            }
            else
            {
                bundleName = thisWindow.buildTarget.ToString().ToLower() + "_" + directoryName + thisWindow.bundleFileExtension;
            }

            List<Object> toInclude = new List<Object>();
            string[] assetsInFolder = Directory.GetFiles(folderPath);

            //To save in the contents file(information)
            List<string> assetListInFolder = new List<string>();

            foreach (string asset in assetsInFolder)
            {
                string thisFileName = Path.GetFileName(asset);
                if (asset.EndsWith(".prefab"))
                {
                    string internalFilePath = "Assets" + assetBundleFolderLocation + dirInfo.Name + "/" + thisFileName;
                    GameObject prefab = (GameObject)Resources.LoadAssetAtPath(internalFilePath, typeof(GameObject));
                    toInclude.Add((Object)prefab);

                    assetListInFolder.Add(thisFileName);
                }
                else if (!asset.EndsWith(".meta"))
                {
                    //toInclude.AddRange(AssetDatabase.LoadAllAssetsAtPath(assetBundleFolderLocation + dirInfo.Name + "/" + thisFileName));
					string internalFilePath = "Assets" + assetBundleFolderLocation + dirInfo.Name + "/" + thisFileName;
                    toInclude.Add((Object)Resources.LoadAssetAtPath(internalFilePath, typeof(Object)));
                    assetListInFolder.Add(thisFileName);
                }
            }

            //Build only if there are any files in this folder
            if (toInclude.Count > 0)
            {
                //Check if the bundle already have been created
                if (bundleContents.ContainsKey(bundleName))
                {
                    bundleContents.Remove(bundleName);
                }
                //Add to the contents text file
                bundleContents.Add(bundleName, assetListInFolder);

                Debug.Log("Building bundle:" + bundleName);
                if (!BuildAssetBundle(thisWindow, toInclude, path + bundleName))
                {
                    return false; //It failed, abort everything
                }
                createdBundles++;

                //Checks to save the version numbers
                int versionNumber = -1;
                bundleVersions.TryGetValue(bundleName, out versionNumber);

                if (versionNumber == -1)
                {
                    versionNumber = 1;
                }
                else
                {
                    versionNumber++;
                    bundleVersions.Remove(bundleName);
                }
                bundleVersions.Add(bundleName, versionNumber);
            }
            toInclude.Clear();
        }

        WriteBundleControlFile(path + bundleControlFileName, bundleVersions, path);
        WriteBundleContentsFile(path + bundleContentsFileName, bundleContents, path);
        bundleVersions.Clear();
        foreach (KeyValuePair<string, List<string>> pair in bundleContents)
        {
            pair.Value.Clear();
        }
        bundleContents.Clear();

        Debug.Log("***Successfully Created " + createdBundles + " AssetBundles!***");
        return true;
    }

    /// <summary>
    /// Helper function to build an asset bundle
    /// This will iterate through all the settings of the AssetBundleWindow and set them accordingly
    /// </summary>
    /// <param name="thisWindow"></param>
    /// <param name="toInclude"></param>
    /// <param name="bundlePath"></param>
    public static bool BuildAssetBundle(AssetBundleWindow thisWindow, List<Object> toInclude, string bundlePath)
    {
        BuildAssetBundleOptions buildAssetOptions = 0;
        if(thisWindow.buildAssetBundleOptions)
        {
            if(thisWindow.collectDependencies)
            {
                if(buildAssetOptions == 0)
                {
                    buildAssetOptions = BuildAssetBundleOptions.CollectDependencies;
                }
                else
                {
                    buildAssetOptions |= BuildAssetBundleOptions.CollectDependencies;
                }
            }
            if(thisWindow.completeAssets)
            {
                if(buildAssetOptions == 0)
                {
                    buildAssetOptions = BuildAssetBundleOptions.CompleteAssets;
                }
                else
                {
                    buildAssetOptions |= BuildAssetBundleOptions.CompleteAssets;
                }
            }
            if(thisWindow.disableWriteTypeTree)
            {
                if(buildAssetOptions == 0)
                {
                    buildAssetOptions = BuildAssetBundleOptions.DisableWriteTypeTree;
                }
                else
                {
                    buildAssetOptions |= BuildAssetBundleOptions.DisableWriteTypeTree;
                }
            }
            if(thisWindow.deterministicAssetBundle)
            {
                if(buildAssetOptions == 0)
                {
                    buildAssetOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
                }
                else
                {
                    buildAssetOptions |= BuildAssetBundleOptions.DeterministicAssetBundle;
                }
            }
            if(thisWindow.uncompressedAssetBundle)
            {
                if(buildAssetOptions == 0)
                {
                    buildAssetOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
                }
                else
                {
                    buildAssetOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
                }
            }
        }

        //If none of "BuildAssetBundleOptions" or "Optional Settings" are set, then create without dependency tracking
        if (!thisWindow.buildAssetBundleOptions && !thisWindow.optionalSettings)
        {
            if (!BuildPipeline.BuildAssetBundle(null, toInclude.ToArray(), bundlePath))
            {
                return false;
            }
        }
        else
        {
              if (buildAssetOptions == 0) //If it's still zero, set default values
              {
                  Debug.LogWarning("No BuildAssetBundleOptions are set, reverting back to dependency tracking. If you want no dependency tracking uncheck the 'BuildAssetBundleOptions' && 'Optional Settings' toggles all together");
                  buildAssetOptions = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
				  thisWindow.buildAssetBundleOptions = true;
                  thisWindow.collectDependencies = true;
                  thisWindow.completeAssets = true;
              }
              if (thisWindow.optionalSettings) //Support for different build targets
              {
                  if (!BuildPipeline.BuildAssetBundle(null, toInclude.ToArray(), bundlePath, buildAssetOptions, thisWindow.buildTarget))
                  {
                      return false;
                  }
              }
              else
              {
                  if (!BuildPipeline.BuildAssetBundle(null, toInclude.ToArray(), bundlePath, buildAssetOptions))
                  {
                      return false;
                  }
              }
        }
        return true;
    }
}
