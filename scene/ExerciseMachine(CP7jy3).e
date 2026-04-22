14
240518168578
2414754999
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
  "previous_sibling": 2564003666,
  "parent": 3660471946,
  "spawn_as_networked_entity": true,
  "linked_prefab": "ExerciseMachine.prefab"
},
{
  "cid": 1,
  "aoid": 1869710222,
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "8_Gym_Singles_48x48/Gym_Singles_48x48_87.png"
  }
},
{
  "cid": 2,
  "aoid": 815092427,
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
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
    ]
  }
},
{
  "cid": 3,
  "aoid": 1185945132,
  "component_type": "Mono_Component",
  "mono_component_type": "Activity",
  "data": {
    "PromptText": "Lift Weights",
    "MaxRole": 3,
    "ActiveTexture": "8_Gym_Singles_48x48/Gym_Singles_48x48_87.png",
    "OnCompleteSfx": "sfx/gym.wav",
    "CooldownSeconds": 30,
    "SpawnsDuringDay": true,
    "XpReward": 12
  }
}
