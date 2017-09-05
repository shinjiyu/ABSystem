using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tangzx.ABSystem
{



    /// <summary>
    /// Loader 父类
    /// </summary>
    public abstract class AssetBundleLoader 
    {
        public AssetBundleData bundleData;
        public AssetBundleManager bundleManager;
        public AssetBundleLoader[] depLoaders;

        protected AssetBundle _bundle;
        public AssetBundle bundle
        {
            get { return _bundle; }
        }


        /// <summary>
        /// 同步加载
        /// 注意：这个方法其实应该是只在DEMO开发期使用.
        /// 就像原作者所说的,否则会不会卡谁知道呢
        /// </summary>
        public virtual void  LoadBundleSync()
        {
        }

    }

    /// <summary>
    /// 在手机运行时加载
    /// </summary>
    public class MobileAssetBundleLoader : AssetBundleLoader
    {
        protected int _currentLoadingDepCount;
        
        protected bool _hasError;
        protected string _assetBundleCachedFile;
        protected string _assetBundleSourceFile;

  

        /// <summary>
        /// 增加一组同步接口
        /// </summary>
        override public void LoadBundleSync()
        {
            //已经加载过了
            
                if (depLoaders == null)
                {
                    depLoaders = new AssetBundleLoader[bundleData.assetBundleDepends.Length];
                    for (int i = 0; i < bundleData.assetBundleDepends.Length; i++)
                    {
                        depLoaders[i] = bundleManager.CreateLoader(bundleData.assetBundleDepends[i]);
                        depLoaders[i].LoadBundleSync();
                    }
                }

                _assetBundleSourceFile = bundleManager.pathResolver.GetBundleSourceFile(bundleData.assetBundleName, false);

                _bundle = AssetBundle.LoadFromFile(_assetBundleSourceFile);


        }
    }
}
