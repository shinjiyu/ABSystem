using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

public class AssetBundleCreateSingleAssetBundle : EditorWindow
{

    public GameObject obj = null;

    private ReorderableList _list;
    private List<Object> _AssetBundleNames = new List<Object>(); 
    
    [MenuItem("ABSystem/Create Single Bundle")]
    static void Open()
    {
        GetWindow<AssetBundleCreateSingleAssetBundle>("ABSystem/Create Single Bundle", true);
    }

    void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
    {
        _AssetBundleNames[index] = (Object)EditorGUI.ObjectField(rect, "Find Dependency", _AssetBundleNames[index], typeof(Object));
    }

    void OnListHeaderGUI(Rect rect)
    {
        EditorGUI.LabelField(rect, "Asset Filter");
    }

    void InitFilterListDrawer()
    {
        _list = new ReorderableList(_AssetBundleNames, typeof(Object));
        _list.drawElementCallback = OnListElementGUI;
        _list.drawHeaderCallback = OnListHeaderGUI;
        _list.draggable = true;
        _list.elementHeight = 22;
        _list.onAddCallback = (list) => _AssetBundleNames.Add(null);
    }

    void BuildSingleBundle()
    {
        AssetBundleBuild[] buildMap  = new AssetBundleBuild[2];
 
        buildMap[0].assetBundleName = "resources";//打包的资源包名称 随便命名
        
        List<string> GObjNames = new List<string>();
        foreach (Object obj in _AssetBundleNames)
        {
            if(obj!=null)
            {
                string str = AssetDatabase.GetAssetPath(obj);
                GObjNames.Add(str);
            }
        }
        buildMap[0].assetNames = GObjNames.ToArray();

        BuildPipeline.BuildAssetBundles("Assets/ABs", buildMap, BuildAssetBundleOptions.ChunkBasedCompression|BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }

    void OnGUI()
    {
        if (_list == null)
        {
            InitFilterListDrawer();
        }
        GUILayout.BeginVertical();
        {
            _list.DoLayoutList();
        }
        GUILayout.EndVertical();
        if (GUILayout.Button("open", EditorStyles.toolbarButton))
        {
            BuildSingleBundle();
        }
    }
}
