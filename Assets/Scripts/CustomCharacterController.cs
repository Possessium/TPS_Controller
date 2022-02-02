using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterInputReceiver))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public class CustomCharacterController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private CharacterInputReceiver inputs;
    [SerializeField] private CharacterMovementValues movementValues;
    [SerializeField] private Animator animator;
    private GameObject mainCamera;


    // player
    private bool grounded = true;
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    [SerializeField] private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    // timeout deltatime
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;

    private void Start()
    {
        TryGetComponent(out controller);
        TryGetComponent(out inputs);
        TryGetComponent(out animator);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    private void Update()
    {
        JumpAndGravity();
        CheckGround();
        Move();
    }


    private void Move()
    {
        float _speed = inputs.Sprint ? movementValues.SprintSpeed : movementValues.MoveSpeed;

        if (inputs.Move == Vector2.zero)
            _speed = 0;

        float _horizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (_horizontalSpeed < _speed - .1f || _horizontalSpeed > _speed + .1f)
        {
            speed = Mathf.Lerp(_horizontalSpeed, _speed, Time.deltaTime * movementValues.SpeedChangeRate);

            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
            speed = _speed;

        animationBlend = Mathf.Lerp(animationBlend, _speed, Time.deltaTime * movementValues.SpeedChangeRate);

        Vector3 _inputDirection = new Vector3(inputs.Move.x, 0, inputs.Move.y).normalized;

        if(inputs.Move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;

            float _rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, movementValues.RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0, _rotation, 0);
        }

        Vector3 _targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        controller.Move(_targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, 1);
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            fallTimeoutDelta = movementValues.FallTimeout;

            animator.SetBool(animIDJump, false);
            animator.SetBool(animIDFreeFall, false);

            if (verticalVelocity < 0.0f)
                verticalVelocity = -2f;

            if (inputs.Jump && jumpTimeoutDelta <= 0.0f)
            {
                verticalVelocity = Mathf.Sqrt(movementValues.JumpHeight * -2f * movementValues.Gravity);

                animator.SetBool(animIDJump, true);
            }

            if (jumpTimeoutDelta >= 0.0f)
                jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            jumpTimeoutDelta = movementValues.JumpTimeout;

            if(fallTimeoutDelta >= 0.0f)
                fallTimeoutDelta -= Time.deltaTime;
            else
                animator.SetBool(animIDFreeFall, true);

            inputs.Jump = false;
        }

        if(verticalVelocity < terminalVelocity)
            verticalVelocity += movementValues.Gravity * Time.deltaTime;
    }

    private void CheckGround()
    {
        Vector3 _spherePosition = new Vector3(transform.position.x, transform.position.y - movementValues.GroundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(_spherePosition, movementValues.GroundedRadius, movementValues.GroundLayers, QueryTriggerInteraction.Ignore);

        animator.SetBool(animIDGrounded, grounded);
    }
}
