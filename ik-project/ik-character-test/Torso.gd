extends Sprite

var scale_amount = 0.05
var bounce_amount = 0.01
var motionCounter = 0
var animSpeed = 1

func _physics_process(delta):
	flip_and_scale()

func bounce(vel):
	var velMod = pow(vel.length() * 3, 0.4)
	print(velMod)
	
	motionCounter += (animSpeed * velMod)
	
	if motionCounter > 360:
		motionCounter = 0
	
	var animCounter = deg2rad(motionCounter)
	var baseYOffset = cos(animCounter)
	
	position.y += velMod * bounce_amount * baseYOffset

func flip_and_scale():
	var facingDirection = (global_position - get_global_mouse_position()).angle()
	var lookMod = global_position.x - (global_position.x + 1 * cos(facingDirection))
	
	scale.x = 0.95 + scale_amount * abs(lookMod)
	
	if lookMod < 0:
		flip_h = true
	else:
		flip_h = false
	
