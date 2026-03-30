using UnityEngine;
using TMPro;

/// <summary>
/// Manages the bowling score display.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI roundText;
    [SerializeField] TextMeshProUGUI feedbackText; // For Strike/Spare messages

    [Header("UI (World Space - for VR)")]
    [SerializeField] TextMeshPro scoreText3D;    // Alternative: World Space text
    [SerializeField] TextMeshPro roundText3D;
    [SerializeField] TextMeshPro feedbackText3D;

    int totalScore = 0;
    int currentRound = 1;
    int currentAttempt = 1;
    int maxRounds = 10;
    int firstAttemptScore = 0;

    void Start()
    {
        UpdateUI();
        SetFeedback("");
    }

    public void AddScore(int pinsKnocked)
    {
        totalScore += pinsKnocked;
        UpdateUI();
    }

    public void SetRound(int round, int attempt)
    {
        currentRound = round;
        currentAttempt = attempt;
        UpdateUI();
    }

    public void ShowStrike()
    {
        SetFeedback("STRIKE!");
        Invoke(nameof(ClearFeedback), 2.5f);
    }

    public void ShowSpare()
    {
        SetFeedback("SPARE!");
        Invoke(nameof(ClearFeedback), 2.5f);
    }

    public void ShowMiss()
    {
        SetFeedback("Miss...");
        Invoke(nameof(ClearFeedback), 2f);
    }

    public void ShowGameOver()
    {
        SetFeedback($"GAME OVER!\nFinal Score: {totalScore}");
    }

    public void ResetScore()
    {
        totalScore = 0;
        currentRound = 1;
        currentAttempt = 1;
        UpdateUI();
        SetFeedback("");
    }

    void UpdateUI()
    {
        string scoreString = $"Score: {totalScore}";
        string roundString = $"Round: {currentRound}/{maxRounds}  Attempt: {currentAttempt}/2";

        // Update Canvas UI
        if (scoreText != null) scoreText.text = scoreString;
        if (roundText != null) roundText.text = roundString;

        // Update World Space UI (for VR)
        if (scoreText3D != null) scoreText3D.text = scoreString;
        if (roundText3D != null) roundText3D.text = roundString;
    }

    void SetFeedback(string message)
    {
        if (feedbackText != null) feedbackText.text = message;
        if (feedbackText3D != null) feedbackText3D.text = message;
    }

    void ClearFeedback()
    {
        SetFeedback("");
    }

    public int TotalScore => totalScore;
    public int CurrentRound => currentRound;
}