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

    [Header("Death Settings")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float fadeDelay = 1f;

    private float deathTimer = 0f;
    private bool isDead = false;
    private Transform killer;

    private bool endingOne = false;

    private bool isCrouched = false;
    private bool isCrouchingTransition = false;
    private float crouchTimer = 0.5f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private CharacterController characterController;

    private bool canMove = true;

    void OnEnable()
    {
        Enemy_AI_Script.OnPlayerKilled += HandleDeath;
        Enemy_AI_Script.OnBadEnding += disable;
    }

    void OnDisable()
    {
        Enemy_AI_Script.OnPlayerKilled -= HandleDeath;
        Enemy_AI_Script.OnBadEnding -= disable;
    }

    void OnDestroy()
    {
        Enemy_AI_Script.OnPlayerKilled -= HandleDeath;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void disable()
    {       
        endingOne = true;
        crouchSpeed = 0f;
        lookSpeed = 0f;    
        walkSpeed = 0f;
    }


    void Update()
    {
        if (endingOne)
        {
            badEnding();
        }
        if (isDead)
        {
            HandleDeathCamera();
            return; //HARD STOP everything else
        }

        HandleMovement();
        HandleLook();
        HandleCrouch();
    }

    private void badEnding()
    {
        StartCoroutine(TurnAfterDelay(2f)); //drop after delay
    }

    private IEnumerator TurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HandleDeathCamera();

    }


    // Movement

    void HandleMovement()
    {
        if (!canMove) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");

        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && characterController.isGrounded)
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


    // Look
    void HandleLook()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
    }

    // Crouch

    void HandleCrouch()
    {
        if (!canMove) return;

        if (Input.GetKey(KeyCode.R) && !isCrouchingTransition)
        {
            if (isCrouched)
            {
                if (CanStandUp())
                {
                    isCrouched = false;
                    isCrouchingTransition = true;
                }
            }
            else
            {
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


    // Death Handling
    void HandleDeath(Transform enemy)
    {
        if (isDead) return;

        isDead = true;
        killer = enemy;
        canMove = false;

        deathTimer = fadeDelay;

        //HARD STOP movement completely
        moveDirection = Vector3.zero;

        if (characterController != null)
            characterController.Move(Vector3.zero);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleDeathCamera()
    {
        if (killer != null)
        {
            //Raise the look target (note to self: adjust this value when height changes)
            Vector3 targetPos = killer.position + Vector3.up * 5f;

            Vector3 dir = (targetPos - playerCamera.transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);

            playerCamera.transform.rotation = Quaternion.Slerp(
                playerCamera.transform.rotation,
                targetRot,
                Time.deltaTime * 10f
            );
        }

        if (deathTimer > 0)
        {
            deathTimer -= Time.deltaTime;
            return;
        }

        if (blackScreen != null)
        {
            blackScreen.alpha += Time.deltaTime * fadeSpeed;
        }
    }

    // Stand/Crouch Check

    private bool CanStandUp()
    {
        Vector3 start = transform.position + Vector3.up * crouchHeight;
        float checkHeight = defaultHeight - crouchHeight;

        return !Physics.SphereCast(start, characterController.radius, Vector3.up, out RaycastHit hit, checkHeight);
    }
}