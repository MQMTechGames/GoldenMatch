using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTNodeParallelData
{
	public BTNodeResponse   _lastNodeResponse;
	public BTNode 			_node = null;
	
	public BTNodeParallelData(BTNode node)
	{
		_node = node;

		init ();
	}

	public void init()
	{
		_lastNodeResponse = BTNodeResponse.INIT;
	}
}

public class BTNodeParallel : BTNode
{
	List<BTNodeParallelData> 	 _nodes	= null;

	List<BTNodeParallelData> 	 _currNodes	= null;
	int							 _currNode;
	
	public BTNodeParallel(BT tree, string name)
		:base(tree, name, BTNodeType.BTNODE_PARALLEL)
	{
		_nodes = new List<BTNodeParallelData> ();

		_currNodes = new List<BTNodeParallelData> ();
		_currNode = -1;
	}
	
	public void addNode(BTNode node)
	{
		node._parent = this;

		BTNodeParallelData data = new BTNodeParallelData (node);
		_nodes.Add (data);
	}

	public override void onStart ()
	{
		_currNodes.Clear();
		_currNode = -1;

		foreach (BTNodeParallelData nodeData in _nodes)
		{
			_currNodes.Add(nodeData);
		}

		foreach (BTNodeParallelData nodeData in _currNodes)
		{
			nodeData.init();
			nodeData._node.onStart();
		}
	}

	public override BTNode getBacktrackingNode()
	{
		return this;
	}

	// Method called from the tree when updating parallel nodes
	// and some child is calling the _tree.setCurrentNode
	public void setCurrentNode(BTNode node)
	{
		DebugUtils.assert (_currNode >= 0 && _currNode < _nodes.Count, "[PlayerController] hash must not be null");
		_currNodes [_currNode]._node = node;
	}

	public override BTNodeResponse Update ()
	{
		bool someNodeUpdated = false;

		_tree.registerParallelNode(this);

		_currNode = -1;
		foreach (BTNodeParallelData nodeData in _currNodes)
		{
			_currNode++;

			if(BTNodeResponse.LEAVE == nodeData._lastNodeResponse)
			{
				continue;
			} else {
				nodeData._lastNodeResponse = nodeData._node.Update();
				someNodeUpdated = true;
			}
		}
		_tree.unregisterParallelNode(this);

		if (false == someNodeUpdated) {

			return BTNodeResponse.LEAVE;
		}

		return BTNodeResponse.STAY;
	}
}
