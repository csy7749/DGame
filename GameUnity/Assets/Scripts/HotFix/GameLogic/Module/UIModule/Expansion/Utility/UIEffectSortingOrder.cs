using System.Collection.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameLogic
{
public class UIEffectSortingOrder:MonoBehaviour
{
public enum ApplyTarget
{
Auto,
SortingGroup,
Renderer,
Both
}

ApplyTarget m_applyTarget；
}
}