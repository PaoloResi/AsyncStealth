using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public string ID;
    public Vector3 position;
    public Quaternion rotation;
    //public string previousPointID = null;

    public BuildingData(string ID, Vector3 position, Quaternion rotation){
        this.ID = ID;
        this.position = position;
        this.rotation = rotation;
    }
}

[Serializable]
public class PatrolData : BuildingData
{
    public string RouteID;
    public string PointID;
    public string previousPointID;
    public string nextPointID;
    public PatrolData(string ID, Vector3 position, Quaternion rotation, string routeID, string pointID,string previousPointID, string nextPointID) : base(ID, position, rotation)
    {
        this.RouteID = routeID;
        this.PointID = pointID;
        this.previousPointID = previousPointID;
        this.nextPointID = nextPointID;
    }
}

[Serializable]
public class BuildingDataList
{
    public List<BuildingData> buildings = new List<BuildingData>();
    //public List<PatrolData> patrols = new List<PatrolData>();
}

[Serializable]
public class SavesList
{
    public List<BuildingDataList> Saves = new List<BuildingDataList>(); 
}
