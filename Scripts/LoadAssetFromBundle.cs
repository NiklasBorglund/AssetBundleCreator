//
//  LoadAssetFromBundle.cs
//
// This is an example script on how you could load assets from a bundle
// and download it. Be sure to build the asset bundles before
// you use this script.
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
using System.Collections;

public class LoadAssetFromBundle : MonoBehaviour
{
	
#if UNITY_EDITOR
	public string baseURL = "file://";
#else
	public string baseURL = "ONLINE_URL_HERE";
#endif
	
    private Object loadedAsset;	
  	private bool isDone = false;
    private bool downloadStarted = false;
    private string assetName;
    private string bundleName;
    private int version;
    private AssetBundle thisAssetBundle;
	private AssetBundleManager assetManager;
	
	/// <summary>
	/// Gets the name of the asset.
	/// </summary>
	/// <value>
	/// The name of the asset.
	/// </value>
    public string AssetName
    {
        get
        {
            return assetName;
        }
    }
	/// <summary>
	/// Gets the name of the asset bundle.
	/// </summary>
	/// <value>
	/// The name of the asset bundle.
	/// </value>
	public string AssetBundleName
	{
		get
		{
			return bundleName;	
		}
	}
	/// <summary>
	/// Gets a value indicating whether this download has started.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance has download started; otherwise, <c>false</c>.
	/// </value>
	public bool HasDownloadStarted
	{
		get
		{
			return downloadStarted;	
		}
	}
	/// <summary>
	/// Gets a value indicating whether the download is done
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is download done; otherwise, <c>false</c>.
	/// </value>
	public bool IsDownloadDone
	{
		get
		{
			return isDone;
		}
	}
	/// <summary>
	/// Gets the downloadedloaded asset from the downloaded AssetBundle(uninstantiated)
	/// </summary>
	/// <value>
	/// The loaded asset.
	/// </value>
	public Object GetDownloadedAsset
	{
		get
		{
			return loadedAsset;
		}
	}
	
	/// <summary>
	/// Queues the bundle download. Use DownloadAsset to initiate the download
	/// </summary>
	/// <param name='asset'>
	/// Asset name to be loaded from the bundle
	/// </param>
	/// <param name='bundleName'>
	/// the name of the bundle
	/// </param>
	/// <param name='version'>
	/// Version.
	/// </param>
    public void QueueBundleDownload(string asset, string bundleName, int version)
    {
#if UNITY_EDITOR
		//Get the base URL to the folder where the asset bundles are
		baseURL += Application.dataPath + PlayerPrefs.GetString("cws_exportFolder");
#endif
		
        downloadStarted = false;
        this.assetName = asset;
        this.bundleName = bundleName;
        this.version = version;
    }
	
	/// <summary>
	/// Initiates the download of the asset bundle. Only works if properties have been set with QueueBundleDownload first.
	/// </summary>
    public void DownloadAsset()
    {
        assetManager = AssetBundleManager.Instance;
		//Check from in the assetBundleManager if this bundle is already downloaded.
        AssetBundleContainer thisBundle = assetManager.GetAssetBundle(bundleName);

        if (thisBundle == null)
        {
			//if not, download it
            StartCoroutine(DownloadAssetBundle(assetName, bundleName, version));
        }
        else
        {
			//if it is, just load the asset directly
            loadedAsset = thisBundle.ThisAssetBundle.Load(assetName, typeof(GameObject));
            isDone = true;
        }
    }

	private IEnumerator DownloadAssetBundle(string asset, string bundleName, int version)
    {
        loadedAsset = null;

		// Wait for the Caching system to be ready
        while (!Caching.ready)
        {
            yield return null;
        }

        string url = baseURL + bundleName;

		// Start the download
        using (WWW www = WWW.LoadFromCacheOrDownload(url, version))
        {
            yield return www;
            if (www.error != null)
            {
                throw new System.Exception("AssetBundle - WWW download:" + www.error);
            }
            thisAssetBundle = www.assetBundle;
			
			//load the specified asset into memory
            loadedAsset = thisAssetBundle.Load(asset, typeof(GameObject));

            www.Dispose();

            isDone = true;
        }
	}
	
	/// <summary>
	/// Instantiates the asset and returns it. 
	/// </summary>
	/// <returns>
	/// The asset.
	/// </returns>
    public GameObject InstantiateAsset()
    {
        if (isDone)
        {
            GameObject newAsset = Instantiate(loadedAsset) as GameObject;
            assetManager.AddBundle(bundleName, thisAssetBundle, newAsset);

            return newAsset;
        }
        else
        {
            Debug.LogError("Asset is not downloaded!");
            return null;
        }
    }
}