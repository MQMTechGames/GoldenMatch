using System.Collections;
using System.Collections.Generic;

public class BT
{
	public string  _name			= "";

	private BTNode _rootNode 		= null;
	private BTNode _currNode 		= null;
	private BTNode _lastPushedNode 	= null;
	List<BTNodeParallel> _parallelNodesStack = null;

	Dictionary<string, BTNode> _nodes = null;

    public static bool enableDebugLog = false;

	public BTNodeResponse			_lastNodeState;

	public BT()
	{
		_nodes = new Dictionary<string, BTNode> ();
		_lastNodeState = BTNodeResponse.INIT;
		_parallelNodesStack = new List<BTNodeParallel> ();
	}

	public void Update()
	{
		if (   BTNodeResponse.LEAVE == _lastNodeState
		    ) 
		{
			if(_lastPushedNode == _currNode) {
				reset();
			} else {
				_currNode = _currNode._parent;

				// Start recursivity to get back to the first node that can be restored
				// susch as parallel or sequences nodes
				if(_currNode != null){
					_currNode = _currNode.getBacktrackingNode();
				}
			}
		}

		if (   null == _currNode
		    ) {
			setCurrentNode(_rootNode);
		}

        if (enableDebugLog)
        {
            DebugUtils.log("Update current node: " + _currNode._name);
        }

		_lastNodeState = _currNode.Update ();
	}

	public void reset()
	{
		_currNode = null;
		_lastPushedNode = null;
		_parallelNodesStack.Clear();
	}

	public void setRootNode(BTNode node)
	{
		bool existNode = getExistNode (node._name);
		DebugUtils.assert (true == existNode, "[BT->setRootNode]: node does not exist");

		_rootNode = node;
	}

	public bool pushNodeByName(string name)
	{
		BTNode node;
		bool existNode = _nodes.TryGetValue (name, out node);

		if (!existNode) {
			return false;
		}

		pushNode (node);
		return true;
	}

	public void pushNode(BTNode node)
	{
		reset ();

		_lastPushedNode = _currNode;
		setCurrentNode (node);
	}

	public void setCurrentNode(BTNode node)
	{
        if (enableDebugLog)
        {
            DebugUtils.log("Set current node: " + node._name);
        }

		int nParallelNodes = _parallelNodesStack.Count;
		if (nParallelNodes > 0) {
			_parallelNodesStack [nParallelNodes - 1].setCurrentNode (node);
		} else {
			_currNode = node;
		}

		_currNode.onStart ();
	}

	public void registerNode(BTNode node)
	{
		_nodes.Add (node._name, node);
	}

	public void registerParallelNode(BTNodeParallel node)
	{
		_parallelNodesStack.Add (node);
	}

	public void unregisterParallelNode(BTNodeParallel node)
	{
		_parallelNodesStack.Remove (node);
	}

	public BTNode getNodeByName(string name)
	{
		DebugUtils.assert (true == getExistNode(name), "[BT->getnodeByName]: node does not exist!");

		return _nodes[name];
	}

	private bool getExistNode(string name)
	{
		return _nodes.ContainsKey (name);
	}
}
