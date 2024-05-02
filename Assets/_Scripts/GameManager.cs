using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int day = 0;
    private Dictionary<Player, bool> players = new Dictionary<Player, bool>();

    public static GameManager Instance { get; private set; } = null;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        StartMatch();
        foreach(Player player in FindObjectsOfType<Player>())
        {
            players.Add(player, false);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ProceedToNextDay();
        }
    }
    public void StartMatch()
    {
        ProceedToNextDay();
    }
    private void ProceedToNextDay()
    {
        day++;
        foreach (ManageableBehaviour beh in ManageableBehaviour.Instances)
        {
            StartCoroutine(beh.DayStarted(day));
        }
    }
    public void PlayerFinishedDay(Player player)
    {
        players[player] = true;
        CheckEndDay();
    }
    private void CheckEndDay()
    {
        foreach(Player player in players.Keys)
        {
            if (players[player] == false) return;
        }
        EndDay();
    }
    private void EndDay()
    {
        ProceedToNextDay();
    }
}
