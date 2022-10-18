using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* EGM */
using Abb.Egm;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using System.ComponentModel;
using System;
using TMPro;

public class EgmCommunication : MonoBehaviour
{
    /* UDP port where EGM communication should happen (specified in RobotStudio) */
    public static int port = 6510;
    /* UDP client used to send messages from computer to robot */
    private UdpClient server = null;
    /* Endpoint used to store the network address of the ABB robot.
     * Make sure your robot is available on your local network. The easiest option
     * is to connect your computer to the management port of the robot controller
     * using a network cable. */
    private IPEndPoint robotAddress;
    /* Variable used to count the number of messages sent */
    private uint sequenceNumber = 0;

    /* Robot joints values (in degrees) */
    /* If you are using a robot with 7 degrees of freedom (e.g., YuMi), 
       please adapt this code. */
    private double j1, j2, j3, j4, j5, j6;
    /* Current state of EGM communication (disconnected, connected or running) */
    private string egmState = "Undefined";

    /* This worker creates a secondary thread that listens to every message
     * sent by the robot over the network. */
    private BackgroundWorker worker;

    public Slider j1Slider, j2Slider, j3Slider, j4Slider, j5Slider, j6Slider;
    public TextMeshProUGUI egmStateText;

    /* (Unity) Start is called before the first frame update */
    void Start()
    {
        /* Initializes EGM connection with robot */
        CreateConnection();

        j1Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1Slider.value, j2, j3, j4, j5, j6); });
        j2Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1, j2Slider.value, j3, j4, j5, j6); });
        j3Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1, j2, j3Slider.value, j4, j5, j6); });
        j4Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1, j2, j3, j4Slider.value, j5, j6); });
        j5Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1, j2, j3, j4, j5Slider.value, j6); });
        j6Slider.onValueChanged.AddListener(delegate { SendJointsMessageToRobot(j1, j2, j3, j4, j5, j6Slider.value); });

        UpdateSlidersWithJointValues();
    }

    /* (Unity) Update is called once per frame */
    void Update()
    {
        Debug.Log(string.Format("Joint 1: {0}, Slider 1: {1}", j1, j1Slider.value));
        egmStateText.text = "EGM State: " + egmState;
    }

    /* (Unity) OnApplicationQuit is called when the program is closed */
    void OnApplicationQuit()
    {
        worker.CancelAsync(); /* Destroys secondary thread */
    }

    private void CreateConnection()
    {
        server = new UdpClient(port);
        robotAddress = new IPEndPoint(IPAddress.Any, port);
    }

    private void UpdateSlidersWithJointValues()
    {
        /* Receives the messages sent by the robot in as a byte array */
        var bytes = server.Receive(ref robotAddress);

        if (bytes != null)
        {
            /* De-serializes the byte array using the EGM protocol */
            EgmRobot message = EgmRobot.Parser.ParseFrom(bytes);

            ParseJointValuesFromMessage(message);
        }

        j1Slider.value = (float)j1;
        j2Slider.value = (float)j2;
        j3Slider.value = (float)j3;
        j4Slider.value = (float)j4;
        j5Slider.value = (float)j5;
        j6Slider.value = (float)j6;
    }

    private void ParseJointValuesFromMessage(EgmRobot message)
    {
        /* Parse the current robot position and EGM state from message
            received from robot and update the related variables */

        /* Checks if header is valid */
        if (message.Header.HasSeqno && message.Header.HasTm)
        {
            j1 = message.FeedBack.Joints.Joints[0];
            j2 = message.FeedBack.Joints.Joints[1];
            j3 = message.FeedBack.Joints.Joints[2];
            j4 = message.FeedBack.Joints.Joints[3];
            j5 = message.FeedBack.Joints.Joints[4];
            j6 = message.FeedBack.Joints.Joints[5];
            egmState = message.MciState.State.ToString();
        }
        else
        {
            Debug.Log("The message received from robot is invalid.");
        }
    }

    private void SendJointsMessageToRobot(double j1, double j2, double j3, double j4, double j5, double j6)
    {
        /* Send message containing new positions to robot in EGM format.
         * This is the primary method used to move the robot in cartesian coordinates. */

        /* Warning: If you are planning to manipulate an ABB robot with Hololens, this implementation
         * will not work. Hololens runs under Universal Windows Platform (UWP), which at the present
         * moment does not work with UdpClient class. DatagramSocket should be used instead. */

        using (MemoryStream memoryStream = new MemoryStream())
        {
            EgmSensor message = new EgmSensor();
            /* Prepare a new message in EGM format */
            CreateJointsMessage(message, j1, j2, j3, j4, j5, j6);

            message.WriteTo(memoryStream);

            /* Send the message as a byte array over the network to the robot */
            int bytesSent = server.Send(memoryStream.ToArray(), (int)memoryStream.Length, robotAddress);

            if (bytesSent < 0)
            {
                Debug.Log("No message was sent to robot.");
            }
        }
    }

    private void CreateJointsMessage(EgmSensor message, double j1, double j2, double j3, double j4, double j5, double j6)
    {
        /* Create a message in EGM format specifying a new joint configuration 
           for the ABB robot. The message contains a header with general
           information and a body with the planned joint configuration.

           Notice that in order for this code to work, your robot must be running a EGM client 
           in RAPID containing EGMActJoint and EGMRunJoint methods.

           See one example here: https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMJointCommunication.modx */

        EgmHeader hdr = new EgmHeader();
        hdr.Seqno = sequenceNumber++;
        hdr.Tm = (uint) DateTime.Now.Ticks;
        hdr.Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection;

        message.Header = hdr;
        EgmPlanned plannedTrajectory = new EgmPlanned();
        EgmJoints jointsConfiguration = new EgmJoints();

        jointsConfiguration.Joints.Add(j1);
        jointsConfiguration.Joints.Add(j2);
        jointsConfiguration.Joints.Add(j3);
        jointsConfiguration.Joints.Add(j4);
        jointsConfiguration.Joints.Add(j5);
        jointsConfiguration.Joints.Add(j6);

        plannedTrajectory.Joints = jointsConfiguration;
        message.Planned = plannedTrajectory;
    }
}
