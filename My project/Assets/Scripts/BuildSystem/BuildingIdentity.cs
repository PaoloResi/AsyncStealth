using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct BuildingPiece
{
    public Vector3 offset;
    public Vector3 size;
}
public class BuildingIdentity : MonoBehaviour
{
    public string buildId;
    [SerializeField] public List<BuildingPiece> locInfo = new List<BuildingPiece>();
}
