# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots in C#. 

_Last update: 10/12/2022_

Disclaimer: This repository is not supported, sponsored or approved by ABB. Always refer to the official EGM application manual for updates. 

## What is EGM?
EGM is a feature available in RobotWare 6 and 7 that provides external devices the ability to interact and manipulate ABB robots through a network. In this approach, the information is encoded using Google Protocol Buffers and trasported through UDP sockets. EGM is a great option for those who need to manipulate an ABB robot from an external device.

For a more detailed explanation, visit the [Externally Guided Motion - Application Manual](https://library.e.abb.com/public/f05090fae99a4d0ba2ee332e50865791/3HAC073318%20AM%20Externally%20Guided%20Motion%20RW7-en.pdf) provided by ABB.

## Why should I use EGM?
In our laboratory, we implement applications in mixed reality that are used to manipulate ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit. One of the current limitations of Unity is that it does not accept external development kits, such as the ABB PC SDK. One may argue that other options (e.g., ABB Robot Web Services) could be used to solve this problem, but most of the availabe alternatives do not allow developers to directly manipulate ABB robots from external devices. For this reason, in our laboratory, we tend to implement the communication between devices and ABB robots using EGM. EGM is simple, efficient and works perfectly with Unity (or any other environment that allows UDP communication in C#).

For those looking for options that are not implemented in C#, please refer to libraries such as [abb_libegm](https://www.rosin-project.eu/tool/abb-libegm#:~:text=abb_libegm%20is%20a%20C%2B%2B%20communication,well%20as%20providing%20user%20APIs.) (in C++) and [abbegm](https://docs.rs/abbegm/latest/abbegm/) (in Rust).

## What is available in this repository?
- [Tutorial on how to prepare your ABB robot controller to run EGM](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGM-Preparing-your-robot.pdf)
- [WPF Application](https://github.com/vcuse/egm-for-abb-robots/tree/main/WPF-Example): Example of a desktop application implemented in C# (.NET) to move an ABB robot to a specific location (x, y, z) and rotation (rx, ry, rz) using _EGMActPose_ and _EGMRunPose_.
- [Unity Application](https://github.com/vcuse/egm-for-abb-robots/tree/main/WPF-Example): Example of an Unity application implemented in C# to move an ABB robot to a specific joint configuration using _EGMActJoint_ and _EGMRunJoint_.
- Examples of RAPID code to turn your controller into an EGM client: 
    - [Robot movement based on _EGMActPose_ and _EGMRunPose_](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMPoseCommunication.modx) (Position)
    - [Robot movement based on _EGMActJoint_ and _EGMRunJoint_](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGMJointCommunication.modx) (Joint Values)


# Common questions
- **Is EGM available in all ABB robots?** As far as I know, some robots using RobotWare 6 do not support EGM by default. We have a YuMi in our lab, and we had to contact ABB to get access to this feature in our IRC5 controller. My recommendation is to always contact ABB support when you have questions about your robot.

- **Does it work with virtual controllers?**
The answer is yes, but there are limitations. As far as I am aware of, it is not possible to run both virtual controlller and your application on a same machine. The reason is pretty simple, your computer would send and receive messages from both the EGM client and the server at the same time, creating issues in the communication process (e.g., errors from virtual controller saying that the inbound message is invalid). 

You also have to make your virtual controller public on the network if you want to control it with an external device. To make it accessible, add a new line containing the external IP address (e.g., <host ip="192.168.0.36"/>) in the C:\Users\your_username\AppData\Roaming\ABB Industrial IT\Robotics IT\VCConf.xml file on your computer. 

# Need help?
Don't hesitate to contact me:
- Felipe Fronchetti - fronchettl@vcu.edu.
