using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // References to your buttons
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;

    // Event that other scripts can subscribe to
    public delegate void GameEvent();
    public static event GameEvent OnGameStart;
    public static event GameEvent OnGameRestart;

    private void Start()
    {
        // Add listeners to the buttons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void StartGame()
    {
        Debug.Log("Starting game...");

        // Trigger the game start event - any script that needs to respond can listen
        OnGameStart?.Invoke();
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        // If in editor, stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // If in build, quit the application
            Application.Quit();
#endif
    }

    private void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Trigger the game restart event
        OnGameRestart?.Invoke();
    }

    // Clean up listeners when destroyed
    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }
}