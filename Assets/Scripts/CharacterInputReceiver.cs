using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputReceiver : MonoBehaviour
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Jump;
    public bool Sprint;

    public void OnMove(InputValue _value) => Move = _value.Get<Vector2>();
    public void OnLook(InputValue _value) => Look = _value.Get<Vector2>();
    public void OnJump(InputValue _value) => Jump = _value.isPressed;
    public void OnSprint(InputValue _value) => Sprint = _value.isPressed;
}
