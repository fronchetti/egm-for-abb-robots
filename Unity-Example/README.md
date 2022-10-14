# Using Unity to define joints position in ABB robots
_Last update: 10/13/2022_

![Screenshot - Unity Application](https://github.com/vcuse/egm-for-abb-robots/blob/main/Images/Screenshot-Unity-Application.jpg?raw=true)
**Image Description:** Screenshot of this application running on Unity. The application contains an interface to manipulate the robot axes using sliders. At the bottom a text component shows the current state of the EGM process.

# Requirements
In order to run this project, you need:
- Unity (Version 2020.3.14f1)
- To allow Unity in your operating system firewall to receive and send messages 
- An ABB robot running EGM to serve as your EGM client

# How to run this application?
Import this project folder using Unity Hub and open the project using Unity 2020.3.14f1. Click on the play button available on the top of the Unity interface to run the program. Use the sliders to manipulate the robot joints.

If your robot is not running EGM already, please refer to our [tutorial](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGM-Preparing-your-robot.pdf) on how to setup your ABB robot for EGM communication.

# What files in this project are related to EGM?
If you are here just to check how we implemented EGM code that runs in Unity, the [Scripts](https://github.com/vcuse/egm-for-abb-robots/tree/main/Unity-Example/Assets/Scripts) folder is what you need. Inside of it you will find:
- [Egm.cs](https://github.com/vcuse/egm-for-abb-robots/blob/main/WPF-Example/Egm.cs) This file contains the Abb.Egm library used in [EgmCommunication.cs](https://github.com/vcuse/egm-for-abb-robots/blob/main/Unity-Example/Assets/Scripts/EgmCommunication.cs) to write messages in EGM format. Notice that this file is generated automatically. To create your own version of this file, refer to the EGM manual provided by ABB (Section 3.2 - Building an EGM sensor communication endpoint) or follow our [tutorial](https://github.com/vcuse/egm-for-abb-robots/blob/main/EGM-Preparing-your-robot.pdf).


- Import Egm.cs to Unity project (drag and drop)
- Install NugetForUnity to install Google.Protobuf and Google.Protobuf.Tools (required to run Egm.cs). Restart Unity once installed.
