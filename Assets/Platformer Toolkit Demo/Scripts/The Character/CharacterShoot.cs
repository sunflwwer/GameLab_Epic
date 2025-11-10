using Cinemachine;
using UnityEngine;

namespace GMTK.PlatformerToolkit {
    public class CharacterShoot : MonoBehaviour {
        [Header("Ammo")]
        [SerializeField] private int maxAmmo = 8;
        private int currentAmmo;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] [Tooltip("Time between shots for automatic fire.")] private float fireRate = 0.2f;
        private float nextFireTime = 0f;

        [Header("UI")]
        [SerializeField] private AmmoUI ammoUI;
        
        [Header("Camera Recoil")]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float recoilStrength = 1.0f; // 반동 배율 (난수로 약간 흔들려야 자연스러움)

        private float RandomJitter => UnityEngine.Random.Range(0.9f, 1.1f);


        private void Awake() {
            currentAmmo = maxAmmo;
            if (ammoUI != null) {
                ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
            }
        }

        public bool CanShoot() {
            return currentAmmo > 0;
        }

        public void TryShootDownwards() {
            if (CanShoot() && Time.time >= nextFireTime) {
                nextFireTime = Time.time + fireRate;

                currentAmmo--;

                if (bulletPrefab != null && firePoint != null) {
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null) {
                        rb.linearVelocity = Vector2.down * bulletSpeed;
                    }
                }

                if (ammoUI != null) {
                    ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
                    
                    if (impulseSource != null)
                    {
                        // 수직으로만 ‘탁’ 치기 (아래 방향), 약간의 랜덤 배율로 질감 추가
                        Vector3 dir = new Vector3(0f, -1f, 0f) * recoilStrength * RandomJitter;
                        impulseSource.GenerateImpulse(dir);
                    }
                }
            }
        }

        public void Reload() {
            currentAmmo = maxAmmo;
            if (ammoUI != null) {
                ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
            }
        }
    }
}
