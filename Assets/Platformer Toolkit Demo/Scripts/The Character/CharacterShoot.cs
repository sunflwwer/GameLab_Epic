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
