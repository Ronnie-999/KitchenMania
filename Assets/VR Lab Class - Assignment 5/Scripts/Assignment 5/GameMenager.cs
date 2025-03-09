using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Text gameStatusText;
    public float gameDuration = 60f;
    private bool isGameActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator GameLoop()
    {
        StartGame();
        yield return new WaitForSeconds(gameDuration);
        EndGame();
    }

    public void StartGame()
    {
        if (!IsServer) return;

        isGameActive = true;
        gameStatusText.text = "Game Started!";
        StartGameClientRpc();
    }

    public void EndGame()
    {
        if (!IsServer) return;

        isGameActive = false;
        gameStatusText.text = "Game Over!";
        EndGameClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    private void StartGameClientRpc()
    {
        isGameActive = true;
        gameStatusText.text = "Game Started!";
    }

    [Rpc(SendTo.NotServer)]
    private void EndGameClientRpc()
    {
        isGameActive = false;
        gameStatusText.text = "Game Over!";
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    public void ResetGame()
    {
        if (!IsServer) return;

        StopAllCoroutines();
        ResetGameClientRpc();

        StartCoroutine(GameLoop());
    }

    [Rpc(SendTo.NotServer)]
    private void ResetGameClientRpc()
    {
        gameStatusText.text = "Game Reset!";
        isGameActive = false;
    }
}
