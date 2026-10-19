using UnityEngine;

public class DashIndicatorComponent : ActorComponent
{
    private readonly SpriteRenderer m_sprite;
    private readonly Transform m_transform;
    private readonly float m_width;
    private readonly float m_heightOffset;
    private readonly float m_minLength;

    public DashIndicatorComponent(
        Actor owner,
        SpriteRenderer sprite,
        float width = 1.5f,
        float heightOffset = 0.05f,
        float minLength = 0.5f) : base(owner)
    {
        m_sprite = sprite;
        m_transform = sprite.transform;
        m_width = width;
        m_heightOffset = heightOffset;
        m_minLength = minLength;

        m_sprite.drawMode = SpriteDrawMode.Sliced;
        Hide();
    }

    public void Show(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        delta.y = 0f;

        float length = delta.magnitude;
        if (length < 0.01f)
        {
            Hide();
            return;
        }

        float yaw = Quaternion.LookRotation(delta).eulerAngles.y;

        m_transform.SetPositionAndRotation(
            from + Vector3.up * m_heightOffset,
            Quaternion.Euler(90f, yaw, 0f));

        m_sprite.size = new Vector2(m_width, Mathf.Max(length, m_minLength));
        m_sprite.enabled = true;
    }

    public void Hide() => m_sprite.enabled = false;
}