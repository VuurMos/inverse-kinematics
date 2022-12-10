extends KinematicBody2D

var _max_speed := 120
var acceleration := 500
var friction := 400
var _move_direction: Vector2 = Vector2.ZERO
var _velocity: Vector2 = Vector2.ZERO

onready var ik = $Node2D
onready var ik_target = $IKTarget

func _get_direction_input() -> Vector2:
	var direction_input = Vector2.ZERO
	direction_input.x = Input.get_action_strength("right") - Input.get_action_strength("left")
	direction_input.y = Input.get_action_strength("down") - Input.get_action_strength("up")
	return direction_input.normalized()

func _apply_friction(_amount: float):
	if _velocity.length() > _amount:
		_velocity -= _velocity.normalized() * _amount
	else:
		_velocity = Vector2.ZERO

func _apply_acceleration(_amount: Vector2):
	_velocity += _amount
	_velocity = _velocity.limit_length(_max_speed)

func _move():
	_velocity = move_and_slide(_velocity)

func _physics_process(delta):
	_move_direction = _get_direction_input()
	
	if _move_direction == Vector2.ZERO:
		_apply_friction(friction * delta)
	else:
		_apply_acceleration(_move_direction * acceleration * delta)
	
	_move()
	
	ik.UpdateIK(ik_target.global_position, _velocity)

