using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;
    public bool attackButton;
    public bool slideButton;
    public bool inputSwitch;

    // Update is called once per frame
    void Update()
    {
        if (inputSwitch)
        {
            if (!attackButton && Time.timeScale != 0)
            {
                attackButton = Input.GetButtonDown("Attack");
            }

            if (!slideButton && Time.timeScale != 0)
            {
                slideButton = Input.GetButtonDown("Slide");
            }
        
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }
        
    }

    public void OnDisable()
    {
        ClearCache();
    }

    public void ClearCache()
    {
        attackButton = false;
        slideButton = false;
        horizontalInput = 0;
        verticalInput = 0;
    }
}
