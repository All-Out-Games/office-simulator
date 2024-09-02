13
47244640257
288373715534631 1709456416154145500
{
  "name": "RequestPromoTeleporter",
  "local_enabled": true,
  "local_position": {
    "X": -45.2567214965820312,
    "Y": 2.0836534500122070
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 8,
    "Y": 8
  },
  "previous_sibling": "287660153059416:1709456192270255000",
  "parent": "21563711469729:1717370815923742600",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "288373715640391:1709456416154177700",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Clothing_Store_Singles_48x48_469.png",
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
  "aoid": "288373715730219:1709456416154205800",
  "component_type": "Internal_Component",
  "internal_component_type": "Interactable",
  "data": {
    "prompt_offset": {
      "X": 0,
      "Y": 1
    },
    "text": "Request a Promotion",
    "hold_text": "",
    "radius": 2,
    "required_hold_time": 0.6000000238418579
  }
},
{
  "cid": 3,
  "aoid": "437507250408632:1725065537337339100",
  "component_type": "Mono_Component",
  "mono_component_type": "Teleporter",
  "data": {
    "Target": "287660153059416:1709456192270255000"
  }
},
{
  "cid": 4,
  "aoid": "381271494237656:1725253331251573500",
  "component_type": "Mono_Component",
  "mono_component_type": "SpriteFlasher",
  "data": {

  }
},
{
  "cid": 5,
  "aoid": "556674514228786:1725308369118306300",
  "component_type": "Internal_Component",
  "internal_component_type": "Box_Collider",
  "data": {
    "size": {
      "X": 0.1535682678222656,
      "Y": 0.2678102552890778
    },
    "offset": {
      "X": -0.0025963820517063,
      "Y": 0.0566019155085087
    },
    "is_trigger": false,
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
}
