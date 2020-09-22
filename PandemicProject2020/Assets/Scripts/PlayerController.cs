using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditorInternal;
using UnityEngine;
//using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    //Changable variables in the unity editor. 
    //Speed is character movement speed
    //JumpForce is the amount of force added to a jump.  
    //extraJumps will be used to keep track of multiple jumps for double jump and is able to be adjusted with Unity editor.
    //JumpTime will be used to allow the player to do quick jumps or longer jumps by holding the jump key
    //isGrounded is a check to see if the player is standing on any object with tag Ground. 
    [Header("Movements")]
    public float Speed;
    public float JumpForce;
    public int extraJumps;
    public float jumpDelay = 0.25f;

    private int extraJumpCounter;
    private float JumpTimeCounter;

    [Header("Physics")]
    public float gravity = 1;
    public float fallGravity = 5;

    //Private variables to setup basic parts of the character object so that I can use the different attributes associated to the player. 
    //rb to access the attributes associated to the rigidbody
    //Sprite to access the attributes associated to the sprite renderer
    [Header("Components")]
    public LayerMask groundLayer;
    private Rigidbody2D rb;
    private SpriteRenderer Sprite;

    [Header("Collision")]
    public bool isGrounded = false;
    public float groundCheckLength;
    public float raycastOffset;
    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();
        extraJumpCounter = extraJumps;
    }

    private void Update()
    {
        
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckLength, groundLayer);
        
        //Jump() checked every update to ensure jump is responsive
        Jump();

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
            extraJumps = extraJumpCounter;
            JumpTimeCounter = jumpDelay;
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = gravity;
            if(rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * fallGravity;
            }
            else if(rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.gravityScale = gravity * (fallGravity / 2);
            }
        }

        //Initial jump checks to see if player is on ground and number of jumps available is greater than zero
        if (Input.GetButtonDown("Jump") && isGrounded == true && extraJumps > 0)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            //rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            
            extraJumps--;        
        } 

        //Double jump check
        else if (Input.GetButtonDown("Jump") && extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            //rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            rb.gravityScale = fallGravity;
            extraJumps--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckLength);
    }
}
