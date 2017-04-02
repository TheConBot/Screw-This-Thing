using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemGenerator : EditorWindow
{

    private enum Collumn
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }
    private enum SortBy
    {
        Round = Collumn.A,
        Scale = Collumn.B,
        Time = Collumn.C,
        Taps = Collumn.F
    }
    private SortBy sortBy;
    private TextAsset spreadsheet;
    private string savePath;

    [MenuItem("Tools/Item Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemGenerator>(false, "Item Generator", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Options", EditorStyles.boldLabel);
        spreadsheet = (TextAsset)EditorGUILayout.ObjectField("SpreadSheet", spreadsheet, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("Items Folder Path", savePath);
        sortBy = (SortBy)EditorGUILayout.EnumPopup("Item Sorting", sortBy);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate Items"))
        {
            GenerateItems();
        }
    }

    private void GenerateItems()
    {
        if (spreadsheet == null)
        {
            Debug.LogError("No spreadsheet entered, assign in the Item Generator inspector.");
            return;
        }
        else if (!AssetDatabase.IsValidFolder(savePath))
        {
            Debug.LogError("Invalid file path, assign in the Item Generator insepctor.");
            return;
        }
        else
        {
            Debug.Log("***ITEM GENERATION START***");
        }
        string[] lines = spreadsheet.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            Collumn currentCollumn;
            ItemData data = null;
            bool createNewAsset = false;
            int currentRow = i + 1;
            string[] lineComponents = lines[i].Split(',');

            //Check to see if the item already exists or not
            if ((data = AssetDatabase.LoadAssetAtPath<ItemData>(GenerateFullPath(savePath, GenerateFileName(lineComponents[(int)Collumn.E], lineComponents[(int)sortBy])))) == null)
            {
                data = CreateInstance<ItemData>();
                createNewAsset = true;
            }
            currentCollumn = Collumn.A;
            //Check to see if the current row in the spreadsheet is valid
            if (!int.TryParse(lineComponents[(int)currentCollumn], out data.roundNumber))
            {
                DisplayItemError(currentRow, currentCollumn);
                continue;
            }
            currentCollumn = Collumn.B;
            if (!int.TryParse(lineComponents[(int)currentCollumn], out data.itemScale))
            {
                DisplayItemError(currentRow, currentCollumn);
                continue;
            }
            currentCollumn = Collumn.C;
            if (!float.TryParse(lineComponents[(int)currentCollumn], out data.roundTime))
            {
                DisplayItemError(currentRow, currentCollumn);
                continue;
            }
            currentCollumn = Collumn.E;
            if ((data.displayName = lineComponents[(int)currentCollumn]) == "")
            {
                DisplayItemError(currentRow, currentCollumn);
                continue;
            }
            currentCollumn = Collumn.F;
            if (!int.TryParse(lineComponents[(int)currentCollumn], out data.tapGoal))
            {
                DisplayItemError(currentRow, currentCollumn);
                continue;
            }

            //Save the new data or overwrite the old data
            SaveAsset(data, savePath, GenerateFileName(data.displayName, lineComponents[(int)sortBy]), createNewAsset);
            DisplayItemSuccess(currentRow, data);
        }
        RefreshEditor();
        Debug.Log("***ITEM GENERATION COMPLETE***");
    }

    private void SaveAsset(Object asset, string path, string fileName, bool createNewAsset)
    {
        //string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName + ".asset");
        if (createNewAsset) AssetDatabase.CreateAsset(asset, GenerateFullPath(path, fileName));
    }

    private void RefreshEditor()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
    }

    private string GenerateFullPath(string path, string fileName)
    {
        return (path + "/" + fileName + ".asset");
    }

    private string GenerateFileName(string displayName, string sort)
    {
        return sort + ". " + displayName;
    }

    private void DisplayItemError(int row, Collumn collumn)
    {
        Debug.LogWarning(row + ". Invalid Element\nSpreadsheet Position: " + (row) + ", Collumn " + collumn + "\nStatus: Skipped");
    }

    private void DisplayItemSuccess(int row, ItemData data)
    {
        Debug.Log(string.Format("{0}. Item Generated\nItem Details\n\nDisplay Name: {1}\nRound Number: {2}\nItem Scale: {3}\nRound Time: {4}\nTap Goal: {5}\n" +
            "Status: Success", row, data.displayName, data.roundNumber, data.itemScale, data.roundTime, data.tapGoal));
    }
}