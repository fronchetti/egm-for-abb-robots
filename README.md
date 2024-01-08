# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots in C#. 

_Last update: 10/20/2022_

Disclaimer: This repository is not supported, sponsored or approved by ABB. Always refer to the official EGM application manual for consistent information.

### :warning: Warning 
The organization and authors of this repository are not liable for any consequential damage or injury that any code or information available in this repository may produce to you or others. The code available in this repository should be used only for reading purposes as different robots and settings may act differently during  program execution. Use the code and information available here at your own risk, and always make sure you are following all the safety procedures recommended by your robot manufacturer. Robots can be dangerous if used inappropriately, be careful!

## What is EGM?
![Diagram describing the application of EGM](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGM-Overview.png)

EGM is a feature available in RobotWare 6 and 7 that provides external devices the ability to interact and manipulate ABB robots through a network. In this approach, the information is encoded using Google Protocol Buffers and transported through UDP sockets. EGM is a great option for those who need to manipulate an ABB robot from an external device.

For a more detailed explanation, visit the [Externally Guided Motion - Application Manual](https://library.e.abb.com/public/f05090fae99a4d0ba2ee332e50865791/3HAC073318%20AM%20Externally%20Guided%20Motion%20RW7-en.pdf) provided by ABB.

## Why should I use EGM?
In our laboratory, we implement applications in mixed reality that are used to manipulate ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit. One of the current limitations of Unity is that it does not accept external development kits, such as the ABB PC SDK. One may argue that other options (e.g., ABB Robot Web Services) could be used to solve this problem, but most of the available alternatives do not allow developers to directly manipulate ABB robots from external devices. For this reason, in our laboratory, we tend to implement communication between devices and ABB robots using EGM. EGM is simple, efficient, and works perfectly with Unity (or any other environment that allows UDP communication in C#).

For those looking for options that are not implemented in C#, please refer to libraries such as [abb_libegm](https://www.rosin-project.eu/tool/abb-libegm#:~:text=abb_libegm%20is%20a%20C%2B%2B%20communication,well%20as%20providing%20user%20APIs.) (in C++) and [abbegm](https://docs.rs/abbegm/latest/abbegm/) (in Rust).

## What is available in this repository?
- [Tutorial on how to prepare your ABB robot controller to run EGM](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGM-Preparing-your-robot.pdf)
- [WPF Application](https://github.com/vcuse/egm-for-abb-robots/tree/main/WPF-Example): Example of a desktop application implemented on Windows Presentation Foundation (WPF) to manipulate an ABB robot to a specific location and orientation using _EGMActPose_ and _EGMRunPose_. In this example, the [UdpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient) class is used for network communication.
- [UWP Application (works with Hololens)](https://github.com/fronchetti/egm-for-abb-robots/tree/main/UWP-Example): Example of a desktop application implemented on Universal Windows Platform (UWP) to manipulate an ABB robot to a specific location and orientation using _EGMActPose_ and _EGMRunPose_. In this example, the [DatagramSocket](https://learn.microsoft.com/en-us/uwp/api/windows.networking.sockets.datagramsocket/) class is used for network communication. This is a great example for those aiming to implement EGM for Microsoft Hololens. At the present moment, Hololens is based on UWP, and UWP doesn't support the UdpClient class. [Developers should use DatagramSocket instead](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/udp-packets-in-unity).
- [Unity Application](https://github.com/vcuse/egm-for-abb-robots/tree/main/Unity-Example): Example of an Unity application implemented in C# to move an ABB robot to a specific joint configuration using _EGMActJoint_ and _EGMRunJoint_. In this example, the [UdpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient) class is used for network communication.
- Examples of RAPID code to turn your controller into an EGM client: 
    - [Robot movement based on _EGMActPose_ and _EGMRunPose_](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMPoseCommunication.modx) (Position)
    - [Robot movement based on _EGMActJoint_ and _EGMRunJoint_](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMJointCommunication.modx) (Joint Values)

## Common questions
- **Is EGM available in all ABB robots?** As far as I know, some robots using RobotWare 6 do not support EGM by default. We have a YuMi in our lab, and we had to contact ABB to get access to this feature in our IRC5 controller. My recommendation is to always contact ABB support when you have questions about your robot, especially if they are not available in the documentation.

- **Does it work with virtual controllers?**
The answer is yes, but there are limitations. As far as I am aware, it is not possible to run both the virtual controller and your application on the same machine. The reason is pretty simple, your computer would send and receive messages from both the EGM client and the server at the same time, creating issues in the communication process (e.g., errors from the virtual controller saying that the inbound message is invalid). You also have to make your virtual controller public on the network if you want to control it with an external device. To make it accessible, add a new line containing the external IP address (e.g., `<host ip="192.168.0.36"/>`) in the `C:\Users\your_username\AppData\Roaming\ABB Industrial IT\Robotics IT\VCConf.xml` file on your computer running RobotStudio. Always check if your firewall allows UDP communication in the port that you are using for EGM, and contact ABB support if something goes wrong.

- **Can I get information from the robot using EGM?**
Yes, you can. However, the information provided is way more limited than what you can get using the ABB PC SDK. To get general information from the robot using EGM, inspect the message header sent from the robot to the external device using the Egm.cs library provided by ABB. You can also refer to their manual for a better understanding of what kind of information they provide through EGM.

## Sanity Checklist
To help developers (and myself) with common issues with EGM, here is a sanity checklist:
- [ ] First of all, make sure your ABB robot is compatible (and enabled) with externally guided motion. You can do that by connecting your robot controller to your computer, and checking if EGM is available and enabled in its RobotWare installation (Controller Tab > Installation Manager in RobotStudio). Please be careful, as any wrong changes in your system may result in software issues. 
- [ ] Make sure you registered a UDP Unicast Device for your external device (e.g., HoloLens) in your robot controller system. You can also do that by connecting your robot to your computer and registering the device in the Configuration > Communication tab of RobotStudio. Right-click to register a new device (writing access is necessary). This option will only be available if EGM is enabled on your controller.
- [ ] Make sure your RAPID code refers to the same UDP Unicast Device you have registered in your robot controller. The _EGMSetupUC_ instruction in RAPID, for example, must refer to the same device name you have used in RobotStudio. Please also make sure you are using your external device's IP address in the remote address field, and the correct ports.
- [ ] Make sure both devices (robot and external device) are connected and visible in the same network. Firewall settings may apply. You can do that by connecting your computer to the same network of your EGM solution and pinging the robot and external device. Important note for HoloLens developers: Microsoft disabled pinging on HoloLens, so be aware of this weird decision.
- [ ] Make sure the EGM code is running on the robot and external device. I provide a few examples in this repository, so check them out.
- [ ] Important note for Unity developers: When starting a new project, make sure _InternetClient_ and _InternetClientServer_ capabilities are enabled in your project. You can do that by going to Build Settings > Player Settings > Player and looking for the Capabilities checklist. Changes may apply to new Unity versions, so be aware and always refer to the documentation.

## Need help?
If your question is related to the information contained in this repository, feel free to [open a new issue](https://github.com/vcuse/egm-for-abb-robots/issues).

