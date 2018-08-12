using UnityEngine;

public class Tile_Network : MonoBehaviour
{
    public Color color;

    [HideInInspector] public int xIndex;
    [HideInInspector] public int yIndex;

    private Board_Network m_board;

    public void Init(int x, int y, Board_Network board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }

    void OnMouseDown ()
    {
        if (m_board != null && m_board.m_playerInputEnabled)
        {
            m_board.ClickTile(this);
        }
    }

    void OnMouseEnter ()
    {
        if (m_board != null && m_board.m_playerInputEnabled)

        {
            m_board.DragToTile(this);
            m_board.ReleaseTile();
        }
    }

    void OnMouseUp ()
    {
        if (m_board != null && m_board.m_playerInputEnabled)

        {
            m_board.ReleaseTile();
        }
    }
}