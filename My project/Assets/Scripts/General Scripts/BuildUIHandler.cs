using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class BuildUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject LargeBuildings;

    [SerializeField] private GameObject SmallBuildings;
    [SerializeField] private GameObject enemyBuildings;
    [SerializeField] private GameObject MenuButtons;
    [SerializeField] private GameObject goBackButton;
    

    public void openUI(int num)
    {
        switch (num)
        {
            case 0:
                LargeBuildings.SetActive(true);
                break;
            case 1:
                SmallBuildings.SetActive(true);
                break;
            case 2:
                enemyBuildings.SetActive(true);
                break;
        }

        MenuButtons.gameObject.SetActive(false);
        goBackButton.gameObject.SetActive(true);
    }

    public void closeUI()
    {
        LargeBuildings?.SetActive(false);
        SmallBuildings?.SetActive(false);
        enemyBuildings?.SetActive(false);


        MenuButtons.gameObject.SetActive(true);
        goBackButton.gameObject.SetActive(false);
    } 

}
