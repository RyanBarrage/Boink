using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MatchmakingPanelUI : UIPanel
{

    private static MatchmakingPanelUI Instance;
    public NetworkRunnerHandler runnerHandler;
    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;
    public MenuPanelUI menuPanel;
    public TextMeshProUGUI matchmakingText;
    private float matchmakingTimeout = 30f;  // Timeout duration in seconds
    private int maxPlayersPerGame = 2;


    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        runner = runnerHandler.GetComponent<NetworkRunner>();
        sceneManager = runnerHandler.GetComponent<NetworkSceneManagerDefault>();
    }

    public static MatchmakingPanelUI Get()
    {
        return Instance;
    }

    public async void CancelMatchmaking()
    {
        await runner.Shutdown(); // Leave session
                                 // Show message / return to menu
        Hide();
        menuPanel.Show();
    }

    public async void StartRandomMatch()
    {
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = Fusion.GameMode.AutoHostOrClient, // or GameMode.Shared
        });

        if (result.Ok)
        {
            await WaitForPlayersAndStart();
        }
        else
        {
            Debug.Log($"Failed to Start: {result.ShutdownReason}");
            Hide();
            menuPanel.Show();
            return;
        }
    }

    public async void StartCodeMatch(string code)
    {
        runner.ProvideInput = true;
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = Fusion.GameMode.AutoHostOrClient,
            SessionName = code,
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });

        if (!result.Ok)
        {
            Debug.Log($"Failed to join match: {result.ShutdownReason}");
            Hide();
            menuPanel.Show();
            return;
        }

        await WaitForPlayersAndStart();
    }

    private async Task WaitForPlayersAndStart()
    {
        float timer = 0f;

        Debug.Log("Waiting for another player to join...");

        while (runner.SessionInfo.PlayerCount < maxPlayersPerGame && timer < matchmakingTimeout)
        {
            await Task.Delay(100);
            timer += 0.1f;
        }

        if (runner.SessionInfo.PlayerCount >= maxPlayersPerGame)
        {
            Debug.Log("Match found! Loading game scene...");
            Hide();
            await runner.SceneManager.LoadScene(SceneRef.FromIndex(SceneManager.GetSceneByName("GameScene").buildIndex),
                new NetworkLoadSceneParameters());
        }
        else
        {
            Debug.LogWarning("Matchmaking timed out.");
            await runner.Shutdown(); // Leave session
                                     // Show message / return to menu
            Hide();
            menuPanel.Show();
        }
        
    }
}