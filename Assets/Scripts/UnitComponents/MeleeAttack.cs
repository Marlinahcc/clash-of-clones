﻿using UnityEngine;

/// <summary>
/// Represents an attack on a single target.  Projectiles ignore other enemies and always do direct damage.
/// For example, a bullet or an arrow.
// <summary>
/// 
/// </summary>/ </summary>
[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(EntityAggro))]
[RequireComponent(typeof(Rigidbody))]
public class MeleeAttack : MonoBehaviour 
{
    /// <summary>
    /// The area at which to hurt enemies if this unit has an area melee attack.
    /// </summary>
    [SerializeField] private float _attackRadius;
    [SerializeField] private bool _isDirectional;
    [SerializeField] private AudioSource _attackAudio;

    private float _cooldownSeconds;
    private Entity _entity;
    private EntityAggro _aggro;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _aggro = GetComponent<EntityAggro>();

        // EARLY OUT! //
        if(_entity == null || _aggro == null || GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning("Requires entity, aggro, rigidbody.");
            return;
        }
    }

    private void Update()
    {
        // Our code which does simple shooting AI on cooldown if we have a target.
        _cooldownSeconds -= Time.deltaTime;
    }

    /// <summary>
    /// Attempt attacks when we detect a rigidbody collision.
    /// </summary>
    private void OnTriggerStay (Collider other)
    {
        // EARLY OUT! //
        // If on cooldown don't attack.
        if(_cooldownSeconds > 0f) return;
            
        // EARLY OUT! //
        // If the trigger isn't an entity, ignore it.
        if(!CombatUtils.IsEntity(other.gameObject.layer)) return;

        // EARLY OUT! //
        // If the collider isn't an enemy, ignore it.
        var target = other.GetComponent<Entity>();
        if(!CombatUtils.IsEnemy(_entity.Owner, target)) return;

        bool didAttack = false;
        didAttack = attemptDirectAttack(target);
        didAttack |= attemptAreaAttack(target);

        if(didAttack)
        {
            // Reset cooldown.
            _cooldownSeconds = _entity.Definition.AttackSpeed;

            // Play attack audio if we have any.
            if(_attackAudio != null)
            {
                _attackAudio.Play();
            }
        }
    }

    private bool attemptDirectAttack(Entity target)
    {
        bool didAttack = false;
        if(target != null && _entity.Definition.DirectAttackDamage > 0)
        {
            if(target != null)
            {
                // If the entity needs to aim, we have to wait until the target is in our sights.
                // If the direction doesn't matter, we should be ready to fire.
                if (_aggro.IsInSights(target.transform, _isDirectional))
                {
                    attack(target, _entity.Definition.DirectAttackDamage);
                    didAttack = true;
                }
            }
        }
        return didAttack;
    }

    private bool attemptAreaAttack(Entity target)
    {
        bool didAttack = true;

        // Trigger enter only applies to area attacks.
        if(target != null && _entity.Definition.AreaAttackDamage > 0)
        {
            Collider[] colliders = Physics.OverlapSphere (transform.position, _attackRadius, CombatUtils.EntityMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                var areaTarget = colliders[i].GetComponent<Entity>();
                if(CombatUtils.IsEnemy(_entity.Owner, areaTarget))
                {
                    attack(areaTarget, _entity.Definition.AreaAttackDamage);
                    didAttack = true;
                }
            }
        }
        return didAttack;
    }

    private void attack(Entity target, int damage)
    {
        if(target != null)
        {
            target.TakeDamage(damage);
        }
    }
}