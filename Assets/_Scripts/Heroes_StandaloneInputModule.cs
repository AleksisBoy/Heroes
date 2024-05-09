using UnityEngine;
using UnityEngine.EventSystems;

public class Heroes_StandaloneInputModule : StandaloneInputModule
{
    public GameObject GetCurrent()
    {
        return GetCurrentFocusedGameObject();
    }
}
