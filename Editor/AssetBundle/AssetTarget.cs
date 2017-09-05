using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tangzx.ABSystem
{
    public enum AssetType
    {
        Asset,
        Builtin
    }

    public class AssetTarget : System.IComparable<AssetTarget>
    {   
        /// <summary>
        /// 文件路径
        /// </summary>
        public FileInfo file;
        /// <summary>
        /// 相对于Assets文件夹的目录
        /// </summary>
        public string assetPath;
        /// <summary>
        /// 此文件是否已导出
        /// </summary>
        public bool isExported;
        /// <summary>
        /// 素材类型
        /// </summary>
        public AssetType type = AssetType.Asset;
        /// <summary>
        /// 导出类型
        /// </summary>
        public AssetBundleExportType exportType = AssetBundleExportType.Asset;
        /// <summary>
        /// 保存地址
        /// </summary>
        public string bundleSavePath;
        /// <summary>
        /// BundleName
        /// </summary>
        public string bundleName;
        /// <summary>
        /// 短名
        /// </summary>
        public string bundleShortName;


        public BundleTarget bundleTarget;
        
        public int level = -1;
        public List<AssetTarget> levelList;

        //目标文件是否已改变
        private bool _isFileChanged = false;
        //是否已分析过依赖
        private bool _isAnalyzed = false;
        //依赖树是否改变（用于增量打包）
        private bool _isDepTreeChanged = false;
        //.meta 文件的Hash
        private string _metaHash;
        //上次打好的AB的CRC值（用于增量打包）
        private string _bundleCrc;
        //是否是新打包的
        private bool _isNewBuild;
        /// <summary>
        /// 我要依赖的项
        /// </summary>
        private HashSet<AssetTarget> _dependParentSet = new HashSet<AssetTarget>();
        /// <summary>
        /// 依赖我的项
        /// </summary>
        private HashSet<AssetTarget> _dependChildrenSet = new HashSet<AssetTarget>();

        /// <summary>
        /// 所有依赖我的root集合. 如果自已是root 则自己一定在这个集合中
        /// </summary>
        public  List<AssetTarget> ServerRootSet = new List<AssetTarget>();
        public AssetTarget(FileInfo file, string assetPath)
        {
            this.file = file;
            this.assetPath = assetPath;
            this.bundleShortName = file.Name.ToLower();
            this.bundleName = HashUtil.Get(AssetBundleUtils.ConvertToABName(assetPath)) + ".ab";
            this.bundleSavePath = Path.Combine(AssetBundleUtils.pathResolver.BundleSavePath, bundleName);

            _isFileChanged = true;
            _metaHash = "0";
        }

        
        /// <summary>
        /// 分析引用关系
        /// </summary>
        public void Analyze()
        {
            if (_isAnalyzed) return;
            _isAnalyzed = true;
            
            
            string[] deps = AssetDatabase.GetDependencies(assetPath);

            List<string> depsList = new List<string>();
            for (int i = 0; i < deps.Length; i++)
            {
                string path = deps[i];
                //不包含脚本对象
                //不包含LightingDataAsset对象
//                 if (o is MonoScript || o is LightingDataAsset)
//                     continue;

                if (path.EndsWith(".cs"))
                    continue;


                //竟然还有js脚本。。。。。。why
                if (path.EndsWith(".js"))
                    continue;

                //不包含builtin对象
                if (path.StartsWith("Resources"))
                    continue;

                depsList.Add(path);
            }
            deps = depsList.ToArray();

            var paths = deps.Distinct().ToArray();

            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]) == false)
                {
                    //Debug.Log("invalid:" + paths[i]);
                    continue;
                }
                FileInfo fi = new FileInfo(paths[i]);
                AssetTarget target = AssetDataManager.Instance.AddIfNotExist(fi);
                if (target == null)
                    continue;

                this.AddDependParent(target);
                target.AddDependChild(this);
                target.Analyze();
            }
        }

        public void Merge()
        {
            if (this.NeedExportStandalone())
            {
                var children = new List<AssetTarget>(_dependChildrenSet);
                this.RemoveDependChildren();
                foreach (AssetTarget child in children)
                {
                    child.AddDependParent(this);
                }
            }
        }



        public void AddServerRoot(AssetTarget rootTarget)
        {
            if (ServerRootSet.Contains(rootTarget))
            {
                return;
            }
            ServerRootSet.Add(rootTarget);
            foreach (AssetTarget at in _dependParentSet)
            {    
                at.AddServerRoot(rootTarget);
            }
        }

        /// <summary>
        /// 是不是只被一个scene所依赖
        /// </summary>
        public bool IsServerSceneOnly
        {
            get
            {
                return ServerRootSet.Count == 1 && ServerRootSet[0].assetPath.IndexOf(".unity") != -1;
            }
        }

        /// <summary>
        /// 是否是scene
        /// </summary>
        public bool IsScene
        {
            get
            {
                return assetPath.EndsWith(".unity");
            }
        }

        public bool IsRoot
        {
            get
            {
                return exportType == AssetBundleExportType.Root;
            }
        }



        private bool beforeExportProcess;


        /// <summary>
        /// 在导出之前执行
        /// </summary>
        public void BeforeExport()
        {
            if (beforeExportProcess) return;
            beforeExportProcess = true;

            {
                if (this.exportType == AssetBundleExportType.Asset && ServerRootSet.Count > 1)
                {
                    this.exportType = AssetBundleExportType.Standalone;
                }
                string fileNameSet = "";
                foreach (AssetTarget at in ServerRootSet)
                {
                    fileNameSet = fileNameSet + at.assetPath;
                }
                //如是这个asset只被scene依赖，
                //且自己不是scene 则不具名导出
                
                bundleName = HashUtil.Get(AssetBundleUtils.ConvertToABName(fileNameSet)) + ".ab";
                bundleSavePath = Path.Combine(AssetBundleUtils.pathResolver.BundleSavePath, bundleName);               
                
            }
        }


        /// <summary>
        /// 获取所有依赖项
        /// </summary>
        /// <param name="list"></param>
        public void GetDependencies(HashSet<AssetTarget> list)
        {
            var ie = _dependParentSet.GetEnumerator();
            while (ie.MoveNext())
            {
                AssetTarget target = ie.Current;
                if (target.IsRoot)
                {
                    list.Add(target);
                }
                else
                {
                    target.GetDependencies(list);
                }
            }
        }

        public List<AssetTarget> dependencies
        {
            get { return new List<AssetTarget>(_dependParentSet); }
        }

        public AssetBundleExportType compositeType
        {
            get
            {
                AssetBundleExportType type = exportType;
                if (type == AssetBundleExportType.Root && _dependChildrenSet.Count > 0)
                    type |= AssetBundleExportType.Asset;
                return type;
            }
        }

        public bool isNewBuild
        {
            get { return _isNewBuild; }
        }

        public string bundleCrc
        {
            get { return _bundleCrc; }
            set
            {
                _isNewBuild = value != _bundleCrc;
                if (_isNewBuild)
                {
                    Debug.Log("Export AB : " + bundleName);
                }
                _bundleCrc = value;
            }
        }


        /// <summary>
        /// (作为AssetType.Asset时)是否需要单独导出
        /// </summary>
        /// <returns></returns>
        private bool NeedExportStandalone()
        {
            return _dependChildrenSet.Count > 1;
        }

        /// <summary>
        /// 增加依赖项
        /// </summary>
        /// <param name="target"></param>
        private void AddDependParent(AssetTarget target)
        {
            if (target == this || this.ContainsDepend(target))
                return;

            _dependParentSet.Add(target);
        }

        /// <summary>
        /// 是否已经包含了这个依赖（检查子子孙孙）
        /// </summary>
        /// <param name="target"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        private bool ContainsDepend(AssetTarget target, bool recursive = true)
        {
            if (_dependParentSet.Contains(target))
                return true;
            if (recursive)
            {
                var e = _dependParentSet.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.ContainsDepend(target, true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void AddDependChild(AssetTarget parent)
        {
            _dependChildrenSet.Add(parent);
        }

        /// <summary>
        /// 我依赖了这个项，那么依赖我的项不需要直接依赖这个项了
        /// </summary>
        private void ClearParentDepend(AssetTarget target = null)
        {
            IEnumerable<AssetTarget> cols = _dependParentSet;
            if (target != null) cols = new AssetTarget[] { target };
            foreach (AssetTarget at in cols)
            {
                var e = _dependChildrenSet.GetEnumerator();
                while (e.MoveNext())
                {
                    AssetTarget dc = e.Current;
                    dc.RemoveDependParent(at);
                }
            }
        }

        /// <summary>
        /// 移除依赖项
        /// </summary>
        /// <param name="target"></param>
        /// <param name="recursive"></param>
        private void RemoveDependParent(AssetTarget target, bool recursive = true)
        {
            _dependParentSet.Remove(target);
            target._dependChildrenSet.Remove(this);

            //recursive
			var dcc = new HashSet<AssetTarget>(_dependChildrenSet);
            var e = dcc.GetEnumerator();
            while (e.MoveNext())
            {
                AssetTarget dc = e.Current;
                dc.RemoveDependParent(target);
            }
        }

        private void RemoveDependChildren()
        {
            var all = new List<AssetTarget>(_dependChildrenSet);
            _dependChildrenSet.Clear();
            foreach (AssetTarget child in all)
            {
                child._dependParentSet.Remove(this);
            }
        }

        /// <summary>
        /// 依赖我的项
        /// </summary>
        public List<AssetTarget> dependsChildren
        {
            get { return new List<AssetTarget>(_dependChildrenSet); }
        }

        int System.IComparable<AssetTarget>.CompareTo(AssetTarget other)
        {
            return other._dependChildrenSet.Count.CompareTo(_dependChildrenSet.Count);
        }

        public string GetHash()
        {
            if (type == AssetType.Builtin)
                return "0000000000";
            else
                return AssetBundleUtils.GetFileHash(file.FullName);
        }

        public void WriteCache(StreamWriter sw)
        {
            sw.WriteLine(this.assetPath);
            sw.WriteLine(GetHash());
            sw.WriteLine(_metaHash);
            sw.WriteLine(this._bundleCrc);
            HashSet<AssetTarget> deps = new HashSet<AssetTarget>();
            this.GetDependencies(deps);
            sw.WriteLine(deps.Count.ToString());
            foreach (AssetTarget at in deps)
            {
                sw.WriteLine(at.assetPath);
            }
        }
    }
}
