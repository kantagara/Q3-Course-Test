using Photon.Deterministic;
using Quantum;

namespace QuantumUser.Simulation.Systems
{
    public unsafe class GameManagerSystem : SystemMainThreadFilter<GameManagerSystem.Filter>, ISignalOnComponentAdded<GameManager>
    , ISignalOnPlayerDisconnected, ISignalPlayerKilled
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameManager* GameManager;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.GameManager->CurrentGameState == GameState.WaitingForPlayers)
            {
                filter.GameManager->TimeToWaitForPlayers -= f.DeltaTime;
                if (filter.GameManager->TimeToWaitForPlayers <= 0)
                {
                    if (ThereIsWinner(f, out var winner))
                    {
                        GameOver(f, filter.GameManager, winner.Entity);
                    }
                    else
                    {
                        filter.GameManager->CurrentGameState = GameState.Playing;
                        f.Events.GameManagerChangedState(GameState.Playing);
                    }
                }
            }
        }

        private void GameOver(Frame f, GameManager* gameManager, EntityRef winner)
        {
            gameManager->CurrentGameState = GameState.GameOver;
            f.Events.GameManagerChangedState(GameState.GameOver);
            f.Events.GameEnded(winner);
        }
       

        public void OnAdded(Frame f, EntityRef entity, GameManager* gameManager)
        {
            var config = f.FindAsset(gameManager->GameManagerConfig);
            gameManager->CurrentGameState = GameState.WaitingForPlayers;
            gameManager->TimeToWaitForPlayers = config.TimeToWaitForPlayers;
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            if (!ThereIsWinner(f, out var winner))
                return;
            GameOver(f, f.Unsafe.GetPointerSingleton<GameManager>(), winner.Entity);
        }

        public void PlayerKilled(Frame f)
        {
            if (!ThereIsWinner(f, out var winner))
                return;
            GameOver(f, f.Unsafe.GetPointerSingleton<GameManager>(), winner.Entity);
        }

        private bool ThereIsWinner(Frame f, out EntityComponentPair<PlayerLink> winner)
        {
            winner = default;
            var count = 0;
            foreach (var player in f.GetComponentIterator<PlayerLink>())
            {
                winner = player;
                count++;
            }
            
            return count == 1;
        }
    }
}