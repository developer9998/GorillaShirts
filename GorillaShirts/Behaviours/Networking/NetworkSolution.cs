using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking;

internal abstract class NetworkSolution : MonoBehaviourPunCallbacks
{
    public static NetworkSolution Instance { get; set; }

    public abstract bool TransferOnlyInRooms { get; }

    public Action<NetPlayer, Hashtable> OnPlayerPropertiesChanged;

    private readonly Hashtable _properties = [];
    private bool _isPropertiesReady;
    private float _propertySetTimer;

    private Player[] playerArray;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        _propertySetTimer = Mathf.Max(_propertySetTimer - Time.unscaledDeltaTime, 0f);

        if (_isPropertiesReady && _propertySetTimer <= 0)
        {
            _isPropertiesReady = false;
            _propertySetTimer = Constants.NetworkRaiseInterval;

            try
            {
                SendProperties(_properties, playerArray);
            }
            catch (Exception ex)
            {
                Logging.Fatal("NetworkSolution failed to send player properties");
                Logging.Error(ex);
            }
        }
    }

    public void SetProperty(string key, object value)
    {
        if (_properties.ContainsKey(key)) _properties[key] = value;
        else _properties.Add(key, value);

        _isPropertiesReady = !TransferOnlyInRooms || PhotonNetwork.InRoom || _isPropertiesReady;
    }

    public void NotifyPropertiesRecieved(Player player, Hashtable properties)
    {
        Logging.Message($"{player}: {string.Join(", ", properties)}");
        OnPlayerPropertiesChanged?.Invoke(player, properties);
    }

    public sealed override async void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playerArray = PhotonNetwork.PlayerListOthers;

        if (TransferOnlyInRooms)
        {
            await Task.Delay(PhotonNetwork.GetPing());
            _isPropertiesReady = true;
        }
    }

    public sealed override void OnLeftRoom()
    {
        base.OnLeftRoom();
        playerArray = null;
    }

    public sealed override async void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        playerArray = PhotonNetwork.PlayerListOthers;

        if (TransferOnlyInRooms)
        {
            while (VRRigCache.rigsInUse.All(player => player.Key.ActorNumber != newPlayer.ActorNumber)) await Task.Delay(PhotonNetwork.GetPing());

            try
            {
                SendProperties(_properties, [newPlayer]);
            }
            catch (Exception ex)
            {
                Logging.Fatal("NetworkSolution failed to send player properties");
                Logging.Error(ex);
            }
        }
    }

    public sealed override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        playerArray = PhotonNetwork.PlayerListOthers;
    }

    public abstract void SendProperties(Hashtable properties, Player[] targetPlayers);

    public abstract bool IsCompatiblePlayer(Player player);
}
