using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance {get; private set;}    private InputSystem_Actions inputActions;

    public event EventHandler OnAttackAction;
    public event EventHandler OnDashAction;
    public event EventHandler OnBowAttackStarted;
    public event EventHandler OnBowAttackCanceled;private void Awake()
    {
        Instance = this;        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Attack.performed += Attack_performed;
        inputActions.Player.Dash.performed += Dash_performed;
        inputActions.Player.BowAttack.started += BowAttack_started;
        inputActions.Player.BowAttack.canceled += BowAttack_canceled;
    }    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Attack.performed -= Attack_performed;
            inputActions.Player.Dash.performed -= Dash_performed;
            inputActions.Player.BowAttack.started -= BowAttack_started;
            inputActions.Player.BowAttack.canceled -= BowAttack_canceled;
            inputActions.Dispose();
        }
    }

    private void BowAttack_started(InputAction.CallbackContext context)
    {
        Debug.Log("Bow attack started! Right mouse button held.");
        OnBowAttackStarted?.Invoke(this, EventArgs.Empty);
    }

    private void BowAttack_canceled(InputAction.CallbackContext context)
    {
        Debug.Log("Bow attack canceled! Right mouse button released.");
        OnBowAttackCanceled?.Invoke(this, EventArgs.Empty);
    }    

    private void Attack_performed(InputAction.CallbackContext context)
    {
        Debug.Log("Attack performed! Left mouse button clicked.");
        OnAttackAction?.Invoke(this, EventArgs.Empty);
    }

    private void Dash_performed(InputAction.CallbackContext context)
    {
        OnDashAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovmentVector()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized; // Нормализуем для предотвращения диагонального ускорения
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }

    public Vector2 GetMouseWorldPosition()
    {
        // Преобразование экранных координат мыши в мировые
        Vector3 mouseScreenPosition = GetMousePosition();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }
}
