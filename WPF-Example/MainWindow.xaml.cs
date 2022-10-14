using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/* Author: Felipe Fronchetti
 * Email: fronchettl@vcu.edu
 * Disclaimer: This code is heavily inspired on egm-sensor.cs by ABB.
 * Please always refer to the EGM manual provided by ABB for more detailed information. */

/* EGM */
using Abb.Egm;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using System.ComponentModel;

namespace WPFProject
{
    /* This application has two main functionalities:
     * a) When a directional button on the user interface is pressed,
     *  the application submits a UDP message in EGM format telling the robot
     *  to which direction to move. 
     * b) When the robot moves (either commanded by this application or by external forces),
     *  this application listens to the EGM message sent by the robot over the network, de-serializes it
     *  and saves its current position in variables. The current position is displayed on
     *  the user interface. */

    public partial class MainWindow : Window
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

        /* Robot cartesian position and rotation values */
        double x, y, z, rx, ry, rz;
        /* Number of degrees moved in robot when a button is clicked */
        double degreeIncrement = 10;
        /* Current state of EGM communication (disconnected, connected or running) */
        string egmState = "Undefined";

        public MainWindow()
        {
            /* This method is executed once the application runs */
            /* Opens the main window */
            InitializeComponent();
    
            /* Initializes EGM connection with robot */
            CreateConnection();

            /* This worker creates a thread that listens to every message
             * sent by the robot over the network. If you don't use a secondary thread,
             * the WPF interface will freeze. It must run in asynchronous mode. */
            BackgroundWorker worker = new BackgroundWorker();
            /* CollectRobotMessages is the method that does the listening */
            worker.DoWork += new DoWorkEventHandler(CollectRobotMessages); 
            worker.RunWorkerAsync();
        }

        private void CreateConnection()
        {
            server = new UdpClient(port);
            robotAddress = new IPEndPoint(IPAddress.Any, port);
        }

        private void CollectRobotMessages(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (!worker.CancellationPending)
            {
                /* Receives the messages sent by the robot in as a byte array */
                var bytes = server.Receive(ref robotAddress);

                if (bytes != null)
                {
                    /* De-serializes the byte array using the EGM protocol */
                    EgmRobot message = EgmRobot.Parser.ParseFrom(bytes);

                    ParseCurrentPositionFromMessage(message);
                    DisplayMessageOnInterface();
                }
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

        private void DisplayMessageOnInterface()
        {
            TranslationValues.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                TranslationValues.Text = string.Format("X = {0}, Y = {1}, Z = {2}", Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z));
            }
));

            RotationValues.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                RotationValues.Text = string.Format("X = {0}, Y = {1}, Z = {2}", Convert.ToInt32(rx), Convert.ToInt32(ry), Convert.ToInt32(rz));
            }
            ));

            EGMState.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                EGMState.Text = string.Format("EGM State: {0}", egmState);
            }
            ));
        }

        private void SendPoseMessageToRobot(double x, double y, double z, double rx, double ry, double rz)
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

                /* Send the message as a byte array over the network to the robot */
                int bytesSent = server.Send(memoryStream.ToArray(), (int) memoryStream.Length, robotAddress);

                if (bytesSent < 0)
                {
                    Console.WriteLine("No message was sent to robot.");
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

        /* The remaining methods act as listeners to clicks on the directional buttons. 
           When a button is presssed, a message containing a new position is sent to the robot. */

        private void Translation_LeftButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y + degreeIncrement, z, rx, ry, rz);
        }

        private void Translation_RightButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y - degreeIncrement, z, rx, ry, rz);
        }

        private void Translation_UpButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z + degreeIncrement, rx, ry, rz);
        }

        private void Translation_DownButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z - degreeIncrement, rx, ry, rz);
        }

        private void Translation_ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x + degreeIncrement, y, z, rx, ry, rz);
        }

        private void Translation_BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x - degreeIncrement, y, z, rx, ry, rz);
        }

        private void Rotation_LeftButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y + degreeIncrement, z, rx, ry, rz);
        }

        private void Rotation_RightButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y - degreeIncrement, z, rx, ry, rz);
        }

        private void Rotation_UpButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z, rx, ry, rz + degreeIncrement);
        }

        private void Rotation_DownButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z, rx, ry, rz - degreeIncrement);
        }

        private void Rotation_ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z, rx + degreeIncrement, ry, rz);
        }

        private void Rotation_BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            SendPoseMessageToRobot(x, y, z, rx - degreeIncrement, ry, rz);
        }
    }
}
