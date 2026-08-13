using System.Collections.Generic;
using UnityEngine;

public class PatrolFinder : MonoBehaviour
{
    public List<List<PatrolIdentity>> patrolGroups = new List<List<PatrolIdentity>>();

    public List<PatrolIdentity> FindStartPoints()
    {
        List<PatrolIdentity> allPatrolPoints = new List<PatrolIdentity>();
        List<PatrolIdentity> startPoints = new List<PatrolIdentity>();

        foreach (PatrolIdentity patrolPoint in FindObjectsByType<PatrolIdentity>(FindObjectsSortMode.None))
        {
            allPatrolPoints.Add(patrolPoint);
        }

        foreach (PatrolIdentity patrolPoint in allPatrolPoints)
        {
            if (patrolPoint.previousPoint == null)
            {
                startPoints.Add(patrolPoint);
            }
        }

        foreach (PatrolIdentity startPoint in allPatrolPoints)
        {
            print(startPoint);
        }

        return startPoints;
    }
}
