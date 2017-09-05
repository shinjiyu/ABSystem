using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Tangzx.ABSystem
{
    public class ABBuilder
    {
        protected AssetBundleDataWriter dataWriter = new AssetBundleDataWriter();
        protected AssetBundlePathResolver pathResolver;

        private AssetDataManager assetDataManager
        {
            get
            {
                return AssetDataManager.Instance;
            }
        }

        public ABBuilder() : this(new AssetBundlePathResolver())
        {

        }

        public ABBuilder(AssetBundlePathResolver resolver)
        {
            this.pathResolver = resolver;
            this.InitDirs();
            AssetBundleUtils.pathResolver = pathResolver;
        }

        void InitDirs()
        {
            new DirectoryInfo(pathResolver.BundleSavePath).Create();
            new FileInfo(pathResolver.HashCacheSaveFile).Directory.Create();
        }

        public void Begin()
        {
            EditorUtility.DisplayProgressBar("Loading", "Loading...", 0.1f);
            assetDataManager.CleanOldData();
        }

        public void End()
        {
            EditorUtility.ClearProgressBar();
        }

        public virtual void Analyze()
        {
            var all = assetDataManager.GetAllAssets();
            
            foreach (AssetTarget target in all)
            {
                target.Analyze();
            }

            //因为分析后地增加AssetTarget
            all = assetDataManager.GetAllAssets();
            var rootSet = from s in all
                          where s.exportType == AssetBundleExportType.Root
                          select s;


            //遍历整个森林，让大家知道自己为哪些prefab服务
            foreach (AssetTarget rootTarget in rootSet)
            {
                rootTarget.AddServerRoot(rootTarget);
            }

            //
            foreach (AssetTarget target in all)
            {
                target.BeforeExport();
            }

            assetDataManager.FillBundleList();
        }

        public virtual void Export()
        {
            this.Analyze();
        }

        public void AddRootTargets(DirectoryInfo bundleDir, string[] partterns = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (partterns == null)
                partterns = new string[] { "*.*" };
            for (int i = 0; i < partterns.Length; i++)
            {
                FileInfo[] prefabs = bundleDir.GetFiles(partterns[i], searchOption);
                foreach (FileInfo file in prefabs)
                {
                    if (file.Extension.Contains("meta"))
                        continue;
                    
                    AssetTarget target = AssetDataManager.Instance.AddIfNotExist(file); //这个地方会引起一些误解。UTIL内部竟然存的状态
                    
                    //这个不仅有用 还得重点弄一下。
                    target.exportType = AssetBundleExportType.Root;   //这个值是有用的 
                }
            }
        }



        protected void SaveAssetsToBundlesMap(List<AssetTarget> prefabs)
        {
            string path = Path.Combine(pathResolver.BundleSavePath, pathResolver.A2BMapFileName);
            if (File.Exists(path))
                File.Delete(path);

            AssetBundleDataWriter writer = dataWriter;
            writer.SaveMap(path, prefabs.ToArray());
        }


        public void SetDataWriter(AssetBundleDataWriter w)
        {
            this.dataWriter = w;
        }
    }
}
