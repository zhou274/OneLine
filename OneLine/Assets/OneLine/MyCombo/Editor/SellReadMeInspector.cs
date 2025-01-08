#define UAS
//#define CHUPA
//#define SMA

#pragma warning disable 0618

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SellReadMe))]
public class SellReadMeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("1. Edit Game Settings (Admob, In-app Purchase..)", EditorStyles.boldLabel);

        if (GUILayout.Button("Edit Game Settings", GUILayout.MinHeight(40)))
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/OneLine/MyCombo/GameMaster.prefab");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("2. Game Documentation", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Full Documentation", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://docs.google.com/document/d/1AoJyDZyrMsJfWkdxLe7inRieT6voc5nyEmZa3POg_pU/edit?usp=sharing");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("How to create more packages and levels", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://youtu.be/o66hVmwmwug");
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("3. My Other Great Source Codes", EditorStyles.boldLabel);

        if (products != null)
        {
            foreach(var product in products)
            {
                if (GUILayout.Button(product.name, GUILayout.MinHeight(30)))
                {
#if UAS
                    Application.OpenURL(product.uas);
#elif CHUPA
                    Application.OpenURL(product.chupa);
#elif SMA
                    Application.OpenURL(product.sma);
#endif
                }
                EditorGUILayout.Space();
            }
        }
        else
        {
            if (GUILayout.Button("Knife Hit (Ketchapp)", GUILayout.MinHeight(30)))
            {
#if UAS
                Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/knife-hit-115843");
#elif CHUPA
                Application.OpenURL("https://www.chupamobile.com/unity-arcade/knife-hit-unity-clone-20090");
#elif SMA
                Application.OpenURL("https://www.sellmyapp.com/downloads/knife-hit-unity-clone/");
#endif
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Pixel Art - Color by Number", GUILayout.MinHeight(30)))
            {
#if UAS
                Application.OpenURL("https://www.chupamobile.com/unity-arcade/pixel-art-color-by-number-top-free-game-20127");
#elif CHUPA
                Application.OpenURL("https://www.chupamobile.com/unity-arcade/pixel-art-color-by-number-top-free-game-20127");
#elif SMA
                Application.OpenURL("https://www.sellmyapp.com/downloads/pixel-art-color-by-number-top-free-game/");
#endif
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Sudoku Pro", GUILayout.MinHeight(30)))
            {
#if UAS
                Application.OpenURL("https://www.chupamobile.com/unity-arcade/sudoku-pro-20433");
#elif CHUPA
                Application.OpenURL("https://www.chupamobile.com/unity-arcade/sudoku-pro-20433");
#elif SMA
                Application.OpenURL("https://www.sellmyapp.com/downloads/sudoku-pro/");
#endif
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.LabelField("4. Contact Us For Support", EditorStyles.boldLabel);
        EditorGUILayout.TextField("Email: ", "moana.gamestudio@gmail.com");
    }

    private List<MyProduct> products;
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("my_products"))
            products = JsonUtility.FromJson<MyProducts>(PlayerPrefs.GetString("my_products")).products;

        var www = new WWW("http://66.45.240.107/myproducts/my_products.json");
        ContinuationManager.Add(() => www.isDone, () =>
        {
            if (!string.IsNullOrEmpty(www.error)) return;
            PlayerPrefs.SetString("my_products", www.text);
            products = JsonUtility.FromJson<MyProducts>(www.text).products;

            Repaint();
        });
    }
}

[System.Serializable]
public class MyProduct
{
    public string name;
    public string uas;
    public string chupa;
    public string sma;
}

public class MyProducts
{
    public List<MyProduct> products;
}
