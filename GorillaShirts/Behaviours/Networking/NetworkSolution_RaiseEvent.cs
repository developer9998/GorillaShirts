using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace GorillaShirts.Behaviours.Networking;

internal class NetworkSolution_RaiseEvent : NetworkSolution
{
    public override bool TransferOnlyInRooms => true;

    private readonly byte eventCode = 176;

    private readonly int id = StaticHash.Compute("GorillaShirts".GetStaticHash());

    public void Awake()
    {
        Instance = this;

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new() { { Constants.NetworkPropertyKey, Constants.Version } });
    }

    public override bool IsCompatiblePlayer(Player player) => true;// player != null && player.CustomProperties.ContainsKey(Constants.NetworkPropertyKey);

    public override void SendProperties(Hashtable properties, Player[] targetPlayers)
    {
        object[] content = [id, properties];

        RaiseEventOptions raiseEventOptions = new()
        {
            TargetActors = [.. from player in targetPlayers select player.ActorNumber]
        };

        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEvent(EventData data)
    {
        if (data.Code != eventCode) return;

        object[] eventData = (object[])data.CustomData;

        if (eventData.Length < 2 || eventData[0] is not int)
        {
            Logging.Error("Invalid parameters");
            return;
        }

        int eventId = (int)eventData[0];
        if (eventId != id) return;

        Player player = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
        NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(data.Sender);
        if (player.IsLocal || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer)) return;

        if (eventData[1] is Hashtable properties)
        {
            if (!networkedPlayer.IsShirtUser)
            {
                Logging.Message($"Player has GorillaShirts: {netPlayer.NickName}");
                networkedPlayer.IsShirtUser = true;
            }

            Logging.Message($"{netPlayer.NickName}: {string.Join(", ", properties)}");

            networkedPlayer.OnPlayerPropertyChanged(properties);
            NotifyPropertiesRecieved(player, properties);
            return;
        }
    }
}
