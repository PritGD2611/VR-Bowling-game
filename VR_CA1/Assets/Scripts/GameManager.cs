using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag ALL your pre-placed balls here")]
    [SerializeField] List<BowlingBall> bowlingBalls = new List<BowlingBall>();
    [SerializeField] PinManager pinManager;
    [SerializeField] ScoreManager scoreManager;

    [Header("Game Settings")]
    [SerializeField] int maxRounds = 10;
    [SerializeField] int attemptsPerRound = 2;
    [SerializeField] float pinSettleTime = 2f;
    [SerializeField] float resetDelay = 2f;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip strikeSound;
    [SerializeField] AudioClip spareSound;
    [SerializeField] AudioClip gutterSound;
    [SerializeField] AudioClip pinHitSound;

    int currentRound = 1;
    int currentAttempt = 1;
    int pinsKnockedThisRound = 0;
    bool isProcessing = false;
    bool gameOver = false;

    void Start()
    {
        StartGame();
    }

    // ═══════════════════════════════════════════════════
    // SUBSCRIBE TO ALL BALL EVENTS
    // ═══════════════════════════════════════════════════
    void OnEnable()
    {
        foreach (var ball in bowlingBalls)
        {
            if (ball == null) continue;
            ball.OnBallReleased += HandleBallReleased;
            ball.OnBallStopped += HandleBallStopped;
            ball.OnBallMissed += HandleBallMissed;    // ADD THIS
            ball.OnInvalidThrow += HandleInvalidThrow;
            ball.OnBallDestroyed += HandleBallDestroyed;
        }
    }

    void OnDisable()
    {
        foreach (var ball in bowlingBalls)
        {
            if (ball == null) continue;
            ball.OnBallReleased -= HandleBallReleased;
            ball.OnBallStopped -= HandleBallStopped;
            ball.OnBallMissed -= HandleBallMissed;    // ADD THIS
            ball.OnInvalidThrow -= HandleInvalidThrow;
            ball.OnBallDestroyed -= HandleBallDestroyed;
        }
    }

    // ADD THIS NEW HANDLER
    void HandleBallMissed(BowlingBall ball)
    {
        if (gameOver || isProcessing) return;
        isProcessing = true;
        Debug.Log("Ball missed all pins!");
        StartCoroutine(ProcessMissedThrow(ball));
    }

    // ADD THIS COROUTINE
    IEnumerator ProcessMissedThrow(BowlingBall ball)
    {
        scoreManager.ShowMiss();
        PlaySound(gutterSound);
        scoreManager.AddScore(0);

        yield return new WaitForSeconds(resetDelay);

        ball.ResetBall();

        if (currentAttempt >= attemptsPerRound)
        {
            NextRound();
        }
        else
        {
            // Miss on attempt 1 - keep ALL pins, go to attempt 2
            currentAttempt++;
            scoreManager.SetRound(currentRound, currentAttempt);
            // DO NOT call ClearFallenPins() - keep all pins!
            Debug.Log("Attempt 2 - All pins kept (ball missed)");
        }

        isProcessing = false;
    }

    // ═══════════════════════════════════════════════════
    // GAME FLOW
    // ═══════════════════════════════════════════════════
    public void StartGame()
    {
        currentRound = 1;
        currentAttempt = 1;
        pinsKnockedThisRound = 0;
        isProcessing = false;
        gameOver = false;

        scoreManager.ResetScore();
        scoreManager.SetRound(currentRound, currentAttempt);
        pinManager.ResetAllPins();
        ResetAllBalls();

        Debug.Log("🎳 GAME STARTED!");
    }

    // ═══════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Ball was picked up and thrown
    /// </summary>
    void HandleBallReleased(BowlingBall ball)
    {
        if (gameOver || isProcessing) return;
        Debug.Log($"Ball thrown: {ball.gameObject.name}");
    }

    /// <summary>
    /// Ball stopped normally → Wait for pins → Count score
    /// </summary>
    void HandleBallStopped(BowlingBall ball)
    {
        if (gameOver || isProcessing) return;

        isProcessing = true;
        Debug.Log("Ball stopped - counting pins...");
        StartCoroutine(ProcessNormalThrow(ball));
    }

    /// <summary>
    /// Ball went into gutter → 0 points this attempt
    /// </summary>
    void HandleInvalidThrow(BowlingBall ball)
    {
        if (gameOver || isProcessing) return;

        isProcessing = true;
        Debug.Log("GUTTER BALL - 0 points");
        StartCoroutine(ProcessZeroThrow("Gutter!"));
    }

    /// <summary>
    /// Ball went completely out of bounds → 0 points
    /// </summary>
    void HandleBallDestroyed(BowlingBall ball)
    {
        if (gameOver || isProcessing) return;

        isProcessing = true;
        Debug.Log("BALL OUT OF BOUNDS - 0 points");
        StartCoroutine(ProcessZeroThrow("Out of Bounds!"));
    }

    // ═══════════════════════════════════════════════════
    // COROUTINES
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Normal throw - wait for pins to settle, count score
    /// </summary>
    IEnumerator ProcessNormalThrow(BowlingBall ball)
    {
        // Wait for pins to settle
        yield return new WaitForSeconds(pinSettleTime);

        // Count fallen pins
        int newlyFallen = pinManager.CountAndMarkNewlyFallenPins();
        pinsKnockedThisRound += newlyFallen;

        Debug.Log($"Pins knocked: {newlyFallen} | Round total: {pinsKnockedThisRound}");

        // Add to score
        scoreManager.AddScore(newlyFallen);

        if (newlyFallen > 0)
            PlaySound(pinHitSound);

        // ── Check for Strike ──────────────────────────
        if (currentAttempt == 1 && pinManager.AllPinsDown())
        {
            Debug.Log("🎳 STRIKE!");
            scoreManager.ShowStrike();
            PlaySound(strikeSound);
            yield return new WaitForSeconds(resetDelay);
            ball.ResetBall();
            NextRound();
        }
        // ── Check for Spare ───────────────────────────
        else if (currentAttempt == 2 && pinManager.AllPinsDown())
        {
            Debug.Log("🎳 SPARE!");
            scoreManager.ShowSpare();
            PlaySound(spareSound);
            yield return new WaitForSeconds(resetDelay);
            ball.ResetBall();
            NextRound();
        }
        // ── Second attempt done → Next round ──────────
        else if (currentAttempt >= attemptsPerRound)
        {
            if (newlyFallen == 0)
                scoreManager.ShowMiss();
            yield return new WaitForSeconds(resetDelay);
            ball.ResetBall();
            NextRound();
        }
        // ── First attempt done → Second attempt ───────
        else
        {
            yield return new WaitForSeconds(resetDelay);
            ball.ResetBall();
            NextAttempt();
        }

        isProcessing = false;
    }

    /// <summary>
    /// Invalid throw (gutter/out of bounds) - 0 points, advance attempt
    /// </summary>
    IEnumerator ProcessZeroThrow(string reason)
    {
        // Show feedback
        scoreManager.ShowMiss();
        PlaySound(gutterSound);

        // Score 0 for this attempt
        scoreManager.AddScore(0);

        Debug.Log($"0 points: {reason}");

        yield return new WaitForSeconds(resetDelay);

        // Advance game state
        if (currentAttempt >= attemptsPerRound)
        {
            NextRound();
        }
        else
        {
            NextAttempt();
        }

        isProcessing = false;
    }

    // ═══════════════════════════════════════════════════
    // GAME STATE
    // ═══════════════════════════════════════════════════
    void NextAttempt()
    {
        currentAttempt++;
        pinsKnockedThisRound = 0;
        scoreManager.SetRound(currentRound, currentAttempt);

        // Hide fallen pins, keep standing ones
        pinManager.ClearFallenPins();

        Debug.Log($"Attempt {currentAttempt} - Round {currentRound}");
    }

    void NextRound()
    {
        currentRound++;
        currentAttempt = 1;
        pinsKnockedThisRound = 0;

        if (currentRound > maxRounds)
        {
            EndGame();
            return;
        }

        scoreManager.SetRound(currentRound, currentAttempt);

        // Reset ALL pins for new round
        pinManager.ReactivateAllPins();
        pinManager.ResetAllPins();

        // Reset all balls to original positions
        ResetAllBalls();

        Debug.Log($"Round {currentRound} started!");
    }

    void EndGame()
    {
        gameOver = true;
        scoreManager.ShowGameOver();
        Debug.Log($"🏆 GAME OVER! Final Score: {scoreManager.TotalScore}");
    }

    void ResetAllBalls()
    {
        foreach (var ball in bowlingBalls)
        {
            if (ball != null)
                ball.ResetBall();
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        StartGame();
    }
}