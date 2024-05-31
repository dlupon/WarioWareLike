using System;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CarCrash : MonoBehaviour
{
    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // VARIABLES

    // UTILITIES
    private Vector2 middlePoint = Vector2.zero;

    // GAME PROPERTIES
    private bool win = false;

    // INPUT
    // KeyBoard
    private const KeyCode INPUT_PLAYER_UP = KeyCode.UpArrow;
    private const KeyCode INPUT_PLAYER_DOWN = KeyCode.DownArrow;
    private const KeyCode INPUT_PLAYER_LEFT = KeyCode.LeftArrow;
    private const KeyCode INPUT_PLAYER_RIGHT = KeyCode.RightArrow;
    // Mobile
    public float inputTouchMaxRange = 1f;
    private bool inputIsTouching;
    private Vector2 inputFirstPosition;
    private Vector2 inputCurrentPosition;
    // Properties
    private Vector3 inputDirection = Vector2.zero;



    // CAMERA
    // Init
    public Camera camera;
    public float cameraPlayerVelocityRatio = .2f;

    // PLAYER PROPERTIES
    // Init
    public GameObject player;
    // Movements
    private Action PlayerMove;
    public float playerAccelerationSpeed = 10f;
    public float playerFriction = .1f;
    public float playerAngularAccelerationSpeed = 30f;
    public float playerAngularFriction = .1f;
    private Vector3 playerAccelerationDirection = Vector3.up;
    private Vector3 playerAcceleration;
    private Vector3 playerVelocity;
    private float playerAngularAcceleration;
    private float playerAngularVelocity;

    // ARROW
    public GameObject arrow;
    public float arrowDistance = 1f;

    // TARGET PROPERTIES
    // Init
    public GameObject target;
    public float targetDistance = 10f;
    // Movements
    public float targetSpeed = 10f;
    private Vector3 targetPosition = Vector3.zero;
    private float targetRotation;
    private float targetMoveDirection;
    private Vector3 direction = Vector3.zero;
    // Detection
    public float targetDistancDetection = 1f;
    // To Target
    private Vector3 playerToTargetDirection;
    private float playerToTargetDistance;

    // Line
    public LineRenderer lineRenderer;
    public int linePointCount = 10;

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // INITIALIZATION
    
    void Start()
    {
        TargetInit();
        PlayerInit();
        CreateLine();
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // UPDATE
    void Update()
    {
        // Input
        InputUpdate();

        // Player Behaviour
        PlayerMove();

        // Arrow Behaviour
        ArrowLookAtTarget();

        // Target Behaviour
        TargetManagment();

        // Camera Behaviour
        CameraFollowPlayer();
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // INPUT

    private void InputUpdate()
    {
        KeyBoardInput();
        MobileInput();
    }

    // KeyBoard Input
    private void KeyBoardInput()
    {
        // Update Input Direction Based On Arrows
        inputDirection.x = GetAxisOnInput(Input.GetKey(INPUT_PLAYER_LEFT), Input.GetKey(INPUT_PLAYER_RIGHT));
        inputDirection.y = GetAxisOnInput(Input.GetKey(INPUT_PLAYER_DOWN), Input.GetKey(INPUT_PLAYER_UP));
        // Clamp It If It's Above 1f
        if (inputDirection.magnitude > 1f) inputDirection = inputDirection.normalized;
    }

    private float GetAxisOnInput(bool pKeyAIsPressed, bool pKeyBIsPressed) => pKeyAIsPressed ? pKeyBIsPressed ? 0f : -1f : pKeyBIsPressed ? 1f : 0f;

    // Mobile
    private void MobileInput()
    {

        if (Input.touchCount > 0)
        {
            if (!inputIsTouching) { inputFirstPosition = Input.GetTouch(0).position; inputIsTouching = true; }

            inputCurrentPosition = Input.GetTouch(0).position;
            inputDirection = (inputCurrentPosition - inputFirstPosition);
            inputDirection = inputDirection.magnitude > inputTouchMaxRange ? inputDirection.normalized * inputTouchMaxRange : inputDirection;
            inputDirection /= inputTouchMaxRange;


            print(inputDirection);

            return;
        }
        inputIsTouching = false;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // TARGET

    private void TargetInit()
    {
        // Init Target Position
        targetRotation = UnityEngine.Random.Range(0f, 360f);
        targetMoveDirection = UnityEngine.Random.Range(-1f, 1f);

        // Place Target
        TargetMove();
    }

    private void TargetManagment()
    {
        // Player To Target
        PlayerToTargetUpdate();
        TargetPlayerDetection();

        // Movements
        TargetMove();
    }

    // Player To Target
    private void PlayerToTargetUpdate()
    {
        playerToTargetDirection = (Vector2)target.transform.position - (Vector2)player.transform.position;
        playerToTargetDistance = playerToTargetDirection.magnitude;
        playerToTargetDirection = playerToTargetDirection.normalized;
    }

    private void TargetPlayerDetection()
    {
        if (playerToTargetDistance > targetDistancDetection) return;
        TargetPlayerDetected();
    }

    private void TargetPlayerDetected()
    {
        win = true;
        SetPlayerMovementFollowTarget();
    }

    // Target Movements
    private void TargetMove()
    {
        // Apply Rotation
        targetRotation += targetSpeed * targetMoveDirection * Time.deltaTime;

        // Set Target Position
        targetPosition.x = Mathf.Cos(Mathf.Deg2Rad * targetRotation);
        targetPosition.y = Mathf.Sin(Mathf.Deg2Rad * targetRotation);

        // Apply Target Position
        target.transform.position = middlePoint + (Vector2)targetPosition * targetDistance;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // ARROW

    private void ArrowLookAtTarget()
    {
        arrow.transform.eulerAngles = Mathf.Rad2Deg * arrow.transform.forward * Mathf.Atan2(playerToTargetDirection.y, playerToTargetDirection.x);
        arrow.transform.position = player.transform.position + playerToTargetDirection * arrowDistance;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // PLAYER

    // Init
    private void PlayerInit()
    {
        SetPlayerMovementInput();
        player.transform.eulerAngles = player.transform.forward * UnityEngine.Random.Range(0f, 360f);
    }

    // Movements State Machin
    private void SetPlayerMovementInput() => PlayerMove = PlayerMoveInput;

    private void SetPlayerMovementFollowTarget() => PlayerMove = PlayerMoveFollowTarget;

    private void SetPlayerMovementVoid() => PlayerMove = PlayerMoveVoid;


    // Player Movements
    private void PlayerMoveInput()
    {
        /*// Apply Player Movement
        playerAcceleration = Quaternion.Euler(0, 0, player.transform.eulerAngles.z) * playerAccelerationDirection * inputDirection.y * playerAccelerationSpeed;
        playerVelocity += playerAcceleration * Time.deltaTime;
        playerVelocity *= Mathf.Pow(playerFriction, Time.deltaTime);
        player.transform.position += playerVelocity * Time.deltaTime;
        
        // Apply Player Rotation
        playerAngularAcceleration = inputDirection.x * playerVelocity.magnitude * playerAngularAccelerationSpeed;
        playerAngularVelocity += playerAngularAcceleration * Time.deltaTime;
        playerAngularVelocity *= Mathf.Pow(playerAngularFriction, Time.deltaTime);
        player.transform.eulerAngles += player.transform.forward * playerAngularVelocity * Time.deltaTime;*/

        direction.x = Mathf.Cos(Mathf.Deg2Rad * player.transform.forward.z);
        direction.y = Mathf.Sin(Mathf.Deg2Rad * player.transform.forward.z);

        // Apply Player Movement
        playerAcceleration = Quaternion.Euler(0, 0, player.transform.eulerAngles.z) * direction * playerAccelerationSpeed * inputDirection.magnitude;
        playerVelocity += playerAcceleration * Time.deltaTime;
        playerVelocity *= Mathf.Pow(playerFriction, Time.deltaTime);
        player.transform.position += playerVelocity * Time.deltaTime;

        if (inputDirection.magnitude <= 0f) return;

        player.transform.eulerAngles = player.transform.forward * Mathf.LerpAngle(player.transform.eulerAngles.z, Mathf.Rad2Deg * Mathf.Atan2(inputDirection.y, inputDirection.x), Time.deltaTime);
    }

    private void PlayerMoveFollowTarget() => player.transform.position = (Vector3)(Vector2)target.transform.position + Vector3.forward * player.transform.position.z;

    private void PlayerMoveVoid() { }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // CAMERA

    private void CameraFollowPlayer()
        => camera.transform.position = (Vector3)(Vector2)player.transform.position + playerVelocity * cameraPlayerVelocityRatio + Vector3.forward * camera.transform.position.z;


    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // LINE

    private void CreateLine()
    {
        Vector3 lCurrentPointPosition = Vector3.forward;
        print(lCurrentPointPosition);
        float lAngle = Mathf.Deg2Rad * 360f / (float)linePointCount;
        lineRenderer.positionCount = linePointCount;

        for (int lCurrentPointIndex = 0; lCurrentPointIndex < linePointCount; lCurrentPointIndex++)
        {
            lCurrentPointPosition.x = Mathf.Cos(lAngle * lCurrentPointIndex) * targetDistance;
            lCurrentPointPosition.y = Mathf.Sin(lAngle * lCurrentPointIndex) * targetDistance;
            lineRenderer.SetPosition(lCurrentPointIndex, lCurrentPointPosition);
        }
    }
}
