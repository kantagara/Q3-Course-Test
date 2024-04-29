using Quantum;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;
    public void SetWinnerText(string winnerNickname)
    {
        winnerText.SetText($"Winner is {winnerNickname}");
    }
    
    public async void Disconnect()
    {
        await QuantumRunner.ShutdownAllAsync();
    }
}
