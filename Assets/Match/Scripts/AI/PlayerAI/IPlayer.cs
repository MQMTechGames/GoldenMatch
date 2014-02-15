using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class IPlayer : MonoBehaviour
{
    protected BT _bt = null;
    public int _isStandalone = -1;

    public abstract BT initStandalone();

    public bool getIsStandalone()
    {
        if(_isStandalone < 0)
        {
            _isStandalone = this.GetType() == typeof(BasePlayerAI) ? 1 : 0;
            
            if(0 == _isStandalone)
            {
                _isStandalone = null == GetComponent<BasePlayerAI>() ? 1 : 0;
            }
        }

        return 1 == _isStandalone ? true : false;
    }

    public virtual void pushState(string state)
    {
        _bt.pushNodeByName(state);
    }

    public virtual void Awake()
    {
        if(getIsStandalone())
        {
            _bt = initStandalone();
        }
    }

    public void FixedUpdate()
    {
        if (getIsStandalone())
        {
            _bt.Update();
        }
    }
}
