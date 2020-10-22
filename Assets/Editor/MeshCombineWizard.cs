using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class MeshCombineWizard : ScriptableWizard
{
    public GameObject combineParent;
    public bool is32bit = true;
    public string nameOfPrefab = "test";

    [MenuItem("Prefab Helper/Mesh Combine Wizard")]
    static void CreateWizard()
    {
        var wizard = DisplayWizard<MeshCombineWizard>("Mesh Combine Wizard");

        // If there is selection, and the selection of one Scene object, auto-assign it
        var selectionObjects = Selection.objects;
        if (selectionObjects != null && selectionObjects.Length == 1)
        {
            var firstSelection = selectionObjects[0] as GameObject;
            if (firstSelection != null)
            {
                wizard.combineParent = firstSelection;
            }
        }
    }

    void OnWizardCreate()
    {
        // Verify there is existing object root, ptherwise bail.
        if (combineParent == null)
        {
            Debug.LogError("Mesh Combine Wizard: Parent of objects to combne not assigned. Operation cancelled.");
            return;
        }

        // Locals
        Dictionary<Material, List<MeshCollider>> materialToMeshColliderList = new Dictionary<Material, List<MeshCollider>>();
        List<GameObject> combinedObjects = new List<GameObject>();

        /*
         * List<MeshCollider> meshColliders = new List<MeshCollider>();
        foreach(Transform child in combineParent.transform)
        {
            MeshCollider childCollider = child.gameObject.GetComponent<MeshCollider>();
            if (childCollider) meshColliders.Add(childCollider);
        }
        */
        MeshCollider[] meshColliders = combineParent.GetComponentsInChildren<MeshCollider>();
        Debug.Log(meshColliders.Length);
        // Go through all mesh filters and establish the mapping between the materials and all mesh filters using it.
        Material[] materials = meshColliders[0].GetComponent<Renderer>().materials;
        foreach (MeshCollider meshCollider in meshColliders)
        {
            Mesh mesh = meshCollider.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("The Mesh Collider on object " + meshCollider.name + " has no Mesh component attached. Skipping.");
                continue;
            }
            
            if (materials == null)
            {
                Debug.LogWarning("The collider object " + meshCollider.name + " has no material assigned. Skipping.");
                continue;
            }

            // If there are multiple materials on a single mesh, cancel.
            if (materials.Length > 1)
            {
                Debug.LogError("Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each. Operation cancelled.");
                return;
            }
            var material = materials[0];

            // Add material to mesh filter mapping to dictionary
            if (materialToMeshColliderList.ContainsKey(material)) materialToMeshColliderList[material].Add(meshCollider);
            else materialToMeshColliderList.Add(material, new List<MeshCollider>() { meshCollider });
        }

        // For each material, create a new merged object, in the scene and in the assets folder.
        foreach (var entry in materialToMeshColliderList)
        {
            List<MeshCollider> meshesWithSameMaterial = entry.Value;
            // Create a convenient material name
            string materialName = entry.Key.ToString().Split(' ')[0];

            CombineInstance[] combine = new CombineInstance[meshesWithSameMaterial.Count];
            for (int i = 0; i < meshesWithSameMaterial.Count; i++)
            {
                combine[i].mesh = meshesWithSameMaterial[i].sharedMesh;
                combine[i].transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
            }

            // Create a new mesh using the combined properties
            var format = is32bit ? IndexFormat.UInt32 : IndexFormat.UInt16;
            Mesh combinedMesh = new Mesh { indexFormat = format };
            combinedMesh.CombineMeshes(combine);

            // Create asset
            materialName = nameOfPrefab;
            AssetDatabase.CreateAsset(combinedMesh, "Assets/Prefab/Meshes/CombinedMeshes_" + materialName + ".asset");
            AssetDatabase.SaveAssets();
        }

        // If there were more than one material, and thus multiple GOs created, parent them and work with result
        GameObject resultGO = null;
        if (combinedObjects.Count > 1)
        {
            resultGO = new GameObject("CombinedMeshes_" + combineParent.name);
            foreach (var combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
        }
        else
        {
            Debug.Log(combinedObjects.Count);
            //resultGO = combinedObjects[0];
        }

        // Create prefab
       //PrefabUtility.SaveAsPrefabAsset(resultGO, "Assets/Prefab/" + resultGO.name + ".prefab");
    }
}