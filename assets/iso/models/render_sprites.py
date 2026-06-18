# Blender Iso-Sprite Renderer fuer Doener Empire
# Laedt .glb-Modelle, rendert PNG-Sprites mit Iso-Kamera
import bpy
import os
import math

PROJECT_ROOT = "C:/Users/Kaan/Documents/GitHub/Doener-Empire-3D"
MODELS_DIR = os.path.join(PROJECT_ROOT, "assets", "iso", "models")
OUTPUT_DIR = os.path.join(PROJECT_ROOT, "assets", "iso")
SPRITE_SIZE = 512

model_files = sorted([f for f in os.listdir(MODELS_DIR) if f.endswith('.glb')])
print(f"Gefundene Modelle: {len(model_files)}")
for i, f in enumerate(model_files):
    print(f"  {i+1}. {f}")

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for block in bpy.data.meshes:
        bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        bpy.data.materials.remove(block)
    for block in bpy.data.cameras:
        bpy.data.cameras.remove(block)
    for block in bpy.data.lights:
        bpy.data.lights.remove(block)

def setup_scene():
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.render.resolution_x = SPRITE_SIZE
    scene.render.resolution_y = SPRITE_SIZE
    scene.render.film_transparent = True
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGBA'
    scene.cycles.samples = 64
    scene.cycles.use_denoising = True
    world = bpy.data.worlds["World"]
    world.use_nodes = True
    tree = world.node_tree  # Placeholder - will be rebuilt in render loop
    for node in tree.nodes:
        tree.nodes.remove(node)
    bg = tree.nodes.new('ShaderNodeBackground')
    output = tree.nodes.new('ShaderNodeOutputWorld')
    bg.inputs[0].default_value = (0.06, 0.04, 0.02, 1.0)
    bg.inputs[1].default_value = 1.0
    tree.links.new(bg.outputs[0], output.inputs[0])

def setup_camera():
    bpy.ops.object.camera_add()
    cam = bpy.context.object
    cam.name = "IsoCam"
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = 8.0
    cam.rotation_euler = (
        math.radians(60),
        0,
        math.radians(45),
    )
    cam.location = (0, -20, 20)
    return cam

def setup_lighting():
    bpy.ops.object.light_add(type='AREA', location=(8, -8, 12))
    key = bpy.context.object
    key.name = "KeyLight"
    key.data.energy = 800
    key.data.color = (1.0, 0.82, 0.6)
    key.data.size = 5

    bpy.ops.object.light_add(type='AREA', location=(-8, 8, 6))
    fill = bpy.context.object
    fill.name = "FillLight"
    fill.data.energy = 300
    fill.data.color = (0.6, 0.7, 1.0)
    fill.data.size = 5

    bpy.ops.object.light_add(type='AREA', location=(0, 0, 15))
    rim = bpy.context.object
    rim.name = "RimLight"
    rim.data.energy = 200
    rim.data.color = (0.8, 0.9, 1.0)
    rim.data.size = 3

def load_and_center(glb_path):
    bpy.ops.import_scene.gltf(filepath=glb_path)
    objs = [o for o in bpy.context.selected_objects if o.type == 'MESH']
    if not objs:
        objs = [o for o in bpy.data.objects if o.type == 'MESH']
    if not objs:
        print("  Keine Mesh-Objekte gefunden!")
        return None

    # Parent alle Meshes unter einem Empty
    bpy.ops.object.empty_add(type='PLAIN_AXES', location=(0, 0, 0))
    root = bpy.context.object
    root.name = "ModelRoot"
    for o in objs:
        o.parent = root

    # Zentrieren
    bpy.ops.object.select_all(action='DESELECT')
    root.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
    root.location = (0, 0, 0)
    return root

def render_sprite(output_path):
    bpy.context.scene.render.filepath = output_path
    bpy.ops.render.render(write_still=True)
    print(f"  -> Gespeichert: {output_path}")

sprite_names = [
    "building_owned.png",
    "building_empty.png",
    "building_competitor.png",
    "building_filler.png",
]

if len(model_files) >= 4:
    for i, glb_file in enumerate(model_files[:4]):
        glb_path = os.path.join(MODELS_DIR, glb_file)
        sprite_name = sprite_names[i]
        output_path = os.path.join(OUTPUT_DIR, sprite_name)

        print(f"\n=== Render {i+1}/4: {glb_file} -> {sprite_name} ===")
        try:
            clear_scene()
            setup_scene()
            setup_lighting()
            cam = setup_camera()
            bpy.context.scene.camera = cam

            root = load_and_center(glb_path)
            if root:
                dims = root.dimensions
                max_dim = max(dims)
                if max_dim > 0:
                    cam.data.ortho_scale = max_dim * 1.8
                    print(f"  Groesse: {dims}, ortho_scale: {cam.data.ortho_scale:.2f}")

            render_sprite(output_path)
        except Exception as e:
            print(f"  FEHLER: {e}")
else:
    print(f"Nur {len(model_files)} Modelle - rendere alle...")
    for i, glb_file in enumerate(model_files):
        glb_path = os.path.join(MODELS_DIR, glb_file)
        sprite_name = f"building_model_{i}.png"
        output_path = os.path.join(OUTPUT_DIR, sprite_name)
        try:
            clear_scene()
            setup_scene()
            setup_lighting()
            cam = setup_camera()
            bpy.context.scene.camera = cam
            root = load_and_center(glb_path)
            if root:
                dims = root.dimensions
                max_dim = max(dims)
                if max_dim > 0:
                    cam.data.ortho_scale = max_dim * 1.8
            render_sprite(output_path)
        except Exception as e:
            print(f"  FEHLER bei {glb_file}: {e}")

print(f"\nFertig! Sprites in: {OUTPUT_DIR}")
