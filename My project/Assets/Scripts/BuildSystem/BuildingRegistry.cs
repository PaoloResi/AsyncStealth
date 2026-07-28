using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Building/Registry")]
public class BuildingRegistry : ScriptableObject
{
    public List<GameObject> prefabs;

    private Dictionary<string, GameObject> lookup;

    public GameObject GetPrefab(string id)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, GameObject>();
            foreach (var prefab in prefabs)
            {
                BuildingIdentity identifier = prefab.GetComponent<BuildingIdentity>();

                if (identifier != null)
                {
                    lookup[identifier.buildId] = prefab;
                }
            }
        }

        return lookup.TryGetValue(id, out var result) ? result : null; 
    }

}
