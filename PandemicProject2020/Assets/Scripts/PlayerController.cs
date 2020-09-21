using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
//using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    //Changable variables in the unity editor. 
    //Speed is character movement speed
    //JumpForce is the amount of force added to a jump.  
    //JumpCounter will be used to keep track of multiple jumps for double jump and is able to be adjusted with Unity editor.
    //JumpTime will be used to allow the player to do quick jumps or longer jumps by holding the jump key
    //isGrounded is a check to see if the player is standing on any object with tag Ground. 
    public float Speed = 0;
    public float JumpForce = 0;
    public int extraJumps = 2;
    public float JumpTime;
    public bool isGrounded = false;
    public LayerMask groundLayer;

    //Private variables to setup basic parts of the character object so that I can use the different attributes associated to the player. 
    //rb to access the attributes associated to the rigidbody
    //Sprite to access the attributes associated to the sprite renderer
    private Rigidbody2D rb;
    private SpriteRenderer Sprite;
    private int JumpCounterValue;
    private float JumpTimeCounter;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();
        JumpCounterValue = extraJumps;
    }

    private void Update()
    {
        //Jump() checked every update to ensure jump is responsive
        Jump();
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, groundLayer);

        //Checks horizontal input every update to ensure sprite is facing correct way. 
        if (Input.GetAxis("Horizontal") > 0 )
        {
            Sprite.flipX = true;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            Sprite.flipX = false;
        }


    }

    // FixedUpdate called at set intervals for physic calculations.  
    void FixedUpdate()
    {
        
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        
        transform.position += movement * Time.deltaTime * Speed;
                
    }

    void Jump()
    {
        //resets jump counter to public value when grounded
        if (isGrounded)
        {
            extraJumps = JumpCounterValue;
            JumpTimeCounter = JumpTime;
        }

        //Initial jump checks to see if player is on ground and number of jumps available is greater than zero
        if (Input.GetButtonDown("Jump") && isGrounded == true && extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            extraJumps--;        
        } 

        //Double jump check
        else if (Input.GetButtonDown("Jump") && extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            extraJumps--;
        }
    }
}
