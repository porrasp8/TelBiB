extends Node3D

var viewport : Viewport
var sprite : Sprite3D
var http_request : HTTPRequest

var camera_url = "http://192.168.0.193:8080/shot.jpg"
var image_directory = "res://captured_images/"

var request_interval = 0.001  # Tiempo en segundos entre solicitudes
var time_since_last_request = 0.0

func _ready():
	# Obtén referencias al Viewport y al Sprite2D
	viewport = $RobotCamera
	#sprite = $RobotCamera/Image2
	
	http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._http_request_completed)
	
	_make_request()


func _process(delta):
	time_since_last_request += delta

	# Realiza una nueva solicitud si ha pasado el tiempo del intervalo
	if time_since_last_request >= request_interval:
		_make_request()


func _make_request():
	# Reinicia el temporizador
	time_since_last_request = 0.0

	# Realiza la solicitud HTTP
	var http_error = http_request.request(camera_url)
	if http_error != OK:
		print("An error occurred in the HTTP request.")

func _http_request_completed(result, response_code, headers, body):

	if response_code == 200:
		var image = Image.new()
		var image_error = image.load_jpg_from_buffer(body)
		if image_error != OK:
			print("An error occurred while trying to display the image.")
		
		#image.convert(Image.FORMAT_RGBA8)
		var texture = ImageTexture.new()
		texture.create_from_image(image)

		# Guardar la imagen en el directorio del proyecto
		var image_filename = "captured_image.jpg"
		var image_path = image_directory + image_filename
		image.save_jpg(image_path)
		print("Image saved:", image_path)
		
		var image2 = Image.load_from_file(image_path)
		$RobotCameraWindow.texture = ImageTexture.create_from_image(image2)
		
	else:
		print("Error in HTTP request. Response code:", response_code)

