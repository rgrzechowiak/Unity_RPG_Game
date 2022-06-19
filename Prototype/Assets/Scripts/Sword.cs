using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PerformAttack()
    {
        animator.SetTrigger("Base_Attack");
    }
}
