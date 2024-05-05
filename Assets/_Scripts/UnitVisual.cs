using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVisual : MonoBehaviour
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private float moveSpeed = 3f;

    public Animator Animator => animator;
    public float MoveSpeed => moveSpeed;
    private void Start()
    {
        animator.Play("Idle", 0, Random.Range(0f, 1f));
    }
}
