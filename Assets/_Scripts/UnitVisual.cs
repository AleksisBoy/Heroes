using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVisual : MonoBehaviour
{
    [SerializeField] private Animator animator = null;

    public Animator Animator => animator;
    private void Start()
    {
        animator.Play("Idle", 0, Random.Range(0f, 1f));
    }
}
