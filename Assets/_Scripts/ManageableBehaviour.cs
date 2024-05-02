using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public abstract class ManageableBehaviour : MonoBehaviour
{
    public abstract IEnumerator DayStarted(int day);

    private static readonly HashSet<ManageableBehaviour> instances = new HashSet<ManageableBehaviour>();

    public static HashSet<ManageableBehaviour> Instances => new HashSet<ManageableBehaviour>(instances);

    protected virtual void Awake()
    {
        instances.Add(this);
    }

    protected virtual void OnDestroy()
    {
        instances.Remove(this);
    }
}
