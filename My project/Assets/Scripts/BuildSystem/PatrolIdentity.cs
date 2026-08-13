using UnityEngine;

public class PatrolIdentity : BuildingIdentity
{
    public string RouteID;
    public string PointID;
    public string previousPoint = null;
    public string nextPoint = null;
}
