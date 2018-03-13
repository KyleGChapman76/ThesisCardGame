using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientRequests : NetworkBehaviour
{
	public NetworkedGameManager localCopyOfNetworkedGameManager;

	[Command]
	public void CmdRequestTurnEnd()
	{
		if (localCopyOfNetworkedGameManager == null)
		{
			Debug.LogError("Can't complete client request since don't have link to local networked game manager.");
            return;
		}
		localCopyOfNetworkedGameManager.ClientRequestEndTurn();
    }
}
