extends Node3D

@onready var camera = $InnerGimbal/Camera3D
@onready var innergimbal = $InnerGimbal

@export var max_zoom = 10000.0
@export var min_zoom = 0.001
@export var zoom_speed = 0.16
var zoom = 1

@export var speed = 0.1
@export var drag_speed = 600
@export var acceleration = 0.16
@export var mouse_sensitivity = 0.005

var move = Vector3()


# Called when the node enters the scene tree for the first time.
func _ready():
	fit_to_grounds_bounds(Rect2(Vector2(6046592, 99040), Vector2(1063887, 1143400)), 0.1)
	pass

func _input(event):
	if Input.is_action_pressed("rotate_cam"):
		if event is InputEventMouseMotion:
			if event.relative.x != 0:
				rotate(Vector3.UP, -event.relative.x * mouse_sensitivity)
		if event is InputEventMouseMotion:
			if event.relative.y != 0:
				rotate_object_local(Vector3.RIGHT, -event.relative.y * mouse_sensitivity)
	if Input.is_action_pressed("move_cam"):
		if event is InputEventMouseMotion:
			move.x -= event.relative.x * drag_speed
			move.z -= event.relative.y * drag_speed

	var previous_zoom = zoom
	var previous_zoom_speed = zoom_speed
	if event.is_action_pressed("zoom_in"):
		zoom -= zoom_speed
	if event.is_action_pressed("zoom_out"):
		zoom += zoom_speed
	# zoom = clamp(zoom, min_zoom, max_zoom)
	zoom_speed = zoom * previous_zoom_speed / previous_zoom
	print(zoom_speed)

func _process(delta):
	#zoom camera
	scale = lerp(scale, Vector3.ONE * zoom, zoom_speed)
	if (Vector3.ONE * zoom != scale):
		print(scale, ' ', Vector3.ONE * zoom)
	#clamp rotation
	# innergimbal.rotation.x = clamp(innergimbal.rotation.x, -1.1, 0.3)
	#move camera
	move_cam(delta)

func move_cam(_delta):
	#get inputs
	if Input.is_action_pressed("move_forward"):
		move.z = lerp(move.z,-speed, acceleration)
	elif Input.is_action_pressed("move_backward"):
		move.z = lerp(move.z,speed, acceleration)
	else:
		move.z = lerp(move.z, 0.0, acceleration)
	if Input.is_action_pressed("move_left"):
		move.x = lerp(move.x,-speed, acceleration)
	elif Input.is_action_pressed("move_right"):
		move.x = lerp(move.x,speed, acceleration)
	else:
		move.x = lerp(move.x, 0.0, acceleration)
	
	#move camera
	position += move.rotated(Vector3.UP,self.rotation.y) * zoom
	# position.x = clamp(position.x,-20,20)
	# position.z = clamp(position.z,-20,20)

# Fit the camera to a ground bounds
func fit_to_grounds_bounds(bounds: Rect2, margin_ratio: float):
	var size = bounds.size * (1 + margin_ratio)
	var fov = deg_to_rad(camera.fov)
	var viewport_aspect_ratio = get_viewport().size.x / get_viewport().size.y
	innergimbal.position.z = abs(max(size.y, size.x) / (2 * viewport_aspect_ratio * tan(fov / 2)))
	position.x = bounds.get_center().x
	position.z = bounds.get_center().y
