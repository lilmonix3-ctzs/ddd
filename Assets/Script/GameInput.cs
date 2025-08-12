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

    public Vector2 GetMovementInputnormalized() => InputActions.Player.Move.ReadValue<Vector2>().normalized;

    public Vector2 GetAimDir() => InputActions.Player.Aim.ReadValue<Vector2>().normalized;

    //°´ÏÂÉÁ±Ü¼ü
    public bool IsDodgeClicked() => InputActions.Player.Dodge.triggered;

    public bool IsAttackPressed() => InputActions.Player.Attack.IsPressed();

    public bool IsAttackClick() => InputActions.Player.Attack.triggered;

    public bool IsComfirmClicked() => InputActions.Player.Comfirm.triggered;

    public bool IsSwitchWeaponClicked() => InputActions.Player.SwitchWeapon.triggered;
}
