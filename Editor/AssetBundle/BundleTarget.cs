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
        /// �ڲ�������assets�б�
        /// </summary>
        public List<AssetTarget> InnerAssets = new List<AssetTarget>();


        /// <summary>
        /// �ڲ�����prefab������
        /// </summary>
        public List<AssetTarget> InnerPrefabs = new List<AssetTarget>();

        /// <summary>
        /// ��¼���л������������ԭʼprefabs
        /// </summary>
        public List<AssetTarget> SurportingPrefabs = new List<AssetTarget>();

        public string[] dependsNames;
        /// <summary>
        /// ��������
        /// </summary>
        public string Name;

        public string BundleCrc;

    }
}