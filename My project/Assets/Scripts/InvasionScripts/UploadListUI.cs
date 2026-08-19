using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UploadListUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject buttonPrefab;

    public void Awake()
    {
        Refresh();
    }
    public void Refresh()
    {
        foreach(Transform child in content) Destroy(child.gameObject);

        List<BuildingDataList> uploadedSaves = GameManager.instance.savesList.Saves;

        for (int i = 0; i< uploadedSaves.Count; i++)
        {
            int index = i;
            GameObject go = Instantiate(buttonPrefab, content);

            go.GetComponentInChildren<TextMeshProUGUI>().text = $"Save {index + 1}";
            go.GetComponent<Button>().onClick.AddListener(() => InvasionManager.instance.Load(index));
        }

    }
}
