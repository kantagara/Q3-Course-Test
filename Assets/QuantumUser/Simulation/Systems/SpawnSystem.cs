using Photon.Deterministic;

namespace Quantum
{
    public unsafe class SpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if(!firstTime) return;
            var playerAvatar = f.GetPlayerData(player).PlayerAvatar;
            var playerEntity = f.Create(playerAvatar);
            var playerLink = new PlayerLink()
            {
                Player = player
            };
            f.Add(playerEntity, playerLink);
            var playerTransform = f.Unsafe.GetPointer<Transform3D>(playerEntity);
            playerTransform->Position += FPVector3.Right * player._index;
        }
    }
}