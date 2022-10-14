# Example of desktop application to control ABB robots

## Requirements
To make this application work you will need:
- Visual Studio
- .NET Core 3.1
- An ABB robot running EGM to act as your client

## How to run the application?
Open the project using Visual Studio by clicking on the [WPFProject.sln](https://github.com/vcuse/egm-for-abb-robots/blob/main/WPF-Example/WPFProject.sln) executable. Click on the play button on Visual Studio to run the application.

## What files in this project are related to EGM?
There are only two files in this project that should care about if you are learning about EGM:
- [MainWindow.xaml.cs](https://github.com/vcuse/egm-for-abb-robots/blob/main/WPF-Example/MainWindow.xaml.cs) This desktop application has only one window, and is in the C# file associated with this window that we run the EGM communication between our computer and the robot. This is the most important file as it contains the entire implementation made by for this project.

