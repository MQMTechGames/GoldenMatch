using UnityEngine;
using System.Collections;

public interface IInputPlayerController
{
    void setTarget(Transform target, Animator animator = null);
    void move();
}