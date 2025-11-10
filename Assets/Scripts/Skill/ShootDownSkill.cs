using UnityEngine;
using GMTK.PlatformerToolkit;

public class ShootDownSkill : SkillBase
{
    private CharacterShoot _shoot;

    private void Awake()
    {
        _shoot = GetComponentInParent<CharacterShoot>();
    }

    protected override void OnActivate(Transform caster)
    {
        if (_shoot != null)
            _shoot.TryShootDownwards();
    }
}