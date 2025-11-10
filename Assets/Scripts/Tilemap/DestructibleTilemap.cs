using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
public class DestructibleTilemap : MonoBehaviour {
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TilemapCollider2D tilemapCollider;

    [Header("Hit Points")]
    [SerializeField] private int defaultHitPoints = 1;

    private readonly Dictionary<Vector3Int, int> hp = new();

    private void Reset() {
        tilemap = GetComponent<Tilemap>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }
    private void Awake() {
        if (!tilemap) tilemap = GetComponent<Tilemap>();
        if (!tilemapCollider) tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    // === 새로 추가: 견고한 파괴 진입점 ===
    public void HitWorldRobust(Vector2 worldPos, Vector2 hitNormal, int damage, float radius) {
        // 1) 먼저 노멀 반대로 셀 안쪽으로 nudge
        float cellMin = Mathf.Max(0.02f, Mathf.Min(tilemap.cellSize.x, tilemap.cellSize.y));
        Vector2 nudged = worldPos - hitNormal.normalized * (cellMin * 0.2f);

        bool did = radius > 0f
            ? TryRadiusAt(nudged, radius, damage)
            : TryHitAt(nudged, damage);

        if (did) return;

        // 2) 실패하면 주변 8방향 후보들을 순회하며 재시도
        Vector2[] offsets = {
            new(cellMin*0.25f, 0), new(-cellMin*0.25f, 0),
            new(0, cellMin*0.25f), new(0, -cellMin*0.25f),
            new(cellMin*0.25f, cellMin*0.25f), new(cellMin*0.25f, -cellMin*0.25f),
            new(-cellMin*0.25f, cellMin*0.25f), new(-cellMin*0.25f, -cellMin*0.25f),
        };
        foreach (var o in offsets) {
            var p = nudged + o;
            did = radius > 0f ? TryRadiusAt(p, radius, damage) : TryHitAt(p, damage);
            if (did) return;
        }
        // 그래도 못 찾으면 그냥 무시(모서리/빈 공간일 수 있음)
    }

    private bool TryHitAt(Vector2 worldPos, int damage) {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        if (!tilemap.HasTile(cell)) return false;
        HitCell(cell, damage);
        return true;
    }

    private bool TryRadiusAt(Vector2 worldPos, float radius, int damage) {
        Vector3Int center = tilemap.WorldToCell(worldPos);
        bool any = false;

        int r = Mathf.CeilToInt(radius / Mathf.Min(tilemap.cellSize.x, tilemap.cellSize.y));
        for (int y = -r; y <= r; y++) {
            for (int x = -r; x <= r; x++) {
                Vector3Int c = new(center.x + x, center.y + y, 0);
                if (!tilemap.HasTile(c)) continue;

                Vector3 centerW = tilemap.GetCellCenterWorld(c);
                if (Vector2.Distance(worldPos, (Vector2)centerW) <= radius) {
                    HitCell(c, damage);
                    any = true;
                }
            }
        }
        return any;
    }

    // 기존 API 유지
    public void HitAtWorld(Vector2 worldPos, int damage = 1) {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        HitCell(cell, damage);
    }
    public void HitRadiusWorld(Vector2 worldPos, float radius, int damage = 1) {
        if (radius <= 0f) { HitAtWorld(worldPos, damage); return; }
        TryRadiusAt(worldPos, radius, damage);
    }

    private void HitCell(Vector3Int cell, int damage) {
        if (!tilemap.HasTile(cell)) return;

        if (!hp.TryGetValue(cell, out int curr)) curr = defaultHitPoints;
        curr -= Mathf.Max(1, damage);

        if (curr <= 0) {
            tilemap.SetTile(cell, null);
            if (hp.ContainsKey(cell)) hp.Remove(cell);
#if UNITY_2021_2_OR_NEWER
            if (tilemapCollider) tilemapCollider.ProcessTilemapChanges();
#endif
        } else {
            hp[cell] = curr;
        }
    }
}
