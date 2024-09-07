13
1288490188801
348293531070407 1725242984247620300
{
  "name": "Watercooler",
  "local_enabled": true,
  "local_position": {
    "X": 14.6482048034667969,
    "Y": 8.1046142578125000
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "previous_sibling": "347559593558489:1725242753970986700",
  "next_sibling": "348492307466089:1725243046614727300",
  "parent": "812070970290054:1709620732295121700",
  "spawn_as_networked_entity": true,
  "linked_prefab": "GarbageCan.prefab"
},
{
  "cid": 1,
  "aoid": "348293531240035:1725242984247673000",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "48x48/Modern_Office_Singles_48x48_173.png",
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
  "aoid": "348293531388419:1725242984247719600",
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
  "aoid": "348293531441559:1725242984247736300",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Grab Some Water",
    "MinimumRoleRequired": 0,
    "MaxRole": 3,
    "ActiveTexture": "48x48/Modern_Office_Singles_48x48_173.png",
    "CooldownTexture": "48x48/Modern_Office_Singles_48x48_173.png",
    "OnCompleteSfx": "sfx/character_eating_loop_end_swallow_short.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 20,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": true,
    "XpReward": 5,
    "CashReward": 0,
    "CashCost": 0
  }
}
