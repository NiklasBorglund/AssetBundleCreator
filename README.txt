* This Plugin is not being actively developed anymore *

﻿AssetBundleCreator by Niklas Borglund - niklasborglund[at]gmail[dot]com, @NiklasBorglund
---------------------------------------
This editor extension is created to make it really easy for everyone to create asset bundles.

This extension lets you specify a folder in your unity project, which this extension will then take
and create an assetbundle for each and every subfolder in that folder.

By default, that folder is : "/BundleCreator/Data/AssetBundles/"
and the asset bundles will by default be created in: Application.dataPath + "/../AssetBundles/"

If you create a subfolder called assetbundle01, the asset bundle will be named assetbundle01.unity3d by default.
If you also specify a target platform, for example web player, the bundle will be named webplayer_assetbundle01.unity3d by default

You can find the editor extension in Assets->Bundle Creator->Asset Bundle Creator.
You can change all the settings necessary in the editor extension and create your bundles.

The bundle is packaged with a method on how you can load your bundles after creating them. 
To try this, open the editor extension, build the assetbundles and start the scene "LoadAssetBundleScene.unity"

Video Tutorial:
http://www.youtube.com/watch?v=BeXbcF-4g54

Unity forum thread:
http://forum.unity3d.com/threads/174621-Asset-Bundle-Creator-RELEASED

This project is licensed under the MIT License.

contact information: niklasborglund[at]gmail[dot]com
twitter handle: https://twitter.com/NiklasBorglund
