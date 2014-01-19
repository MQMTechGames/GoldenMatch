using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    public delegate float CostCalculatorCallback(Tile origin, Tile dest);
    public delegate float HeuristicCalculatorCallback(Tile current);

    public class TiledAStar
    {
        AStar.Tile[,] _tiles = null;

        public int _wight, _height;

        Transform _debugMesh = null;

        List<Tile> _openList = null;   // nodes that we have to explore
        List<Tile> _closedList = null; // nodes that have been already explored
        List<Tile> _path = null; // nodes that have been already explored

        Tile _origin = null;
        Tile _destination = null;
        List<Tile> _adjacents = null;

        CostCalculatorCallback      _costCalculatorCallback = null;
        HeuristicCalculatorCallback _heuristicCalculatorCallback = null;

        public static void create(out TiledAStar tiledAStar, int width, int height)
        {
            tiledAStar = new TiledAStar(width, height);
            tiledAStar.init();
        }

        TiledAStar(int width, int height)
        {
            _openList = new List<Tile>();
            _closedList = new List<Tile>();
            _adjacents = new List<Tile>();
            _path = new List<Tile>();

            _wight = width;
            _height = height;

            _tiles = new Tile[_wight, _height];
        }

        void init()
        {
            _costCalculatorCallback = defaultCostCalculatorCallback;
            _heuristicCalculatorCallback = defaultHeuristicCalculatorCallback;
        }

        public void setDebugTileMesh(Transform debugMesh)
        {
            _debugMesh = debugMesh;
        }

        public Tile addTile(AStar.Tile tile)
        {
            _tiles[tile._id._x, tile._id._y] = tile;

            return _tiles[tile._id._x, tile._id._y];
        }

        public Tile getTileByID(AStar.ID id)
        {
            return _tiles[id._x, id._y];
        }

        public TiledAStar setOrigin(Tile origin)
        {
            _origin = origin;

            return this;
        }

        public TiledAStar setDestination(Tile destination)
        {
            _destination = destination;

            return this;
        }

        // Perform AStar
        public void calculatePath(Tile origin, Tile destination)
        {
            _origin = origin;
            _origin._isAccesible = true;
            _origin.setDebugColor(DebugColors.origin);

            _destination = destination;
            _destination._isAccesible = true;
            _destination.setDebugColor(DebugColors.destination);

            clearTemporalData();

            _openList.Add(_origin);

            List<Tile> adjacents = new List<Tile>();
            while (_openList.Count > 0)
            {
                Tile current = _openList[0];
                
                _closedList.Add(current);

                // debug
                current.setDebugColor(DebugColors.closestListTile);

                _openList.Remove(current);

                if (_destination == current)
                {
                    reconstructPath();
                    
                    return;
                }

                adjacents.Clear();
                getAdjacentTiles(adjacents, current);

                // process adjacent tiles
                processAdjacentTiles(adjacents, current);
            }

            DebugUtils.log("No path found");
        }

        public void prepareCalculatePathBySteps(Tile origin, Tile destination)
        {
            _origin = origin;
            _origin._isAccesible = true;
            _origin.setDebugColor(DebugColors.origin);

            _destination = destination;
            _destination._isAccesible = true;
            _destination.setDebugColor(DebugColors.destination);

            clearTemporalData();

            _openList.Add(_origin);
        }

        public void calculatePathStep()
        {
            if (_openList.Count > 0)
            {
                Tile current = _openList[0];

                _closedList.Add(current);

                // debug
                current.setDebugColor(DebugColors.closestListTile);

                _openList.Remove(current);

                if (_destination == current)
                {
                    reconstructPath();

                    return;
                }

                _adjacents.Clear();
                getAdjacentTiles(_adjacents, current);

                // process adjacent tiles
                processAdjacentTiles(_adjacents, current);
            }
        }

        void clearTemporalData()
        {
            _openList.Clear();
            _closedList.Clear();
            _path.Clear();
        }

        void reconstructPath()
        {
            Tile current = _destination;

            while (current != null)
            {
                current.setDebugColor(DebugColors.activeTile);
                DebugUtils.log("reconstruction of path");

                _path.Add(current);

                current = current._previous;
            }
        }

        void processAdjacentTiles(List<Tile> adjacents, Tile current)
        {
            foreach (Tile adjacent in adjacents)
            {
                bool isInClosedList = getIsTileInClosedList(adjacent);
                
                // Discart if in closed list
                if (isInClosedList)
                {
                    continue;
                }

                // Discart if is not accesible
                if(false == adjacent._isAccesible)
                {
                    continue;
                }

                // Calculate dx and fx
                float dx = current._dx + _costCalculatorCallback(adjacent, current);
                float fx = dx + _heuristicCalculatorCallback(adjacent);

                bool isInOpenList = getIsTileInOpenList(adjacent);
                
                // Discart if already in open List and fx is greater
                if (isInOpenList && fx >= adjacent._fx)
                {
                    continue;
                }

                adjacent._fx = fx;
                adjacent._dx = dx;
                adjacent._previous = current;

                if (false == isInOpenList)
                {
                    addTileToOpenList(adjacent);

                    // debug
                    adjacent.setDebugColor(DebugColors.openListTile);
                }
            }
        }

        void addTileToOpenList(Tile tile)
        {
            bool inserted = false;

            Tile prevTile = null;
            int   prevFxPos = -1;
            float newFX = tile._fx;

            // add ordered
            for (int i = 0; i < _openList.Count; i++)
            {
                Tile aTile = _openList[i];
                if (aTile._fx > tile._fx)
                {
                    _openList.Insert(i, tile);
                    inserted = true;
                 
                    // for debugging
                    prevTile = aTile;
                    prevFxPos = i;
   
                    break;
                }
            }

            if (false == inserted)
            {
                _openList.Add(tile);
            }

            if (null!=prevTile)
            {
                DebugUtils.assert(prevTile == _openList[prevFxPos +1], "Error, Previous tails is not in prev place +1");

                if (prevFxPos > 0)
                {
                    DebugUtils.log("PrevFx: " + _openList[prevFxPos - 1]._fx);
                }

                DebugUtils.log("CurrNewFx: " + _openList[prevFxPos]._fx + ", equals to: " + tile._fx);

                if (prevFxPos+1 < _openList.Count)
                {
                    DebugUtils.log("PosFX: " + _openList[prevFxPos +1]._fx);
                }
            }
            
        }


        bool getIsTileInClosedList(Tile tile)
        {
            bool found = _closedList.Exists(
                delegate(Tile t)
                {
                    return t._id._x == tile._id._x
                        && t._id._y == tile._id._y;
                }
                );

            return found;
        }

        bool getIsTileInOpenList(Tile tile)
        {
            bool found = _openList.Exists(
                delegate(Tile t)
                {
                    return t._id._x == tile._id._x
                        && t._id._y == tile._id._y;
                }
                );

            return found;
        }

        void getAdjacentTiles(List<Tile> adjacents, Tile tile)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    // if tile is out of range, discart it
                    ID id = new ID(tile._id._x + x, tile._id._y + y);
                    bool isInsideMap = getIsTileInsideMapById(id);

                    if (!isInsideMap)
                    {
                        continue;
                    }

                    Tile adjacent = getTileByID(id);
                    DebugUtils.assert(null != adjacent, "[TiledAStar->getAdjacentTiles]: tile must NOT be null");

                    adjacents.Add(adjacent);
                }
            }
        }

        bool getIsTileInsideMapById(ID id)
        {
            bool isInsideMap =
                id._x >= 0
                && id._x < _wight
                && id._y >= 0
                && id._y < _height
                ;

            return isInsideMap;
        }

        // Callback setters
        public void setCostCalculatorCallback(CostCalculatorCallback callback)
        {
            _costCalculatorCallback = callback;
        }

        public void setHeuristicCalculatorCallback(HeuristicCalculatorCallback callback)
        {
            _heuristicCalculatorCallback = callback;
        }

        // Default callbacks
        float defaultCostCalculatorCallback(Tile a, Tile b)
        {
            Vector2 dir = new Vector2(b._id._x, b._id._y) - new Vector2(a._id._x, a._id._y);

            return dir.magnitude;
        }

        float defaultHeuristicCalculatorCallback(Tile current)
        {
            Vector2 dir = new Vector2(_destination._id._x, _destination._id._y) - new Vector2(current._id._x, current._id._y);

            return dir.magnitude;
        }
    }
}
