using UnityEngine;

public class vco_CheckWall : MonoBehaviour
{
    private bool sw_TouchWall = false;
    private bool sw_TouchWallEnter, sw_TouchWallStay, sw_TouchWallExit;

    //接地判定を返すメソッド
    //物理判定の更新毎に呼ぶ必要がある
    public bool Sw_TouchWall()
    {
        if (sw_TouchWallEnter || sw_TouchWallStay)
        {
            sw_TouchWall = true;
        }
        else if (sw_TouchWallExit)
        {
            sw_TouchWall = false;
        }

        sw_TouchWallEnter = false;
        sw_TouchWallStay = false;
        sw_TouchWallExit = false;

        return sw_TouchWall;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            sw_TouchWallEnter = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            sw_TouchWallStay = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == vco_CrystalPanel.TAG_WALL)
        {
            sw_TouchWallExit = true;
        }
    }
}
