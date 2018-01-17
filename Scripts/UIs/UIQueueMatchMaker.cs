using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

public class UIQueueMatchMaker : MonoBehaviour
{
    public string gameModeName;
    public QueueMatchMakerClient client;
    public Text textCountMatchingTime;
    public List<GameObject> enableObjectsOnMatching;
    public List<GameObject> enableObjectsOnUnmatching;
    public List<GameObject> disableObjectsOnMatching;
    public List<GameObject> disableObjectsOnUnmatching;
    private bool isMatching;

    protected virtual void Awake()
    {
        client = client ?? FindObjectOfType<QueueMatchMakerClient>();
        client.MatchMakingLobbyCreated += MatchMakingLobbyCreated;
        StartCoroutine(CountMatchingTimeRoutine());
    }

    private void OnDestroy()
    {
        client.MatchMakingLobbyCreated -= MatchMakingLobbyCreated;
    }

    protected void MatchMakingLobbyCreated(int lobbyId)
    {
        isMatching = false;
    }

    public void OnClickStartMatchMaking(string gameModeName)
    {
        this.gameModeName = gameModeName;
        OnClickStartMatchMaking();
    }

    public void OnClickStartMatchMaking()
    {
        var connection = Msf.Client.Connection;
        var packet = new QueueMatchMakerStartPacket();
        packet.gameModeName = gameModeName;
        connection.SendMessage((short)QueueMatchMakerOpCodes.MatchMakingStart, packet, (status, response) =>
        {
            isMatching = true;
        });
    }

    public void OnClickStopMatchMaking()
    {
        var connection = Msf.Client.Connection;
        connection.SendMessage((short)QueueMatchMakerOpCodes.MatchMakingStop, (status, response) =>
        {
            isMatching = false;
        });
    }

    protected void ObjectActivation()
    {
        foreach (var obj in enableObjectsOnMatching)
        {
            if (isMatching)
                obj.SetActive(true);
        }
        foreach (var obj in enableObjectsOnUnmatching)
        {
            if (!isMatching)
                obj.SetActive(true);
        }
        foreach (var obj in disableObjectsOnMatching)
        {
            if (isMatching)
                obj.SetActive(false);
        }
        foreach (var obj in disableObjectsOnUnmatching)
        {
            if (!isMatching)
                obj.SetActive(false);
        }
    }

    IEnumerator CountMatchingTimeRoutine()
    {
        var countingTime = 0;
        bool isMatchingDirty = false;
        ObjectActivation();
        while (true)
        {
            if (isMatching)
            {
                ++countingTime;
                if (textCountMatchingTime != null)
                    textCountMatchingTime.text = countingTime.ToString("N0");

                yield return new WaitForSeconds(1f);
                if (isMatchingDirty != isMatching)
                {
                    ObjectActivation();
                    isMatchingDirty = isMatching;
                }
            }
            else
            {
                countingTime = 0;
                if (textCountMatchingTime != null)
                    textCountMatchingTime.text = "";
                
                if (isMatchingDirty != isMatching)
                {
                    ObjectActivation();
                    isMatchingDirty = isMatching;
                }
            }
        }
    }
}
