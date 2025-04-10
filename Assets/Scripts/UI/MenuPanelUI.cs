using UnityEngine;
using Fusion;

public class MenuPanelUI : UIPanel
{
    public NetworkRunnerHandler runnerHandler;
    public MatchmakingPanelUI matchmakingPanel;

    public void OnClickRandomMatch()
    {
        Hide();
        matchmakingPanel.Show();
        matchmakingPanel.StartRandomMatch();
    }

    public void OnClickCodeMatch(string code)
    {
        Hide();
        matchmakingPanel.Show();
        matchmakingPanel.StartCodeMatch(code); // you'd get the code from an input field
    }

    public void OnClickPlayAI()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
