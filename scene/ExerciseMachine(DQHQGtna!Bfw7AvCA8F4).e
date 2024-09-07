13
240518168578
14301437549018 1725138192526721400
{
  "name": "ExerciseMachine",
  "local_enabled": true,
  "local_position": {
    "X": -58.4650878906250000,
    "Y": 14.5263671875000000
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "previous_sibling": "14292806585087:1725138189818712500",
  "parent": "19843171803632:1717370276095583100",
  "spawn_as_networked_entity": true,
  "linked_prefab": "ExerciseMachine.prefab"
},
{
  "cid": 1,
  "aoid": "14301437669165:1725138192526758600",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "8_Gym_Singles_48x48/Gym_Singles_48x48_87.png",
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
  "aoid": "14301437775221:1725138192526791900",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": false,
    "is_loop": true,
    "points": [
      {
        "X": -0.1751070022583008,
        "Y": -0.0567789077758789
      },
      {
        "X": -0.1751251220703125,
        "Y": 0.0459046363830566
      },
      {
        "X": 0.2038393020629883,
        "Y": 0.0341262817382812
      },
      {
        "X": 0.1792058944702148,
        "Y": -0.0772607326507568
      }
    ],
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
},
{
  "cid": 3,
  "aoid": "14301437823519:1725138192526807000",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Lift Weights",
    "MinimumRoleRequired": 0,
    "MaxRole": 3,
    "ActiveTexture": "8_Gym_Singles_48x48/Gym_Singles_48x48_87.png",
    "CooldownTexture": "",
    "OnCompleteSfx": "sfx/gym.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 30,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": false,
    "XpReward": 12,
    "CashReward": 0,
    "CashCost": 0
  }
}
