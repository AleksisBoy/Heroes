using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float timer = 2f;
    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(timer);

        Destroy(gameObject);
    }
}
