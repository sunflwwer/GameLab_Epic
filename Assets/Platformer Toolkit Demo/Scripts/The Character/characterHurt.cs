using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;  // ← 추가

namespace GMTK.PlatformerToolkit {
    public class characterHurt : MonoBehaviour {
        [Header("Components")]
        [SerializeField] Vector3 checkpointFlag;
        [SerializeField] Animator myAnim;
        [SerializeField] AudioSource hurtSFX;
        private Coroutine flashRoutine;
        Rigidbody2D body;
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] movementLimiter myLimit;

        [Header("Settings")]
        [SerializeField] float respawnTime = 1.0f;   // 리스타트까지 대기 시간(초)
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Events")]
        [SerializeField] public UnityEvent onHurt = new UnityEvent();

        [Header("Current State")]
        bool waiting = false;
        bool hurting = false;

        void Start() {
            body = GetComponent<Rigidbody2D>();
        }

        public void newCheckpoint(Vector3 flagPos) {
            checkpointFlag = flagPos;
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            // 레이어 7/8에 닿으면 데미지 처리(예전 로직 유지)
            if (collision.gameObject.layer == 7 || collision.gameObject.layer == 8) {
                if (!hurting) {
                    if (collision.gameObject.layer == 8) {
                        body.linearVelocity = Vector2.zero;
                    }
                    hurting = true;
                    hurtRoutine();
                }
            }
        }

        public void hurtRoutine() {
            // 잘못된 가드 → 올바르게 수정
            // if (hurting == false) return;  // ❌ 이러면 대부분 바로 return됨
            if (hurting) return;               // ✅ 중복 진입 방지
            hurting = true;                    // ✅ 이제부터 '죽음 처리 중' 플래그

            if (myLimit) myLimit.CharacterCanMove = false;

            onHurt?.Invoke();
            if (hurtSFX) hurtSFX.Play();

            // 짧은 히트스톱(Realtime)
            Stop(0.1f);

            // 씬 리로드 코루틴 시작(Realtime)
            StartCoroutine(ReloadSceneAfterDelay(respawnTime));
        }


        private IEnumerator ReloadSceneAfterDelay(float delay) {
            // 리스타트 대기 (타임스케일 무시)
            yield return new WaitForSecondsRealtime(delay);

            // 혹시 멈춰있을 수도 있으니 원복
            Time.timeScale = 1f;

            // 현재 활성 씬 재로딩
            var active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex);
        }

        public void Stop(float duration) {
            Stop(duration, 0.0f);
        }

        public void Stop(float duration, float timeScale) {
            if (waiting) return;
            Time.timeScale = timeScale;
            StartCoroutine(Wait(duration));
        }

        IEnumerator Wait(float duration) {
            waiting = true;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
            waiting = false;
        }

        // 아래는 현재 미사용(애니메이션/플래시)
        public void Flash() {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine() {
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.enabled = false;
            flashRoutine = null;
        }

        // 더 이상 체크포인트 리스폰을 쓰지 않으므로 호출되지 않게 유지
        private void respawnRoutine() {
            // 이전 방식(체크포인트 복귀)은 사용하지 않음
            // transform.position = checkpointFlag;
            // myLimit.CharacterCanMove = true;
            // if (myAnim) myAnim.SetTrigger("Okay");
            // hurting = false;
            // GetComponent<PlayerHealth2D>()?.RestoreFull();
        }
    }
}
