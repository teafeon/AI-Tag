using System.Collections;
using UnityEngine;
using TMPro; 
using Unity.MLAgents.Policies; 

public class GameManager : MonoBehaviour
{
    [Header("Agents")]
    public TagAgent seekerAgent; 
    public TagAgent runnerAgent; 

    [Header("Agent Behaviors")]
    public BehaviorParameters seekerBehavior; 
    public BehaviorParameters runnerBehavior; 

    [Header("Score & Timer UI")]
    public TextMeshProUGUI orangeScoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI blueScoreText;

    [Header("Menu UI")]
    public GameObject roleMenuPanel;
    public TextMeshProUGUI countdownText;
    public CanvasGroup roleMenuCanvasGroup;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winLoseText;
    public CanvasGroup gameOverCanvasGroup;

    private int orangeScore = 0;
    private int blueScore = 0;
    private float timer = 30f;

    private bool matchRunning = false;
    private string selectedRole = "";

    void Start()
    {
        UpdateUI();
        
        // Hide Game Over panel just in case it was left on
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        ShowMenu(); 
    }

    void Update()
    {
        if (!matchRunning) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(timer).ToString();

        // If time runs out, the Runner (Blue) wins the round!
        if (timer <= 0)
        {
            blueScore++;
            runnerAgent.AddReward(1.0f);   
            seekerAgent.AddReward(-1.0f);  
            
            // Trigger Game Over with reason: "TimeOut"
            ProcessGameOver("TimeOut");
        }
    }

    // --- GAME OVER LOGIC ---

    public void SeekerCaughtRunner()
    {
        if (!matchRunning) return; 

        orangeScore++;
        // The seeker's script already gives it a +1 reward
        
        // Trigger Game Over with reason: "Caught"
        ProcessGameOver("Caught");
    }

    private void ProcessGameOver(string endReason)
    {
        matchRunning = false;
        Time.timeScale = 0f;

        bool humanWon = false;

        if (selectedRole == "Spectate")
        {
            StartCoroutine(GameOverRoutine(false, endReason));
            return;
        }

        if (endReason == "TimeOut")
        {
            humanWon = (selectedRole == "Runner");
        }
        else if (endReason == "Caught")
        {
            humanWon = (selectedRole == "Seeker");
        }

        StartCoroutine(GameOverRoutine(humanWon, endReason));
    }

    IEnumerator GameOverRoutine(bool humanWon, string endReason)
    {
        matchRunning = false;
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;

        if (selectedRole == "Spectate")
        {
            if (endReason == "TimeOut")
            {
                winLoseText.text = "RUNNER WINS!";
                winLoseText.color = Color.cyan;
            }
            else if (endReason == "Caught")
            {
                winLoseText.text = "SEEKER WINS!";
                winLoseText.color = new Color(1f, 0.5f, 0f); // orange-ish
            }
        }
        else if (humanWon)
        {
            winLoseText.text = "YOU WIN!";
            winLoseText.color = Color.green;
        }
        else
        {
            winLoseText.text = "YOU LOSE!";
            winLoseText.color = Color.red;
        }

        yield return new WaitForSecondsRealtime(0.25f);
        yield return StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 0f, 1f, 0.25f));
        yield return new WaitForSecondsRealtime(1.5f);
        yield return StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 1f, 0f, 0.25f));

        gameOverPanel.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);

        ResetRound();
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        cg.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;
    }

    // --- MENU LOGIC ---

    void ShowMenu()
    {
        Time.timeScale = 0f; 
        matchRunning = false;

        roleMenuPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);

        roleMenuCanvasGroup.alpha = 0f;
        roleMenuCanvasGroup.interactable = false;
        roleMenuCanvasGroup.blocksRaycasts = false;

        CameraManager camManager = FindObjectOfType<CameraManager>();
        if (camManager != null)
        {
            camManager.ShowArenaMenuView();
        }

        StartCoroutine(FadeMenuInRoutine());
    }

    IEnumerator FadeMenuInRoutine()
    {
        yield return StartCoroutine(FadeCanvasGroup(roleMenuCanvasGroup, 0f, 1f, 0.25f));

        roleMenuCanvasGroup.interactable = true;
        roleMenuCanvasGroup.blocksRaycasts = true;
    }

    public void ChooseRunner()
    {
        selectedRole = "Runner";
        StartCoroutine(CountdownRoutine());
    }

    public void ChooseSeeker()
    {
        selectedRole = "Seeker";
        StartCoroutine(CountdownRoutine());
    }

    public void ChooseSpectate()
    {
        selectedRole = "Spectate";
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        roleMenuCanvasGroup.interactable = false;
        roleMenuCanvasGroup.blocksRaycasts = false;

        yield return StartCoroutine(FadeCanvasGroup(roleMenuCanvasGroup, 1f, 0f, 0.25f));

        roleMenuPanel.SetActive(false);
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);

        countdownText.gameObject.SetActive(false);

        if (selectedRole == "Runner")
        {
            runnerBehavior.BehaviorType = BehaviorType.HeuristicOnly;
            seekerBehavior.BehaviorType = BehaviorType.Default;

            runnerAgent.moveSpeed = 35f;
            seekerAgent.moveSpeed = 50f;
        }
        else if (selectedRole == "Seeker")
        {
            seekerBehavior.BehaviorType = BehaviorType.HeuristicOnly;
            runnerBehavior.BehaviorType = BehaviorType.Default;

            seekerAgent.moveSpeed = 35f;
            runnerAgent.moveSpeed = 50f;
        }
        else if (selectedRole == "Spectate")
        {
            runnerBehavior.BehaviorType = BehaviorType.Default;
            seekerBehavior.BehaviorType = BehaviorType.Default;

            runnerAgent.moveSpeed = 50f;
            seekerAgent.moveSpeed = 35f;
        }

        FindObjectOfType<CameraManager>().SetupCameras();

        Time.timeScale = 1f;
        matchRunning = true;
    }

    // --- UTILITY ---

    private void ResetRound()
    {
        timer = 30f;
        UpdateUI();

        seekerAgent.EndEpisode();
        runnerAgent.EndEpisode();

        ShowMenu();
    }

    private void UpdateUI()
    {
        orangeScoreText.text = orangeScore.ToString();
        blueScoreText.text = blueScore.ToString();
        timerText.text = Mathf.CeilToInt(timer).ToString();
    }
}