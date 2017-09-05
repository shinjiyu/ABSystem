using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Tangzx.ABSystem
{
    //�����Ҫ��дԭ��UTIL�࣬ԭ��������Ϊutil��Ӧ�ô�״̬��
    //�������Ϊ������İ����еİ������ؽ��������Կ���ȫ�����ڴ���
    public class AssetDataManager
    {
        /// <summary>
        /// �����е�assets��Ϣ
        /// KEYΪAssets������·��Сд
        /// </summary>
        public Dictionary<string, AssetTarget> AssetsData;

        /// <summary>
        /// AssetsData ��list��ʽ�����ڿ��ٱ�����
        /// �Ҳ�ȷ��DICTIONARY�ı����ǲ�����ĺ���
        /// ����������˺���
        /// </summary>
        public List<AssetTarget> AssetsList;

        /// <summary>
        /// ����bundle����Ϣ
        /// </summary>
        public List<BundleTarget> BundlesData;


        //��ɵ���
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
        /// ������Ҫ�ǵ����ⲿ���������������޸�
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
                    //����ط�������һ��Ҫȥ�������ſ����������asset
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