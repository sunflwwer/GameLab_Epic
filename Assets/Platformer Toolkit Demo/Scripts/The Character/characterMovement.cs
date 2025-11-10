using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace GMTK.PlatformerToolkit {
    //This script handles moving the character on the X axis, both on the ground and in the air.
    public class characterMovement : MonoBehaviour {

        [Header("Components")]
        [SerializeField] private Transform visualRoot;

        private Rigidbody2D body;
        characterGround ground;

        [Header("Movement Stats")]
        [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float maxDecceleration = 52f;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
        [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
        [SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;

        [Header("Options")]
        [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] public bool useAcceleration;

        [Header("Calculations")]
        public float directionX;
        private Vector2 desiredVelocity;
        public Vector2 velocity;
        private float maxSpeedChange;
        private float acceleration;
        private float deceleration;
        private float turnSpeed;

        [Header("Current State")]
        public bool onGround;
        public bool pressingKey;

        // ▼▼▼ 추가: 스킬용 페이싱 API
        [Header("Facing API")]
        [SerializeField] private Transform facingOverride; // 필요 시 visualRoot 대신 지정
        public Transform FacingTransform => facingOverride ? facingOverride : (visualRoot ? visualRoot : transform);
        public float FacingDirX {
            get {
                float sx = FacingTransform.localScale.x;
                return Mathf.Sign(sx == 0 ? 1 : sx);
            }
        }
        // ▲▲▲

        private void Awake() {
            body = GetComponent<Rigidbody2D>();
            ground = GetComponent<characterGround>();
        }

        public void OnMovement(InputAction.CallbackContext context) {
            if (movementLimiter.instance.CharacterCanMove) {
                directionX = context.ReadValue<float>();
            }
        }

        private void Update() {
            if (!movementLimiter.instance.CharacterCanMove) directionX = 0;

            if (directionX != 0) {
                // 그래픽만 좌우 반전(무기는 회전 영향 안 받음)
                if (visualRoot != null) {
                    visualRoot.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
                } else {
                    // 백업: 정말 필요할 때만 루트 반전
                    transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
                }
                pressingKey = true;
            } else {
                pressingKey = false;
            }

            desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed - friction, 0f);
        }

        private void FixedUpdate() {
            onGround = ground.GetOnGround();
            velocity = body.linearVelocity;

            if (useAcceleration) {
                runWithAcceleration();
            }
            else {
                if (onGround) runWithoutAcceleration();
                else runWithAcceleration();
            }
        }

        private void runWithAcceleration() {
            acceleration = onGround ? maxAcceleration : maxAirAcceleration;
            deceleration = onGround ? maxDecceleration : maxAirDeceleration;
            turnSpeed   = onGround ? maxTurnSpeed   : maxAirTurnSpeed;

            if (pressingKey) {
                if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x)) {
                    maxSpeedChange = turnSpeed * Time.deltaTime;
                } else {
                    maxSpeedChange = acceleration * Time.deltaTime;
                }
            } else {
                maxSpeedChange = deceleration * Time.deltaTime;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            body.linearVelocity = velocity;
        }

        private void runWithoutAcceleration() {
            velocity.x = desiredVelocity.x;
            body.linearVelocity = velocity;
        }
    }
}
