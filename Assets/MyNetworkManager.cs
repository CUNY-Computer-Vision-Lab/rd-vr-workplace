﻿using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : MonoBehaviour
{
    public bool isAtStartup = true;
    public NetworkIdentity prefab;
    NetworkClient myClient;
    public Material lineMaterial;

    public class CustomMessage
    {
        public static short hiMessage = MsgType.Highest + 1;
    };
    public class AssetMessage
    {
        public static short assetMessage = MsgType.Highest + 2;
    }

    public class AssetsMessage : MessageBase
    {
        public NetworkHash128 assetId;
    }

    public class PointsMessage : MessageBase
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;
    }
    public void sendAssetId(NetworkHash128 assetId)
    {
        AssetsMessage msg = new AssetsMessage();
        msg.assetId = assetId;
        NetworkServer.SendToAll(AssetMessage.assetMessage, msg);
    }
    public void sendMessage(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
        PointsMessage message = new PointsMessage();
        message.vertices = vertices;
        message.uvs = uvs;
        message.triangles = triangles;
        NetworkServer.SendToAll(CustomMessage.hiMessage, message);
    }

    public void onServerReceiveMessage(NetworkMessage msg)
    {

    }
    public void onClientReceiveMessage(NetworkMessage msg)
    {
        PointsMessage message = msg.ReadMessage<PointsMessage>();

        // line object
        GameObject line = new GameObject();
        Mesh mesh = new Mesh();
        line.AddComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = message.vertices;
        mesh.uv = message.uvs;
        mesh.triangles = message.triangles;
        line.AddComponent<MeshRenderer>().material = lineMaterial;

        //Vector3[] points = message.points;
        //Debug.Log(points.Length + " points received");
        //foreach (Vector3 point in points)
        //{
        //    Debug.Log(point);
        //}
    }
    private void Start()
    {
        // setup code for VR
        SetupServer();
    }
    void Update()
    {
        if (isAtStartup)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("Server");
                SetupServer();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Client");
                SetupClient();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Both");
                SetupServer();
                SetupLocalClient();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (myClient == null)
                {
                    Debug.Log("Sending message from server to client");
                }
                else
                {
                    Debug.Log("Sending message from client to server");
                    PointsMessage message = new PointsMessage();
                    myClient.Send(CustomMessage.hiMessage, message);
                }
            }
        }
    }
    void OnGUI()
    {
        if (isAtStartup)
        {
            GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
            GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");
            GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
        }
    }

    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Listen(4444);
        isAtStartup = false;
        NetworkServer.RegisterHandler(MsgType.Connect, onServerReceiveConnect);
        NetworkServer.RegisterHandler(MsgType.Ready, OnClientReady);
        NetworkServer.RegisterHandler(CustomMessage.hiMessage, onServerReceiveMessage);
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        ClientScene.RegisterPrefab(prefab.gameObject);
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(CustomMessage.hiMessage, onClientReceiveMessage);
        myClient.Connect("127.0.0.1", 4444);
        isAtStartup = false;
    }

    // Create a local client and connect to the local server
    public void SetupLocalClient()
    {
        ClientScene.RegisterPrefab(prefab.gameObject);
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        isAtStartup = false;
    }
    public void OnClientReady(NetworkMessage netMsg) {
        Debug.Log("Client ready");
        NetworkServer.SetClientReady(netMsg.conn);
        GameObject sphere = Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(sphere);
    }
    public void onServerReceiveConnect(NetworkMessage netMsg)
    {
        Debug.Log("Received a client connection");
        sendAssetId(prefab.assetId);
    }
    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        //ClientScene.Ready(netMsg.conn);
        Debug.Log("Connected to server");
    }
}