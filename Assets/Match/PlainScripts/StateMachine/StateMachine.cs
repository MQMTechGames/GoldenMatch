using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void StateMachineMethod();

public class StateMachine
{
	string currState = "";
	bool isFirstEnter = true;
	SortedDictionary<string, StateMachineMethod> stateMethods;

	public StateMachine()
	{ 
		stateMethods = new SortedDictionary<string, StateMachineMethod> ();
	}

	public void addStateMethod(string name, StateMachineMethod method)
	{
		bool alreadyExist = stateMethods.ContainsKey (name);

		DebugUtils.assert (false == alreadyExist, "alread");

		stateMethods.Add (name, method);

		if ("" == currState) {
			currState = name;
		}
	}

	public void clearStates()
	{
		stateMethods.Clear ();
	}

	public void changeState(string name)
	{
		isFirstEnter = true;

		bool alreadyExist = stateMethods.ContainsKey (name);
		DebugUtils.assert (alreadyExist, "the state " + name + " does NOT exist");

		currState = name;
	}

	public void Update ()
	{
		// lets assume that the _currState is right
		DebugUtils.assert (currState != "", "currState can not be unset");

		stateMethods [currState] ();

		isFirstEnter = false;
	}

	public bool getIsFirstEnter()
	{
		return isFirstEnter;
	}
}
