// TowerAttack.cs
using System.Collections;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public enum AttackPattern { AttackFirst, AttackLast, AttackClosest, Area, Slow }

    [Header("Base Settings")]
    public AttackPattern attackPattern = AttackPattern.AttackFirst;

    // Current effective stats (base + upgrades)
    private float _currentDamage;
    private float _currentAttackInterval;
    private float _currentAttackRadius;
    private float _currentSlowFactor;
    private float _currentSlowDuration;

    private Coroutine _shootingCoroutine;
    private readonly Collider[] _hits = new Collider[100];

    private ColorPulse _colorPulse;
    private PlacedTower _placedTower;

    private void Awake()
    {
        _colorPulse = GetComponent<ColorPulse>();
        _placedTower = GetComponent<PlacedTower>();
        // Initialize current stats to base values first
        InitializeBaseStats();
        ApplyUpgrades();
    }

    /// <summary>Initialize current stats to base values.</summary>
    private void InitializeBaseStats()
    {
        if (_placedTower != null && _placedTower.Data != null)
        {
            TowerData data = _placedTower.Data;
            _currentDamage = Mathf.Max(0.1f, data.damage);
            _currentAttackInterval = Mathf.Max(0.1f, data.attackInterval);
            _currentAttackRadius = Mathf.Max(0.1f, data.attackRadius);
            _currentSlowFactor = Mathf.Max(0.1f, data.slowFactor);
            _currentSlowDuration = Mathf.Max(0.1f, data.slowDuration);
        }
        else
        {
            _currentDamage = 1f;
            _currentAttackInterval = 1f;
            _currentAttackRadius = 2f;
            _currentSlowFactor = 2f;
            _currentSlowDuration = 5f;
        }
    }

    /// <summary>Recalculates stats based on current level and upgrade definitions.</summary>
    public void ApplyUpgrades()
    {
        // Always start with base stats
        InitializeBaseStats();

        // Get PlacedTower component if not already cached
        if (_placedTower == null)
            _placedTower = GetComponent<PlacedTower>();

        if (_placedTower == null || _placedTower.Data == null) return;

        int level = _placedTower.Level;
        TowerData data = _placedTower.Data;

        // For level 2 and higher, apply the target stats defined by the corresponding upgrade entry.
        if (level > 1)
        {
            int upgradeIndex = level - 2;
            if (upgradeIndex >= 0 && upgradeIndex < data.upgrades.Count)
            {
                TowerUpgrade upgrade = data.upgrades[upgradeIndex];
                _currentDamage = Mathf.Max(0.1f, upgrade.damage);
                _currentAttackInterval = Mathf.Max(0.1f, upgrade.attackInterval);
                _currentAttackRadius = Mathf.Max(0.1f, upgrade.attackRadius);
                _currentSlowFactor = Mathf.Max(0.1f, upgrade.slowFactor);
                _currentSlowDuration = Mathf.Max(0.1f, upgrade.slowDuration);
            }
        }
    }

    /// <summary>Gets the current attack radius after upgrades.</summary>
    public float GetCurrentAttackRadius()
    {
        return _currentAttackRadius;
    }

    public void StopShooting()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _currentAttackRadius, _hits, LayerMask.GetMask("Enemy"));

        if (count > 0 && _shootingCoroutine == null)
            _shootingCoroutine = StartCoroutine(ShootRoutine());
        else if (count == 0 && _shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            _colorPulse.Pulse();

            switch (attackPattern)
            {
                case AttackPattern.Area: DealDamageToAll(); break;
                case AttackPattern.Slow: ApplySlowToAll(); break;
                default: GetTarget()?.TakeDamage(_currentDamage); break;
            }

            yield return new WaitForSeconds(_currentAttackInterval);
        }
    }

    private void DealDamageToAll()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _currentAttackRadius, _hits, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < count; i++)
            _hits[i].GetComponent<EnemyHealth>()?.TakeDamage(_currentDamage);
    }

    private void ApplySlowToAll()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _currentAttackRadius, _hits, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < count; i++)
            _hits[i].GetComponent<EnemyMover>()?.ApplySlowness(_currentSlowFactor, _currentSlowDuration);
    }

    private EnemyHealth GetTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _currentAttackRadius, _hits, LayerMask.GetMask("Enemy"));
        if (count == 0) return null;

        EnemyHealth bestHealth = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            EnemyHealth health = _hits[i].GetComponent<EnemyHealth>();
            EnemyMover mover = _hits[i].GetComponent<EnemyMover>();
            if (health == null || mover == null) continue;

            float score = attackPattern switch
            {
                AttackPattern.AttackFirst   => mover.PathProgress,
                AttackPattern.AttackLast    => -mover.PathProgress,
                AttackPattern.AttackClosest => -Vector3.Distance(transform.position, _hits[i].transform.position),
                _                           => 0f
            };

            if (score > bestScore)
            {
                bestHealth = health;
                bestScore = score;
            }
        }

        return bestHealth;
    }
}