/* Written by Marshall Nystrom on July 6th, 2023 as a supplementary addition to the project.
 * This script allows the camera to zoom in and out and pivot around the central point of
 * the boundary cube.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlScript : MonoBehaviour
{
    Vector3 initialPosition;
    Quaternion initialRotation;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.A) || (Input.GetKey(KeyCode.LeftArrow)))
        {

        }
        if (Input.GetKey(KeyCode.D) || (Input.GetKey(KeyCode.RightArrow)))
        {

        }
        if (Input.mouseScrollDelta != Vector2.zero)
        {

        }

    }
}
