using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public class BulletData : AssetObject
    {
        public EntityPrototype Entity;
        public FP Duration;
        public FP Speed;
    }
}