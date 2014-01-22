using System;
using System.Collections.Generic;

namespace TeamData
{
    using PlayerActionList = List<ActionData>;

    public enum ActionId
    {
        DO_RECOVER_POSSESION = 0,
        IS_RECOVERING_POSESSION
    }

    public class ActionData
    {
        public int _id;

        public ActionData(int id)
        {
            _id = id;
        }
    }

    public class Blackboard
    {
        Dictionary<ActionId, List<ActionData>> _actions = new Dictionary<ActionId, PlayerActionList>();

        public bool addAction(ActionId action, int playerId)
        {
            return addAction(action, new ActionData(playerId));
        }

        public bool addAction(ActionId action, ActionData data)
        {
            DebugUtils.assert(null!=data, "data must NOT be NULL");

            PlayerActionList players = null;

            bool exist = _actions.TryGetValue(action, out players);

            if (!exist)
            {
                players = new PlayerActionList();
                _actions.Add(action, players);
            }

            // check if exist
            if (false == players.Exists(element => element._id == data._id))
            {
                players.Add(data);

                return true;
            }

            return false;
        }

        public bool isPlayerAssignedTo(ActionId action, int id, bool remove = false)
        {
            PlayerActionList actions = null;
            bool exist = getPlayersAssignedTo(action, out actions);
            
            if(false == exist)
            {
                return false;
            }

            ActionData data = actions.Find(element => element._id == id);

            if(null == data)
            {
                return false;
            }

            if(remove)
            {
                actions.Remove(data);
            }

            return true;
        }

        public bool getPlayerAssignedTo(ActionId action, ref int id, bool remove = false)
        {
            ActionData data = null;

            if (getPlayerAssignedTo(action, out data, remove))
            {
                id = data._id;

                return true;
            }

            return false;
        }

        public bool getPlayerAssignedTo(ActionId action, out ActionData data, bool remove = false)
        {
            PlayerActionList allActions = null;

            bool exist = _actions.TryGetValue(action, out allActions);

            data = allActions[0];

            if(remove)
            {
                allActions.RemoveAt(0);
            }

            return exist;
        }

        public bool getPlayersAssignedTo(ActionId action, out PlayerActionList actions)
        {
            bool exist = _actions.TryGetValue(action, out actions);

            return exist;
        }

        public int getNumPlayersAssignedTo(ActionId action)
        {
            PlayerActionList allActions = null;

            bool exist = _actions.TryGetValue(action, out allActions);

            return exist ? allActions.Count : 0;
        }

        // Remove single player form action
        public void removePlayerFromAction(int id, ActionId action)
        {
            PlayerActionList players;
            if (getPlayersAssignedTo(action, out players))
            {
                ActionData playerAction = players.Find(element => element._id == id);
                if (null != playerAction)
                {
                    players.Remove(playerAction);
                }
            }
        }

        // Remove all players from action
        public void resetPlayersDoingAction(ActionId action)
        {
            PlayerActionList players;
            if (getPlayersAssignedTo(action, out players))
            {
                players.Clear();
            }
        }

        public void resetAll()
        {
            foreach (KeyValuePair<ActionId,PlayerActionList> pair in _actions)
            {
                pair.Value.Clear();
            }

            _actions.Clear();
        }
    }
}
