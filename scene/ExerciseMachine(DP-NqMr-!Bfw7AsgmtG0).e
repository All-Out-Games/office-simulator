13
244813135874
14292806585087 1725138189818712500
{
  "name": "ExerciseMachine",
  "local_enabled": true,
  "local_position": {
    "X": -55.0568161010742188,
    "Y": 14.7975416183471680
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "previous_sibling": "318723592423140:1709189822651796700",
  "next_sibling": "14301437549018:1725138192526721400",
  "parent": "19843171803632:1717370276095583100",
  "spawn_as_networked_entity": true,
  "linked_prefab": "ExerciseMachine.prefab"
},
{
  "cid": 1,
  "aoid": "14292806761128:1725138189818767300",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "8_Gym_Singles_48x48/Gym_Singles_48x48_95.png",
    "depth_offset": 0.2108933925628662,
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
  "aoid": "14292806885254:1725138189818806200",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": false,
    "is_loop": true,
    "points": [
      {
        "X": -0.0742197036743164,
        "Y": -0.1972305774688721
      },
      {
        "X": -0.1355609893798828,
        "Y": 0.2417461872100830
      },
      {
        "X": 0.1741666793823242,
        "Y": 0.2002949714660645
      },
      {
        "X": 0.1218376159667969,
        "Y": -0.2117776870727539
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
  "aoid": "14292806939362:1725138189818823200",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Go for a Run",
    "MinimumRoleRequired": 0,
    "MaxRole": 3,
    "ActiveTexture": "8_Gym_Singles_48x48/Gym_Singles_48x48_95.png",
    "CooldownTexture": "",
    "OnCompleteSfx": "sfx/gym-weight.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 45,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": false,
    "XpReward": 6,
    "CashReward": 0,
    "CashCost": 5
  }
}
