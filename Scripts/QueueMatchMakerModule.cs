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
    
    public event Action<int> GamesUnderwayChanged;
    public event Action<int> GamesPlayedChanged;
    public event Action<int> GamesAbortedChanged;

    protected readonly Dictionary<string, Dictionary<string, QueueMatchMakerPlayer>> waitingPlayers = new Dictionary<string, Dictionary<string, QueueMatchMakerPlayer>>();
    protected readonly Dictionary<int, QueueMatchMakerMatch> matches = new Dictionary<int, QueueMatchMakerMatch>();

    private SpawnersModule spawnersModule;

    private int gamesPlayed;
    /// <summary>
    /// Number of games that completed successfully
    /// </summary>
    public int GamesPlayed
    {
        get
        {
            return gamesPlayed;
        }
        set
        {
            gamesPlayed = value;
            if (GamesPlayedChanged != null)
                GamesPlayedChanged.Invoke(gamesPlayed);
        }
    }

    private int gamesUnderway;
    /// <summary>
    /// Number of games that currently playing
    /// </summary>
    public int GamesUnderway
    {
        get
        {
            return gamesUnderway;
        }
        set
        {
            gamesUnderway = value;
            if (GamesUnderwayChanged != null)
                GamesUnderwayChanged.Invoke(gamesUnderway);
        }
    }

    private int gamesAborted;
    /// <summary>
    /// Number of games that aborted and never start
    /// </summary>
    public int GamesAborted
    {
        get
        {
            return gamesAborted;
        }
        set
        {
            gamesAborted = value;
            if (GamesAbortedChanged != null)
                GamesAbortedChanged.Invoke(gamesAborted);
        }
    }

    public override void Initialize(IServer server)
    {
        base.Initialize(server);

        logger.LogLevel = logLevel;
        
        GamesPlayed = 0;
        GamesAborted = 0;
        GamesUnderway = 0;

        spawnersModule = server.GetModule<SpawnersModule>();

        // Register functions that will be called when server receive messages with defined op-codes
        server.SetHandler((short)QueueMatchMakerOpCodes.matchMakingStart, OnMatchMakingStart);
        server.SetHandler((short)QueueMatchMakerOpCodes.matchMakingStop, OnMatchMakingStop);
        server.SetHandler((short)QueueMatchMakerOpCodes.matchDetails, OnReceiveMatchDetailsFromGameServer);
        server.SetHandler((short)QueueMatchMakerOpCodes.matchComplete, OnGameServerComplete);

        // Start coroutine to manage match maker
        StartCoroutine(MatchmakerCoroutine());
    }

    private void OnGameServerComplete(IIncommingMessage message)
    {
        QueueMatchMakerCompletePacket matchCompletion = message.Deserialize(new QueueMatchMakerCompletePacket());
        logger.Info(string.Format("OnGameServerComplete {0} {1}", matchCompletion.SpawnId, matchCompletion.Success));

        if (matches.ContainsKey(matchCompletion.SpawnId))
        {
            QueueMatchMakerMatch match = matches[matchCompletion.SpawnId];
            matches.Remove(matchCompletion.SpawnId);
            if (matchCompletion.Success)
                GamesPlayed = GamesPlayed + 1;
            else
                GamesAborted = GamesAborted + 1;

            GamesUnderway = GamesUnderway - 1;
        }
    }

    private void OnReceiveMatchDetailsFromGameServer(IIncommingMessage message)
    {
        QueueMatchMakerDetailsPacket details = message.Deserialize(new QueueMatchMakerDetailsPacket());
        logger.Info(string.Format("OnReceiveMatchDetailsFromGameServer  SpawnId: {0}  MachineId: {1}  SpawnCode: {2}  AssignedPort: {3}",
            details.SpawnId,
            details.MachineId,
            details.SpawnCode,
            details.AssignedPort));

        if (matches.ContainsKey(details.SpawnId))
        {
            QueueMatchMakerMatch match = matches[details.SpawnId];
            match.MachineId = details.MachineId;
            match.SpawnCode = details.SpawnCode;
            match.AssignedPort = details.AssignedPort;

            GamesUnderway = GamesUnderway + 1;

            NotifyClientsAndStartMatch(match);
        }
        else
        {
            logger.Error("OnReceiveMatchDetailsFromGameServer: could not find match with spawnId = " + details.SpawnId);
        }
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

            bool started = TryToStartAMatch();
            frequency = started ? .1f : pollFrequency;
        }
    }

    private bool TryToStartAMatch()
    {
        var usersInMatch = new List<QueueMatchMakerPlayer>();
        
        lock (waitingPlayers)
        {
            var gameModeNames = new List<string>(waitingPlayers.Keys);
            foreach (var gameModeName in gameModeNames)
            {
                var gameMode = QueueMatchMakerGameModes.Singleton.GameModes[gameModeName];
                var playersPerMatch = gameMode.playersPerMatch;

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
                        break;
                }
            }
        }
        StartMatch(usersInMatch);
        return true;
    }

    public bool AddMatchmakerPlayer(QueueMatchMakerPlayer player)
    {
        lock (waitingPlayers)
        {
            var gameModeNames = new List<string>(QueueMatchMakerGameModes.Singleton.GameModes.Keys);
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

    private bool StartMatch(List<QueueMatchMakerPlayer> usersInMatch)
    {
        SpawnTask spawnTask = SpawnGameServer(usersInMatch);

        if (spawnTask == null)
        {
            logger.Info("No gameservers available");
            // put the users back in the queue
            foreach (QueueMatchMakerPlayer player in usersInMatch)
            {
                AddMatchmakerPlayer(player);
            }
            return false;
        }
        else
        {
            spawnTask.WhenDone(t =>
            {
                QueueMatchMakerMatch match = new QueueMatchMakerMatch(spawnTask.SpawnId, usersInMatch);
                matches.Add(match.SpawnId, match);
            });
            spawnTask.StatusChanged += (SpawnStatus status) =>
            {
            };
        }

        if (GamesUnderwayChanged != null)
            GamesUnderwayChanged.Invoke(matches.Count);

        return true;
    }

    private SpawnTask SpawnGameServer(List<QueueMatchMakerPlayer> usersInMatch)
    {
        // TODO: Settings
        var settings = new Dictionary<string, string>();

        SpawnTask task = spawnersModule.Spawn(settings);

        return task;
    }

    private void ReturnPlayersToMatchmaker(List<QueueMatchMakerPlayer> usersInMatch)
    {
        foreach (QueueMatchMakerPlayer player in usersInMatch)
        {
            AddMatchmakerPlayer(player);
        }
    }

    private void NotifyClientsAndStartMatch(QueueMatchMakerMatch match)
    {
        var msg = Msf.Create.Message((short)QueueMatchMakerOpCodes.matchDetails,
            new QueueMatchMakerDetailsPacket()
            {
                SpawnId = match.SpawnId,
                MachineId = match.MachineId,
                SpawnCode = match.SpawnCode,
                AssignedPort = match.AssignedPort,
            });

        foreach (var player in match.Players)
        {
            player.Peer.SendMessage(msg);
        }
    }
}
