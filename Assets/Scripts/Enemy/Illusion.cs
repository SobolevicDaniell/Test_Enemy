using UnityEngine;

public class Illusion : MonoBehaviour
{
    private float _damage;
    private FlyingEnemy _owner;
    private Transform _player;
    [SerializeField] private float _moveSpeed = 5f;
    
    public void Initialize(FlyingEnemy owner, float damage)
    {
        _owner = owner;
        _damage = damage;
        _player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (_player != null)
        {
            MoveTowardsPlayer();
            LookAtPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }
    private void LookAtPlayer()
    {
        transform.LookAt(new Vector3(_player.position.x, transform.position.y, _player.position.z));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
            Debug.Log("Illusion hit player, damage: " + _damage);

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        _owner.OnIllusionDestroyed(this);
    }
}