using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    public class ID
    {
        public int _x;
        public int _y;

        public ID(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public ID(Vector2 xy)
        {
            _x = (int)xy.x;
            _y = (int)xy.y;
        }
    }

    public class DebugColors
    {
        public static Color accesible = Color.white;
        public static Color nonAccesible = Color.black;
        public static Color origin = Color.yellow;
        public static Color destination = Color.red;
        public static Color activeTile = Color.green;

        public static Color openListTile = Color.cyan;
        public static Color closestListTile = Color.grey;
    }

    public class Tile
    {
        public ID       _id;
        public Vector3  _pos;
        public bool     _isAccesible;

        public float _dx = 0;
        public float _fx = 0;

        public Tile _previous = null;

        Transform   _debugMesh = null;
        Transform   _instanceMesh = null;

        public Tile(ID id, Vector3 pos, bool isAccesible, Transform debugMesh = null)
        {
            _id = id;
            _pos = pos;
            _isAccesible = isAccesible;

            _debugMesh = debugMesh;

            instantiateDebugMesh();
        }

        void instantiateDebugMesh()
        {
            if (_debugMesh)
            {
                DebugUtils.log("creating new mesh");

                _instanceMesh = GameObject.Instantiate(_debugMesh, _pos, Quaternion.identity) as Transform;

                if (_isAccesible)
                {
                    setDebugColor(DebugColors.accesible);
                }
                else
                {
                    setDebugColor(DebugColors.nonAccesible);
                }
            }
        }

        public void setDebugColor(Color color)
        {
            _instanceMesh.renderer.material.color = color;
        }
    }
}
