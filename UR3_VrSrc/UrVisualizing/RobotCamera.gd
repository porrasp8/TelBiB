extends Node3D

#-- Global vars
var http_request : HTTPRequest
var XRCamera : XRCamera3D  #-- To know headset position(Finally not used)
var default_posy_window : float

#-- Constants
const  CAMARA_URL = "http://192.168.0.193:8080/shot.jpg"
const  IMAGE_DIR = "res://captured_images/"
const  REQUEST_INTERVAL = 0.01
const WINDOW_Y_SCALE_VAL = -6
const DEBUG = false;

func _ready():
	
	default_posy_window = position[1]
	
	#-- HTTP request init
	http_request = HTTPRequest.new()
	XRCamera = XRCamera3D.new()
	add_child(http_request)
	
	#-- Init callback and connect
	http_request.request_completed.connect(self._http_request_completed)
	
	_make_request()


func _process(delta):
	
	#-- Update window position in function of headset position(inherits of XrCamera3D)
	if(XRCamera.rotation[0] < 0):
		position[1] = default_posy_window + XRCamera.rotation[0] * WINDOW_Y_SCALE_VAL
	else:
		position[1] = default_posy_window

	#-- New request when previous finished
	#print("HTTP status: ", http_request.get_http_client_status())
	if (http_request.get_http_client_status() != HTTPClient.STATUS_REQUESTING and 
		http_request.get_http_client_status() != HTTPClient.STATUS_CONNECTING and
		http_request.get_http_client_status() != HTTPClient.STATUS_BODY and 
		http_request.get_http_client_status() != HTTPClient.STATUS_CONNECTED ) : 
		_make_request()


func _make_request():
	
	#-- Make a new http request and check error
	var http_error = http_request.request(CAMARA_URL)
	if (http_error != OK and DEBUG):
		print("An error occurred in the HTTP request.")
		

func _http_request_completed(result, response_code, headers, body):

	if response_code == 200:
		#-- Load new image
		var image = Image.new()
		var image_error = image.load_jpg_from_buffer(body)
		if (image_error != OK and DEBUG):
			print("An error occurred while trying to display the image.")
		
		#-- Transform image to texture and assign to the 3D
		$RobotCameraWindow.texture = ImageTexture.create_from_image(image)
		#--print("HTTP: texture changed")
		
		#-- Save image(Debugging)
		'''
		var image_filename = "captured_image.jpg"
		var image_path = IMAGE_DIR + image_filename
		image.save_jpg(image_path)
		print("Image saved:", image_path)
		
		var image2 = Image.load_from_file(image_path)
		'''
		
	else:
		print("Error in HTTP request. Response code:", response_code)

