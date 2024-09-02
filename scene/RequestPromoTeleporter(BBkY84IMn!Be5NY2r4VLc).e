13
47244640257
288373715534631 1709456416154145500
{
  "name": "RequestPromoTeleporter",
  "local_enabled": true,
  "local_position": {
    "X": -45.2506484985351562,
    "Y": 1.5748260021209717
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 6,
    "Y": 6
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
}
