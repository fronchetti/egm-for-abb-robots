# egm-for-abb-robots
:gear: Using Externally Guided Motion (EGM) to control ABB robots in C#. 

_Last update: 10/12/2022_

## What?
EGM is a feature available in RobotWare 6 and 7 that provides external devices the ability to interact and manipulate ABB robots through a network. In this approach, the information is encoded using Google Protocol Buffers and trasported through UDP sockets. EGM is a great option for those who need to manipulate an ABB robot from an external device.

For a more detailed explanation, visit the [Externally Guided Motion - Application Manual](https://library.e.abb.com/public/f05090fae99a4d0ba2ee332e50865791/3HAC073318%20AM%20Externally%20Guided%20Motion%20RW7-en.pdf) provided by ABB.

## Why?
In our laboratory, we implement applications in mixed reality that are used to manipulate ABB robots. Such applications are made in Unity using the Microsoft Mixed Reality Toolkit. One of the current limitations of Unity is that it does not accept external development kits, such as the ABB PC SDK. One may argue that other options (e.g., ABB Robot Web Services) could be used to solve this problem, but most of the availabe alternatives do not allow developers to directly manipulate ABB robots. For this reason, we implement the communication between our devices and the ABB robots using the EGM funcionality. EGM is simple, efficient and works perfectly with Unity (or any other environment that allow UDP communication in C#).

For those looking for options that are not implemented in C#, please refer to libraries such as [abb_libegm](https://www.rosin-project.eu/tool/abb-libegm#:~:text=abb_libegm%20is%20a%20C%2B%2B%20communication,well%20as%20providing%20user%20APIs.) (in C++) and [abbegm](https://docs.rs/abbegm/latest/abbegm/) (in Rust).

# Common questions
- **Does it work with virtual controllers?**
The answer is yes, but there are limitations. As far as I am aware of, it is not possible to run both virtual controlller and your application on a same machine. The reason is pretty simple, your computer would send and receive messages from both the EGM client and the server at the same time, creating issues in the communication process (e.g., errors on robot controller saying that the inbound message is invalid). 
