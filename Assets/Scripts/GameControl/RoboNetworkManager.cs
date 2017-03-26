#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

namespace UnityEngine.Networking
{

    [AddComponentMenu("Network/RoboNetworkManager")]
    public class RoboNetworkManager : NetworkManager {

        public new static string networkSceneName = "";
        public static AsyncOperation s_LoadingSceneAsync;
        protected static int s_StartPositionIndex;
        public static List<Transform> s_StartPositions = new List<Transform>();

        public new List<Transform> startPositions { get { return s_StartPositions; }}

        public void ServerChangeScene(string newSceneName, bool wait = false) {
            if (string.IsNullOrEmpty(newSceneName))
            {
                if (LogFilter.logError) { Debug.LogError("ServerChangeScene empty scene name"); }
                return;
            }

            if (LogFilter.logDebug) { Debug.Log("ServerChangeScene " + newSceneName); }
            NetworkServer.SetAllClientsNotReady();
            networkSceneName = newSceneName;

            cutToLoadScene();

            s_LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
            s_LoadingSceneAsync.allowSceneActivation = !wait;

            StringMessage msg = new StringMessage(networkSceneName);
            NetworkServer.SendToAll(MsgType.Scene, msg);

            s_StartPositionIndex = 0;
            s_StartPositions.Clear();
        }

        private void cutToLoadScene() {
            string scenePath = SceneCatalog.sceneCatalog.getScenePath(0);
            SceneData ? sceneData = SceneCatalog.sceneCatalog.getSceneData(scenePath);
            if (sceneData != null && sceneData.Value.isLoadingScene() && !SceneManager.GetActiveScene().path.Equals(sceneData.Value.path)) {
                SceneManager.LoadScene(0);
            }
        }

        public override void OnServerSceneChanged(string sceneName) {
            bool addPlayer = (ClientScene.localPlayers.Count == 0);
            bool foundPlayer = false;
            for (int i = 0; i < ClientScene.localPlayers.Count; i++)
            {
                if (ClientScene.localPlayers[i].gameObject != null)
                {
                    foundPlayer = true;
                    break;
                }
            }
            if (!foundPlayer)
            {
                // there are players, but their game objects have all been deleted
                addPlayer = true;
            }
            if (addPlayer)
            {
                ClientScene.AddPlayer(0);
            }
        }

    }

}
#endif //ENABLE_UNET
