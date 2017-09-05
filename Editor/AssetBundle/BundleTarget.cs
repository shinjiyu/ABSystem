using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tangzx.ABSystem
{

    public enum BundleType
    {
        enumBundleTypeScene = 1,
        enumBundleTypePrefab = 2,
        enumBundleTypeShared = 3,
    }

    public class BundleTarget
    {


        public BundleType bundleType
        {
            get
            {
                bool bIsPrefab = false;
                foreach(AssetTarget at in InnerAssets)
                {
                    if(at.IsServerSceneOnly)
                    {
                        return BundleType.enumBundleTypeScene;
                    }
                    else if(at.IsRoot)
                    {
                        bIsPrefab =  true;
                    }
                }
                if(bIsPrefab)
                {
                    return BundleType.enumBundleTypePrefab;
                }
                else
                {
                    return BundleType.enumBundleTypeShared;
                }
            }
        }

        /// <summary>
        /// 内部包含的assets列表
        /// </summary>
        public List<AssetTarget> InnerAssets = new List<AssetTarget>();


        /// <summary>
        /// 内部存有prefab的数量
        /// </summary>
        public List<AssetTarget> InnerPrefabs = new List<AssetTarget>();

        /// <summary>
        /// 记录所有会依赖这个包的原始prefabs
        /// </summary>
        public List<AssetTarget> SurportingPrefabs = new List<AssetTarget>();

        public string[] dependsNames;
        /// <summary>
        /// 包的名字
        /// </summary>
        public string Name;

        public string BundleCrc;

    }
}