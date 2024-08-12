using MyUtils.Structs;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Weapon")]
    public class WeaponSO : ScriptableObject {
        public Sprite _sprite;
        public Vector3 _firePointPos;
        public Vector3 _spread;
        public int _maxBullet;
        public int _bulletUsage;
        public Transform _bulletPref;
        public float _reloadTime;
        public float _shotDelay;
        public BulletSetting _bulletSetting;
    }
    