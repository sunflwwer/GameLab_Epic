using UnityEngine;

namespace GMTK.PlatformerToolkit
{
    /// <summary>
    /// 달릴 때는 회전하지 않고, 공중(점프/낙하)에서만 좌/우 이동 방향으로 빙글빙글 회전.
    /// Animator 필요 없음. characterSprite(또는 지정한 Transform)만 회전시킴.
    /// </summary>
    public class AirSpinController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private characterMovement move;   // 자동 할당
        [SerializeField] private characterJump jump;       // 자동 할당
        [SerializeField] private Transform target;         // 회전시킬 대상(비우면 characterJuice.characterSprite 또는 this)

        [Header("Spin Settings")]
        [SerializeField, Tooltip("X 속도가 이 값보다 커야 스핀 시작")]
        private float minAirXSpeed = 0.1f;

        [SerializeField, Tooltip("기본 공중 스핀 속도(도/초). 방향은 X속도 부호를 따름")]
        private float baseSpinSpeed = 540f;

        [SerializeField, Tooltip("최대 스핀 속도(도/초)")]
        private float maxSpinSpeed = 1080f;

        [SerializeField, Tooltip("스핀 가속도(도/초^2)")]
        private float spinAcceleration = 4000f;

        [SerializeField, Tooltip("스핀 감속도(지상에서 0으로 줄이는 속도, 도/초^2)")]
        private float spinDeceleration = 6000f;

        [SerializeField, Tooltip("지상에서 0도 자세로 돌아가는 속도(도/초)")]
        private float groundReturnSpeed = 1080f;

        [SerializeField, Tooltip("점프 순간에 초기 회전 임펄스(도/초)를 더해줌")]
        private float jumpImpulseSpin = 360f;

        [SerializeField, Tooltip("착지 시 각속도 거의 0이 되면 0도로 스냅")]
        private bool snapToZeroOnGround = true;

        [Header("Optional: 한 번 점프당 1바퀴 완성")]
        [SerializeField] private bool completeFlipPerJump = false;

        private float angularVel;   // 현재 각속도(도/초)
        private bool lastOnGround;
        private float accumulatedThisJump; // 이번 점프에서 누적 회전각(도)

        private void Awake()
        {
            if (!move) move = GetComponent<characterMovement>();
            if (!jump) jump = GetComponent<characterJump>();

            if (!target)
            {
                // characterJuice 안의 characterSprite를 자동 검색
                var juice = GetComponent<characterJuice>();
                if (juice != null && juice.GetType().GetField("characterSprite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public) != null)
                {
                    // characterSprite는 SerializeField이므로 Reflection 대신, public getter가 없다면
                    // 씬에서 직접 target에 할당하는 걸 권장.
                }

                // 못 찾으면 자기 자신
                if (!target) target = transform;
            }
        }

        private void Update()
        {
            if (move == null || jump == null || target == null) return;

            bool onGround = jump.onGround;
            float vx = move.velocity.x;
            float ax = Mathf.Abs(vx);

            // 지면→공중 전이(점프 시작) 감지: 초기 임펄스 부여
            if (lastOnGround && !onGround)
            {
                // 점프 순간 X 입력 방향으로 임펄스
                float dir = Mathf.Sign(vx == 0f ? (transform.localScale.x >= 0 ? 1f : -1f) : vx);
                angularVel += dir * jumpImpulseSpin;
                accumulatedThisJump = 0f;
            }

            if (!onGround && ax > minAirXSpeed)
            {
                // 공중: 목표 각속도는 X속도 부호를 따른다. 속도 클수록 더 빨리 도달하도록 스케일링(선형 스케일)
                float dir = Mathf.Sign(vx);
                float targetSpin = dir * Mathf.Clamp(baseSpinSpeed * Mathf.InverseLerp(minAirXSpeed, maxSpinSpeed, ax), 0f, maxSpinSpeed);

                // 가속/감속
                angularVel = Mathf.MoveTowards(angularVel, targetSpin, spinAcceleration * Time.deltaTime);

                // 회전 적용
                float deltaAngle = angularVel * Time.deltaTime;
                target.Rotate(0f, 0f, deltaAngle);
                accumulatedThisJump += Mathf.Abs(deltaAngle);

                // 한 번 점프당 360도 완성 옵션
                if (completeFlipPerJump && accumulatedThisJump >= 360f)
                {
                    // 남은 각도는 360의 배수로 스냅
                    Vector3 e = target.eulerAngles;
                    e.z = Mathf.Round(e.z / 360f) * 360f;
                    target.eulerAngles = e;
                    // 각속도는 유지하거나 줄여도 됨. 여기선 살짝 줄여 부자연스러움 방지
                    angularVel *= 0.5f;
                    completeFlipPerJump = false; // 원하면 한 번만
                }
            }
            else
            {
                // 지상: 각속도를 0으로 감속
                angularVel = Mathf.MoveTowards(angularVel, 0f, spinDeceleration * Time.deltaTime);

                // 각속도가 거의 0이면 0도 자세로 복귀
                if (snapToZeroOnGround && Mathf.Abs(angularVel) < 5f)
                {
                    Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
                    target.rotation = Quaternion.RotateTowards(target.rotation, targetRot, groundReturnSpeed * Time.deltaTime);
                }
            }

            lastOnGround = onGround;
        }
    }
}
