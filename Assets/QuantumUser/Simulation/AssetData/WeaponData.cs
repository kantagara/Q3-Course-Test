using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public class WeaponData : AssetObject
    {
        public FP Cooldown;
        public FPVector3 Offset;
        public BulletData BulletData;
    }
}