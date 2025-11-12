using UnityEngine;

namespace GMTK.PlatformerToolkit
{
    [DisallowMultipleComponent]
    public class PlayerComboJump : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private characterJump jump;

        [Header("Combo Window")]
        [SerializeField, Range(0.05f, 1f)] private float comboWindow = 0.20f; // 살짝 넉넉하게

        [Header("Bounce Power")]
        [SerializeField] private float normalBounceSword  = 16f;
        [SerializeField] private float normalBounceShield = 16f;
        [SerializeField] private float highBounce        = 30f;

        [Header("Rules")]
        [SerializeField] private bool requireFalling = true; 
        [SerializeField] private float minInterval   = 0.08f;

        [Header("Combo Ease Options")]
        [SerializeField] private bool comboByPressOnly = true;    // 입력만으로 콤보 예열
        [SerializeField] private bool comboBypassFalling = true;  // 콤보일 땐 상승 중에도 허용

        private float lastSwordTime   = -999f;
        private float lastShieldTime  = -999f;
        private float lastSwordPress  = -999f;
        private float lastShieldPress = -999f;

        // ▼ 콤보 예열 기간 (이 시간까지 들어오는 최초 트리거는 무조건 콤보로 처리)
        private float armedComboUntil = -999f;

        private float lastBounceTime  = -999f;
        private Rigidbody2D rb;

        private void Awake()
        {
            if (!jump) jump = GetComponent<characterJump>();
            rb = GetComponent<Rigidbody2D>();
        }

        private bool CanBounce(bool isComboNow)
        {
            if (!jump) return false;
            if (Time.time - lastBounceTime < minInterval) return false;

            // slowFalling(공중 슬로우 모드) 중에는 바운스 금지
            // 이렇게 하면 좌클릭+우클릭 동시 입력 시 의도치 않은 높이 뛰기 방지
            if (jump.IsSlowFalling) return false;

            // 콤보일 때는 상승 중 제한을 우회할 수 있게
            if (!isComboNow || !comboBypassFalling)
            {
                if (requireFalling && rb && rb.linearVelocity.y > 0f) return false;
            }
            return true;
        }

        public void ReportSwordPress()
        {
            lastSwordPress = Time.time;

            if (comboByPressOnly && Mathf.Abs(lastSwordPress - lastShieldPress) <= comboWindow)
            {
                armedComboUntil = Time.time + comboWindow; // 입력만으로 콤보 예열
            }
        }

        public void ReportShieldPress()
        {
            lastShieldPress = Time.time;

            if (comboByPressOnly && Mathf.Abs(lastSwordPress - lastShieldPress) <= comboWindow)
            {
                armedComboUntil = Time.time + comboWindow; // 입력만으로 콤보 예열
            }
        }

        public bool ReportSwordHit()
        {
            lastSwordTime = Time.time;
            return TryResolveBounce(isSword: true);
        }

        public bool ReportShieldParry()
        {
            lastShieldTime = Time.time;
            return TryResolveBounce(isSword: false);
        }

        private bool TryResolveBounce(bool isSword)
        {
            // 1) 입력-입력 동시성으로 예열되어 있나?
            bool armed = Time.time <= armedComboUntil;

            // 2) 히트-히트 동시성도 여전히 인정
            bool hitCombo = Mathf.Abs(lastSwordTime - lastShieldTime) <= comboWindow;

            bool isComboNow = armed || hitCombo;

            if (!CanBounce(isComboNow)) return false;

            float power = isComboNow ? highBounce : (isSword ? normalBounceSword : normalBounceShield);

            jump.bounceUp(power);
            lastBounceTime = Time.time;

            if (isComboNow)
            {
                // 예열/히트 타임스탬프 소진 및 해제
                armedComboUntil = -999f;
                lastSwordTime   = -999f;
                lastShieldTime  = -999f;
                lastSwordPress  = -999f;
                lastShieldPress = -999f;
            }
            return true;
        }
    }
}
