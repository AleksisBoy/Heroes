using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCurve : MonoBehaviour
{
    [SerializeField] private BezierCurve bCurve = null;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private bool reverseCurve = false;
    [SerializeField] private float secondsToReachEnd = 4f;

    private float time = 0;
    private void OnValidate()
    {
        if (secondsToReachEnd <= .1f) secondsToReachEnd = .1f;
    }
    private void Update()
    {
        time += Time.deltaTime / secondsToReachEnd;
        if(reverseCurve) transform.position = bCurve.GetPositionReversedAt(time) + offset;
        else transform.position = bCurve.GetPositionAt(time) + offset;

        if (time > 1f)
        {
            Debug.Log("End of curve reached for " + name);
            enabled = false;
        }
    }
}
