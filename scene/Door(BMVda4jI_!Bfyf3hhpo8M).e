13
1434519076865
335726233399870 1725581762247429900
{
  "name": "Door",
  "local_enabled": true,
  "local_position": {
    "X": -26.4316558837890625,
    "Y": -0.9272033572196960
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 9.1520376205444336,
    "Y": 4.3205676078796387
  },
  "previous_sibling": "335726233432993:1725581762247440200",
  "next_sibling": "335726233366844:1725581762247419500",
  "parent": "335726232875950:1725581762247266000",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "335726234422653:1725581762247750700",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Condominium_Singles_48x48_82.png",
    "depth_offset": 0,
    "tint": {
      "X": 0.6000000238418579,
      "Y": 0.7098039388656616,
      "Z": 0.7019608020782471,
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
  "aoid": "335726234482621:1725581762247769500",
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
  "aoid": "335726234515637:1725581762247779900",
  "component_type": "Mono_Component",
  "mono_component_type": "OfficeDoor",
  "data": {
    "Inside": "335726233335859:1725581762247409800",
    "Outside": "335726233366844:1725581762247419500",
    "RoomName": 14
  }
}
