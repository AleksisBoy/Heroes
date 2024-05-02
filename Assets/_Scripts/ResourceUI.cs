using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resourceCountText = null;
    [SerializeField] private ResourceType resource;

    public ResourceType Resource { get { return resource; } }
    public void UpdateCount(int count)
    {
        resourceCountText.text = count.ToString();
    }
}
