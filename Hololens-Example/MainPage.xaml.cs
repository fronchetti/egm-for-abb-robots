using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

/* EGM */
using Abb.Egm;
using Google.Protobuf;

/* Datagram Sockets */
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UWP_Example
{
    /// <summary>
    /// An example of EGM communicatio using Universal Windows Platform.
    /// UWP is the base of Hololens 2. If you plan to use EGM with Hololens,
    /// you can use this code as reference.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DatagramSocket socket;
        /* UDP port where EGM communication should happen (specified in RobotStudio) */
        private string port = "6510";
        /* Robot IP address over the network. */
        private HostName robotAddress = new HostName("192.168.125.1");
        /* Variable used to count the number of messages sent */
        private uint sequenceNumber = 0;

        /* Robot cartesian position and rotation values */
        private double x, y, z, rx, ry, rz;
        /* Number of degrees moved in robot when a button is clicked */
        double degreeIncrement = 10;
        /* Current state of EGM communication (disconnected, connected or running) */
        private string egmState = "Undefined";

        public MainPage()
        {
            InitializeComponent();
            CreateConnectionAsync();
        }

        private void CreateConnectionAsync()
        {
            socket = new DatagramSocket();
            socket.MessageReceived += RobotSentMessage; /* Listens to messages from robot */
        }

        private void RobotSentMessage(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Console.WriteLine("Message received.");
            DataReader reader = args.GetDataReader();
            byte[] bytes = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(bytes);

            /* If message isn't empty, parse message
               content using EGM */
            if (bytes != null)
            {
                /* De-serializes the byte array using the EGM protocol */
                EgmRobot message = EgmRobot.Parser.ParseFrom(bytes);

                ParseCurrentPositionFromMessage(message);
            }
        }

        private void ParseCurrentPositionFromMessage(EgmRobot message)
        {
            /* Parse the current robot position and EGM state from message
                received from robot and update the related variables */
            if (message.Header.HasSeqno && message.Header.HasTm)
            {
                x = message.FeedBack.Cartesian.Pos.X;
                y = message.FeedBack.Cartesian.Pos.Y;
                z = message.FeedBack.Cartesian.Pos.Z;
                rx = message.FeedBack.Cartesian.Euler.X;
                ry = message.FeedBack.Cartesian.Euler.Y;
                rz = message.FeedBack.Cartesian.Euler.Z;
                egmState = message.MciState.State.ToString();
            }
            else
            {
                Console.WriteLine("The message received from robot is invalid.");
            }
        }

        private async Task SendPoseMessageToRobotAsync(double x, double y, double z, double rx, double ry, double rz)
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
                CreatePoseMessage(message, x, y, z, rx, ry, rz);

                message.WriteTo(memoryStream);

                /* Sends the message asynchronously as a byte array over the network to the robot */
                using (IOutputStream asyncCommunication = await socket.GetOutputStreamAsync(robotAddress, port))
                {
                    using (DataWriter writer = new DataWriter(asyncCommunication))
                    {
                        writer.WriteBytes(memoryStream.ToArray());
                        await writer.StoreAsync();
                    }
                }
            }
        }

        private void CreatePoseMessage(EgmSensor message, double x, double y, double z, double rx, double ry, double rz)
        {
            /* Create a message in EGM format specifying a new location to where
               the ABB robot should move to. The message contains a header with general
               information and a body with the planned trajectory.
            
               Notice that in order for this code to work, your robot must be running a EGM client 
               in RAPID containing EGMActPose and EGMRunPose methods.
            
               See one example here: https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMPoseCommunication.modx */

            EgmHeader hdr = new EgmHeader();
            hdr.Seqno = sequenceNumber++;
            hdr.Tm = (uint) DateTime.Now.Ticks;
            hdr.Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection;

            message.Header = hdr;
            EgmPlanned planned_trajectory = new EgmPlanned();
            EgmPose cartesian_pos = new EgmPose();
            EgmCartesian tcp_p = new EgmCartesian();
            EgmEuler ea_p = new EgmEuler();

            /* Translation values */
            tcp_p.X = x;
            tcp_p.Y = y;
            tcp_p.Z = z;

            /* Rotation values (in Euler angles) */
            ea_p.X = rx;
            ea_p.Y = ry;
            ea_p.Z = rz;

            cartesian_pos.Pos = tcp_p;
            cartesian_pos.Euler = ea_p;

            planned_trajectory.Cartesian = cartesian_pos;
            message.Planned = planned_trajectory;
        }

        private void ConnectRobotButton_Click(object sender, RoutedEventArgs e)
        {
            robotAddress = new HostName(RobotAddressTextbox.Text);
        }

        /* The remaining methods act as listeners to clicks on the buttons. 
            When a button is presssed, a message containing a new position is sent to the robot. */

        private void Translation_LeftButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y + degreeIncrement, z, rx, ry, rz);
        }

        private void Translation_RightButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y - degreeIncrement, z, rx, ry, rz);
        }

        private void Translation_UpButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z + degreeIncrement, rx, ry, rz);
        }

        private void Translation_DownButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z - degreeIncrement, rx, ry, rz);
        }

        private void Translation_ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x + degreeIncrement, y, z, rx, ry, rz);
        }

        private void Translation_BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x - degreeIncrement, y, z, rx, ry, rz);
        }

        private void Rotation_LeftButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y + degreeIncrement, z, rx, ry, rz);
        }

        private void Rotation_RightButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y - degreeIncrement, z, rx, ry, rz);
        }

        private void Rotation_UpButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z, rx, ry, rz + degreeIncrement);
        }

        private void Rotation_DownButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z, rx, ry, rz - degreeIncrement);
        }

        private void Rotation_ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z, rx + degreeIncrement, ry, rz);
        }

        private void Rotation_BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPoseMessageToRobotAsync(x, y, z, rx - degreeIncrement, ry, rz);
        }
    }
}
