using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    [SerializeField] private CharacterMovementValues values;
    private Animator animator;
    private CharacterController controller;
    private CharacterInputReceiver input;
    private GameObject mainCamera;

    #region Animation hash
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    #endregion

    float speed;
    float rotationVelocity;
    float verticalVelocity;

    [SerializeField] float deb;


    private void Start()
    {
        TryGetComponent(out animator);
        TryGetComponent(out controller);
        TryGetComponent(out input);

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        AssignAnimationIDs();
    }

    private void Update()
    {
        deb = verticalVelocity;

        Move();
        Jump();
    }

    private void Move()
    {
        controller.Move(new Vector3(0, verticalVelocity, 0) * values.MoveSpeed);
    }

    private void Jump()
    {
        if(input.Jump && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(values.JumpHeight * -2 * values.Gravity);
            animator.SetBool(animIDJump, true);
            animator.SetBool(animIDFreeFall, false);
        }
        else
            animator.SetBool(animIDJump, false);

        if(!controller.isGrounded)
        {
            verticalVelocity += values.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity, -values.TerminalVelocity, 0);
            animator.SetBool(animIDFreeFall, true);
        }
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

}
