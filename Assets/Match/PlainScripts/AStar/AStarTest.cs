using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AStar;

public class AStarTest : MonoBehaviour
{
    public Transform       _debugMesh = null;

    TiledAStar _aStar = null;
    public int _width = 10;
    public int _height = 10;
    public float _nonAccesibleProb = 0.25f;

    public Vector2 _origin = new Vector2(0,0);
    public Vector2 _destination = new Vector2(2, 2);

    public bool _calculatePathByStep = false;

    public void Awake()
    {
       //  create the astar
        TiledAStar.create(out _aStar, _width, _height);

        // add all the tiles
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {

                float r = Random.Range(0f, 1f);

                Tile tile = null;
                if (r < _nonAccesibleProb)             {
                    tile = new Tile(new ID(x, y), new Vector3(x * 1.3f, 1, y * 1.3f), false, _debugMesh);
                }
                else
                {
                    tile = new Tile(new ID(x, y), new Vector3(x * 1.3f, 1, y * 1.3f), true, _debugMesh);

                    DebugUtils.log("Creating tile");
                }
                
                _aStar.addTile(tile);
            }
        }

        Tile origin = _aStar.getTileByID(new ID(_origin));
        Tile destination = _aStar.getTileByID(new ID(_destination));

        DebugUtils.assert(null != origin, "origin must NOT be null");
        DebugUtils.assert(null != destination, "destination must NOT be null");

        //_aStar.calculatePath(origin, destination);
        //origin.setAsOrigin();
        //destination.setAsDestination();

        _aStar.prepareCalculatePathBySteps(origin, destination);
    }

    public void Update()
    {
        if (_calculatePathByStep)
        {
            _aStar.calculatePathStep();

            Tile origin = _aStar.getTileByID(new ID(_origin));
            Tile destination = _aStar.getTileByID(new ID(_destination));

            origin.setDebugColor(DebugColors.origin);
            destination.setDebugColor(DebugColors.destination);
        }
    }
}

