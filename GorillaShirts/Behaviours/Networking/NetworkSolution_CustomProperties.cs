using ExitGames.Client.Photon;
using Photon.Realtime;
using System;

namespace GorillaShirts.Behaviours.Networking;

internal class NetworkSolution_CustomProperties : NetworkSolution
{
    public override bool TransferOnlyInRooms => false;

    public override bool IsCompatiblePlayer(Player player)
    {
        throw new NotImplementedException();
    }

    public override void SendProperties(Hashtable properties, Player[] targetPlayers)
    {
        throw new NotImplementedException();
    }
}
