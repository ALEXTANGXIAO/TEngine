using UnityEngine.UI;

public class EmptyGraph : Graphic
{
    public bool m_debug = false;

    protected override void OnPopulateMesh(VertexHelper vbo)
    {
        vbo.Clear();

#if UNITY_EDITOR
        if (m_debug)
        {
            base.OnPopulateMesh(vbo);
        }
#endif
    }
}