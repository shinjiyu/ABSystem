using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tangzx.ABSystem
{


    public class AssetBundleData
    {
        public string assetBundleName;
        public string[] assetBundleDepends;
        public string assetBundleHash;
    }


    public class AssetContainerData
    {
        //增加字段好了
        public string assetName;
        public string bundleName;
        public string bundleHash;
    }
    /// <summary>
    /// 文本文件格式说明
    /// *固定一行字符串ABDT
    /// 循环 { AssetBundleData
    ///     *名字(string)
    ///     *短名字(string)
    ///     *Hash值(string)
    ///     *类型(AssetBundleExportType)
    ///     *依赖文件个数M(int)
    ///     循环 M {
    ///         *依赖的AB文件名(string)
    ///     }
    /// }
    /// </summary>
    public class AssetBundleDataReader
    {

        public Dictionary<string, AssetContainerData> asset2bundleMap = new Dictionary<string, AssetContainerData>();

        public Dictionary<string,AssetBundleData> bundle2bundleMap = new  Dictionary<string,AssetBundleData>();
        
        public void Init(string bundlePath)
        {
#if UNITY_5_1 || UNITY_5_2
            AssetBundle ab = AssetBundle.CreateFromFile(bundlePath);
#else
            AssetBundle ab = AssetBundle.LoadFromFile(bundlePath);
#endif
            AssetBundleManifest assetBundleManifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

            string[] assetBundleNames = assetBundleManifest.GetAllAssetBundles();
            for (int i = 0; i < assetBundleNames.Length ;i++ )
            {
                AssetBundleData abd = new AssetBundleData();
                abd.assetBundleName = assetBundleNames[i];
                abd.assetBundleDepends = assetBundleManifest.GetDirectDependencies(abd.assetBundleName);
                abd.assetBundleHash = assetBundleManifest.GetAssetBundleHash(abd.assetBundleName).ToString();
                bundle2bundleMap.Add(assetBundleNames[i].ToLower(), abd);
            }
            ab.Unload(true);
        }

        public virtual void Read(Stream fs)
        {
            StreamReader sr = new StreamReader(fs);
            char[] fileHeadChars = new char[6];
            sr.Read(fileHeadChars, 0, fileHeadChars.Length);
            //读取文件头判断文件类型，ABDT 意思即 Asset-Bundle-Data-Text
            if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D' || fileHeadChars[3] != 'T')
                return;

            while (true)
            {
                string assetName = sr.ReadLine();
                if(string.IsNullOrEmpty(assetName))
                {
                    break;
                }
                string bundleName = sr.ReadLine();
                string assetHash = sr.ReadLine();
                string space = sr.ReadLine();
                AssetContainerData info = new AssetContainerData();
                info.assetName = assetName;
                info.bundleName = bundleName;
                info.bundleHash = assetHash;
                var names  = assetName.Split('.');
                if (names.Length >= 1)
                {
                    assetName = names[0];
                    if (names.Length > 2)
                    {
                        for (int i = 1; i < names.Length - 1; i++)
                        {
                            assetName = assetName + "." + names[i];
                        }
                    }
                }
                asset2bundleMap[assetName.ToLower()] = info;
            }
            sr.Close();
        }

        public string getAssetBundleNameByAssetPath(string assetPath)
        {
            if(asset2bundleMap.ContainsKey(assetPath.ToLower()))
            {
                return asset2bundleMap[assetPath.ToLower()].bundleName;
            }
            else
            {
                return "";
            }
        }
        public string getAssetNameByAssetPath(string assetPath)
        {
            if (asset2bundleMap.ContainsKey(assetPath.ToLower()))
            {
                return asset2bundleMap[assetPath.ToLower()].assetName;
            }
            else
            {
                return "";
            }
        }

        public AssetBundleData getAssetBundleData(string bundleName)
        {
            if (bundle2bundleMap.ContainsKey(bundleName.ToLower()))
            {
                return bundle2bundleMap[bundleName.ToLower()];
            }
            else
            {
                return null;
            }
        }
        public string[] getDependsBundleNamesByBundleName(string bundleName)
        {
            if (bundle2bundleMap.ContainsKey(bundleName.ToLower()))
            {
                return bundle2bundleMap[bundleName.ToLower()].assetBundleDepends;
            }
            else
            {
                return null;
            }
        }
    }
}