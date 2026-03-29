using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Camera & Look")]
    public Camera playerCamera;
    public float lookSpeed = 2f;
    public float lookXLimit = 70f;

    [Header("Movement")]
    public float walkSpeed = 15f;
    public float runSpeed = 15f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Crouch")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private bool isCrouched = false;
    private bool isCrouchingTransition = false;
    private float crouchTimer = 0.5f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleCrouch();
    }

    // -------------------------
    // Movement
    // -------------------------
    void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    // -------------------------
    // Look
    // -------------------------
    void HandleLook()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
    }

    // -------------------------
    // Crouch
    // -------------------------
    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.R) && canMove && !isCrouchingTransition)
        {
            if (isCrouched)
            {
                // Try to stand up safely
                if (CanStandUp())
                {
                    isCrouched = false;
                    isCrouchingTransition = true;
                }
                else
                {
                    // Optional: play bump sound here
                }
            }
            else
            {
                // Crouch down
                isCrouched = true;
                isCrouchingTransition = true;
            }
        }

        if (isCrouchingTransition)
        {
            crouchTimer -= Time.deltaTime;

            if (crouchTimer <= 0f)
            {
                isCrouchingTransition = false;
                crouchTimer = 0.5f;
            }
        }

        // Apply heights and speeds
        if (isCrouched)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 15f;
            runSpeed = 15f;
        }
    }

    // -------------------------
    // Check if we have room to stand
    // -------------------------
    private bool CanStandUp()
    {
        // Bottom of the standing capsule
        Vector3 start = transform.position + Vector3.up * crouchHeight;
        float checkHeight = defaultHeight - crouchHeight;

        // SphereCast to detect collisions above
        return !Physics.SphereCast(start, characterController.radius, Vector3.up, out RaycastHit hit, checkHeight);
    }
}