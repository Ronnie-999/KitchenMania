using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    // UI Elements
    [Header("UI Elements")]
    public GameObject gameUI;
    public GameObject startPanel;
    public Button startButton;
    public Button restartButton;
    public Button exitButton;
    public Text scoreText;

    // Game Prefabs
    [Header("Game Prefabs")]
    public GameObject ratSpawnerPrefab;
    public GameObject panSpawner1Prefab;
    public GameObject panSpawner2Prefab;
    public GameObject plateSpawner1Prefab;
    public GameObject plateSpawner2Prefab;
    public GameObject panPrefab;
    public GameObject platePrefab;

    // Game Objects
    private GameObject ratSpawnerInstance;
    private GameObject panSpawner1Instance;
    private GameObject panSpawner2Instance;
    private GameObject plateSpawner1Instance;
    private GameObject plateSpawner2Instance;

    // Game Variables
    private int score = 0;
    private static int panHitCount = 0;
    private static int plateHitCount = 0;

    private void Start()
    {
        // Initialize UI elements
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        // Initialize score display
        UpdateScoreDisplay();

        // Make sure the game UI is visible at start
        if (gameUI != null) gameUI.SetActive(true);
    }

    public void StartGame()
{
    // Hide UI
    if (gameUI != null) gameUI.SetActive(false);

    // Reset score
    ResetScore();

    // Check if rat spawner already exists
    if (ratSpawnerInstance == null && ratSpawnerPrefab != null)
    {
        ratSpawnerInstance = Instantiate(ratSpawnerPrefab, Vector3.zero, Quaternion.identity);

        NetworkObject netObj = ratSpawnerInstance.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn();
            Debug.Log("[GameUIManager] RatSpawner spawned successfully.");
        }
        else
        {
            Debug.LogWarning("[GameUIManager] RatSpawner was already spawned or missing NetworkObject.");
        }
    }
}

    public void RestartGame()
    {
        // Destroy existing spawners
        DestroyGameObjects();


        // Reset score
        ResetScore();
    }

    public void ExitGame()
    {
        // In editor, this stops play mode
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // In build, this quits the application
            Application.Quit();
        #endif
    }


    private void DestroyGameObjects()
    {
        // Destroy RatSpawner if it exists
        if (ratSpawnerInstance != null)
        {
            NetworkObject ratSpawnerNetObj = ratSpawnerInstance.GetComponent<NetworkObject>();
            if (ratSpawnerNetObj != null && ratSpawnerNetObj.IsSpawned)
            {
                ratSpawnerNetObj.Despawn();
            }
            Destroy(ratSpawnerInstance);
            ratSpawnerInstance = null;
        }

        // Destroy PanSpawner1 if it exists
        if (panSpawner1Instance != null)
        {
            NetworkObject panSpawner1NetObj = panSpawner1Instance.GetComponent<NetworkObject>();
            if (panSpawner1NetObj != null && panSpawner1NetObj.IsSpawned)
            {
                panSpawner1NetObj.Despawn();
            }
            Destroy(panSpawner1Instance);
            panSpawner1Instance = null;
        }

        // Destroy PanSpawner2 if it exists
        if (panSpawner2Instance != null)
        {
            NetworkObject panSpawner2NetObj = panSpawner2Instance.GetComponent<NetworkObject>();
            if (panSpawner2NetObj != null && panSpawner2NetObj.IsSpawned)
            {
                panSpawner2NetObj.Despawn();
            }
            Destroy(panSpawner2Instance);
            panSpawner2Instance = null;
        }

        // Destroy PlateSpawner1 if it exists
        if (plateSpawner1Instance != null)
        {
            NetworkObject plateSpawner1NetObj = plateSpawner1Instance.GetComponent<NetworkObject>();
            if (plateSpawner1NetObj != null && plateSpawner1NetObj.IsSpawned)
            {
                plateSpawner1NetObj.Despawn();
            }
            Destroy(plateSpawner1Instance);
            plateSpawner1Instance = null;
        }

        // Destroy PlateSpawner2 if it exists
        if (plateSpawner2Instance != null)
        {
            NetworkObject plateSpawner2NetObj = plateSpawner2Instance.GetComponent<NetworkObject>();
            if (plateSpawner2NetObj != null && plateSpawner2NetObj.IsSpawned)
            {
                plateSpawner2NetObj.Despawn();
            }
            Destroy(plateSpawner2Instance);
            plateSpawner2Instance = null;
        }
    }

    private void ResetScore()
    {
        // Reset score tracking variables
        score = 0;
        panHitCount = 0;
        plateHitCount = 0;
        
        // Update UI
        UpdateScoreDisplay();
    }

    public void UpdateScore(int panHits, int plateHits)
    {
        // Update hit counts
        panHitCount = panHits;
        plateHitCount = plateHits;
        
        // Calculate score: 1 point per pan hit, 2 points per plate hit
        score = (panHitCount * 1) + (plateHitCount * 2);
        
        // Update UI
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString() + "\n" +
                             "Pan Hits: " + panHitCount + "\n" +
                             "Plate Hits: " + plateHitCount;
        }
    }

    public void ShowGameUI()
    {
        if (gameUI != null) gameUI.SetActive(true);
    }

    public void HideGameUI()
    {
        if (gameUI != null) gameUI.SetActive(false);
    }

    // Method to access the pan prefab
    public GameObject GetPanPrefab()
    {
        return panPrefab;
    }

    // Method to access the plate prefab
    public GameObject GetPlatePrefab()
    {
        return platePrefab;
    }
}