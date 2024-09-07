13
1550483193857
32041718141905 1725318741516408400
{
  "name": "GarbageCan",
  "local_enabled": true,
  "local_position": {
    "X": -7.5275411605834961,
    "Y": -11.0667619705200195
  },
  "local_rotation": 0,
  "local_scale": {
    "X": -5,
    "Y": 5
  },
  "previous_sibling": "32021063312875:1725318735035850200",
  "next_sibling": "32147479885771:1725318774699697100",
  "parent": "812070970290054:1709620732295121700",
  "spawn_as_networked_entity": true,
  "linked_prefab": "GarbageCan.prefab"
},
{
  "cid": 1,
  "aoid": "32041718255629:1725318741516443500",
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
  "aoid": "32041718367823:1725318741516478700",
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
  "aoid": "32041718417819:1725318741516494400",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Refill Fire Extinguisher",
    "MinimumRoleRequired": 0,
    "MaxRole": 1,
    "ActiveTexture": "Grocery/Conference_Hall_Singles_48x48_59.png",
    "CooldownTexture": "Grocery/Conference_Hall_Singles_48x48_59.png",
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
