using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Barebones.Logging;
using Barebones.MasterServer;
using Barebones.Networking;

public class QueueMatchMakerModule : ServerModuleBehaviour
{
    private readonly BmLogger logger = Msf.Create.Logger(typeof(QueueMatchMakerModule).Name);

    public LogLevel logLevel = LogLevel.Info;
    public float pollFrequency = 2f;

    protected readonly Dictionary<string, Dictionary<string, QueueMatchMakerPlayer>> waitingPlayers = new Dictionary<string, Dictionary<string, QueueMatchMakerPlayer>>();
    protected readonly Dictionary<int, QueueMatchMakerMatch> matches = new Dictionary<int, QueueMatchMakerMatch>();
    
    private QueueMatchMakerLobbiesModule lobbiesModule;

    public override void Initialize(IServer server)
    {
        base.Initialize(server);

        logger.LogLevel = logLevel;

        lobbiesModule = server.GetModule<QueueMatchMakerLobbiesModule>();

        // Register functions that will be called when server receive messages with defined op-codes
        server.SetHandler((short)QueueMatchMakerOpCodes.MatchMakingStart, OnMatchMakingStart);
        server.SetHandler((short)QueueMatchMakerOpCodes.MatchMakingStop, OnMatchMakingStop);

        // Start coroutine to manage match maker
        StartCoroutine(MatchmakerCoroutine());
    }

    private void OnMatchMakingStart(IIncommingMessage message)
    {
        int peerId = message.Peer.Id;

        var peer = Server.GetPeer(peerId);
        if (peer == null)
        {
            message.Respond("Peer with a given ID is not in the game", ResponseStatus.Error);
            return;
        }

        var account = peer.GetExtension<IUserExtension>();
        if (account == null)
        {
            message.Respond("Peer has not been authenticated", ResponseStatus.Failed);
            return;
        }

        QueueMatchMakerStartPacket request = message.Deserialize(new QueueMatchMakerStartPacket());
        AddMatchmakerPlayer(new QueueMatchMakerPlayer(message.Peer, account.Username, request.gameModeName, Time.time, request.properties));

        // Response to clients that match making started
        message.Respond(ResponseStatus.Success);
    }

    private void OnMatchMakingStop(IIncommingMessage message)
    {
        int peerId = message.Peer.Id;

        var peer = Server.GetPeer(peerId);
        if (peer == null)
        {
            message.Respond("Peer with a given ID is not in the game", ResponseStatus.Error);
            return;
        }

        var account = peer.GetExtension<IUserExtension>();
        if (account == null)
        {
            message.Respond("Peer has not been authenticated", ResponseStatus.Failed);
            return;
        }
        
        RemoveMatchmakerPlayer(account.Username);

        // Response to clients that match making stopped
        message.Respond(ResponseStatus.Success);
    }

    protected IEnumerator MatchmakerCoroutine()
    {
        float frequency = pollFrequency;

        while (true)
        {
            yield return new WaitForSeconds(frequency);

            bool started = TryToCreateLobby();
            frequency = started ? .1f : pollFrequency;
        }
    }

    private bool TryToCreateLobby()
    {
        var usersInMatch = new List<QueueMatchMakerPlayer>();
        
        lock (waitingPlayers)
        {
            var gameModeNames = new List<string>(waitingPlayers.Keys);
            foreach (var gameModeName in gameModeNames)
            {
                var gameMode = lobbiesModule.GameModes[gameModeName];
                var playersPerMatch = gameMode.PlayersPerMatch;

                var players = waitingPlayers[gameModeName];
                if (players.Count < playersPerMatch)
                    return false;

                var usernames = new List<string>(players.Keys);
                foreach (var username in usernames)
                {
                    var player = players[username];
                    RemoveMatchmakerPlayer(username);
                    usersInMatch.Add(player);

                    if (usersInMatch.Count >= playersPerMatch)
                    {
                        lobbiesModule.CreateGameLobby(gameModeName, usersInMatch);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool AddMatchmakerPlayer(QueueMatchMakerPlayer player)
    {
        lock (waitingPlayers)
        {
            var gameModeNames = new List<string>(lobbiesModule.GameModes.Keys);
            if (gameModeNames.Contains(player.GameModeName) && !waitingPlayers.ContainsKey(player.GameModeName))
                waitingPlayers.Add(player.GameModeName, new Dictionary<string, QueueMatchMakerPlayer>());

            if (waitingPlayers.ContainsKey(player.GameModeName) && !waitingPlayers[player.GameModeName].ContainsKey(player.Username))
            {
                waitingPlayers[player.GameModeName].Add(player.Username, player);
                return true;
            }
        }
        return false;
    }

    public bool RemoveMatchmakerPlayer(string username)
    {
        lock (waitingPlayers)
        {
            var gameModeNames = new List<string>(waitingPlayers.Keys);
            foreach (var gameModeName in gameModeNames)
            {
                if (waitingPlayers[gameModeName].ContainsKey(username))
                {
                    waitingPlayers[gameModeName].Remove(username);
                    return true;
                }
            }
        }
        return false;
    }
}
