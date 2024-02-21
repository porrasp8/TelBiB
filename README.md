# UR3_VrTeleop

## Index
* [Introduction](#introduction)
* [Features](#features)
* [Newtowk communication](newtowk-communication)

### Introduction

This repository contains a godot project that generates a virtual reality space (tested with an oculus quest 2) that communicates with a UR3 robotic arm using the RTDE library to perform teleoperation tasks.
You can see useful information in the following pages:
- [Interesting VR TFG](https://core.ac.uk/reader/154758810)
- [Interesting Predictive Paper](https://www.alaris.kz/wp-content/uploads/2013/11/lbr1899_Omarali.pdf)
- [UR RTDE guide](https://www.universal-robots.com/download/manuals-e-seriesur20ur30/script/script-manual-e-series-sw-511/)
- [URScript API Reference](https://s3-eu-west-1.amazonaws.com/ur-support-site/50689/scriptManual.pdf)

### Features:
1. Own network(UR3_wifi) to unify communication with the robot, sending orders from the computer connected to the VR headset and video streaming(Camera IP)
2. RTDE protocol to communicate with UR3
3. Camera Ip streaming visulized in the VR escene(following user view)
4. Some debug items in the VR environment(robot area, operation table, Tcp position..)
5. Virtual safety boundaries in robot space
6. 70Hz response(limited by headset refresh rate, robot allows 125)
7. Gripper remote activation(Robotiq)


``` py

```


### Newtowk communication

To allow the remote communication we configurte a robot dedicated router. A esta red se conectaran los siguientes elementos de nuestro sistema:




