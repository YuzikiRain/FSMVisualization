using BordlessFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundState : State
{
    public readonly string airStateName = typeof(AirState).Name;

    public override void Reason()
    {
        if (true) ChangeState(airStateName);
    }

}
