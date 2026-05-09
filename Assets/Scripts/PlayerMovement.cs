using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] bool lockCursor = true;

    [SerializeField] [Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField] [Range(0.0f, 0.5f)] float lookSmoothTime = 0.03f;
    float velocityY = 0f;
    float gravity = -10f;
    

    [SerializeField] Transform playerCamera;
    CharacterController controller = null;

    Vector2 currentMove = Vector2.zero;
    Vector2 currentMoveVelocity = Vector2.zero;

    Vector2 currentMouse = Vector2.zero;
    Vector2 currentMouseVelocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMouseLook();
        PlayerMove();
    }

  

    void UpdateMouseLook()
    {
        Vector2 targetlook = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouse = Vector2.SmoothDamp(currentMouse, targetlook, ref currentMouseVelocity, lookSmoothTime);

        transform.Rotate(Vector3.up * currentMouse.x * mouseSensitivity);
    }
    void PlayerMove()
    {
        Vector2 targetMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetMove.Normalize();
        currentMove = Vector2.SmoothDamp(currentMove, targetMove, ref currentMoveVelocity, moveSmoothTime);

        if (controller.isGrounded)
        
            velocityY = 0f;

        velocityY += gravity * Time.deltaTime;

        Vector3 velocity = (transform.forward * currentMove.y + transform.right * currentMove.x) * moveSpeed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);
    }
}
