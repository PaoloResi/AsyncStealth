using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public string ID;
    public Vector3 position;
    public Quaternion rotation;

    public BuildingData(string ID, Vector3 position, Quaternion rotation){
        this.ID = ID;
        this.position = position;
        this.rotation = rotation;
    }
}

[Serializable]
public class BuildingDataList
{
    public List<BuildingData> buildings = new List<BuildingData>();
}

[Serializable]
public class SavesList
{
    public List<BuildingDataList> Saves = new List<BuildingDataList>(); 
}
