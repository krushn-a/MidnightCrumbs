using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputSystem_Actions GameInputs;
    public event EventHandler OnPauseAction;

    private void Awake()
    {
        Instance = this;
        GameInputs = new InputSystem_Actions();
        GameInputs.Player.Enable();
        GameInputs.Player.Pause.performed += Pause_performed;
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 PlayerInputsNormalized()
    {
        Vector2 PlayerMovementVector = GameInputs.Player.Move.ReadValue<Vector2>();
        PlayerMovementVector = PlayerMovementVector.normalized;

        return PlayerMovementVector;
    }

    public bool PlayerJump()
    {
        bool _isPlayerJumped = GameInputs.Player.Jump.IsPressed();
        return _isPlayerJumped;
    }

    public Vector2 PlayerLook()
    {
        Vector2 PlayerLookVector = GameInputs.Player.Look.ReadValue<Vector2>();
        return PlayerLookVector;
    }
    
    public bool PlayerSprint()
    {
        bool _isPlayerSprinted = GameInputs.Player.Sprint.IsPressed();
        return _isPlayerSprinted;
    }
}
