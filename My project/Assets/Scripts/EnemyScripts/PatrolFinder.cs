using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolFinder : MonoBehaviour
{

    public List<PatrolIdentity> FindStartPoints()
    {
        print("entered find start points");
        List<PatrolIdentity> startPoints = new List<PatrolIdentity>();

        foreach (PatrolIdentity patrolPoint in FindObjectsByType<PatrolIdentity>(FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(patrolPoint.previousPoint))
            {
                print("found a start point");
                startPoints.Add(patrolPoint);
            }

        }

        return startPoints;
    }
}
