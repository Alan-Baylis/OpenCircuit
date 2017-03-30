using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkController : MonoBehaviour {

    public static NetworkController networkController;

    private NetworkClient client; //Local client if we're the server

	void Start () {
	    DontDestroyOnLoad(gameObject);
	    networkController = this;
	    NetworkServer.RegisterHandler(MsgType.Scene, clientChangeScene);
	}

    public bool listen() {
        if (NetworkManager.singleton.runInBackground)
            Application.runInBackground = true;

        NetworkCRC.scriptCRCCheck = NetworkManager.singleton.scriptCRCCheck;
        NetworkServer.useWebSockets = NetworkManager.singleton.useWebSockets;


            if (NetworkManager.singleton.serverBindToIP && !string.IsNullOrEmpty(NetworkManager.singleton.serverBindAddress))
            {
                if (!NetworkServer.Listen(NetworkManager.singleton.serverBindAddress, NetworkManager.singleton.networkPort))
                {
                    if (LogFilter.logError) { Debug.LogError("StartServer listen on " + NetworkManager.singleton.serverBindAddress + " failed."); }
                    return false;
                }
            }
            else
            {
                if (!NetworkServer.Listen(NetworkManager.singleton.networkPort))
                {
                    if (LogFilter.logError) { Debug.LogError("StartServer listen failed."); }
                    return false;
                }
            }

        // this must be after Listen(), since that registers the default message handlers
        registerServerMessages();

        if (LogFilter.logDebug) { Debug.Log("NetworkManager StartServer port:" + NetworkManager.singleton.networkPort); }
        //isNetworkActive = true;
        connectLocalClient();
        return true;
    }

    public void connect() {

        if (NetworkManager.singleton.runInBackground)
            Application.runInBackground = true;

        //isNetworkActive = true;

        NetworkClient client = new NetworkClient();

        registerClientMessages(client);

        if (NetworkManager.singleton.secureTunnelEndpoint != null)
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkManager StartClient using provided SecureTunnel"); }
            client.Connect(NetworkManager.singleton.secureTunnelEndpoint);
        }
        else
        {
            if (string.IsNullOrEmpty(NetworkManager.singleton.networkAddress))
            {
                if (LogFilter.logError) { Debug.LogError("Must set the Network Address field in the manager"); }
                return;// null;
            }
            if (LogFilter.logDebug) { Debug.Log("NetworkManager StartClient address:" + NetworkManager.singleton.networkAddress + " port:" + NetworkManager.singleton.networkPort); }

            if (NetworkManager.singleton.useSimulator)
            {
                client.ConnectWithSimulator(NetworkManager.singleton.networkAddress, NetworkManager.singleton.networkPort, NetworkManager.singleton.simulatedLatency, NetworkManager.singleton.packetLossPercentage);
            }
            else
            {
                client.Connect(NetworkManager.singleton.networkAddress, NetworkManager.singleton.networkPort);
            }
        }

        //s_Address = m_NetworkAddress;
        //return client;
    }

    public void serverAddPlayer(GameObject playerPrefab, Vector3 pos, Quaternion rotation, NetworkConnection conn, short playerControllerId = 0) {
        GameObject player = Instantiate(playerPrefab, pos, rotation);
//        print("null? " + (ClientScene.readyConnection == null));
//        print(ClientScene.localPlayers.Count);
        //ClientScene.Ready(conn);

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public void serverChangeScene(string path, SceneLoadListener listener) {
        SceneLoader.sceneLoader.loadScene(path, listener);
        StringMessage msg = new StringMessage(path);
        NetworkServer.SendToAll(MsgType.Scene, msg);
    }

    //This should only ever be called client side
    private void clientChangeScene(NetworkMessage netMsg) {
        SceneLoader.sceneLoader.loadScene(netMsg.reader.ReadString(), null);
    }

    private void serverSpawnPlayer(NetworkMessage netMsg) {
        GlobalConfig.globalConfig.spawnPlayerForConnection(netMsg.conn);
    }

    private void registerServerMessages() {
        NetworkServer.RegisterHandler(MsgType.AddPlayer, serverSpawnPlayer);
    }

    private void registerClientMessages(NetworkClient client) {
        client.RegisterHandler(MsgType.Connect, onClientConnected);
//        client.RegisterHandler(MsgType.Disconnect, OnClientDisconnectInternal);
//        client.RegisterHandler(MsgType.NotReady, OnClientNotReadyMessageInternal);
//        client.RegisterHandler(MsgType.Error, OnClientErrorInternal);
//        client.RegisterHandler(MsgType.Scene, OnClientSceneInternal);

//        if (m_PlayerPrefab != null)
//        {
//            ClientScene.RegisterPrefab(m_PlayerPrefab);
//        }
        for (int i = 0; i < NetworkManager.singleton.spawnPrefabs.Count; i++)
        {
            var prefab = NetworkManager.singleton.spawnPrefabs[i];
            if (prefab != null)
            {
                ClientScene.RegisterPrefab(prefab);
            }
        }
    }

    private void onClientConnected(NetworkMessage netMsg) {
        ClientScene.Ready(netMsg.conn);

    }

    private NetworkClient connectLocalClient() {
        if (LogFilter.logDebug) { Debug.Log("NetworkManager StartHost port:" + NetworkManager.singleton.networkPort); }
        //m_NetworkAddress = "localhost";
        client = ClientScene.ConnectLocalServer();
        registerClientMessages(client);
        return client;
    }
}
