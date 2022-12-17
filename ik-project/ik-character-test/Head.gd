extends Sprite

var offset_dist = 1

func _physics_process(delta):
	var facingDirection = (global_position - get_global_mouse_position()).angle()
	var lookMod = global_position.x - (global_position.x + 1 * cos(facingDirection))
	
	position.x = offset_dist * lookMod
	
	if lookMod < 0:
		flip_h = true
	else:
		flip_h = false
