# UR3_VrTeleop

## Index
* [Introduction](#introduction)
* [Features](#features)
* [Newtowk communication](newtowk-communication)
* [RTDE protocol](real-time-data-exchange(rtde)-protocol)

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


### Camera IP

While the robot is being teleoperated, it will be necessary for the VR scene to show some type of information about its real situation, either through a two-dimensional image or a cloud of points. Initially we decided to place an IP camera that transmits real-time streaming over the network that we have previously configured, allowing it to be read by the main computer and the recreation of this image in the virtual scene.


///////////////////////////////

Para poder visualizar esta imagen en el entorno virtual necesitaremos crear dos nuevos elementos en la escena 3D del proyetco:
- Un modulo HTTP Requets: para poder hacer la solicitud de imagenes a la camara Ip a traves de la red. Usaremos la URL previamente definida.
- Un modulo Sprite3D: Actua como pantalla donde se proyectará la imagen obtenida en cada instante.

Es importante que estos modulos sean hijos del modulo camara para permitir que la pantalla se vaya rotando cuando el usuario gire la cabeza y asi pueda seguir la situación del robot en cualquier instante. Además es recomendable definir una función que limite el traspaso del suelo por parte de esta pantalla para evitar que se solape con el y no se pueda visualizar.

  <div class="figure">
  <p align="center">
    <img
      src="https://github.com/porrasp8/UR3_VrTeleop/assets/72991722/4092003b-155f-424f-a195-1c40ac83bb98"
      alt="RobotCamera Window Tree" />
  </p>
   <p align="center"> RobotCamera Window Tree</p>
</div>


Para poder realizar las solicitiudes HTTP y manetener la camara de la forma indicada añadiremos un godot script(gd) al modulo "RobotCamera"(RobotCamera.gd). Necesitaremos que este script se comunique con otros dos modulos, el HTTP para permitirle hacer las solicitudes y el camara para conocer la posición del headset. 

A continuacion puedes ver un snippet con las llamadas más importantes realizadas por HTTP:

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


Para reforzar el solapamiento de la pantalla usamos las siguientes llamadas:
``` gd
#-- Called every delta
func _process(delta):	
	#-- Update window position in function of headset position(inherits of XrCamera3D)
	if(XRCamera.rotation[0] < 0):
		position[1] = default_posy_window + XRCamera.rotation[0] * WINDOW_Y_SCALE_VAL
	else:
		position[1] = default_posy_window
```

Como este script esta asociado al nodo "Robot Camara" la variable "position" que se esta modificadno se refeiere a la posición de este propio nodo. Para comprobar las varianles que puedes modificar de un determinado tipo de nodo puedes acceder a la API de Godot y buscar el tipo de clase y de que otras clases hereda para saber por que valores esta formado, por ejemplo en este caso podriamos consultar [Aqui](https://docs.godotengine.org/en/stable/classes/class_node3d.html).


Ejemplo del funcionamiento:

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



