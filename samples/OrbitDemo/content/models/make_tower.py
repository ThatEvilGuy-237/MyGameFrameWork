import sys

import bpy

out = sys.argv[sys.argv.index("--") + 1]
bpy.ops.wm.read_factory_settings(use_empty=True)


def material(name, rgba, roughness=0.8, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    shader = next(node for node in mat.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
    shader.inputs["Base Color"].default_value = rgba
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    return mat


def finish(name, mat):
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name
    obj.data.materials.append(mat)
    return obj


stone = material("Stone", (0.55, 0.53, 0.48, 1.0))
plaster = material("Plaster", (0.86, 0.80, 0.68, 1.0))
roof = material("Roof", (0.62, 0.16, 0.12, 1.0), roughness=0.6)
wood = material("Wood", (0.35, 0.22, 0.12, 1.0))
flag = material("Flag", (0.24, 0.39, 0.87, 1.0), roughness=0.5)

bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.75, depth=0.3, location=(0, 0, 0.15))
finish("Base", stone)
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.48, depth=1.7, location=(0, 0, 1.15))
finish("Body", plaster)
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.62, depth=0.25, location=(0, 0, 2.1))
finish("Rampart", stone)
bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.68, radius2=0.0, depth=0.9, location=(0, 0, 2.67))
finish("Roof", roof)
bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.03, depth=0.7, location=(0, 0, 3.4))
finish("Pole", wood)
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0.24, 0, 3.6))
banner = finish("Flag", flag)
banner.scale = (0.42, 0.03, 0.24)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

bpy.ops.export_scene.gltf(filepath=out, export_format="GLB")
