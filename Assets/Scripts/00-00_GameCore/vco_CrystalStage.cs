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

    private List<Vector2Int> listCheckDirection = new List<Vector2Int>();
    private List<vco_CrystalPanel> listFixedVcoPanel = new List<vco_CrystalPanel>();
    private static int countFallCrystalPanel = 0;
    private static int countGroupCrystalPanel = 0;

    private int[,] arrayAttributeBoard;

    // Start is called before the first frame update
    void Start()
    {
        listCheckDirection.Add(new Vector2Int(-1, -1));
        listCheckDirection.Add(new Vector2Int(-1, 0));
        listCheckDirection.Add(new Vector2Int(-1, 1));
        listCheckDirection.Add(new Vector2Int(0, -1));
        listCheckDirection.Add(new Vector2Int(0, 1));
        listCheckDirection.Add(new Vector2Int(1, -1));
        listCheckDirection.Add(new Vector2Int(1, 0));
        listCheckDirection.Add(new Vector2Int(1, 1));
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
                    CheckSameGroup();
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
            int _trgRow = _targetPanelVco.CurrentCellRow;
            int _trgCol = _targetPanelVco.CurrentCellCol;
            int _trgAttribute = _targetPanelVco.CurrentAttribute;

            List<bool> _listState = CheckSurroudings(_trgRow, _trgCol, _trgAttribute);
            _targetPanelVco.SetSurroundings(_listState);
        }
    }

    private List<bool> CheckSurroudings(int _catchRow, int _catchCol, int _catchAttribute)
    {
        List<bool> _retArrayState = new List<bool>();

        foreach (var _trgDirection in listCheckDirection)
        {
            int _trgRow = _catchRow + _trgDirection.x;
            int _trgCol = _catchCol + _trgDirection.y;

            bool _chkRowStart = (_trgRow >= 0);
            bool _chkRowEnd   = (_trgRow < MAX_PANEL_ROW);
            bool _chkColStart = (_trgCol >= 0);
            bool _chkColEnd   = (_trgCol < MAX_PANEL_COL);

            bool _isSame = false;

            if (_chkRowStart && _chkRowEnd && _chkColStart && _chkColEnd)
            {
                if(arrayAttributeBoard[_trgRow, _trgCol] == _catchAttribute)
                {
                    _isSame = true;
                }
            }

            _retArrayState.Add(_isSame);
        }

        return _retArrayState;
    }

    private void CheckSameGroup()
    {
        countGroupCrystalPanel = 0;
        List<int> _listNextPanel;
        List<int> _listSameGroup;

        foreach (var _targetPanelVco in listFixedVcoPanel)
        {
            _listNextPanel = new List<int>();
            _listSameGroup = new List<int>();

            if (_targetPanelVco.CurrentGroup == -1)
            {
                vco_CrystalPanel _checkPanelVco = _targetPanelVco;
                bool _isContinue;
                List<int> _listCheckedPanel = new List<int>();

                do
                {
                    _isContinue = false;

                    List<int> _listMemoPanel = new List<int>();

                    _checkPanelVco.CurrentGroup = countGroupCrystalPanel;
                    _listSameGroup.Add(_checkPanelVco.CurrentIndex);
                    _listCheckedPanel.Add(_checkPanelVco.CurrentIndex);

                    int _trgRow = _checkPanelVco.CurrentCellRow;
                    int _trgCol = _checkPanelVco.CurrentCellCol;

                    // 上方向に探索
                    if (_checkPanelVco.isSameAttr_NN)
                    {
                        int _addPoint = ((_trgRow - 1) * MAX_PANEL_ROW) + _trgCol + 0;
                        _listMemoPanel.Add(_addPoint);
                    }

                    // 左方向に探索
                    if (_checkPanelVco.isSameAttr_WW)
                    {
                        int _addPoint = ((_trgRow + 0) * MAX_PANEL_ROW) + _trgCol - 1;
                        _listMemoPanel.Add(_addPoint);
                    }

                    // 右方向に探索
                    if (_checkPanelVco.isSameAttr_EE)
                    {
                        int _addPoint = ((_trgRow + 0) * MAX_PANEL_ROW) + _trgCol + 1;
                        _listMemoPanel.Add(_addPoint);
                    }

                    // 下方向に探索
                    if (_checkPanelVco.isSameAttr_SS)
                    {
                        int _addPoint = ((_trgRow + 1) * MAX_PANEL_ROW) + _trgCol + 0;
                        _listMemoPanel.Add(_addPoint);
                    }

                    if (_listMemoPanel.Count > 0)
                    {
                        _listNextPanel = _listNextPanel.Union(_listMemoPanel).ToList();

                        foreach (int _deleteCellNumber in _listCheckedPanel)
                        {
                            _listNextPanel.Remove(_deleteCellNumber);
                        }

                        if (_listNextPanel.Count > 0)
                        {
                            int _nextCellNumber = _listNextPanel[0];
                            _checkPanelVco = listFixedVcoPanel[_nextCellNumber];
                            _listNextPanel.RemoveAt(0);
                            _isContinue = true;
                        }
                    }

                } while (_isContinue);

                _listSameGroup.Sort();
                countGroupCrystalPanel++;
            }

            _targetPanelVco.ReadyCrystalPanel();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
