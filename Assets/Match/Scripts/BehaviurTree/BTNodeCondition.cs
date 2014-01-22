using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTNodeCondition : BTNode
{
	BTNode  		_node = null;
	BTCondition _condition = null;

	bool 			_init = false;
	
	public BTNodeCondition(BT tree, string name, BTCondition condition)
		:base(tree, name, BTNodeType.BTNODE_CONDITION)
	{
		_condition = condition;
	}

	public void addNode(BTNode node)
	{
		_node = node;
		node._parent = this;
	}
	
	public override void onStart ()
	{
		_init = false;
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
		if (true == _init) {
			return BTNodeResponse.LEAVE;
		}
		_init = true;

		bool passed = true;
		if (null != _condition) {
			passed = _condition ();
		}
		 
		if (passed) {
			_tree.setCurrentNode(_node);

			return BTNodeResponse.STAY;
		}

		return BTNodeResponse.LEAVE;
	}
}
