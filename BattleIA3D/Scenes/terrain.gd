tool
extends MultiMeshInstance

export var terrain_h = 10 setget set_terrain_height
export var terrain_w = 10 setget set_terrain_width
func ready():
	get_tree().reload_current_scene()

func set_terrain_width(newwidth):
	terrain_w = newwidth
	update()
func set_terrain_height(newheight):
	terrain_h = newheight
	update()
	
func update():
	setup_multimesh()
	generate_terrain()
	
func setup_multimesh():
	randomize()
	self.multimesh = MultiMesh.new()
	self.multimesh.transform_format= MultiMesh.TRANSFORM_3D
	self.multimesh.color_format = MultiMesh.COLOR_8BIT
	
	self.multimesh.instance_count= terrain_h * terrain_w * 2
	self.multimesh.visible_instance_count = self.multimesh.instance_count
	var mesh = CubeMesh.new()
	mesh.size = Vector3(0.95,0.95,0.95)
	var material = SpatialMaterial.new()
	material.vertex_color_use_as_albedo = true
	self.multimesh.mesh= mesh
	self.multimesh.mesh.material = material
	
func generate_terrain():
	var cube_size = 1	
	var cube_hs = cube_size / 2.0
	var origin_distance = 0
	var origin = Vector3(-self.terrain_w*cube_hs,-self.terrain_h*cube_hs ,0)
	var index = 0
	
	for x in range(self.terrain_w):
		for y in range(self.terrain_h):
			var pos = origin + Vector3(x * cube_size, y * cube_size,0)
			var color = Color(0.2,0.2,0.2)
			self.multimesh.set_instance_transform(index,Transform(Basis(),pos) )
			self.multimesh.set_instance_color(index, color)
			index += 1

func addblock(_i,_j,_type,_c):
	var cube_size = 1	
	var cube_hs = cube_size / 2.0
	var origin_distance = 0
	var origin = Vector3(-self.terrain_w*cube_hs,-self.terrain_h*cube_hs ,cube_size)
	var index = self.terrain_w *self.terrain_h+_c
	var pos = origin + Vector3(_i * cube_size, _j * cube_size,-2*cube_size)
	var color = Color(0.1,0.1,0.1)
	self.multimesh.set_instance_transform(index,Transform(Basis(),pos) )
	self.multimesh.set_instance_color(index, color)

