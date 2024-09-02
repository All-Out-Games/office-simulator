13
270582939649
801966305833562 1721070907794692900
{
  "name": "Bullet",
  "local_enabled": true,
  "local_position": {
    "X": 0,
    "Y": 0
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 3,
    "Y": 3
  }
},
{
  "cid": 1,
  "aoid": "802081720336320:1721070940732659700",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "revolver_bullet.png",
    "depth_offset": 0,
    "tint": {
      "X": 1,
      "Y": 1,
      "Z": 1,
      "W": 1
    },
    "layer": 0,
    "wait_for_load": false,
    "wrap": false,
    "mask_in_shadow": false
  }
},
{
  "cid": 2,
  "aoid": "802575997750772:1721071081793727400",
  "component_type": "Internal_Component",
  "internal_component_type": "Projectile",
  "data": {
    "speed": 13,
    "direction": {
      "X": 0,
      "Y": 0
    },
    "start_position": {
      "X": 0,
      "Y": 0
    },
    "projectile_id": "",
    "instance_id": "",
    "spawn_id": 0,
    "owner_network_id": 0
  }
},
{
  "cid": 3,
  "aoid": "803912263651156:1721071463148590500",
  "component_type": "Mono_Component",
  "mono_component_type": "MurderProjectile",
  "data": {
    "IsBullet": true
  }
},
{
  "cid": 4,
  "aoid": "818519261751121:1721075631817234800",
  "component_type": "Internal_Component",
  "internal_component_type": "Circle_Collider",
  "data": {
    "size": 0.0799999982118607,
    "offset": {
      "X": 0,
      "Y": 0
    },
    "is_trigger": true,
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
},
{
  "cid": 5,
  "aoid": "818557037922877:1721075642598118100",
  "component_type": "Internal_Component",
  "internal_component_type": "Rigidbody",
  "data": {
    "angular_damping": 0,
    "linear_damping": 0,
    "gravity_scale": 1,
    "fixed_rotation": true
  }
}
