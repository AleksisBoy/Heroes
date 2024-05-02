using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroesInputModule : StandaloneInputModule
{
    public static HeroesInputModule Module { get; private set; }
    protected override void Awake()
    {
        if (Module == null) Module = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public GameObject GetFocused()
    {
        return GetCurrentFocusedGameObject();
    }
}
