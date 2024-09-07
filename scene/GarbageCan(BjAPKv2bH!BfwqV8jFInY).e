13
773094113281
435422891042503 1725064883358435800
{
  "name": "GarbageCan",
  "local_enabled": true,
  "local_position": {
    "X": 22.6325244903564453,
    "Y": -14.6980609893798828
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "previous_sibling": "435406796423896:1725064878308663000",
  "next_sibling": "435459973762249:1725064894993337700",
  "parent": "812070970290054:1709620732295121700",
  "spawn_as_networked_entity": true,
  "linked_prefab": "GarbageCan.prefab"
},
{
  "cid": 1,
  "aoid": "435422891149241:1725064883358468800",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "48x48/Modern_Office_Singles_48x48_169.png",
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
  "aoid": "435422891250973:1725064883358500700",
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
        "X": -0.1648421287536621,
        "Y": 0.0006518363952637
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
  "aoid": "435422891299702:1725064883358516000",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Take Out The Trash",
    "MinimumRoleRequired": 0,
    "MaxRole": 0,
    "ActiveTexture": "48x48/Modern_Office_Singles_48x48_169.png",
    "CooldownTexture": "48x48/Modern_Office_Singles_48x48_167.png",
    "OnCompleteSfx": "sfx/paper.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 20,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": true,
    "XpReward": 8,
    "CashReward": 0,
    "CashCost": 0
  }
}
