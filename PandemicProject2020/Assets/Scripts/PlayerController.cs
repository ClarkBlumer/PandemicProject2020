﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
//using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public float Speed = 0;
    public bool isGrounded = false;

    private Rigidbody2D rb;
    private SpriteRenderer Sprite;
    private float movementX;
    private float movementY;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Jump();
        if (Input.GetKeyDown(KeyCode.D))
        {
            Sprite.flipX = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Sprite.flipX = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Jump();
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        
        transform.position += movement * Time.deltaTime * Speed;
                
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded == true)
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 5f), ForceMode2D.Impulse);
        }
    }
}
