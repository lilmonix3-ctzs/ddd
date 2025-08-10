using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputActions InputActions;

    private void Awake()
    {
        Instance = this;
        InputActions = new InputActions();
        InputActions.Player.Enable();
    }

    public Vector2 GetMovementInputnormalized()
    {
        return InputActions.Player.Move.ReadValue<Vector2>().normalized;
    }

    public Vector2 GetAimDir()
    {
        return InputActions.Player.Aim.ReadValue<Vector2>().normalized;
    }

    //°´ÏÂÉÁ±Ü¼ü
    public bool IsDodgeClicked()
    {
        return InputActions.Player.Dodge.triggered;
    }

    public bool IsAttackPressed()
    {
        return InputActions.Player.Attack.IsPressed();
    }

    public bool IsComfirmClicked()
    {
        return InputActions.Player.Comfirm.triggered;
    }
}
