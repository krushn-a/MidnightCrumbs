using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float playerWalkSpeed = 10f;
    [SerializeField] private float playerRunSpeed = 15f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -30f;
    [SerializeField] private GameObject PauseMenu;
    private CharacterController characterController;

    private bool isgamePaused = false;

    private const float THRESHOLD = 0.01f;

    public GameObject CinemachineCameraTarget;
    [SerializeField] private float RotationSpeed = 1.0f;
    [SerializeField] private float TopClamp = 90.0f;
    [SerializeField] private float BottomClamp = -90.0f;

    private float cinemachineTargetPitch;

    private float rotationVelocity;

    private float playerVerticalVelocity;

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        characterController = GetComponent<CharacterController>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        HandleMovement();
        PlayerJump();
    }

    private void LateUpdate()
    {
        HandleCameraMovement();
    }

    private void HandleMovement()
    {
        float PlayerSpeed = gameInput.PlayerSprint() ? playerRunSpeed : playerWalkSpeed;
        Vector2 PlayerInput = gameInput.PlayerInputsNormalized();
        Vector3 PlayerDir = new Vector3(PlayerInput.x, 0, PlayerInput.y);
        Vector3 PlayerMove = transform.forward * PlayerDir.z + transform.right * PlayerDir.x;
        PlayerMove = PlayerMove * PlayerSpeed * Time.deltaTime;
        characterController.Move(PlayerMove);

        playerVerticalVelocity += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, playerVerticalVelocity * Time.deltaTime, 0));
    }

    private void HandleCameraMovement()
    {
        if (gameInput.PlayerLook().sqrMagnitude >= THRESHOLD)
        {
            cinemachineTargetPitch -= gameInput.PlayerLook().y * RotationSpeed * Time.deltaTime;
            rotationVelocity = gameInput.PlayerLook().x * RotationSpeed * Time.deltaTime;

            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

            transform.Rotate(Vector3.up * rotationVelocity);
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void PlayerJump()
    {
        if (gameInput.PlayerJump() && characterController.isGrounded)
        {
            playerVerticalVelocity = jumpForce;
        }
    }

    private void TogglePauseGame()
    {
        isgamePaused = !isgamePaused;
        if (isgamePaused)
        {
            PauseMenu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            PauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void UnPauseGame()
    {
        isgamePaused = false;
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
