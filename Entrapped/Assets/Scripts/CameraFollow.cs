using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float smoothSpeed = 0.125f; // The smooth speed (delay effect)
    public Vector3 offset; // Offset of the camera from the player position

    private void FixedUpdate()
    {
        // Target position of the camera with the offset
        Vector3 targetPosition = player.position + offset;

        // Smoothly move the camera towards the target position using Lerp
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        // Keep the camera's Z position constant to avoid clipping issues
        smoothedPosition.z = -10f; // or whatever your desired Z position is

        // Update the camera's position
        transform.position = smoothedPosition;
    }
}
