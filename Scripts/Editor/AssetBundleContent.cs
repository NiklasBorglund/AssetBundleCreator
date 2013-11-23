//
//  AssetBundleContent.cs
//
// Editor window that specifies the files in the bundle according to
// the bundle contents file generated when building the asset bundles.
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
using System.Diagnostics;

public class AssetBundleContent : EditorWindow
{
    private List<string> currentContentList;
    private string currentBundleName;
    private string exportPath;
    private float sizeInKiloBytes = 0;

    public void SelectAssetBundle(string currentBundleName, List<string> currentContentList, string exportPath, float sizeInKiloBytes)
    {
        this.currentContentList = currentContentList;
        this.currentBundleName = currentBundleName;
        this.exportPath = exportPath;
        this.sizeInKiloBytes = sizeInKiloBytes;
    }

    void OnGUI()
    {
        if (currentBundleName != null && currentContentList != null)
        {
            GUILayout.Label("Bundle: " + currentBundleName, EditorStyles.boldLabel);
            GUILayout.Label("File Size: " + sizeInKiloBytes + "kb", EditorStyles.boldLabel);

            foreach (string assetName in currentContentList)
            {
                GUILayout.Label(assetName, EditorStyles.label);
            }
			
			if (Application.platform != RuntimePlatform.OSXEditor)
			{
            	if (GUILayout.Button("Show in explorer"))
            	{
               	 string itemPath = exportPath + currentBundleName;
               	 itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like forward slashes
               	 System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
            	}
			}
        }
    }

    //Get the window
    public static AssetBundleContent CreateContentWindow()
    {
        AssetBundleContent thisWindow = (AssetBundleContent)EditorWindow.GetWindow<AssetBundleContent>("Bundle Contents",true,typeof(AssetBundleWindow));
        return thisWindow;
    }
}
