# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots. 

## What?
EGM is a feature available in RobotWare that provides external devices the ability to control ABB robots. In this approach, the communication is done through Google Protocol Buffers using UDP sockets.

## When?
In our laboratory, we implement different applications in mixed reality that are directly connected to ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit (MRTK). Instead of using ROS or the ABB PC SDK, which might require a computer acting as a middleware in this communication, we use EGM to create a direct communication between our mixed reality devices and the robots.
