using NUnit.Framework;
using UnityEngine;

public class PatrolIdentity : BuildingIdentity
{
    public PatrolIdentity previousPoint = null;
    public PatrolIdentity nextPoint = null;
}
