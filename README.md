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

To allow the remote communication we configurte a robot dedicated router. The name of the network is "UR3_wifi" and use the default password, we also configure statics IPs for each system component to simplify te procces in future connections.

<div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/004cf20e-048f-41d9-810c-d5a9855c5e7c"
      alt="Router"
      width="500" 
      height="250" />
  </p>
   <p align="center"> Router</p>
</div>

<div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/c9afc9a2-2f4f-4384-83aa-15c876683da6"
      alt="Wifi" />
  </p>
   <p align="center"> Wifi</p>
</div>

The system is composed for 3 main elements:
- Robotics Arm(UR3 from Universal Robots): Connected directly via Ethernet, IP -> 




