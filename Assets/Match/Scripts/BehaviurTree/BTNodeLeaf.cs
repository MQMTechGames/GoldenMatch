using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTNodeLeaf : BTNode
{
	BTCallback _callback;

	public BTNodeLeaf(BT tree, string name, BTCallback callback)
		:base(tree, name, BTNodeType.BTNODE_LEAF)
	{
		_callback = callback;
	}

	public override void onStart ()
	{
	}
	
	public override BTNodeResponse Update ()
	{
		return _callback();
	}

	public override BTNode getBacktrackingNode()
	{
		if (null != _parent) {
			return _parent.getBacktrackingNode();
		}
		
		return this;
	}
}
