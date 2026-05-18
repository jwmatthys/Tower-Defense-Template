// TowerFrozen.cs
using System.Collections;
using UnityEngine;

public class TowerFrozen : MonoBehaviour
{
    [SerializeField] private Material frozenMaterial;

    private Coroutine _freezeCoroutine;
    private MeshRenderer[] _renderers;
    private Material[] _originalMaterials;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
        _originalMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalMaterials[i] = _renderers[i].sharedMaterial;
    }

    public void Freeze(float dur)
    {
        if (_freezeCoroutine != null)
            StopCoroutine(_freezeCoroutine);
        _freezeCoroutine = StartCoroutine(FreezeRoutine(dur));
    }

    private IEnumerator FreezeRoutine(float dur)
    {
        if (TryGetComponent(out TowerAttack towerAttack))
        {
            Debug.Log($"Tower {this.gameObject.name} is freezing.");
            ApplyFrozenMaterial();
            towerAttack.StopShooting();
            towerAttack.enabled = false;
            yield return new WaitForSeconds(dur);
            towerAttack.enabled = true;
            RestoreOriginalMaterials();
        }
        _freezeCoroutine = null;
    }

    private void ApplyFrozenMaterial()
    {
        if (frozenMaterial == null) return;
        foreach (MeshRenderer r in _renderers)
            r.sharedMaterial = frozenMaterial;
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].sharedMaterial = _originalMaterials[i];
    }
}