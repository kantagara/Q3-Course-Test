using System;
using Quantum;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class VFXPool : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfx;
    ObjectPool<ParticleSystem> _bulletImpactPool;

    void Start()
    {
        // Initialize the particle system pool
        _bulletImpactPool = new ObjectPool<ParticleSystem>(
            createFunc: CreateParticleSystem, // Function to create a new ParticleSystem
            actionOnGet: OnParticleSystemGet, // Action to perform when a ParticleSystem is taken from the pool
            actionOnRelease: OnParticleSystemRelease, // Action to perform when a ParticleSystem is returned to the pool
            actionOnDestroy: OnDestroyParticleSystem, // Action to perform when a ParticleSystem is destroyed
            collectionCheck: false, // Enable this for additional safety checks in development
            defaultCapacity: 10, // Initial capacity
            maxSize: 20 // Maximum number of objects the pool can hold
        );

        QuantumEvent.Subscribe<EventBulletHit>(this, BulletHit);
    }

    private void BulletHit(EventBulletHit callback)
    {
        var f = callback.Game.Frames.Verified;
        var bullet = f.Get<Bullet>(callback.Bullet);
        var bulletTransform = f.Get<Transform2D>(callback.Bullet);
        var impact = _bulletImpactPool.Get();
        impact.transform.position = new Vector3(bulletTransform.Position.X.AsFloat, bullet.HeightOffset.AsFloat, bulletTransform.Position.Y.AsFloat);
    }

    // Function to create a new ParticleSystem
    private ParticleSystem CreateParticleSystem()
    {
        ParticleSystem ps = Instantiate(vfx);
        ps.gameObject.SetActive(false); // Start inactive
        return ps;
    }

    // Action to perform when a ParticleSystem is taken from the pool
    private void OnParticleSystemGet(ParticleSystem ps)
    {
        ps.gameObject.SetActive(true);
        ps.Play();
    }

    // Action to perform when a ParticleSystem is returned to the pool
    private void OnParticleSystemRelease(ParticleSystem ps)
    {
        ps.Stop();
        ps.gameObject.SetActive(false);
    }

    // Action to perform when a ParticleSystem is destroyed (optional)
    private void OnDestroyParticleSystem(ParticleSystem ps)
    {
        Destroy(ps.gameObject);
    }
}