using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatorParams
{
	public int speedFloat;
	public int directionFloat;

	static AnimatorParams _instance = null;

	private AnimatorParams()
	{

	}

	private void init()
	{
		createHashes();
	}

	private static void create(out AnimatorParams animatorParams)
	{
		animatorParams = new AnimatorParams();
		animatorParams.init();
	}

	public static AnimatorParams sharedInstance()
	{
		if(null == _instance)
		{
			AnimatorParams.create(out _instance);
		}

		return _instance;
	}

	private void createHashes()
	{
		speedFloat = Animator.StringToHash("Speed");
		directionFloat = Animator.StringToHash("Direction");
	}
}
