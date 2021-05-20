using System.Collections.Generic;
using UnityEngine;

public class FSMData : ScriptableObject
{
    public List<FSMStateNodeData> NodeDatas = new List<FSMStateNodeData>();
    public List<FSMPortData> PortDatas = new List<FSMPortData>();
}
