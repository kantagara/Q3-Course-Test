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
                        filter.GameManager->CurrentGameState = GameState.GameOver;
                        f.Events.GameManagerChangedState(GameState.GameOver);
                        f.Events.GameEnded(winner.Entity);
                    }
                    else
                    {
                        filter.GameManager->CurrentGameState = GameState.Playing;
                        f.Events.GameManagerChangedState(GameState.Playing);
                    }
                }
            }
            else if (filter.GameManager->CurrentGameState == GameState.GameOver)
            {
                filter.GameManager->TimeToDisconnectAfterWinning -= f.DeltaTime;
                if (filter.GameManager->TimeToDisconnectAfterWinning <= 0)
                {
                    
                }
            }
        }

        public void OnAdded(Frame f, EntityRef entity, GameManager* gameManager)
        {
            var config = f.FindAsset(gameManager->GameManagerConfig);
            gameManager->CurrentGameState = GameState.WaitingForPlayers;
            gameManager->TimeToWaitForPlayers = config.TimeToWaitForPlayers;
            gameManager->TimeToDisconnectAfterWinning = config.TimeToDisconnectAfterWinning;
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            if (!ThereIsWinner(f, out var winner))
                return;
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
            gameManager->CurrentGameState = GameState.GameOver;
            f.Events.GameManagerChangedState(GameState.GameOver);
            f.Events.GameEnded(winner.Entity);

        }

        public void PlayerKilled(Frame f, EntityRef player)
        {
            if (!ThereIsWinner(f, out var winner))
                return;
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
            gameManager->CurrentGameState = GameState.GameOver;
            f.Events.GameManagerChangedState(GameState.GameOver);
            f.Events.GameEnded(winner.Entity);
        }

        private bool ThereIsWinner(Frame f, out EntityComponentPair<PlayerLink> winner)
        {
            var count = 0;
            winner = default;
            foreach (var pair in f.GetComponentIterator<PlayerLink>())
            {
                count++;
                winner = pair;
            }

            if (count > 1)
                return false;
            
            if (count < 0)
            {
                Log.Warn("No winners found.");
                return false;
            }

            return true;
        }
    }
}