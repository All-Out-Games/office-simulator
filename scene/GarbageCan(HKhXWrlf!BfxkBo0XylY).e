13
1614907703297
31509345646943 1725318574481811800
{
  "name": "GarbageCan",
  "local_enabled": true,
  "local_position": {
    "X": -30.0083045959472656,
    "Y": 20.6611251831054688
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 5,
    "Y": 5
  },
  "previous_sibling": "31498721433948:1725318571148410700",
  "next_sibling": "31515718817029:1725318576481426500",
  "parent": "812070970290054:1709620732295121700",
  "spawn_as_networked_entity": true,
  "linked_prefab": "GarbageCan.prefab"
},
{
  "cid": 1,
  "aoid": "31509345766299:1725318574481848900",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "48x48/Modern_Office_Singles_48x48_99.png",
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
  "cid": 4,
  "aoid": "31509345868565:1725318574481881100",
  "component_type": "Internal_Component",
  "internal_component_type": "Polygon_Collider",
  "data": {
    "is_trigger": false,
    "points": [
      {
        "X": -0.1634704023599625,
        "Y": -0.1635914295911789
      },
      {
        "X": -0.1648421436548233,
        "Y": 0.0006519854650833
      },
      {
        "X": -0.0348625220358372,
        "Y": 0.0011680425377563
      },
      {
        "X": -0.0365302599966526,
        "Y": -0.1626369953155518
      }
    ],
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
},
{
  "cid": 2,
  "aoid": "31509345918567:1725318574481896700",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Water Plants",
    "MinimumRoleRequired": 0,
    "MaxRole": 3,
    "ActiveTexture": "48x48/Modern_Office_Singles_48x48_99.png",
    "CooldownTexture": "48x48/Modern_Office_Singles_48x48_99.png",
    "OnCompleteSfx": "sfx/paper.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 20,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": true,
    "XpReward": 2,
    "CashReward": 0,
    "CashCost": 0
  }
}
