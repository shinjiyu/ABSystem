

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tangzx.ABSystem
{   
    public class AssetBundleManager : MonoBehaviour
    {

        private bool bInit = false;

        private static AssetBundleManager _instance;
        public static AssetBundleManager Instance
        {
            get{
                return _instance;
            }
        }

        public AssetBundlePathResolver pathResolver = new AssetBundlePathResolver();

        private Dictionary<string, AssetBundleLoader> _loaderCache = new Dictionary<string, AssetBundleLoader>();


        private Action _initCallback;  //ok


        private AssetBundleDataReader _ABDReader;

        public AssetBundleDataReader depInfoReader { get { return _ABDReader; } }

        public  AssetBundleManager()
        {
            _instance = this;
        }

        public static Object ResourceLoadSync(string path)
        {
            AssetBundleManager bundleManager = AssetBundleManager.Instance;
            return bundleManager.LoadSync(path);
        }

        public static void  SceneLoadSync(string path)
        {
            AssetBundleManager bundleManager = AssetBundleManager.Instance;
            bundleManager.LoadSceneSync(path);
        }

        public void Init(Action callback)
        {
            if(bInit)
                return;
            bInit = true;
            _initCallback = callback;
            this.StartCoroutine(LoadAssetBundlesInfo());
        }

        public void Init(Stream ABDStream, Action callback)
        {
            if (ABDStream.Length > 4)
            {
                BinaryReader br = new BinaryReader(ABDStream);
                if (br.ReadChar() == 'A' && br.ReadChar() == 'B' && br.ReadChar() == 'D')
                {
                    if (br.ReadChar() == 'T')
                        _ABDReader = new AssetBundleDataReader();
                    else
                        _ABDReader = new AssetBundleDataBinaryReader();

                    ABDStream.Position = 0;
                    
                    //简单地来好了
                    _ABDReader.Init(pathResolver.GetBundleSourceFile("AssetBundles",false));
                    _ABDReader.Read(ABDStream);
                }
            }

            ABDStream.Close();

            if (callback != null)
                callback();
        }

        void InitComplete()
        {
            if (_initCallback != null)
                _initCallback();
            _initCallback = null;
        }

        IEnumerator LoadAssetBundlesInfo()
        {
            string ABDFile = string.Format("{0}/{1}", pathResolver.BundleCacheDir, pathResolver.A2BMapFileName);
            //编辑器模式下测试AB_MODE，直接读取
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            ABDFile = pathResolver.GetBundleSourceFile(pathResolver.A2BMapFileName, false);
#endif

            if (File.Exists(ABDFile))
            {
                FileStream fs = new FileStream(ABDFile, FileMode.Open, FileAccess.Read);
                Init(fs, null);
                fs.Close();
            }
            else
            {
                string srcURL = pathResolver.GetBundleSourceFile(pathResolver.A2BMapFileName);
                WWW w = new WWW(srcURL);
                yield return w;

                if (w.error == null)
                {
                    Init(new MemoryStream(w.bytes), null);
                    File.WriteAllBytes(ABDFile, w.bytes);
                }
                else
                {
                    Debug.LogError(string.Format("{0} not exist!", ABDFile));
                }
            }
            this.InitComplete();
        }





        private string GetBundleName(string assetPath)
        {

            if(_ABDReader == null)
            {
                return null;
            }

            return _ABDReader.getAssetBundleNameByAssetPath(assetPath);
        }

        private string GetAssetName(string assetPath)
        {
            if (_ABDReader == null)
            {
                return null;
            }

            return _ABDReader.getAssetNameByAssetPath(assetPath);
        }


        public void LoadSceneSync(string path)
        {
            string bundleName = this.GetBundleName(path);
            AssetBundleLoader loader = this.CreateLoader(bundleName);

            if (loader != null)
            {
                loader.LoadBundleSync();
            }
        }

        public Object LoadSync(string path)
        {
//             path = path.ToLower();
//             path = path.Replace('\\', '.');
//             path = path.Replace('/', '.');
//             path = path.Replace(' ', '_');
            if (path.IndexOf("assets") == -1)
            {
                path = "assets/resources/" + path;
            }

            string bundleName = this.GetBundleName(path);
            AssetBundleLoader loader = this.CreateLoader(bundleName);

            if (loader!=null)
            {
                loader.LoadBundleSync();

                return loader.bundle.LoadAsset(GetAssetName(path));
            }
            return null;
        }


        internal AssetBundleLoader CreateLoader(string abFileName)
        {
            AssetBundleLoader loader = null;

            if (_loaderCache.ContainsKey(abFileName))
            {
                loader = _loaderCache[abFileName];
            }
            else
            {

                AssetBundleData data = _ABDReader.getAssetBundleData(abFileName);
                
                if (data == null)
                {
                    return null;
                }

                loader = this.CreateLoader();
                loader.bundleManager = this;
                loader.bundleData = data;

                _loaderCache[abFileName] = loader;
            }

            return loader;
        }

        protected virtual AssetBundleLoader CreateLoader()
        {
            return new MobileAssetBundleLoader();
        }

    }
}
