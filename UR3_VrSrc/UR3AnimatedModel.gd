extends Node3D

#-- Constants
const BASE_ROT_OFFSET = -3.14;
const SHOULDER_ROT_OFFSET = 1.57;
const WRIST1_ROT_OFFSET = 1.57;

#-- Load urbot node to allow communication
var urbot_script = load("res://urbot.cs")
var urbot_node = urbot_script.new()


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	
	if(urbot_node != null):
		print(urbot_node.GetActualJoints())
		var ur3_current_joints = urbot_node.GetActualJoints()
		
		#-- Rotation of each joint of te robot
		'''
		$Yup2Zup/UnityRobotics_RF3_s1/UR3.rotation[1] = -ur3_current_joints[0] + BASE_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder.rotation[2] = ur3_current_joints[1] + SHOULDER_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow.rotation[2] = ur3_current_joints[2]
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01.rotation[1] = ur3_current_joints[3] + WRIST1_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01/Wrist02.rotation[2] = ur3_current_joints[4]
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01/Wrist02/Wrist03.rotation[1] = ur3_current_joints[5]
		'''
		
		#-- Rotation of each joint of te robot
		$Yup2Zup/UnityRobotics_RF3_s1/UR3.rotation[1] = -ur3_current_joints[0] + BASE_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder.rotation[2] = ur3_current_joints[1] + SHOULDER_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow.rotation[2] = ur3_current_joints[2]
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01/Wrist02.rotation[2] = ur3_current_joints[3] + WRIST1_ROT_OFFSET
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01/Wrist02/Wrist03.rotation[1] = -ur3_current_joints[4]
		$Yup2Zup/UnityRobotics_RF3_s1/UR3/Shoulder/Elbow/Wrist01/Wrist02/Wrist03/EffectorJoint.rotation[2] = ur3_current_joints[5]
	
	else:
		print("NULL")
