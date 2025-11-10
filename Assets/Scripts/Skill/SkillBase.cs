using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    [SerializeField] protected float cooldown = 0.3f;
    private float lastUse = -999f;

    public bool CanActivate() => Time.time >= lastUse + cooldown;

    public void Activate(Transform caster)
    {
        if (!CanActivate()) return;
        lastUse = Time.time;
        OnActivate(caster);
    }

    protected abstract void OnActivate(Transform caster);
}