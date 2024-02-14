using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody objRb;
    public float speed;
    public float lookSpeed;
    private float rotationX = 0f;
    private float rotationY = 0f;
    public float lookXLimit;
    public GameObject playerCamera;

    void Start()
    {
        objRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        rotationY += Input.GetAxis("Mouse X") * lookSpeed;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);

        // Get the forward direction of the camera
        Vector3 cameraForward = playerCamera.transform.forward;
        // Remove the vertical component from the camera's forward direction
        cameraForward.y = 0;
        // Normalize the direction vector
        cameraForward.Normalize();

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = (cameraForward * verticalInput + playerCamera.transform.right * horizontalInput).normalized;

        // Apply movement
        objRb.velocity = moveDirection * speed;
    }
}
