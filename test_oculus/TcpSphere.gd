extends MeshInstance3D

#-- Constants
const POSZ_OFFSET = 0.6;

#-- Load urbot node to allow communication
var urbot_script = load("res://urbot.cs")
var urbot_node = urbot_script.new()

func _ready():
	pass

func _process(delta):
	#-- Set global position for each step 
	#-- position = Vector3(1.2, 1.41, -2.32)
	
	#-- Extract pose and rot values and change Sphere pose
	if(my_csharp_node != null):
		print(urbot_node.GetActualTCPPose())
		var tcp_current_pose = urbot_node.GetActualTCPPose()
		position = Vector3(tcp_current_pose[1], tcp_current_pose[2]+POSZ_OFFSET, tcp_current_pose[0])  #-- set_global_position ???
		
		var tcp_current_RPY = urbot_node.GetActualRPYRot()
		rotation = Vector3(tcp_current_RPY[1], tcp_current_RPY[2], tcp_current_RPY[0])   #--- global_rotate ??
		
	else:
		print("NULL")
	
