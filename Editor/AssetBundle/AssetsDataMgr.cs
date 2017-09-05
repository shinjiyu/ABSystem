using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Tangzx.ABSystem
{
    //这个类要重写原来UTIL类，原因是我认为util不应该存状态。
    //这个类因为不会真的把所有的包都加载进来，所以可以全程在内存中
    public class AssetDataManager
    {
        /// <summary>
        /// 存所有的assets信息
        /// KEY为Assets的完整路径小写
        /// </summary>
        public Dictionary<string, AssetTarget> AssetsData;

        /// <summary>
        /// AssetsData 的list形式，用于快速遍历。
        /// 我不确定DICTIONARY的遍历是不是真的很慢
        /// 这里姑且信了好了
        /// </summary>
        public List<AssetTarget> AssetsList;

        /// <summary>
        /// 所有bundle的信息
        /// </summary>
        public List<BundleTarget> BundlesData;


        //搞成单例
        private AssetDataManager() {
            AssetsData = new Dictionary<string, AssetTarget>();
            AssetsList = new List<AssetTarget>();
            BundlesData = new List<BundleTarget>();
        }

        private static AssetDataManager _Instance;
        public  static AssetDataManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new AssetDataManager();
                }
                return _Instance;
            }
        }

        /// <summary>
        /// 这里主要是担心外部会对这个东西进行修改
        /// </summary>
        /// <returns></returns>
        public List<AssetTarget> GetAllAssets()
        {
            return new List<AssetTarget>(AssetsList);
        }


        public List<AssetTarget> GetAllPrefabs()
        {
            List<AssetTarget> list = new List<AssetTarget>();
            foreach(AssetTarget at  in  AssetsList)
            {
                if(at.IsRoot)
                {
                    list.Add(at);
                }
            }
            return list;
        }


        public List<BundleTarget> GetAllBundles()
        {
            return new List<BundleTarget>(BundlesData);
        }

        public AssetTarget AddIfNotExist(FileInfo file)
        {
            AssetTarget target = null;
            string fullPath = file.FullName;
            int index = fullPath.IndexOf("Assets");
            if (index != -1)
            {
                string assetPath = fullPath.Substring(index);
                assetPath = assetPath.Replace('\\', '/');
                if( AssetsData.ContainsKey(assetPath.ToLower()))  
                {
                    target = AssetsData[assetPath.ToLower()];
                }
                else
                {
                    //这个地方并不是一定要去加载它才可以信任这个asset
                    target = new AssetTarget(file, assetPath);
                    AssetsData[assetPath.ToLower()] = target;
                    AssetsList.Add(target);
                }
            }
            return target;
        }


        public void FillBundleList()
        {
            Dictionary<string, List<AssetTarget>> dicAssets = new Dictionary<string, List<AssetTarget>>();
            foreach(AssetTarget at in AssetsList)
            {
                if(dicAssets.ContainsKey(at.bundleName))
                {
                    dicAssets[at.bundleName].Add(at);
                }
                else
                {
                    dicAssets.Add(at.bundleName, new List<AssetTarget>());
                    dicAssets[at.bundleName].Add(at);
                }
            }
            foreach(string name in dicAssets.Keys)
            {
                BundleTarget bt = new BundleTarget();
                AssetTarget tmp_at = dicAssets[name][0];
                bt.Name = tmp_at.bundleName;
                foreach(AssetTarget at in tmp_at.ServerRootSet)
                {
                    bt.SurportingPrefabs.Add(at);
                }

                foreach(AssetTarget at in dicAssets[name])
                {
                    bt.InnerAssets.Add(at);
                    at.bundleTarget = bt;
                    if(at.IsRoot)
                    {
                        bt.InnerPrefabs.Add(at);
                    }
                }
                BundlesData.Add(bt);
            }
        }

        public void CleanOldData()
        {
            AssetsData.Clear();
            AssetsList.Clear();
            BundlesData.Clear();
        }
    }
}