using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Pool;

[Serializable]
public class ParticleSystemPool
{
    [SerializeField] private ParticleSystem particle;

    private ObjectPool<ParticleSystem> _particlesPool;

    public void Init()
    {
        _particlesPool = new ObjectPool<ParticleSystem>(CreateParticleSystem, OnParticleSystemGet,
            OnParticleSystemRelease, OnDestroyParticleSystem);
    }

    // Function to create a new ParticleSystem
    private ParticleSystem CreateParticleSystem()
    {
        ParticleSystem ps = UnityEngine.Object.Instantiate(particle);
        ps.gameObject.SetActive(false); // Start inactive
        return ps;
    }

    // Action to perform when a ParticleSystem is taken from the pool
    private void OnParticleSystemGet(ParticleSystem ps)
    {
        ps.gameObject.SetActive(true);
        ps.Play();
    }

    private void OnParticleSystemRelease(ParticleSystem ps)
    {
        ps.Stop();
        ps.gameObject.SetActive(false);
    }

    private void OnDestroyParticleSystem(ParticleSystem ps)
    {
        UnityEngine.Object.Destroy(ps.gameObject);
    }

    public ParticleSystem Get()
    {
        return _particlesPool.Get();
    } 
    public void Release(ParticleSystem particleSystem)
    {
        _particlesPool.Release(particleSystem);
    }

}


public class VFXPool : MonoBehaviour
{
    [SerializeField] private ParticleSystemPool weaponFiredPool;


    private void Awake()
    {
        weaponFiredPool.Init();
        QuantumEvent.Subscribe<EventWeaponFired>(this, OnWeaponFired);
    }

    private void OnWeaponFired(EventWeaponFired callback)
    {
        var f = callback.Game.Frames.Predicted;
        
        var ownerTransform = f.Get<Transform2D>(callback.Owner);
        var position = ownerTransform.Position + callback.Offset.XZ.Rotate(ownerTransform.Rotation);
        var ps = weaponFiredPool.Get();
        ps.transform.position = position.XOY.ToUnityVector3() + Vector3.up * callback.Offset.Y.AsFloat;
        StartCoroutine(ReturnBackInPool(ps, weaponFiredPool));
    }

    private IEnumerator ReturnBackInPool(ParticleSystem ps, ParticleSystemPool particleSystemPool)
    {
        yield return new WaitForSeconds(ps.main.duration);
        particleSystemPool.Release(ps);
    }
}