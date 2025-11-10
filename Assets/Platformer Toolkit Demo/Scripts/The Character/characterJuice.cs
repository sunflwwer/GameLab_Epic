using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GMTK.PlatformerToolkit {
    //This script handles purely aesthetic things like particles, squash & stretch, and tilt

    public class characterJuice : MonoBehaviour {
        [Header("Components")]
        characterMovement moveScript;
        characterJump jumpScript;
        [SerializeField] Animator myAnimator;
        [SerializeField] GameObject characterSprite;

        [Header("Components - Particles")]
        [SerializeField] private ParticleSystem moveParticles;
        [SerializeField] private ParticleSystem jumpParticles;
        [SerializeField] private ParticleSystem landParticles;

        [Header("Components - Audio")]
        [SerializeField] AudioSource jumpSFX;
        [SerializeField] AudioSource landSFX;

        [Header("Settings - Squash and Stretch")]
        [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] Vector3 jumpSquashSettings;
        [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] Vector3 landSquashSettings;
        [SerializeField, Tooltip("How powerful should the effect be?")] public float landSqueezeMultiplier;
        [SerializeField, Tooltip("How powerful should the effect be?")] public float jumpSqueezeMultiplier;
        [SerializeField] float landDrop = 1;

        [Header("Tilting")]
        [SerializeField, Tooltip("How far should the character tilt?")] public float maxTilt;
        [SerializeField, Tooltip("How fast should the character tilt?")] public float tiltSpeed;

        [Header("Calculations")]
        public float runningSpeed;
        public float maxSpeed;

        [Header("Current State")]
        public bool squeezing;
        public bool jumpSqueezing;
        public bool landSqueezing;
        public bool playerGrounded;

        [Header("Platformer Toolkit Stuff")]
        [SerializeField] jumpTester jumpLine;
        public bool cameraFalling = false;
        
        [Header("Run Rotate")]
        [SerializeField] private bool enableRunRotate = true;
        // 회전 대상(애니메이터의 부모 피벗). 비워두면 Start에서 자동 생성
        [SerializeField] private Transform rotateRoot;
        [SerializeField, Tooltip("오른쪽 달릴 때 -각도, 왼쪽 +각도(도)")]
        private float runRotateAngle = 12f;
        [SerializeField, Tooltip("회전 속도(도/초)")]
        private float runRotateSpeed = 720f;
        [SerializeField, Tooltip("달리기로 판단할 최소 X속도")]
        private float minRunSpeed = 0.2f;
        [SerializeField, Tooltip("점프 중엔 각도를 이 배율만큼만 적용(0~1)")]
        private float airAngleMultiplier = 0.7f;

        void Start() {
            moveScript = GetComponent<characterMovement>();
            jumpScript = GetComponent<characterJump>();

            // 1) 애니메이터 없어도 동작하도록 우선 characterSprite로 지정
            if (rotateRoot == null) {
                if (characterSprite != null) {
                    rotateRoot = characterSprite.transform;
                } else {
                    rotateRoot = transform; // 최후 대안
                }
            }

            // 2) 애니메이터가 있을 때만 Pivot 생성 (선택사항)
            if (myAnimator != null && rotateRoot == myAnimator.transform) {
                var spriteTf = myAnimator.transform;
                var pivot = new GameObject("RotatePivot").transform;
                pivot.SetParent(spriteTf.parent, worldPositionStays: true);
                pivot.position = spriteTf.position;
                pivot.rotation = spriteTf.rotation;
                pivot.localScale = spriteTf.localScale;
                spriteTf.SetParent(pivot, worldPositionStays: true);
                rotateRoot = pivot;
            }
        }


        void Update() {
            // tiltCharacter(); // 비활성

            runningSpeed = Mathf.Clamp(Mathf.Abs(moveScript.velocity.x), 0, maxSpeed);
            if (myAnimator != null) {
                myAnimator.SetFloat("runSpeed", runningSpeed);
            }

            checkForLanding();
            checkForGoingPastJumpLine();
        }

        public void jumpEffects() {
            if (jumpSFX && jumpSFX.enabled) jumpSFX.Play();

            if (!jumpSqueezing && jumpSqueezeMultiplier > 1f) {
                StartCoroutine(JumpSqueeze(
                    jumpSquashSettings.x / jumpSqueezeMultiplier,
                    jumpSquashSettings.y * jumpSqueezeMultiplier,
                    jumpSquashSettings.z, 0, true));
            }

            if (jumpParticles) jumpParticles.Play();
        }

        
        // Animator가 회전값을 나중에 덮어쓰지 못하도록 LateUpdate에서 회전 적용
        void LateUpdate() {
            if (enableRunRotate) RunRotateLate();
        }
        
        private void RunRotateLate() {
            if (rotateRoot == null) return;

            float vx = moveScript.velocity.x;
            float targetZ = 0f;

            // 달릴 때 기울기 제거: 지상에서는 항상 0도
            if (!jumpScript.onGround && Mathf.Abs(vx) > minRunSpeed) {
                // 공중에서만 좌/우 기울임
                targetZ = (vx > 0f) ? -runRotateAngle : runRotateAngle;
                targetZ *= airAngleMultiplier; // 공중 배율
            }

            var targetRot = Quaternion.Euler(0f, 0f, targetZ);
            rotateRoot.rotation = Quaternion.RotateTowards(
                rotateRoot.rotation,
                targetRot,
                runRotateSpeed * Time.deltaTime
            );
        }

        /*private void tiltCharacter() {
            //See which direction the character is currently running towards, and tilt in that direction
            float directionToTilt = 0;
            if (moveScript.velocity.x != 0) {
                directionToTilt = Mathf.Sign(moveScript.velocity.x);
            }

            //Create a vector that the character will tilt towards
            Vector3 targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, directionToTilt)));

            //And then rotate the character in that direction
            myAnimator.transform.rotation = Quaternion.RotateTowards(myAnimator.transform.rotation, Quaternion.Euler(-targetRotVector), tiltSpeed * Time.deltaTime);
        }*/

        private void checkForLanding() {
            if (!playerGrounded && jumpScript.onGround) {
                //By checking for this, and then immediately setting playerGrounded to true, we only run this code once when the player hits the ground 
                playerGrounded = true;
                cameraFalling = false;

                //This is related to the "ignore jumps" option on the camera panel.
                jumpLine.characterY = transform.position.y;

                //Play an animation, some particles, and a sound effect when the player lands
                //myAnimator.SetTrigger("Landed");
                if (myAnimator != null) myAnimator.SetTrigger("Landed");

                landParticles.Play();

                if (!landSFX.isPlaying && landSFX.enabled) {
                    landSFX.Play();
                }

                moveParticles.Play();

                //Start the landing squash and stretch coroutine.
                if (!landSqueezing && landSqueezeMultiplier > 1) {
                    StartCoroutine(JumpSqueeze(landSquashSettings.x * landSqueezeMultiplier, landSquashSettings.y / landSqueezeMultiplier, landSquashSettings.z, landDrop, false));
                }

            }
            else if (playerGrounded && !jumpScript.onGround) {
                // Player has left the ground, so stop playing the running particles
                playerGrounded = false;
                moveParticles.Stop();
            }
        }

        private void checkForGoingPastJumpLine() {
            //This is related to the "ignore jumps" option on the camera panel.
            if (transform.position.y < jumpLine.transform.position.y - 3) {
                cameraFalling = true;
            }

            if (cameraFalling) {
                jumpLine.characterY = transform.position.y;
            }
        }

        /*public void jumpEffects() {
            //Play these effects when the player jumps, courtesy of jump script
            myAnimator.ResetTrigger("Landed");
            myAnimator.SetTrigger("Jump");

            if (jumpSFX.enabled) {
                jumpSFX.Play();

            }

            if (!jumpSqueezing && jumpSqueezeMultiplier > 1) {
                StartCoroutine(JumpSqueeze(jumpSquashSettings.x / jumpSqueezeMultiplier, jumpSquashSettings.y * jumpSqueezeMultiplier, jumpSquashSettings.z, 0, true));

            }

            jumpParticles.Play();
        }*/

        IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds, float dropAmount, bool jumpSqueeze) {
            //We log that the player is squashing/stretching, so we don't do these calculations more than once
            if (jumpSqueeze) { jumpSqueezing = true; }
            else { landSqueezing = true; }
            squeezing = true;

            Vector3 originalSize = Vector3.one;
            Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);

            Vector3 originalPosition = Vector3.zero;
            Vector3 newPosition = new Vector3(0, -dropAmount, 0);

            //We very quickly lerp the character's scale and position to their squashed and stretched pose...
            float t = 0f;
            while (t <= 1.0) {
                t += Time.deltaTime / 0.01f;
                characterSprite.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
                characterSprite.transform.localPosition = Vector3.Lerp(originalPosition, newPosition, t);
                yield return null;
            }

            //And then we lerp back to the original scale and position at a speed dicated by the developer
            //It's important to do this to the character's sprite, not the gameobject with a Rigidbody an/or collision detection
            t = 0f;
            while (t <= 1.0) {
                t += Time.deltaTime / seconds;
                characterSprite.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
                characterSprite.transform.localPosition = Vector3.Lerp(newPosition, originalPosition, t);
                yield return null;
            }

            if (jumpSqueeze) { jumpSqueezing = false; }
            else { landSqueezing = false; }

            squeezing = false;
        }
    }
}