using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public class WeaponData : AssetObject
    {
        public FPVector3 Offset;
        public FP Cooldown;
        public BulletData BulletData;
    }
}