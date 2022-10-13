# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots. 

_Last update: 10/12/2022_

## What?
EGM is a feature available in RobotWare 6 and 7 that provides external devices the ability to interact with ABB robots through a network communication. In this approach, the information is encoded using Google Protocol Buffers and trasported through UDP sockets.

For a more detailed explanation, visit the [Application Manual](https://library.e.abb.com/public/f05090fae99a4d0ba2ee332e50865791/3HAC073318%20AM%20Externally%20Guided%20Motion%20RW7-en.pdf) provided by ABB.

## Why?
In our laboratory, we implement applications in mixed reality that are directly connected to ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit. One of the current limitations of Unity is that it does not accept external development kits, such as the ones provided by ABB. For this reason, we tend to use EGM to create the communication between our mixed reality devices and the ABB robots. EGM is simple, efficient and works perfectly with Unity.

# Common questions
- **Does it work with virtual controllers?**
The answer is yes, but there are limitations. As far as I am aware of, it is not possible to run both virtual controlller and your application on a same machine. The reason is pretty simple, your computer would send and receive messages from both the EGM client and the server at the same time, creating issues in the communication process (e.g., errors on robot controller saying that the inbound message is invalid). 
