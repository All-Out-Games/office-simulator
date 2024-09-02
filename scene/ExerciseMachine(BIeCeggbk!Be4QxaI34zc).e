13
236223201282
318723592423140 1709189822651796700
{
  "name": "ExerciseMachine",
  "local_enabled": true,
  "local_position": {
    "X": -62.0517120361328125,
    "Y": 14.7896299362182617
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "next_sibling": "14292806585087:1725138189818712500",
  "parent": "19843171803632:1717370276095583100",
  "spawn_as_networked_entity": true,
  "linked_prefab": "ExerciseMachine.prefab"
},
{
  "cid": 1,
  "aoid": "318723593506667:1709189822652136600",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "8_Gym_Singles_48x48/Gym_Singles_48x48_97.png",
    "depth_offset": 0.1988918781280518,
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
  "aoid": "318723593666618:1709189822652186100",
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
  "aoid": "8811140802381:1725136469918146400",
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Workout Arms",
    "MinimumRoleRequired": 0,
    "MaxRole": 3,
    "ActiveTexture": "8_Gym_Singles_48x48/Gym_Singles_48x48_97.png",
    "CooldownTexture": "",
    "OnCompleteSfx": "sfx/gym-weight.wav",
    "OnActiveSfx": "",
    "OnCooldownSfx": "",
    "CooldownSeconds": 15,
    "AvailableForSeconds": 0,
    "HideWhenOnCooldown": false,
    "SpawnsDuringDay": true,
    "SpawnsDuringNight": false,
    "XpReward": 6,
    "CashReward": 0,
    "CashCost": 5
  }
}
