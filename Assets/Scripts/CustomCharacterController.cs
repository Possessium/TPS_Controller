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

    float animationBlend;
    #region Animation hash
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    #endregion

    float speed;
    float targetRotation;
    float rotationVelocity;
    float verticalVelocity;

    bool grounded;
    bool isFalling;
    bool isJumping;
    private float terminalVelocity = 53;


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
        JumpAndGravity();
        GroundCheck();
        Move();
    }

    private void Move()
    {
        float _targetSpeed = input.Sprint ? values.SprintSpeed : values.MoveSpeed;

        if (input.Move == Vector2.zero)
            _targetSpeed = 0;

        float _horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (_horizontalVelocity < _targetSpeed - .1f || _horizontalVelocity > _targetSpeed + .1f)
        {
            speed = Mathf.Lerp(_horizontalVelocity, _targetSpeed, Time.deltaTime * values.SpeedChangeRate);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
            speed = _targetSpeed;

        animationBlend = Mathf.Lerp(animationBlend, _targetSpeed, Time.deltaTime * values.SpeedChangeRate);

        Vector2 _inputs = input.Move;

        if (_inputs != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(_inputs.x, _inputs.y) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float _rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, values.RotationSmoothTime);
            transform.rotation = Quaternion.Euler(new Vector3(0, _rotation, 0));
        }

        Vector3 _direction = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        controller.Move(_direction * (speed * Time.deltaTime) + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, 1);
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            isFalling = false;

            animator.SetBool(animIDJump, false);
            animator.SetBool(animIDFreeFall, false);

            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (input.Jump && isJumping)
            {
                verticalVelocity = Mathf.Sqrt(values.JumpHeight * -2f * values.Gravity);

                animator.SetBool(animIDJump, true);
            }

            if (!isJumping)
                isJumping = true;
        }
        else
        {
            isJumping = false;

            if (!isFalling)
                isFalling = true;
            else
                animator.SetBool(animIDFreeFall, true);

            input.Jump = false;
        }

        if (verticalVelocity < terminalVelocity)
            verticalVelocity += values.Gravity * Time.deltaTime;
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundCheck()
    {
        Vector3 _spherePosition = new Vector3(transform.position.x, transform.position.y - values.GroundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(_spherePosition, values.GroundedRadius, values.GroundLayers, QueryTriggerInteraction.Ignore);

        animator.SetBool(animIDGrounded, grounded);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - values.GroundedOffset, transform.position.z), values.GroundedRadius);
    }
}
