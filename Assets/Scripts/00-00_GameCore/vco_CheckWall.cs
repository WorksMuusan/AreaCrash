using UnityEngine;

public class vco_CheckWall : MonoBehaviour
{
    private bool isTouchWall = false;
    private bool isTouchWallEnter, isTouchWallStay, isTouchWallExit;

    public bool IsTouchWall { get => ReturnTouchWall(); set => isTouchWall = value; }

    //接地判定を返すメソッド
    //物理判定の更新毎に呼ぶ必要がある
    public bool ReturnTouchWall()
    {
        if (isTouchWallEnter || isTouchWallStay)
        {
            isTouchWall = true;
        }
        else if (isTouchWallExit)
        {
            isTouchWall = false;
        }

        isTouchWallEnter = false;
        isTouchWallStay = false;
        isTouchWallExit = false;

        return isTouchWall;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            isTouchWallEnter = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            isTouchWallStay = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            isTouchWallExit = true;
        }
    }
}
