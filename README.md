# UR3_VrTeleop

## Index
* [Introduction](#introduction)
* [Features](#features)
* [Working Schema](#working-schema)
* [Newtowk communication](newtowk-communication)
* [RTDE protocol](real-time-data-exchange(rtde)-protocol)
* [IP Camera](ip-camera)

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
3. IP Camera streaming visulized in the VR escene(following user view)
4. Some debug items in the VR environment(robot area, operation table, Tcp position..)
5. Virtual safety boundaries in robot space
6. 70Hz response(limited by headset refresh rate, robot allows 125)
7. Gripper remote activation(Robotiq)
8. A 3D Virtual Robot model communicated with real robot


### Working Schema





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
- Robotics Arm(UR3 from Universal Robots): Connected directly via Ethernet, IP -> 192.168.0.102
- Camera IP(a mobile phone provisionally): Via wifi real time image streaming, IP->	192.168.0.193, Port -> 8080, URL -> http://192.168.0.193:8080/shot.jpg
- Computer proccesing VR Scene: Via wifi and USB3 cable with headseet, IP-> 192.168.0.100

  <div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/1863d413-8a7a-4ba2-9248-3ba07c7f26c5"
      alt="Statics IPs" />
  </p>
   <p align="center"> Statics IPs</p>
</div>


> ¡TAKE CARE! The Eternet/IP option of the robot needs to be off. It communicates via LAN.

### Real Time Data Exchange(RTDE) protocol

The Real-Time Data Exchange (RTDE) interface provides a way to synchronize external applications with the UR controller over a standard TCP/IP connection, without breaking any real-time properties of the UR controller. This functionality is among others useful for interacting with fieldbus drivers (e.g. Ethernet/IP), manipulating robot I/O and plotting robot status (e.g. robot trajectories). The RTDE interface is by default available when the UR controller is running.

RTDE will be the way in which the information extracted from the VR headset will be transmitted over the network to the robot. In this case and for unify the project we will use **a C# implementation for this library that can be used with Godot game engine**. These is so useful to allow VR escene an robot fast communication. [C# project for Godot and VR](https://sourceforge.net/p/firsttestfchaxel/code/HEAD/tree/trunk/Ur_Rtde/).

You can see more information about it in the following links:
- [RTDE client library github repo](https://github.com/UniversalRobots/RTDE_Python_Client_Library)
- [UR RTDE guide](https://www.universal-robots.com/download/manuals-e-seriesur20ur30/script/script-manual-e-series-sw-511/)


### IP Camera

While the robot is being teleoperated, it will be necessary for the VR scene to show some type of information about its real situation, either through a two-dimensional image or a cloud of points. Initially we decided to place an IP camera that transmits real-time streaming over the network that we have previously configured, allowing it to be read by the main computer and the recreation of this image in the virtual scene.

In order to view this image in the virtual environment we will need to create two new elements in the 3D scene of the project:

- An HTTP Requets module: to be able to request images from the IP camera through the network. We will use the previously defined URL.
- A Sprite3D module: Acts as a screen where the image obtained at each moment will be projected.
It is important that these modules are children of the camera module to allow the screen to rotate when the user turns their head and thus can follow the situation of the robot at any time. Furthermore, it is advisable to define a function that limits the transfer of the floor by this screen to prevent it from overlapping with it and not being able to be viewed.


  <div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/4092003b-155f-424f-a195-1c40ac83bb98"
      alt="RobotCamera Window Tree" />
  </p>
   <p align="center"> RobotCamera Window Tree</p>
</div>


In order to make HTTP requests and maintain the camera as indicated, we will add a godot script (gd) to the "RobotCamera" module (RobotCamera.gd). We will need this script to communicate with two other modules, the HTTP to allow it to make requests and the camera to know the position of the headset.

Below you can see a snippet with the most important calls made by HTTP:

``` gd
#-- Init callback and connect
http_request.request_completed.connect(self._http_request_completed)

#-- Make a new http request and check error
var http_error = http_request.request(CAMARA_URL)
if http_error != OK:
  print("An error occurred in the HTTP request.")

#-- Callback and texture change
func _http_request_completed(result, response_code, headers, body):

	if response_code == 200:
		#-- Load new image
		var image = Image.new()
		var image_error = image.load_jpg_from_buffer(body)
		if image_error != OK:
			print("An error occurred while trying to display the image.")
		
		#-- Transform image to texture and assign to the 3D
		$RobotCameraWindow.texture = ImageTexture.create_from_image(image)
		print("HTTP: texture changed")
```


To reduce the screen overlap we use the following calls:
``` gd
#-- Called every delta
func _process(delta):	
	#-- Update window position in function of headset position(inherits of XrCamera3D)
	if(XRCamera.rotation[0] < 0):
		position[1] = default_posy_window + XRCamera.rotation[0] * WINDOW_Y_SCALE_VAL
	else:
		position[1] = default_posy_window
```

As this script is associated with the "Robot Camera" node, the "position" variable that is being modified does not refer to the position of this node itself. To check the variables that you can modify for a certain type of node, you can access the Godot API and search for the type of class and what other classes it inherits from to know what values ​​it is made of, for example in this case we could consult [Here](https://docs.godotengine.org/en/stable/classes/class_node3d.html).


Operation test:

  <div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/e34e9132-f97b-46ec-b8da-371f1250f331"
      alt="Working Example" 
	width="750" 
      height="500"/>
  </p>
   <p align="center"> Working Example</p>
</div>


///////////////////////////////

### VR Environment

Además de todos los elementos vistos en el apartado [IP Camera](ip-camera) el entorno de realidad virtual contiene varios nodos pensandos para ayudar al usuario a llevar a cabo la operación:

- Area de trabajo(fuera de las singularidades del brazo):

<div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/63df4320-dc79-4cbf-88da-1c60af005b97"
      alt="Work area" />
  </p>
   <p align="center"> Work area</p>
</div>


- Modelo 3D del brazo robotico: Conectado con el robot real. Se reciben las posiciones de los "joints" del robot real y se mueve el robot en la simulacion utilizando un godot script. La textura de este se ha configurado con trasparencia para no interferir en la visualización de la camara:

<div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/5003392b-0742-44b4-ae4f-fb24cc936de0"
      alt="UR3 VR 3D Model" />
  </p>
   <p align="center"> UR3 VR 3D Model</p>
</div>


- Elemento terminal del robot en forma de cono. Este esta pensando para configurar correctamentre las rotaciones entre el TCP real y el del entorno virtual. La rotación de este pasa por un proeceso de transformaciones que nos permite comprobar que el sistema este funcionando correctamente. La rotación del mando se envia al robot con formato "RPY"(Roll Pitch Yaw), que es el sistema de rotaciones estandar usado por godot, el robot recibe estos valores y los transforma en "Vector de Rotación"(formato usado por el UR3) a traves de la función **rpy2rotvec()** de URscript. A continuación el robot realizara la transformación inversa para tartar de recibir los valores iniciales de nuevo y lo alamacena en un registro para que pueda ser leido por el ordenador y posicionar el elemento terminal:

<div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/ab94111c-2399-480f-a686-2f02dedd8aec"
      alt="Simualted Robot Tcp" />
  </p>
   <p align="center"> Simualted Robot Tcp</p>
</div>

