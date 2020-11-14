using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class vco_CrystalStage : MonoBehaviour
{
    public static readonly int STAGE_W = 900;
    public static readonly int STAGE_H = 900;
    public static readonly int MAX_PANEL_ROW = 6;
    public static readonly int MAX_PANEL_COL = 6;
    public static readonly int MAX_PANEL_ALL = MAX_PANEL_ROW * MAX_PANEL_COL;
    public static readonly int COUNT_ATTRIBUTE = 5;
    public List<Vector3Int> ListRegularPosition = new List<Vector3Int>();

    public GameObject Pfb_CrystalPanel;
    public SpriteAtlas Atlas_Panels;

    private List<vco_CrystalPanel> listFixedVcoPanel = new List<vco_CrystalPanel>();
    private static int countFallCrystalPanel = 0;

    private int[,] arrayAttributeBoard;

    // Start is called before the first frame update
    void Start()
    {
        InitializedCrystalStage();
    }

    public void InitializedCrystalStage()
    {
        listFixedVcoPanel = new List<vco_CrystalPanel>();

        SetRegularPosition();
        CreateCrystalPanel();
    }

    // クリスタルパネルの規定座標を算出
    private void SetRegularPosition()
    {
        for (int _countRow = 0; _countRow < MAX_PANEL_ROW; _countRow++)
        {
            for (int _countCol = 0; _countCol < MAX_PANEL_COL; _countCol++)
            {
                Vector3Int _appPosition = new Vector3Int();
                _appPosition.x = (_countCol * vco_CrystalPanel.PANEL_W) + (vco_CrystalPanel.PANEL_W / 2);
                _appPosition.y = ((_countRow * vco_CrystalPanel.PANEL_H) + (vco_CrystalPanel.PANEL_H / 2)) * -1;
                _appPosition.z = 1;
                ListRegularPosition.Add(_appPosition);
            }
        }
    }

    private void CreateCrystalPanel()
    {
        foreach (Vector3Int _createPosition in ListRegularPosition)
        {
            GameObject _newPanel = Instantiate(Pfb_CrystalPanel);
            _newPanel.transform.parent = transform;
            _newPanel.transform.localPosition = _createPosition;

            vco_CrystalPanel _vcoPanel = _newPanel.GetComponent<vco_CrystalPanel>();
            _vcoPanel.myManager = this;
            _vcoPanel.Atlas_Panels = Atlas_Panels;
        }
    }

    public void CatchCallCrystalPanel(vco_CrystalPanel _targetPanelVco, string _callType)
    {
        switch (_callType)
        {
            case vco_CrystalPanel.CALL_READY_START:
                listFixedVcoPanel.Add(_targetPanelVco);

                if (listFixedVcoPanel.Count == MAX_PANEL_ALL)
                {
                    PullUpCrystalPanel(listFixedVcoPanel);
                }

                break;

            case vco_CrystalPanel.CALL_READY_FALL:

                countFallCrystalPanel++;
                _targetPanelVco.Fall();

                break;

            case vco_CrystalPanel.CALL_COMPLETE_FALL:

                int _setRow = _targetPanelVco.CurrentCellRow;
                int _setCol = _targetPanelVco.CurrentCellCol;
                arrayAttributeBoard[_setRow, _setCol] = _targetPanelVco.CurrentAttribute;

                countFallCrystalPanel--;

                if (countFallCrystalPanel == 0)
                {
                    listFixedVcoPanel.OrderBy(vco => vco.CurrentIndex);
                    ConnectCrystalPanel();
                }

                break;

            default:
                break;
        }

    }

    private void PullUpCrystalPanel(List<vco_CrystalPanel> _lisTargetVco)
    {
        arrayAttributeBoard = new int[MAX_PANEL_ROW, MAX_PANEL_COL];

        foreach (var _targetPanelVco in _lisTargetVco)
        {
            _targetPanelVco.WaitFall(STAGE_H);
            _targetPanelVco.SetAttribute(Random.Range(0, COUNT_ATTRIBUTE));
        }
    }

    private void ConnectCrystalPanel()
    {
        foreach (var _targetPanelVco in listFixedVcoPanel)
        {
            bool[] _arrayState = CheckSurroudings(_targetPanelVco.CurrentCellRow, _targetPanelVco.CurrentCellCol);
            _targetPanelVco.SetSurroundings(_arrayState);
        }
    }

    private bool[] CheckSurroudings(int _catchRow, int _catchCol)
    {
        bool[] _retArrayState = new bool[8];
        List<Vector2Int> _listCheckDirection = new List<Vector2Int>();

        _listCheckDirection.Add(new Vector2Int(-1, -1));
        _listCheckDirection.Add(new Vector2Int(-1,  0));
        _listCheckDirection.Add(new Vector2Int(-1,  1));
        _listCheckDirection.Add(new Vector2Int(0 , -1));
        _listCheckDirection.Add(new Vector2Int(0 ,  1));
        _listCheckDirection.Add(new Vector2Int(1 , -1));
        _listCheckDirection.Add(new Vector2Int(1 ,  0));
        _listCheckDirection.Add(new Vector2Int(1 ,  1));

        return _retArrayState;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
