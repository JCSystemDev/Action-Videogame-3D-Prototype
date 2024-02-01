using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Enemy_02_Shoot : MonoBehaviour
{
    public Transform shootingPoint;
    public GameObject damageOrb;
    private Character _cc;
    public AudioSource shootSFX;

    private void Awake()
    {
        _cc = GetComponent<Character>();
    }

    public void ShootTheDamageOrb()
    {
        Instantiate(damageOrb, shootingPoint.position, Quaternion.LookRotation(shootingPoint.forward));
        shootSFX.Play();
    }

    private void Update()
    {
        _cc.RotateToTarget();
    }
}
