using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemGenerator : EditorWindow {
    [SerializeField]
    public TextAsset spreadsheet;

    [MenuItem("Tools/Item Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemGenerator>(false, "Item Generator", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV Spreadsheet", EditorStyles.boldLabel);
        spreadsheet = (TextAsset)EditorGUI.ObjectField(new Rect(4, 30, position.width - 6, 16), spreadsheet, typeof(TextAsset), false);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate Items"))
        {
            GenerateItems();
        }
    }

    private void GenerateItems()
    {
        if(!spreadsheet)
        {
            Debug.LogError("No spreadsheet entered.");
            return;
        }

        //stuff here
        
    }
}
