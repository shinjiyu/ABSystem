#if UNITY_5
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tangzx.ABSystem
{
    public class AssetBundleBuilder5x : ABBuilder
    {
        public AssetBundleBuilder5x(AssetBundlePathResolver resolver)
            : base(resolver)
        {

        }

        public override void Export()
        {

            int intTest = 0;
            base.Export();

            List<BundleTarget> bundles = AssetDataManager.Instance.GetAllBundles();
            
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach (BundleTarget bundle in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundle.Name;
                if (bundle.bundleType == BundleType.enumBundleTypeScene)
                {
                    string name = null;
                    foreach (AssetTarget at in bundle.InnerAssets)
                    {
                        if(at.IsScene)
                        {
                            name = at.assetPath;
                            break;
                        }
                    }
                    if(name == null)
                    {
                        Debug.Log("[Shinjiyu|Export]SceneBundle with main Scene!");
                    }
                    else
                    {
                        build.assetNames = new string[] { name.Replace("/", "\\") };
                    }
                }
                else
                {
                    List<string> tempNames = new List<string>();
                    foreach(AssetTarget at in bundle.InnerAssets)
                    {
                        tempNames.Add(at.assetPath.Replace("/", "\\"));
                    }
                    build.assetNames = tempNames.ToArray();
                }

                //if (intTest < 1)
                {
                    intTest++;
                    list.Add(build);
                }
                
            }


            //开始打包
            BuildPipeline.BuildAssetBundles(pathResolver.BundleSavePath, list.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

#if UNITY_5_1 || UNITY_5_2
            AssetBundle ab = AssetBundle.CreateFromFile(pathResolver.BundleSavePath + "/AssetBundles");
#else
            AssetBundle ab = AssetBundle.LoadFromFile(pathResolver.BundleSavePath + "/AssetBundles");
#endif
            AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            //hash
            foreach (BundleTarget bt in bundles)
            {
                Hash128 hash = manifest.GetAssetBundleHash(bt.Name);
                bt.BundleCrc = hash.ToString();
                bt.dependsNames = manifest.GetDirectDependencies(bt.Name);
            }

            List<AssetTarget> allPrefabs = AssetDataManager.Instance.GetAllPrefabs();
            this.SaveAssetsToBundlesMap(allPrefabs);
            //不再需要额外的依赖记录了。
            ab.Unload(true);
        }
    }
}
#endif