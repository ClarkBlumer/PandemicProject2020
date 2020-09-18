using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public float Offset = 0;
    
    private Transform playerTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Get the main camera current position to store temp
        Vector3 cameraTempPosition = transform.position;

        //Adjust main camera's temp position to player's position along X-Axis.  Y-axis commented out but that would allow vertical following. 
        //cameraPosition.y = playerTransform.position.y;
        cameraTempPosition.x = playerTransform.position.x;
        
        //Allows for offset of the camera along the X-axis instead of keeping player centered
        cameraTempPosition.x += Offset;
        

        //Set the main camera's temp position to the camera's current position (with the player). 
        transform.position = cameraTempPosition;
    }
}
