13
283467841537
444878404265022 1709505520303664100
{
  "name": "PromoNPC",
  "local_enabled": true,
  "local_position": {
    "X": 9.5984373092651367,
    "Y": -46.6185226440429688
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1,
    "Y": 1
  },
  "previous_sibling": "864426264137567:1708507750266046000",
  "next_sibling": "501783085026816:1709523374439378600",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "444878404423198:1709505520303713300",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "48x48/Modern_Office_Singles_48x48_90.png",
    "depth_offset": 0,
    "tint": {
      "X": 1,
      "Y": 1,
      "Z": 1,
      "W": 1
    },
    "layer": -3,
    "wait_for_load": false,
    "wrap": false,
    "mask_in_shadow": false
  }
},
{
  "cid": 2,
  "aoid": "497198806867725:1725084267544203700",
  "component_type": "Mono_Component",
  "mono_component_type": "PromoNPC",
  "data": {
    "candidateSeat1": "95544766965045:1725163683035228100",
    "candidateSeat2": "95565775158631:1725163689626655800"
  }
},
{
  "cid": 3,
  "aoid": "28430631260789:1725142625633231300",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": false,
    "is_loop": true,
    "points": [
      {
        "X": -0.5162802338600159,
        "Y": -0.5723648667335510
      },
      {
        "X": -0.4952488243579865,
        "Y": 0.7931862473487854
      },
      {
        "X": 0.3795767128467560,
        "Y": 0.8282395005226135
      },
      {
        "X": 0.4391651451587677,
        "Y": -0.5828896164894104
      }
    ],
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
}
