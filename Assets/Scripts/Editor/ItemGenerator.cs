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
    private TextAsset spreadsheet;
    private string savePath;

    [MenuItem("Tools/Item Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemGenerator>(false, "Item Generator", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Generator", EditorStyles.boldLabel);
        spreadsheet = (TextAsset)EditorGUILayout.ObjectField("SpreadSheet", spreadsheet, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("Items Folder Path", savePath);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate Items"))
        {
            GenerateItems();
        }
    }

    private void GenerateItems()
    {
        if (!spreadsheet)
        {
            Debug.LogError("No spreadsheet entered, assign in the Item Generator inspector.");
            return;
        }
        else if (Path.GetFullPath(savePath) == null)
        {
            Debug.LogError("Invalid file path, assign in the Item Generator insepctor.");
            return;
        }

        string[] lines = spreadsheet.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string[] lineComponents = lines[i].Split(',');
            bool createNewAsset = false;
            int tempInt = 0;
            float tempFloat = 0;
            Collumn currentCollumn = Collumn.A;
            ItemData data = null;

            //Check to see if the current row in the spreadsheet is valid
            if (int.TryParse(lineComponents[CollumnToInt(currentCollumn)], out tempInt))
            {
                //Check to see if the item already exists or not
                if (AssetDatabase.LoadAssetAtPath<ItemData>(GenerateFullPath(savePath, lineComponents[CollumnToInt(Collumn.E)])) != null)
                {
                    data = AssetDatabase.LoadAssetAtPath<ItemData>(GenerateFullPath(savePath, lineComponents[4]));
                }
                else
                {
                    data = CreateInstance<ItemData>();
                    createNewAsset = true;
                }
                //Fill in item parameters
                data.roundNumber = tempInt;

                currentCollumn = Collumn.B;
                if (int.TryParse(lineComponents[CollumnToInt(currentCollumn)], out tempInt))
                {
                    data.itemScale = tempInt;
                }
                else
                {
                    DisplayItemError(i + 1, currentCollumn);
                    return;
                }

                currentCollumn = Collumn.C;
                if (float.TryParse(lineComponents[CollumnToInt(currentCollumn)], out tempFloat))
                {
                    data.roundTime = tempFloat;
                }
                else
                {
                    DisplayItemError(i + 1, currentCollumn);
                    return;
                }

                currentCollumn = Collumn.E;
                if (data.displayName != "")
                {
                    data.displayName = lineComponents[CollumnToInt(currentCollumn)];
                }
                else
                {
                    DisplayItemError(i + 1, currentCollumn);
                    return;
                }

                currentCollumn = Collumn.F;
                if (int.TryParse(lineComponents[CollumnToInt(currentCollumn)], out tempInt))
                {
                    data.tapGoal = tempInt;
                }
                else
                {
                    DisplayItemError(i + 1, currentCollumn);
                    return;
                }
                //Save the new data or overwrite the old data
                SaveAsset(data, savePath, data.roundNumber + ". " + data.displayName, createNewAsset);
            }
            else
            {
                Debug.LogWarning("Row " + (i + 1) + " is invalid, skipping...");
            }
        }

    }

    private void SaveAsset(Object asset, string path, string fileName, bool createNewAsset)
    {
        //string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName + ".asset");
        if (createNewAsset) AssetDatabase.CreateAsset(asset, GenerateFullPath(path, fileName));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        Debug.Log(fileName + " successfully created/updated!");
    }

    private string GenerateFullPath(string path, string fileName)
    {
        return (path + "/" + fileName + ".asset");
    }

    private int CollumnToInt(Collumn collumn)
    {
        return (int)collumn;
    }

    private void DisplayItemError(int row, Collumn collumn)
    {
        Debug.LogError("Something went wrong, check Row " + (row) + ", Collumn " + collumn + ".");
    }
}