13
1176821039105
349123536157231 1725243244665944200
{
  "name": "Door",
  "local_enabled": true,
  "local_position": {
    "X": -17.2941856384277344,
    "Y": -0.9272033572196960
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 6.8400001525878906,
    "Y": 3.4500000476837158
  },
  "previous_sibling": "349123536188218:1725243244665954100",
  "next_sibling": "349123536124523:1725243244665934100",
  "parent": "349123535230068:1725243244665654000",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "349123537688561:1725243244666424800",
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
  "aoid": "349123537749819:1725243244666443900",
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
  "aoid": "349123537783434:1725243244666454500",
  "component_type": "Mono_Component",
  "mono_component_type": "OfficeDoor",
  "data": {
    "Inside": "349123536093449:1725243244665924400",
    "Outside": "349123536124523:1725243244665934100",
    "RoomName": 18
  }
}
