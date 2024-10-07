using System.Collections;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _illusionPrefab;
    [SerializeField] private Transform[] _illusionSpawnPoints;
    [SerializeField] private Transform _laserPoint;
    [SerializeField] private float _enemyDamage = 10f;
    [SerializeField] private float _illusionDamage = .5f;
    [SerializeField] private float _laserCooldown = 6f;
    [SerializeField] private float _illusionRespawnCooldown = 20f;
    [SerializeField] private float _minDistanceFromPlayer = 5f;
    [SerializeField] private float _maxDistanceFromPlayer = 15f;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _hoverHeight = 3f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _laserDuration = 0.5f;

    private Transform _player;
    private Illusion[] _illusions;
    private bool _illusionsActive = false;
    private bool _canSummonIllusions = true;
    private int _layerMask;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
        _illusions = new Illusion[_illusionSpawnPoints.Length];
        StartCoroutine(AttackRoutine());

        _lineRenderer.enabled = false;
        _layerMask = _playerLayer;
    }

    private void Update()
    {
        MoveToAwayFromPlayer();
        MaintainHoverHeight();
        LookAtPlayer();

        if (!_illusionsActive && _canSummonIllusions)
        {
            SummonIllusions();
        }
    }

    private void MoveToAwayFromPlayer()
    {
        Vector3 direction = transform.position - _player.position;

        if (direction.magnitude < _minDistanceFromPlayer)
        {
            transform.position += direction.normalized * (_speed * Time.deltaTime);
        }
        else if (direction.magnitude > _maxDistanceFromPlayer)
        {
            transform.position -= direction.normalized * (_speed * Time.deltaTime);
        }
    }

    private void MaintainHoverHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, _groundLayer))
        {
            Vector3 desiredPosition = hit.point + Vector3.up * _hoverHeight;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _speed);
        }
    }

    private void LookAtPlayer()
    {
        if (_player != null)
        {
            Vector3 direction = (_player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _speed);
        }
    }

    private void SummonIllusions()
    {
        for (int i = 0; i < _illusions.Length; i++)
        {
            GameObject illusionObj =
                Instantiate(_illusionPrefab, _illusionSpawnPoints[i].position, Quaternion.identity);
            _illusions[i] = illusionObj.GetComponent<Illusion>();
            _illusions[i].Initialize(this, _enemyDamage * _illusionDamage);
        }

        _illusionsActive = true;
        _canSummonIllusions = false;
    }

    private bool AreAllIllusionsDestroyed()
    {
        foreach (var illusion in _illusions)
        {
            if (illusion != null)
                return false;
        }
        return true;
    }

    public void OnIllusionDestroyed(Illusion destroyedIllusion)
    {
        for (int i = 0; i < _illusions.Length; i++)
        {
            if (_illusions[i] == destroyedIllusion)
            {
                _illusions[i] = null;
                break;
            }
        }

        if (AreAllIllusionsDestroyed())
        {
            _illusionsActive = false;
            StartCoroutine(IllusionRespawnCooldown());
        }
    }

    private IEnumerator IllusionRespawnCooldown()
    {
        yield return new WaitForSeconds(_illusionRespawnCooldown);
        _canSummonIllusions = true;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_laserCooldown);
            FireLaser();
        }
    }

    private void FireLaser()
    {
        if (_player == null)
            return;

        _lineRenderer.enabled = true;

        _lineRenderer.SetPosition(0, _laserPoint.position);
        _lineRenderer.SetPosition(1, _player.position);

        Vector3 direction = (_player.position - _laserPoint.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(_laserPoint.position, direction, out hit, Mathf.Infinity, _layerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(_enemyDamage);
                Debug.Log("Laser hit player, damage: " + _enemyDamage);
            }
        }

        StartCoroutine(HideLaser());
    }

    private IEnumerator HideLaser()
    {
        yield return new WaitForSeconds(_laserDuration);
        _lineRenderer.enabled = false;
    }
}