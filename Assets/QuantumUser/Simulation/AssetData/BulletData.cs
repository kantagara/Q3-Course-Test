using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public class BulletData : AssetObject
    {
        public FP Duration;
        public EntityPrototype Entity;
        public FP Damage;
        public FP Speed;
    }
}