using UnityEngine;
using UnityEngine.InputSystem;
using GMTK.PlatformerToolkit;

namespace GMTK.PlatformerToolkit
{
    public class PlayerSkills : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private SkillBase swordSkill;    // Primary(좌클릭)
        [SerializeField] private SkillBase shieldSkill;   // Secondary(우클릭)
        [SerializeField] private PlayerSimpleWeapons simpleWeapons; // 모션 전담

        [Header("Options")]
        [SerializeField] private bool respectMovementLimiter = true;
        
        [SerializeField] private Light2DPulse clickLight;
        
        private movementLimiter limiter;

        // 외부에서 방패 스킬 접근용
        public ShieldBlockSkill ShieldSkill => shieldSkill as ShieldBlockSkill;

        private void Awake()
        {
            limiter = GetComponent<movementLimiter>();
        }

        private bool CanUse()
        {
            if (!respectMovementLimiter || limiter == null) return true;
            return limiter.CharacterCanMove;
        }

        // 좌클릭: 그대로(원한다면 칼도 홀드형으로 바꿀 수 있음)
        public void OnPrimary(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
            {
                if (!CanUse()) return;

                // ▼ 입력 무장(칼)
                var combo = GetComponent<PlayerComboJump>();
                combo?.ReportSwordPress();

                simpleWeapons?.StartSwordHold();
                swordSkill?.Activate(transform);

                clickLight?.Pulse();
                return;
            }

            if (ctx.canceled)
            {
                simpleWeapons?.EndSwordHold();
            }
        }


        // 우클릭: 홀드 유지
        public void OnSecondary(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
            {
                if (!CanUse()) return;

                // ▼ 입력 무장(방패)
                var combo = GetComponent<PlayerComboJump>();
                combo?.ReportShieldPress();

                simpleWeapons?.StartSpearHold();
                shieldSkill?.Activate(transform);
                return;
            }

            if (ctx.canceled)
            {
                simpleWeapons?.EndSpearHold();
                var hold = shieldSkill as ShieldBlockSkill;
                if (hold != null) hold.StopBlocking();
            }
        }
    }
}