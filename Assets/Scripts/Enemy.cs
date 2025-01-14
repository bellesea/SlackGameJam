using System.Collections;
using MyUtils.Classes;
using MyUtils.Functions;
using MyUtils.Interfaces;
using MyUtils.Structs;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable {
    public RoomController _currentRoom;

    public EnemySO _defaultSetting;
    public Weapon _weapon;
    public Transform _target;
    public Transform _firePoint;
    public Transform _weaponHolder;
    public SpriteRenderer _weaponSR;
    public Transform[] _objectToSpawn;
    public Vector2 _moveDirection;
    public float _nextMoveDirectionChange;
    public float _minPlayerDist;
    private Rigidbody2D _rgb;
    private bool _isReloading;
    public Transform _spriteRenderer;
    private int _delayIndex;
    private float _nextShootTime;
    private float _currentHealth;
    private float _currentSpeed;

    public void Awake() {
        _weapon = new(_defaultSetting._defaultWeapon);
        _weapon.Setup(_firePoint, _weaponSR);
        _target = PlayerController._I.transform;
        _currentHealth = _defaultSetting._maxHealth;
        _rgb = GetComponent<Rigidbody2D>();
        _weapon._bulletsInMagazine = Random.Range(0, _weapon._defaultSettings._maxBullet + 1);
        _nextShootTime = Time.time + _defaultSetting._firstShootDelay.GetValue();
        _currentSpeed = _defaultSetting._speed.GetValue();
    }
    void Update() {
        RotateWeaponToPlayer();
        if (_nextShootTime < Time.time) Shoot();
        if (_nextMoveDirectionChange > Time.time) return;
        if (Vector2.Distance(_target.position, transform.position) > _defaultSetting._playerDist) {
            _moveDirection = _target.position - transform.position;
            _nextMoveDirectionChange = Time.time + Random.Range(1f, 3f);
        }
        else {
            Vector2 newVec = new(Mathf.Clamp(transform.position.x - Random.Range(-6f, 6f), _currentRoom.transform.position.x - 10, _currentRoom.transform.position.x + 10), transform.position.y - Mathf.Clamp(Random.Range(-6f, 6f), _currentRoom.transform.position.y - 7, _currentRoom.transform.position.y + 7));
            _moveDirection = newVec - (Vector2)transform.position;
            _nextMoveDirectionChange = Time.time + Random.Range(1f, 3f);
        }
    }
    void FixedUpdate() {
        _rgb.velocity = _moveDirection.normalized * _currentSpeed;
    }
    public void Shoot() {
        if (_isReloading) return;
        if (_weapon._nextShoot > Time.time) return;
        if (_weapon._bulletsInMagazine <= 0) { StartCoroutine(Reload()); _weapon._allBullets += 30; Debug.Log("No bullets"); return; }
        // Debug.Log("Piu");
        float sp = UnityEngine.Random.Range(0f, _weapon._defaultSettings._spread) * (UnityEngine.Random.Range(0, 2) == 1 ? 1 : -1);
        Quaternion spread = Quaternion.Euler(_weaponHolder.rotation.eulerAngles + new Vector3(0, 0, sp));
        var b = Instantiate(_weapon._defaultSettings._bulletPref, _firePoint.position, spread).GetComponentInChildren<BulletMono>();
        b.Setup(_weapon._defaultSettings._bulletSetting, 1, gameObject.layer, "Enemy");
        Physics2D.IgnoreCollision(b.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        _weapon.Shoot(1);
        _nextShootTime = Time.time + _defaultSetting._shootDelays[_delayIndex].GetValue();
        _delayIndex++;
        if (_delayIndex >= _defaultSetting._shootDelays.Count) _delayIndex = 0;

    }
    private IEnumerator Reload() {
        if (_isReloading) yield return null;
        _isReloading = true;
        yield return new WaitForSeconds(_defaultSetting._reloadSpeed.GetValue());
        _weapon.Reload();
        Debug.Log("Reloaded");
        _isReloading = false;
    }
    private void RotateWeaponToPlayer() {

        Vector2 direction = _target.transform.position - transform.position;
        // direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < -90 || angle > 90) ChangeLocalScale(-1);
        else ChangeLocalScale(1);
        angle -= 90;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        _weaponHolder.transform.rotation = Quaternion.Lerp(_weaponHolder.transform.rotation, rot, 100 * Time.deltaTime);
    }
    void ChangeLocalScale(int x) {
        _weaponHolder.transform.GetChild(0).localScale = new(x, 1, 1);
        _spriteRenderer.localScale = new(Mathf.Abs(_spriteRenderer.localScale.x) * x, _spriteRenderer.localScale.y, 1);
    }

    public void Damage(float v) {
        _currentHealth -= v;
        if (_currentHealth <= 0) Die();
    }
    public void Die() {
        Instantiate(_dieParticle, transform.position, Quaternion.identity);
        if (Random.Range(0f, 1f) < 0.2f) Instantiate(MyRandom.GetFromArray<Transform>(_objectToSpawn), transform.position, Quaternion.identity);
        _currentRoom._enemies.Remove(this);
        _currentRoom.OnEnemyKill();
        Destroy(transform.parent.gameObject);
    }
    public Transform _dieParticle;
    void OnDestroy() {
    }
}
