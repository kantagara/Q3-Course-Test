using Quantum;
using UnityEngine;

public class GameStateView : MonoBehaviour
{
    [SerializeField] private GameObject weaponHolderUi;
    [SerializeField] private GameObject countdownUI;
    [SerializeField] private GameOverUI gameOverUI;
    
    
    private void Start()
    {
        QuantumEvent.Subscribe<EventGameEnded>(this, GameEnded);
        
        weaponHolderUi.SetActive(true);
        countdownUI.SetActive(true);
        gameOverUI.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void GameEnded(EventGameEnded callback)
    {
        var f = callback.Game.Frames.Verified;
        weaponHolderUi.SetActive(false);
        countdownUI.SetActive(false);
        gameOverUI.gameObject.SetActive(true);
        var playerData = f.GetPlayerData(f.Get<PlayerLink>(callback.Winner).Player);
        gameOverUI.SetWinnerText(playerData.PlayerNickname);
    }
}