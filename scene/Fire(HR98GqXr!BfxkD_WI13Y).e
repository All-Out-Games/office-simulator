13
1554778161153
32021063312875 1725318735035850200
{
  "name": "Fire",
  "local_enabled": true,
  "local_position": {
    "X": -56.1663208007812500,
    "Y": -13.8796119689941406
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 5,
    "Y": 5
  },
  "previous_sibling": "31907190661719:1725318699307722500",
  "next_sibling": "32041718141905:1725318741516408400",
  "parent": "812070970290054:1709620732295121700",
  "spawn_as_networked_entity": true,
  "linked_prefab": "GarbageCan.prefab"
},
{
  "cid": 1,
  "aoid": "32021063438535:1725318735035888900",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Grocery/Conference_Hall_Singles_48x48_59.png",
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
  "aoid": "32021063575934:1725318735035932100",
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
  "aoid": "32021063630486:1725318735035949100",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Refill Fire Extinguisher",
    "MinimumRoleRequired": 0,
    "MaxRole": 1,
    "ActiveTexture": "48x48/Modern_Office_Singles_48x48_168.png",
    "CooldownTexture": "48x48/Modern_Office_Singles_48x48_167.png",
    "OnCompleteSfx": "sfx/paper.wav",
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
