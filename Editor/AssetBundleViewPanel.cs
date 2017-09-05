using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using Tangzx.ABSystem;
using System.Linq;
public class AssetBundleViewPanel : EditorWindow
{

    public GameObject obj = null;

    private ReorderableList _list_preload;
    private ReorderableList _list_container;
    private List<string> _preloadNames = new List<string>();
    private List<string> _containerNames = new List<string>();

    private Vector2 _scrollPosition = Vector2.zero;
    
    [MenuItem("ABSystem/Viewer Panel")]
    static void Open()
    {
        GetWindow<AssetBundleViewPanel>("ABSystem", true);
    }

    void OnListPreloadElementGUI(Rect rect, int index, bool isactive, bool isfocused)
    {
        EditorGUI.LabelField(rect, _preloadNames[index]);
    }

    void OnListPreloadHeaderGUI(Rect rect)
    {
        EditorGUI.LabelField(rect, "Asset Filter");
    }

    void OnListContainerElementGUI(Rect rect, int index, bool isactive, bool isfocused)
    {
        EditorGUI.LabelField(rect, _containerNames[index]);
    }

    void OnListContainerHeaderGUI(Rect rect)
    {
        EditorGUI.LabelField(rect, "Asset Filter");
    }

    void InitListDrawer()
    {
        _list_preload = new ReorderableList(_preloadNames, typeof(string));
        _list_preload.drawElementCallback = OnListPreloadElementGUI;
        _list_preload.drawHeaderCallback = OnListPreloadHeaderGUI;
        _list_preload.draggable = true;
        _list_preload.elementHeight = 22;



        _list_container = new ReorderableList(_containerNames, typeof(string));
        _list_container.drawElementCallback = OnListContainerElementGUI;
        _list_container.drawHeaderCallback = OnListContainerHeaderGUI;
        _list_container.draggable = true;
        _list_container.elementHeight = 22;
    }

    void OpenABFile()
    {
        _preloadNames.Clear();
        _containerNames.Clear();
        string dataPath = Application.dataPath + "Assets/ABs";
        string selectedPath = EditorUtility.OpenFilePanel("Path", dataPath, "");



        AssetBundle bundle = AssetBundle.LoadFromFile(selectedPath);

        if (bundle != null)
        {
            SerializedObject so = new SerializedObject(bundle);

            foreach (SerializedProperty d in so.FindProperty("m_PreloadTable"))
            {
                if (d.objectReferenceValue != null)
                    _preloadNames.Add(d.objectReferenceValue.name + " " + d.objectReferenceValue.GetType().ToString());
            }

            foreach (SerializedProperty d in so.FindProperty("m_Container"))
                _containerNames.Add(d.displayName);

            bundle.Unload(true);
        }
    }


    void OnGUI()
    {
        if (_list_preload == null && _list_container == null)
        {
            InitListDrawer();
        }

        if (GUILayout.Button("openAb", EditorStyles.toolbarButton))
        {
            OpenABFile();
        }

        GUILayout.BeginVertical();
        {
            //Filter item list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                EditorGUILayout.PrefixLabel("preloads:");
                _list_preload.DoLayoutList();

                EditorGUILayout.PrefixLabel("containers:");
                _list_container.DoLayoutList();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

    }



    void OpenAsset()
    {
        string dataPath = Application.dataPath +"/Assets/Resources" ;
        string selectedPath = EditorUtility.OpenFilePanel("Path", dataPath, "");
    }
}


public class AssetBundleTestPanel : EditorWindow
{

    
    private ReorderableList _list;
    private List<string> _Data = new List<string>();
    private string _title = "";

    private Vector2 _scrollPosition = Vector2.zero;

    [MenuItem("ABSystem/Viewer Test")]
    static void Open()
    {
        GetWindow<AssetBundleTestPanel>("ABSystem", true);
    }

    void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
    {
        if(index>=_Data.Count)
        {
            Debug.Log("[Shinjiyu|Test] index is out of range:" + index);
            return;
        }
        EditorGUI.LabelField(rect, _Data[index]);
    }

    void OnListHeaderGUI(Rect rect)
    {
        EditorGUI.LabelField(rect, _title+":"+_Data.Count);
    }


    void InitListDrawer()
    {
        _list = new ReorderableList(_Data, typeof(string));
        _list.drawElementCallback = OnListElementGUI;
        _list.drawHeaderCallback = OnListHeaderGUI;
        _list.draggable = false;
        _list.elementHeight = 22;
    }

    void OnGUI()
    {
        if (_list == null)
        {
            InitListDrawer();
        }
        if (GUILayout.Button("getAllObjects", EditorStyles.toolbarButton))
        {
            var _all = AssetDatabase.GetAllAssetPaths();


            //不知道C#的[]和list之间有什么好的转法
            _Data.Clear();
            var temp = _all.ToList();
            foreach (string i in temp)
            {
                if (i.Contains(".ab"))
                {
                    _Data.Add(i);
                }
            }
            _title = "All bundles";

        }

        if (GUILayout.Button("getLoadedbundleNames", EditorStyles.toolbarButton))
        {
            var _all = AssetDatabase.GetAllAssetBundleNames();
            

            //不知道C#的[]和list之间有什么好的转法
            _Data.Clear();
            var temp = _all.ToList();
            foreach(string i in temp)
            {
                _Data.Add(i);
            }
            _title = "All bundles";

        }
//         if (GUILayout.Button("getAllbundleName", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
//             var bundlename = from s in _all
//                              let a = s.bundleName
//                              select a;
// 
//             //不知道C#的[]和list之间有什么好的转法
//             bundlename = bundlename.Distinct();
//             _Data.Clear();
//             foreach(string s in bundlename)
//             {
//                 _Data.Add(s);
//             }
//             _title = "All bundles";
// 
//         }
// 
//         //输出所有的打包关系
//         if (GUILayout.Button("1)listAllAssetName", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
//             
//             _Data.Clear();
//             foreach (AssetTarget s in _all)
//             {
//                 _Data.Add(s.assetPath);
//             }
//             _title = "All Assets";
// 
//             FileStream fs = new FileStream("listAllAssetName.txt", FileMode.OpenOrCreate);
//             
//             StreamWriter sw = new StreamWriter(fs);
//             foreach(AssetTarget s in _all)
//             {
//                 sw.WriteLine(s.assetPath+"|"+s.bundleName);
//             }
//             sw.Close();//输出所有的打包关系
//         
//         }
//         if (GUILayout.Button("1.1)listAllAssetName(with bundlename)", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
//             
//             _Data.Clear();
//             foreach (AssetTarget s in _all)
//             {
//                 if(s.bundleName.Length>0)
//                 {
//                     _Data.Add(s.assetPath);
//                 }
//             }
//             _title = "All Assets";
//         }
// 
// 
//         if (GUILayout.Button("2)listAllAssetFromBundle", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
// 
//             _Data.Clear();
//             FileStream fs = new FileStream("listAllAssetInBundle.txt", FileMode.OpenOrCreate);
//             StreamWriter sw = new StreamWriter(fs);
//             
//             var bundlename = from s in _all
//                              let a = s.bundleName
//                              select a;
//             bundlename = bundlename.Distinct();
//             foreach (string s in bundlename)
//             {
//                 AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/StreamingAssets/AssetBundles/" + s);
//                 if(bundle)
//                 {
//                     string[] o = bundle.GetAllAssetNames();
//                     
//                     foreach(string oo in o)
//                     {
//                         _Data.Add(oo);
//                         sw.WriteLine(oo+"|"+s);    
//                     }
//                     string[] o2 = bundle.GetAllScenePaths();
//                     foreach (string oo in o2)
//                     {
//                         _Data.Add(oo);
//                         sw.WriteLine(oo + "|" + s);
//                     }
//                     bundle.Unload(true);
//                 }
//             }
//             
//             _title = "All Assets in bundle";
//             sw.Close();
//         }
// 
// 
//         //
//         if (GUILayout.Button("3)listAllObjects", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
// 
// 
//             var bundlename = from s in _all
//                              let a = s.bundleName
//                              select a;
//             bundlename = bundlename.Distinct();
//             _Data.Clear();
//             
//             foreach(string s in bundlename)
//             {
//                 if(s.Length ==0)
//                 {
//                     continue;
//                 }
// 
//                 AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath+"/StreamingAssets/AssetBundles/"+s);
// 
//                 if (bundle != null)
//                 {
//                     SerializedObject so = new SerializedObject(bundle);
// 
// //                     foreach (SerializedProperty d in so.FindProperty("m_PreloadTable"))
// //                     {
// //                         if (d.objectReferenceValue != null)
// //                             _Data.Add("p:"+d.objectReferenceValue.name + "|" + d.objectReferenceValue.GetType().ToString()+"|"+s);
// //                     }
// 
//                     foreach (SerializedProperty d in so.FindProperty("m_Container"))
//                         _Data.Add("c:"+d.displayName+"| |"+s);
// 
//                     bundle.Unload(true);
//                 }
//                 else
//                 {
//                     Debug.Log("listAllAssetBundleInnerObj LOAD FAILED:" + s);
//                 }
//             }
// 
//             FileStream fs = new FileStream("listAllObjectName.txt", FileMode.OpenOrCreate);
// 
//             StreamWriter sw = new StreamWriter(fs);
//             foreach (string s in _Data)
//             {
//                 sw.WriteLine(s);
//             }
//             sw.Close();//输出所有的打包关系
// 
//             _title = "All Assets";
// 
//         }
//         if (GUILayout.Button("*)list assets", EditorStyles.toolbarButton))
//         {
// 
//             Object[] assets = AssetDatabase.LoadAllAssetsAtPath("D:/FrontierOfficialTest/Assets/ABs/BuildPlayer-MainScene.sharedAssets");
//             _Data.Clear();
//             foreach(Object o in assets)
//             {
//                 _Data.Add(o.name+"|"+o.GetType().ToString());
//             }
//             _title = "inner shared assets";
//         }
// 
//         if (GUILayout.Button("listAllAssetBundleInnerObjEx", EditorStyles.toolbarButton))
//         {
//             var _all = AssetBundleUtils.GetAll();
// 
// 
// 
//             var bundlename = from s in _all
//                              let a = s.bundleName
//                              select a;
//             bundlename = bundlename.Distinct();
//             _Data.Clear();
// 
//             foreach (string s in bundlename)
//             {
// 
//                 if (s.Length == 0)
//                 {
//                     continue;
//                 }
// 
//                 AssetBundle bundle = AssetBundle.LoadFromFile(Application.dataPath + "/StreamingAssets/AssetBundles/" + s);
// 
//                 if (bundle != null)
//                 {
//                     SerializedObject so = new SerializedObject(bundle);
// 
// //                     foreach (SerializedProperty d in so.FindProperty("m_PreloadTable"))
// //                     {
// //                         if (d.objectReferenceValue != null)
// //                             _Data.Add("p:" + d.objectReferenceValue.name + " " + d.objectReferenceValue.GetType().ToString());
// //                     }
// 
//                     foreach (SerializedProperty d in so.FindProperty("m_Container"))
//                         _Data.Add("c:" + d.displayName);
// 
//                     bundle.Unload(true);
//                 }
//                 else
//                 {
//                     Debug.Log("listAllAssetBundleInnerObjEx LOAD FAILED:" + s);
//                 }
//             }
//             var temp = _Data.Distinct().ToList();
//             _Data.Clear();
// 
//             foreach(string s in temp)
//             {
//                 _Data.Add(s);
//             }
//             
//             _title = "All Assets";
// 
//         }
// 
//         if (GUILayout.Button("listAllAssetInnerObj", EditorStyles.toolbarButton))
//         {
//             _Data.Clear();
//             var _all = AssetBundleUtils.GetAll();
//             foreach(AssetTarget t in _all)
//             {
//                 Object[] ol = AssetDatabase.LoadAllAssetsAtPath(t.assetPath);
//                 foreach(Object o in ol)
//                 {
//                     if (o != null)
//                     {
//                         _Data.Add(o.name + " " + o.GetType().ToString());
//                     }
//                 }
//             }
// 
//         }


        GUILayout.BeginVertical();
        {
            //Filter item list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                _list.DoLayoutList();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

    }

}
