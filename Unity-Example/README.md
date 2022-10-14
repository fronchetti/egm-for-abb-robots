# Using Unity to define joints position in ABB robots
_Last update: 10/13/2022_

![Screenshot - Unity Application](https://github.com/vcuse/egm-for-abb-robots/blob/main/Images/Screenshot-Unity-Application.jpg?raw=true)
**Image Description:** Screenshot of this application running on Unity. The application contains an interface to manipulate the robot axes using sliders. At the bottom a text component shows the current state of the EGM process.


- Import Egm.cs to Unity project (drag and drop)
- Install NugetForUnity to install Google.Protobuf and Google.Protobuf.Tools (required to run Egm.cs). Restart Unity once installed.
- Make sure your firewall allows inbound messages for the Unity version you are using.
