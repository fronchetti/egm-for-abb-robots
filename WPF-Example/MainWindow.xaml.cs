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

/* EGM */
using Abb.Egm;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using System.ComponentModel;

namespace WPFProject
{
    public partial class MainWindow : Window
    {
        /* Port where network communication will happen (based on RobotStudio) */
        public static int port = 6510;
        private UdpClient server = null;
        private IPEndPoint robotAddress;
        private uint sequenceNumber = 0;

        /* Robot position values */
        double x, y, z, rx, ry, rz;
        double degreeIncrement = 10;
        string egmState = "Unknown";

        public MainWindow()
        {
            InitializeComponent();
            CreateConnection();


            /* I create this thread to get messages from robot and display them on screen */
            BackgroundWorker worker = new BackgroundWorker();
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
                /* Get message from robot */
                var bytes = server.Receive(ref robotAddress);

                if (bytes != null)
                {
                    /* De-serialize message using EGM */
                    EgmRobot message = EgmRobot.Parser.ParseFrom(bytes);

                    /* Display message on Console */
                    ParseCurrentPositionFromMessage(message);

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
            }
        }

        private void ParseCurrentPositionFromMessage(EgmRobot message)
        {
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
                Console.WriteLine("No header in robot message.");
            }
        }

        private void SendPoseMessageToRobot(double x, double y, double z, double rx, double ry, double rz)
        {
            /* Send message to robot over network */
            using (MemoryStream memoryStream = new MemoryStream())
            {
                /* Prepare a new message using EGM */
                EgmSensor message = new EgmSensor();
                CreatePoseMessage(message, x, y, z, rx, ry, rz);

                message.WriteTo(memoryStream);

                int bytesSent = server.Send(memoryStream.ToArray(), (int) memoryStream.Length, robotAddress);

                if (bytesSent < 0)
                {
                    Console.WriteLine("No message was sent to robot.");
                }
            }
        }

        private void CreatePoseMessage(EgmSensor message, double x, double y, double z, double rx, double ry, double rz)
        {
            EgmHeader hdr = new EgmHeader();
            hdr.Seqno = sequenceNumber++;
            hdr.Tm = (uint) DateTime.Now.Ticks;
            hdr.Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection;

            message.Header = hdr;
            EgmPlanned planned_trajectory = new EgmPlanned();
            EgmPose cartesian_pos = new EgmPose();
            EgmCartesian tcp_p = new EgmCartesian();
            EgmEuler ea_p = new EgmEuler();

            /* Translation */
            tcp_p.X = x;
            tcp_p.Y = y;
            tcp_p.Z = z;

            /* Rotation */
            ea_p.X = rx;
            ea_p.Y = ry;
            ea_p.Z = rz;

            cartesian_pos.Pos = tcp_p;
            cartesian_pos.Euler = ea_p;

            planned_trajectory.Cartesian = cartesian_pos;
            message.Planned = planned_trajectory;
        }

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
