using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScoreService
{
    void ScorePoint(int player);
    void SyncScores(int player1Score, int player2Score);
}

public class ScoreManager : IScoreService
{
    private static ScoreManager _instance;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ScoreManager();  // Initialize the instance if it's null
            }
            return _instance;
        }
    }

    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }

    private ScoreManager() { }

    public void AddScore(int playerID)
    {
        if (playerID == 1) Player1Score++;
        else if (playerID == 2) Player2Score++;
    }

    public void SetScores(int p1, int p2)
    {
        Player1Score = p1;
        Player2Score = p2;
    }

    public void ScorePoint(int player)
    {
        AddScore(player);  // Implement ScorePoint using AddScore logic
    }

    public void SyncScores(int player1Score, int player2Score)
    {
        SetScores(player1Score, player2Score);  // Sync the scores with SetScores logic
    }
}

