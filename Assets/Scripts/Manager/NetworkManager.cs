using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections.Generic;
using System;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour 
{
    private static NetworkManager _instance;
    public static NetworkManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
        }
    }

    ServerSession _session = new ServerSession();

    public void Init()
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        // AWS 전용 하드 코딩
        string ipAddressString = "127.0.0.1";
        // string ipAddressString = "15.164.213.236"; // aws
        IPAddress ipAddr = IPAddress.Parse(ipAddressString);
        //IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint,
            () => { return _session; },
            1);


        //dummyPosition.DestinationPosX = Random.Range(-50, 50);
        //dummyPosition.DestinationPosY = Random.Range(0, 5);
        //dummyPosition.DestinationPosZ = Random.Range(-50, 50);

        //C_Move dummyMovePacket = new C_Move();
        //dummyMovePacket.PosInfo = dummyPosition;
        //_session.Send(dummyMovePacket);

        Screen.SetResolution(1920, 1080, false);
        Application.targetFrameRate = 60; // 60프레임 고정
    }

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }
}
