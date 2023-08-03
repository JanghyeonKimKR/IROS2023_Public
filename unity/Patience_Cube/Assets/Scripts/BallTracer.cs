using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using System.Diagnostics;
using System;

using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;


public class BallTracer : MonoBehaviour
{
    //WebCamTexture _webCamTexture;

    TcpClient client;
    string serverIP = "127.0.0.1";
    int port = 8000;

    byte[] receivedBuffer;
    StreamReader reader;
    bool socketReady = false;
    NetworkStream stream;

    public GameObject ball;

    void Start()
    {
        //WebCamDevice[] devices = WebCamTexture.devices;
        //_webCamTexture = new WebCamTexture(devices[0].name);
        //_webCamTexture.Play();

        CheckReceive();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                receivedBuffer = new byte[100];
                stream.Read(receivedBuffer, 0, receivedBuffer.Length); // stream에 있던 바이트배열 내려서 새로 선언한 바이트배열에 넣기
                string msg = Encoding.UTF8.GetString(receivedBuffer, 0, receivedBuffer.Length); // byte[] to string
                //Debug.Log(msg); 
                string[] token = msg.Split(' ');
                //string result = "[" + token[0] + "] (" + token[3] + "," + token[5] + ")";
                //Debug.Log(result);

                ball.transform.position = new Vector3(float.Parse(token[3]) / 10, float.Parse(token[5]) / 10, 0);
            }
        }
    }

    void CheckReceive()
    {
        if (socketReady) return;
        try
        {
            client = new TcpClient(serverIP, port);
            if (client.Connected)
            {
                stream = client.GetStream();
                Debug.Log("Connect Success");
                socketReady = true;
            }
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    void CloseSocket()
    {
        if (!socketReady) return;

        reader.Close();
        client.Close();
        socketReady = false;
    }


}
