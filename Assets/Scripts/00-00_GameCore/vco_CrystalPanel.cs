using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class vco_CrystalPanel : MonoBehaviour
{
    public static readonly int PANEL_W = 150;
    public static readonly int PANEL_H = 150;
    public static readonly string TAG_PANEL = "Panel";
    public static readonly string TAG_WALL = "Wall";
    public static readonly string NAME_HEAD = "CP_";

    public const string CALL_READY_START = "rs";
    public const string CALL_READY_FALL = "rf";
    public const string CALL_COMPLETE_FALL = "cf";
    public const string CALL_COMPLETE_CRASH = "ca";

    private static readonly string PATH_ATTR_NAME = "img_Attribute_";
    private static readonly string PATH_TILE_NAME = "bgi_Tile_";
    private static readonly string TILE_TYPE_BORDER = "_Border";
    private static readonly string TILE_TYPE_CENTER = "_Center";
    private static readonly string TILE_TYPE_EDGE = "_Edge";
    private static readonly string TILE_SUB_FULL = "_Full";
    private static readonly string TILE_SUB_IN = "_In";
    private static readonly string TILE_SUB_OUT = "_Out";
    private static readonly string TILE_SUB_LINE = "_Line";
    private static readonly string TILE_SUB_LINE_V = "_Line_V";
    private static readonly string TILE_SUB_LINE_H = "_Line_H";
    private static readonly string TILE_CANDIDATE = "_Active";
    private static readonly string TILE_NORMAL = "_Normal";
    private readonly List<string> Lis_attrColor = new List<string>() { "R", "G", "B", "Y", "P" };

    public vco_CrystalStage myManager;

    public SpriteRenderer Spr_Attribute;
    public SpriteRenderer Spr_Tile_NW;
    public SpriteRenderer Spr_Tile_NN;
    public SpriteRenderer Spr_Tile_NE;
    public SpriteRenderer Spr_Tile_WW;
    public SpriteRenderer Spr_Tile_CC;
    public SpriteRenderer Spr_Tile_EE;
    public SpriteRenderer Spr_Tile_SW;
    public SpriteRenderer Spr_Tile_SS;
    public SpriteRenderer Spr_Tile_SE;

    public SpriteAtlas Atlas_Panels;
    public Text Txt_DebugText;

    public Rigidbody2D Rbd_TileRigid;
    public BoxCollider2D Cld_TileCollider;
    public vco_CheckWall Hto_CheckWall;

    private class classTileParts
    {
        private vco_CrystalPanel myParents;
        private SpriteRenderer mySpriteRenderer;
        private SpriteAtlas myAtlas;
        private string myPartsType;
        private string myPartsSpecialType;

        public classTileParts(vco_CrystalPanel _catchVco, SpriteRenderer _catchSpr, SpriteAtlas _ctachAtlas, string _catchType, string _catchSpecial = "")
        {
            myParents = _catchVco;
            mySpriteRenderer = _catchSpr;
            myAtlas = _ctachAtlas;
            myPartsType = _catchType;
            myPartsSpecialType = _catchSpecial;
        }

        public void SetTileImage(string _imgType = "")
        {
            string _currentMode = TILE_NORMAL;

            if (myParents.IsCandidate)
            {
                _currentMode = TILE_CANDIDATE;
            }

            string _spritePath = myParents.pathTile + this.myPartsType + _imgType + _currentMode;
            mySpriteRenderer.sprite = myAtlas.GetSprite(_spritePath);
        }

        public void SetBorderImage(bool _stateAdjacent)
        {
            if (_stateAdjacent)
            {
                SetTileImage(TILE_SUB_FULL);
            }
            else
            {
                SetTileImage(TILE_SUB_LINE);
            }
        }

        public void SetEdgeImage(bool _stateDiagonal, bool _stateVertical, bool _stateHorizon)
        {
            if (_stateDiagonal == false && _stateVertical == false && _stateHorizon == false)
            {
                SetTileImage(TILE_SUB_IN + myPartsSpecialType);
            }
            else if (_stateDiagonal == true && _stateVertical == false && _stateHorizon == false)
            {
                SetTileImage(TILE_SUB_IN + myPartsSpecialType);
            }
            else if (_stateDiagonal == true && _stateVertical == true && _stateHorizon == true)
            {
                SetTileImage(TILE_SUB_FULL);
            }
            else if (_stateDiagonal == false && _stateVertical == true && _stateHorizon == true)
            {
                SetTileImage(TILE_SUB_OUT);
            }
            else if (_stateDiagonal == false && _stateVertical == false && _stateHorizon == true)
            {
                SetTileImage(TILE_SUB_LINE_H);
            }
            else if (_stateDiagonal == true && _stateVertical == false && _stateHorizon == true)
            {
                SetTileImage(TILE_SUB_LINE_H);
            }
            else if (_stateDiagonal == false && _stateVertical == true && _stateHorizon == false)
            {
                SetTileImage(TILE_SUB_LINE_V);
            }
            else if (_stateDiagonal == true && _stateVertical == true && _stateHorizon == false)
            {
                SetTileImage(TILE_SUB_LINE_V);
            }
        }
    }

    private classTileParts Tile_NW, Tile_NN, Tile_NE, Tile_WW, Tile_CC, Tile_EE, Tile_SW, Tile_SS, Tile_SE;

    public int CurrentIndex;
    public int CurrentCellRow;
    public int CurrentCellCol;
    public int CurrentAttribute;
    public int CurrentGroup;
    public bool isSameAttr_NW;
    public bool isSameAttr_NN;
    public bool isSameAttr_NE;
    public bool isSameAttr_WW;
    public bool isSameAttr_EE;
    public bool isSameAttr_SW;
    public bool isSameAttr_SS;
    public bool isSameAttr_SE;
    public bool isEnabledControlEvent = false;

    private bool isCandidate;
    public bool IsCandidate { get => isCandidate; set => ChangeStateCandidate(value); }

    private string pathAttr;
    private string pathTile;


    // Start is called before the first frame update
    void Start()
    {
        Tile_NW = new classTileParts(this, Spr_Tile_NW, Atlas_Panels, TILE_TYPE_EDGE, "_LT");
        Tile_NN = new classTileParts(this, Spr_Tile_NN, Atlas_Panels, TILE_TYPE_BORDER);
        Tile_NE = new classTileParts(this, Spr_Tile_NE, Atlas_Panels, TILE_TYPE_EDGE);
        Tile_WW = new classTileParts(this, Spr_Tile_WW, Atlas_Panels, TILE_TYPE_BORDER);
        Tile_CC = new classTileParts(this, Spr_Tile_CC, Atlas_Panels, TILE_TYPE_CENTER);
        Tile_EE = new classTileParts(this, Spr_Tile_EE, Atlas_Panels, TILE_TYPE_BORDER);
        Tile_SW = new classTileParts(this, Spr_Tile_SW, Atlas_Panels, TILE_TYPE_EDGE);
        Tile_SS = new classTileParts(this, Spr_Tile_SS, Atlas_Panels, TILE_TYPE_BORDER);
        Tile_SE = new classTileParts(this, Spr_Tile_SE, Atlas_Panels, TILE_TYPE_EDGE);

        myManager.CatchCallCrystalPanel(this, CALL_READY_START);
    }

    public void SetAttribute(int _catchAttribute)
    {
        this.gameObject.SetActive(true);

        CurrentAttribute = _catchAttribute;
      
        pathAttr = PATH_ATTR_NAME + Lis_attrColor[CurrentAttribute];
        Spr_Attribute.sprite = Atlas_Panels.GetSprite(pathAttr);

        pathTile = PATH_TILE_NAME + Lis_attrColor[CurrentAttribute];

        SetContourImage();
    }

    public void SetContourImage(bool _isFalling = true)
    {
        bool _chkState_NE = isSameAttr_NE;
        bool _chkState_NN = isSameAttr_NN;
        bool _chkState_NW = isSameAttr_NW;
        bool _chkState_EE = isSameAttr_EE;
        bool _chkState_WW = isSameAttr_WW;
        bool _chkState_SW = isSameAttr_SW;
        bool _chkState_SS = isSameAttr_SS;
        bool _chkState_SE = isSameAttr_SE;
        string _callType = CALL_COMPLETE_FALL;

        if (_isFalling)
        {
            _chkState_NE = false;
            _chkState_NN = false;
            _chkState_NW = false;
            _chkState_EE = false;
            _chkState_WW = false;
            _chkState_SW = false;
            _chkState_SS = false;
            _chkState_SE = false;
            _callType = CALL_READY_FALL;
        }

        Tile_CC.SetTileImage();

        Tile_NN.SetBorderImage(_chkState_NN);
        Tile_WW.SetBorderImage(_chkState_WW);
        Tile_EE.SetBorderImage(_chkState_EE);
        Tile_SS.SetBorderImage(_chkState_SS);

        Tile_NW.SetEdgeImage(_chkState_NW, _chkState_NN, _chkState_WW);
        Tile_NE.SetEdgeImage(_chkState_NE, _chkState_NN, _chkState_EE);
        Tile_SW.SetEdgeImage(_chkState_SW, _chkState_SS, _chkState_WW);
        Tile_SE.SetEdgeImage(_chkState_SE, _chkState_SS, _chkState_EE);

        myManager.CatchCallCrystalPanel(this, _callType);
    }

    public void SetSurroundings(List<bool> _listState)
    {
        isSameAttr_NW = _listState[0];
        isSameAttr_NN = _listState[1];
        isSameAttr_NE = _listState[2];
        isSameAttr_WW = _listState[3];
        isSameAttr_EE = _listState[4];
        isSameAttr_SW = _listState[5];
        isSameAttr_SS = _listState[6];
        isSameAttr_SE = _listState[7];

        SetContourImage(false);
    }

    public void WaitFall(int _shift_Y)
    {
        var _waitPosition = transform.localPosition;
        _waitPosition.y += _shift_Y;
        transform.localPosition = _waitPosition;
    }

    public void Fall()
    {
        CurrentGroup = -1;

        tag = TAG_PANEL;
        Rbd_TileRigid.bodyType = RigidbodyType2D.Dynamic;
    }

    void FixedUpdate()
    {
        if (tag == TAG_PANEL)
        {
            if (Hto_CheckWall.IsTouchWall)
            {
                FixedCrystalPanel();
            }
        }
    }

    public void FixedCrystalPanel()
    {
        tag = TAG_WALL;
        Rbd_TileRigid.bodyType = RigidbodyType2D.Static;

        int _tempRow = Mathf.Abs((int)this.transform.localPosition.y);
        _tempRow = (int)_tempRow / PANEL_H;

        int _tempCol = Mathf.Abs((int)this.transform.localPosition.x);
        _tempCol = (int)_tempCol / PANEL_W;

        int _tempIndex = (vco_CrystalStage.MAX_PANEL_ROW * _tempRow) + _tempCol;

        this.CurrentCellCol = _tempCol;
        this.CurrentCellRow = _tempRow;
        this.CurrentIndex = _tempIndex;
        this.name = NAME_HEAD + CurrentIndex;

        transform.localPosition = myManager.ListRegularPositions[CurrentIndex];
        myManager.CatchCallCrystalPanel(this, CALL_COMPLETE_FALL);
    }

    public void ReadyCrystalPanel()
    {
        string _debugText = "";
        _debugText += this.name + "\n";
        _debugText += "[" + this.CurrentCellRow + "," + this.CurrentCellCol + "]\n";
        _debugText += this.CurrentGroup;

        Txt_DebugText.text = _debugText;

        isEnabledControlEvent = true;

    }

    public void OnMouseEnter()
    {
        if (isEnabledControlEvent)
            myManager.PutInCandidateCrash(CurrentGroup);
    }

    public void OnMouseExit()
    {
        if (isEnabledControlEvent)
            myManager.RemoveCandidateCrash(CurrentGroup);
    }

    public void OnMouseUp()
    {
        if (isEnabledControlEvent)
            myManager.StartCrashCrystalPanels();
    }

    private void ChangeStateCandidate(bool _catchBool)
    {
        this.isCandidate = _catchBool;
        SetContourImage(false);
    }

    public void StartAnimationCrash()
    {
        this.isCandidate = false;
        gameObject.SetActive(false);
        myManager.CatchCallCrystalPanel(this, CALL_COMPLETE_CRASH);
    }
}