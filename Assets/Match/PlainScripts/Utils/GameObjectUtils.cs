using UnityEngine;
using System.Collections;
using System;

struct GameObjectUtils
{
	enum ChildByType {
		NAME
		, TAG
	};

	public static T getComponentByEntityTag<T>(string tag)
	{
		GameObject go = GameObject.FindGameObjectWithTag(tag);
		
		if(go){
			return (T)(object) go.GetComponent(typeof(T));
		}

		return (T)(object) null;
		//return default(T);
	}

	public static T getComponentByEntityName<T>(string name)
	{
		GameObject go = GameObject.Find(name);
		
		if(go){
			return (T)(object) go.GetComponent(typeof(T));
		}

		return (T)(object) null;
	}

	public static T getComponent<T>(Transform transform, bool isRecursive = true)
	{
		if(isRecursive) {
			return (T)(object) getComponentRecursive<T>(transform);
		} else {
			return (T)(object) transform.GetComponent(typeof(T));
		}
	}

	private static T getComponentRecursive<T>(Transform transform)
	{
		T comp = (T)(object) transform.GetComponent(typeof(T));
		if(null!=comp) {
			return (T)(object) comp;
		}

		// Look for the component in children
		int childCount = transform.childCount;
		for(int i=0; i < childCount; ++i)
		{
			Transform trans = transform.GetChild(i);

			comp = (T)(object) transform.GetComponent(typeof(T));
			if(null!=comp) {
				return (T)(object) comp;
			} else {
				comp = GameObjectUtils.getComponentRecursive<T>(trans);
				if(null!=comp){
					return comp;
				}
			}
		}
		
		return (T)(object) null;
	}

	public static T getChildComponentByTag<T>(Transform transform ,string tag)
	{
		Transform trans = getChildBy(transform, tag, ChildByType.TAG);
		if(null == trans) {
			return (T)(object) null;
		}

		return (T)(object) transform.GetComponent(typeof(T));
	}

	public static T getChildComponentByName<T>(Transform transform ,string name)
	{
		Transform trans = getChildBy(transform, name, ChildByType.NAME);
		if(null == trans) {
			return (T)(object) null;
		}

		return (T)(object) transform.GetComponent(typeof(T));
	}

	public static Transform getChildByTag(Transform transform ,string tag)
	{
		return getChildBy(transform, tag, ChildByType.TAG);
	}

	public static Transform getChildByName(Transform transform ,string name)
	{
		return getChildBy(transform, name, ChildByType.NAME);
	}

	private static Transform getChildBy(Transform transform ,string comparator, ChildByType type)
	{
		int childCount = transform.childCount;
		
		for(int i=0; i < childCount; ++i)
		{
			Transform trans = transform.GetChild(i);

			bool found = false;
			if(ChildByType.NAME == type) {
				found = comparator == trans.gameObject.name;
			} else if(ChildByType.TAG == type){
				found = comparator == trans.gameObject.tag;
			}

			if(found) {
				return trans;
			} else {
				trans = GameObjectUtils.getChildByTag(trans, comparator);
				if(null!=trans){
					return trans;
				}
			}
		}
		
		return null;
	}
}
