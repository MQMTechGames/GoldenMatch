#define DEBUG_BT

public enum BTNodeType
{
	  BTNODE_NO_TYPE = 0

	, BTNODE_LEAF
	, BTNODE_PRIORITY
	, BTNODE_SEQUENCE
	, BTNODE_PARALLEL
	, BT_NODE_CONDITION
	, BT_NODE_RANDOM
	, BT_NODE_WHILE
}

public enum BTNodeResponse 
{
	   INIT = 0
	,  STAY
	,  LEAVE
}

public delegate BTNodeResponse BTCallback();

public delegate bool BTCondition();

public delegate void BTBuilderExtension(string parentNodeName, BTBuilder builder);
