using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Mono.Cecil.Cil;
using System;

public class NetworkRunnerHandler : MonoBehaviour
{
    private NetworkRunner runner;
    public NetworkSceneManagerDefault sceneManager;

    private void Awake()
    {
        runner = GetComponent<NetworkRunner>();
        DontDestroyOnLoad(gameObject); // So it survives scene transitions
    }
}
