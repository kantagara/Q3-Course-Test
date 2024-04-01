using Photon.Deterministic;

namespace Quantum
{
    public unsafe class SpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if(!firstTime) return;
            var playerEntity = CreatePlayer(f, player);
            SpawnPlayerOnRandomSpawnPoint(f, playerEntity);
        }

        private void SpawnPlayerOnRandomSpawnPoint(Frame f, EntityRef playerEntity)
        {
            var spawnPointList = f.Unsafe.GetPointerSingleton<SpawnPointList>();
            InitializeSpawnPointListIfNotExist(f, spawnPointList);
            
            var spawnPoint = GetRandomSpawnPoint(f, spawnPointList);

            var spawnPointTransform = f.Get<Transform2D>(spawnPoint);
            var playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            
            playerTransform->Position = spawnPointTransform.Position;
        }

        private static EntityRef GetRandomSpawnPoint(Frame f, SpawnPointList* spawnPointList)
        {
            var availableSpawnPoints = f.ResolveList(spawnPointList->availableSpawnPoints);
            var usedSpawnPoints = f.ResolveList(spawnPointList->usedSpawnPoints);
            var randomIndex = f.RNG->Next(0, availableSpawnPoints.Count);
            var spawnPoint = availableSpawnPoints[randomIndex];
            
            availableSpawnPoints.RemoveAt(randomIndex);
            usedSpawnPoints.Add(spawnPoint);
            return spawnPoint;
        }

        private static void InitializeSpawnPointListIfNotExist(Frame f, SpawnPointList* spawnPointList)
        {
            if (spawnPointList->availableSpawnPoints.Ptr == Ptr.Null)
            {
                spawnPointList->availableSpawnPoints = f.AllocateList<EntityRef>();
                spawnPointList->usedSpawnPoints = f.AllocateList<EntityRef>();
                var availableSpawnPoints = f.ResolveList(spawnPointList->availableSpawnPoints);
                foreach (var spawnPoint in f.GetComponentIterator<SpawnPoint>())
                {
                    availableSpawnPoints.Add(spawnPoint.Entity);
                }
            }
        }

        private static EntityRef CreatePlayer(Frame f, PlayerRef player)
        {
            var playerAvatar = f.GetPlayerData(player).PlayerAvatar;
            var playerEntity = f.Create(playerAvatar);
            var playerLink = new PlayerLink()
            {
                Player = player
            };
            f.Add(playerEntity, playerLink);
            return playerEntity;
        }
    }
}