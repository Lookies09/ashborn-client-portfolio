using UnityEngine;

public class StraightProjectile : MonoBehaviour, IProjectile
{
    private int _damage;
    private float _speed;
    private float _lifetime = 3f;
    private Vector3 _direction;
    private GameObject _hitEffectPrefab;
    private AudioClip _hitSound;
    private float _spawnTime;
    private bool _hasHit;


    private void Update()
    {
        if (Time.time - _spawnTime > _lifetime)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
            return;
        }

        transform.position += _direction * _speed * Time.deltaTime;
    }

    public void Init(int damage, float speed, float lifeTime, Vector3 direction, GameObject hitEffect, AudioClip hitSound)
    {
        _damage = damage;
        _speed = speed; 
        _direction = direction;
        _hitEffectPrefab = hitEffect;
        _hitSound = hitSound;
        _spawnTime = Time.time;
        _hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;


        if (other.TryGetComponent<IDamageable>(out var dmg) && other.CompareTag("Player"))
        {
            dmg.TakeDamage(_damage);
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Spawn(_hitEffectPrefab.name, transform.position, Quaternion.identity);                
            }

            if (_hitSound != null)
            {
                AudioSource.PlayClipAtPoint(_hitSound, transform.position);
            }
            _hasHit = true;
            ObjectPoolManager.Instance.Despawn(gameObject);            
        }

        if (other.CompareTag("Environment"))
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Spawn(_hitEffectPrefab.name, transform.position, Quaternion.identity);                
            }

            if (_hitSound != null)
            {
                AudioSource.PlayClipAtPoint(_hitSound, transform.position);
            }
            _hasHit = true;
            ObjectPoolManager.Instance.Despawn(gameObject);

        }

        
    }

}
