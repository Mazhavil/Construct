using System.Collections.Generic;
using Attributes;
using UnityEngine;

namespace Construct.Views
{
    public sealed class MetaSingulaView : MonoBehaviour
    {
        [ReadOnly] public Dictionary<int, SingulaView> Singulas;
    }
}
