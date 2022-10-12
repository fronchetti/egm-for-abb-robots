# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots. 

## What?
EGM is a feature available in ABB RobotWare that provides external devices the ability to control ABB robots. In this approach, the communication is done through Google Protocol Buffers using UDP sockets.

For more information, visit the [Application Manual](https://library.e.abb.com/public/f05090fae99a4d0ba2ee332e50865791/3HAC073318%20AM%20Externally%20Guided%20Motion%20RW7-en.pdf) provided by ABB.

## When?
In our laboratory, we implement different applications in mixed reality that are directly connected to ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit (MRTK). One of the current limitations of Unity is that it does not accept external kits, such as the ones provided by ABB. For this reason, we use EGM to create a direct communication between our mixed reality devices and the robots. EGM is simple, efficient and works perfectly with Unity.
