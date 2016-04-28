using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkTracker : MonoBehaviour {
    float _deltaTime = 0;
    double _timeSinceLastMessage = 0;
    double _diffBetweenValues = 0;
    DateTime _lastMessagetime = DateTime.MinValue;

    static int RemotePort = 19784;
    UdpClient _receiver;

    void Start() {
        StartReceivingIP();
    }

    // Init the client and be prepared to receive the first message
    void StartReceivingIP() {
        try {
            _receiver = new UdpClient(RemotePort);
            _receiver.BeginReceive(ReceiveData, null);
        } catch (SocketException e) {
            Debug.LogError(e.Message); // TODO
        }
    }

    // Async callback when a message is received
    void ReceiveData(IAsyncResult result) {
        IPEndPoint receiveIPGroup = new IPEndPoint(IPAddress.Any, RemotePort);
        ProcessData(_receiver.EndReceive(result, ref receiveIPGroup));
        _receiver.BeginReceive(ReceiveData, null); // recall the callback for the next message
    }
    
    void ProcessData(byte[] received) {
        string receivedString = Encoding.ASCII.GetString(received);
        _deltaTime = float.Parse(receivedString) * 1000;
        _timeSinceLastMessage = (DateTime.Now - _lastMessagetime).TotalMilliseconds;
        _diffBetweenValues = (double) _deltaTime - _timeSinceLastMessage;
        _lastMessagetime = DateTime.Now;
    }
    
    void OnDisable() {
        _receiver.Close();
    }

}
