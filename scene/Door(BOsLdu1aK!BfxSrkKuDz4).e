13
841813590017
346083594819210 1725242290868862200
{
  "name": "Door",
  "local_enabled": true,
  "local_position": {
    "X": -26.3604164123535156,
    "Y": -0.9272033572196960
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 6.8400001525878906,
    "Y": 3.4500000476837158
  },
  "previous_sibling": "346083594852060:1725242290868872500",
  "next_sibling": "346083594788411:1725242290868852500",
  "parent": "346083594533204:1725242290868773000",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "346083595204257:1725242290868983000",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Condominium_Singles_48x48_82.png",
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
  "aoid": "346083595261811:1725242290869001000",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": false,
    "is_loop": false,
    "points": [
      {
        "X": 0.1882953643798828,
        "Y": -0.1375435590744019
      },
      {
        "X": -0.2494535446166992,
        "Y": -0.1396120786666870
      },
      {
        "X": -0.2546467781066895,
        "Y": 0.2395392656326294
      },
      {
        "X": -0.1999495029449463,
        "Y": 0.2484735250473022
      },
      {
        "X": -0.1839163303375244,
        "Y": 0.1852042675018311
      },
      {
        "X": 0.1327321529388428,
        "Y": 0.1880619525909424
      },
      {
        "X": 0.1486492156982422,
        "Y": 0.2379815578460693
      },
      {
        "X": 0.1894481182098389,
        "Y": 0.2314314842224121
      },
      {
        "X": 0.1883466243743896,
        "Y": -0.1419104337692261
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
  "aoid": "346083595310127:1725242290869016200",
  "component_type": "Mono_Component",
  "mono_component_type": "OfficeDoor",
  "data": {
    "Inside": "346083594758079:1725242290868843000",
    "Outside": "346083594788411:1725242290868852500",
    "RoomName": 14
  }
}