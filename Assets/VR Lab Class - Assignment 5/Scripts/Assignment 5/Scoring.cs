using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoring : MonoBehaviour
{
    public Text player1ScoreText;
    public Text player2ScoreText;
    private Dictionary<int, int> playerScores = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } };

    void Start()
    {
        UpdateScoreUI();
        MouseBehaviour.OnMouseHit += AddScore;
    }

    private void AddScore(int playerID)
    {
        if (playerScores.ContainsKey(playerID))
        {
            playerScores[playerID]++;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        player1ScoreText.text = "Player 1 Score: " + playerScores[1];
        player2ScoreText.text = "Player 2 Score: " + playerScores[2];
    }

    public void ResetScores()
    {
        playerScores[1] = 0;
        playerScores[2] = 0;
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        MouseBehaviour.OnMouseHit -= AddScore;
    }
}