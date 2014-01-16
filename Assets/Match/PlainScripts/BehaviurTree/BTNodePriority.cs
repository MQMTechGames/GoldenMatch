using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTPriorityCondition
{
	public BTCondition  _condition = null;
	public BTNode 			_node = null;

	public BTPriorityCondition(BTCondition condition, BTNode node)
	{
		_condition = condition;
		_node = node;
	}
}

public class BTNodePriority : BTNode
{
	List<BTPriorityCondition> 	 _nodes			= null;
	private bool				_isInit = false;
	
	public BTNodePriority(BT tree, string name)
		:base(tree, name, BTNodeType.BTNODE_PRIORITY)
	{
		_nodes = new List<BTPriorityCondition> ();
	}
	
	public void addNode(BTNode node, BTCondition condition)
	{
		node._parent = this;

		BTPriorityCondition priotityNode = new BTPriorityCondition (condition, node);
		_nodes.Add (priotityNode);
	}
	
	public override void onStart ()
	{
		_isInit = false;
	}

	public override BTNode getBacktrackingNode()
	{
		if (null != _parent) {
			return _parent.getBacktrackingNode();
		}

		return this;
	}
	
	public override BTNodeResponse Update ()
	{
		if (_isInit) {
			return BTNodeResponse.LEAVE;
		}

		_isInit = true;

		int tempCount = -1;
		foreach (BTPriorityCondition nodePriority in _nodes)
		{
			tempCount++;

			// null in condition callback will be interpreted as true by default
			if(null == nodePriority._condition)
			{
				_tree.setCurrentNode(nodePriority._node);

				return BTNodeResponse.STAY;
			}

			// run  the condition
			bool resCondition = nodePriority._condition();

			if(resCondition)
			{
				// Update current node
				_tree.setCurrentNode(nodePriority._node);

				return BTNodeResponse.STAY;
			}
		}

		// if no condition is succesful then leave
		return BTNodeResponse.LEAVE;
	}
}
