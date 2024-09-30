using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class testcontroller : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Animator animator;
    [SerializeField] Toggle t0;
    [SerializeField] Toggle t1;
    [SerializeField] Toggle t2;
    [SerializeField] Toggle t3;
    
    public void ChangeWeapon()
    {
        animator.SetFloat("WeaponType", (int)slider.value);

        float tt = animator.GetFloat("WeaponType");
    }

    public void T0()
    {
        animator.SetBool("IsExistTarget", t0.isOn);
    }

    public void T1()
    {
        animator.SetBool("IsExistNextWayPoint", t1.isOn);
    }

    public void T2()
    {
        animator.SetBool("IsReachPoint", t2.isOn);
    }

    public void T3()
    {
        animator.SetTrigger("Fire");
    }
}
