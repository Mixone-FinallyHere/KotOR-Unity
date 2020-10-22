// Create a folder (right click in the Assets directory, click Create>New Folder)
// and name it “Editor” if one doesn’t exist already. Place this script in that folder.

// This script creates a new menu item Examples>Create Prefab in the main menu.
// Use it to create Prefab(s) from the selected GameObject(s).
// It will be placed in the root Assets folder.

using UnityEngine;
using UnityEditor;

public class PrefabHelper
{
    // Creates a new menu item 'Prefab Helper > Create Prefab' in the main menu.
    //[MenuItem("Prefab Helper/Create Prefab")]
    static void CreatePrefab()
    {
        // Keep track of the currently selected GameObject(s)
        GameObject[] objectArray = Selection.gameObjects;

        // Loop through every GameObject in the array above
        foreach (GameObject gameObject in objectArray)
        {
            // Set the path as within the Assets folder,
            // and name it as the GameObject's name with the .Prefab format
            string localPath = "Assets/Prefab/" + gameObject.name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            // Create the new Prefab.
            PrefabUtility.SaveAsPrefabAsset(gameObject, localPath);
            Debug.Log("Saved Prefab to:" + localPath);
            //PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, localPath, InteractionMode.UserAction);
        }
    }

    // Creates a new menu item 'Prefab Helper > Create MeshFilter' in the main menu.
    //[MenuItem("Prefab Helper/Create MeshFilter")]
    static void CreateMeshFilter()
    {
        GameObject active = Selection.activeGameObject;
        MeshFilter mf = active.GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/Prefab/MesheFilters/" + active.name + ".asset";
            Debug.Log("Saved MeshFilter to:" + savePath);
            SaveObjectToFile(mf, savePath);
        }
    }

    // Creates a new menu item 'Prefab Helper > Create MeshFilter' in the main menu.
    [MenuItem("Prefab Helper/Create Mesh")]
    static void CreateMesh()
    {
        GameObject active = Selection.activeGameObject;
        Mesh mf = active.GetComponent<MeshCollider>().sharedMesh;
        if (mf)
        {
            var savePath = "Assets/Prefab/Meshes/" + active.name + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            SaveObjectToFile(mf, savePath);
        }
    }

    // Creates a new menu item 'Prefab Helper > Create Material' in the main menu.
    [MenuItem("Prefab Helper/Create Material")]
    static void CreateMaterial()
    {
        GameObject active = Selection.activeGameObject;
        Shader shader = active.GetComponent<Renderer>().material.shader;
        Material mf = new Material(shader);
        if (mf)
        {
            var savePath = "Assets/Prefab/Materials/" + active.name + ".mat";
            Debug.Log("Saved Material to:" + savePath);                 
            var savePathTex = "Assets/Prefab/Textures/" + active.name + ".png";
            var savePathLight = "Assets/Prefab/Textures/" + active.name + "_lightmap.png";
            //TextureImporter textureImporter = AssetImporter.GetAtPath(savePathLight) as TextureImporter;
            //textureImporter.textureType = TextureImporterType.Lightmap;
            //AssetDatabase.ImportAsset(savePathLight);
            Texture2D new_tex = (Texture2D)AssetDatabase.LoadAssetAtPath(savePathTex, typeof(Texture2D));
            Texture new_light = (Texture)AssetDatabase.LoadAssetAtPath(savePathLight, typeof(Texture));
            mf.mainTexture = new_tex;
            mf.SetTexture("_LightMap", new_light);
            SaveObjectToFile(mf, savePath);
                        
        }
    }

    // Creates a new menu item 'Prefab Helper > Create Texture' in the main menu.
    [MenuItem("Prefab Helper/Create Texture")]
    static void CreateTexture()
    {
        GameObject active = Selection.activeGameObject;        
        Texture2D texComp = active.GetComponent<Renderer>().material.mainTexture as Texture2D;
        Texture2D tex = DeCompress(texComp);
        if (tex)
        {
            var savePathTex = "Assets/Prefab/Textures/" + active.name + ".png";
            SaveTextureAsPNG(tex, savePathTex);
        }
    }

    // Creates a new menu item 'Prefab Helper > Create LightMap' in the main menu.
    [MenuItem("Prefab Helper/Create LightMap")]
    static void CreateLightMap()
    {
        GameObject active = Selection.activeGameObject;
        Texture2D texComp = active.GetComponent<Renderer>().material.GetTexture("_LightMap") as Texture2D;
        Texture2D tex = DeCompress(texComp);
        if (tex)
        {
            var savePathTex = "Assets/Prefab/Textures/" + active.name + "_lighmap.png";
            SaveTextureAsPNG(tex, savePathTex);
        }
    }

    // Disable the menu item if no selection is in place.
    //[MenuItem("Prefab Helper/Create Prefab", true)]
    //[MenuItem("Prefab Helper/Create MeshFilter", true)]
    //[MenuItem("Prefab Helper/Create Mesh", true)]
    [MenuItem("Prefab Helper/Create Material", true)]
    [MenuItem("Prefab Helper/Create Texture", true)]
    [MenuItem("Prefab Helper/Create LightMap", true)]
    static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
    }

    public static void SaveObjectToFile(Object obj, string fileName)
    {
        AssetDatabase.CreateAsset(obj, fileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    public static Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
