using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankTile
{
    public Vector2 pos;
}

public enum Item                                               //타일에 사용할 아이템
{
    Default,                                                        //기본 상태
    ColumnArrow,                                               //세로 열 폭탄
    RowArrow,                                                    //가로 열 폭탄
    Bomb,                                                         //3x3 폭탄
    ColorBomb                                                   //색깔 폭탄
}

public enum TileType                                          //생성할 타일의 종류
{
    Default,                                                        //움직일 타일
    Blank,                                                          //비어있는 공간타일
    Grid                                                            //움직이지않는 배경타일
}
public class BackBoard : MonoBehaviour
{
    [Header("GameBoardControll")]
    public int boardWidth;                                   //맵 가로길이
    public int boardheight;                                   //맵 세로길이
    public int boardOffSet;                                   //하늘에서 떨어지는 offset범위  

    [Header("BlankTileSetting")]
    public List<BlankTile> blankTileList;                  //생성하지않을 타일 

    [HideInInspector]
    public int bombRange;                                  //3x3 = 1, 5x5 = 2, 7*7 = 3...

    [Header("SpawnTransform")]                           //그리드타일, 게임타일 가질 부모트랜스폼
    public Transform BoardTransform;
    public Transform tileTransform;

    [Header("TileObjects")]
    public GameObject[,] totalTiles;                      //만든 타일오브젝트들 가지고있는 배열
    public bool[,] blankTiles;                               //만든 타일오브젝트들 가지고있는 배열

    private MatchesCheck matchesCheck;              //정답체크하는 스크립트 가진 오브젝트
    [HideInInspector]
    public Tile currentTile;                                  //아이템생성할 위치 담을 타일 
    
    [HideInInspector]
    public GameStateController stateController;      //상태체크하는 스크립트 가진 오브젝트
    [HideInInspector]
    public float delay = 0.5f;                              //타일 생성,처리 딜레이

    void Start()
    {
        matchesCheck = FindObjectOfType<MatchesCheck>();
        stateController = new GameStateController();

        blankTiles = new bool[boardWidth, boardheight];
        totalTiles = new GameObject[boardWidth, boardheight];
        TileSetting();

        stateController.ChangeGameState(GameState.Continue);
    }

    //최초 타일 세팅
    void TileSetting()                                                                                                 
    {
        //빈 타일 먼저 생성
        for (int i =0; i< blankTileList.Count; i++)                                          
        {
            CreateTile((int)blankTileList[i].pos.x, (int)blankTileList[i].pos.y, TileType.Blank);
        }

        for (int i = 0; i < boardWidth; i++)                                                
        {
            for (int j = 0; j < boardheight; j++)
            {
                if (blankTiles[i, j] == false)
                {
                    //그리드타일 생성
                    CreateTile(i, j, TileType.Grid);
                    //디폴트타일 생성
                    CreateTile(i, j, TileType.Default);                                         
                }
            }
        }
    }

    //게임 타일 생성
    void CreateTile(int _xPos, int _yPos,TileType _tileType, int _offset =0)                               
    {
        GameObject tileObject;
        if (_tileType.Equals(TileType.Blank))
        {
            tileObject = ObjectPooling.Instance().GetObject(0, new Vector2(_xPos,_yPos+ _offset));
            blankTiles[_xPos, _yPos] = true;
        }
        else if(_tileType.Equals(TileType.Grid))
        {
            tileObject = ObjectPooling.Instance().GetObject(6, new Vector2(_xPos, _yPos));
            tileObject.transform.parent = BoardTransform;
            return;
        }
        else
        {
            int tileObjectNum = Random.Range(0, ObjectPooling.Instance().DefaultTileCount);
            int randomMaxValue = 10;

            //중복검사
            while (CreateMatchTiles(_xPos, _yPos, ObjectPooling.Instance().poolingItem[tileObjectNum+1].prefab) && randomMaxValue > 0)    
            {
                tileObjectNum = Random.Range(0, ObjectPooling.Instance().DefaultTileCount);
                randomMaxValue--;
            }

            //사용할 타일 Active
            tileObject = ObjectPooling.Instance().GetObject((tileObjectNum + 1), new Vector2(_xPos, _yPos+ _offset));                                
            tileObject.GetComponent<Tile>().defaultFace.SetActive(true);
            tileObject.GetComponent<Tile>().shadow.SetActive(true);
        }

        tileObject.GetComponent<Tile>().row = _yPos;
        tileObject.GetComponent<Tile>().column = _xPos;
        tileObject.transform.parent = tileTransform;
        tileObject.GetComponent<Tile>().TileType = _tileType;
        totalTiles[_xPos, _yPos] = tileObject;
    }

    //첫게임타일 생성시 매칭되어있는지 확인 함수
    private bool CreateMatchTiles(int column, int row, GameObject tile)                                
    {
        if (column > 1 && row > 1)
        {
            if(totalTiles[column-1,row] != null && totalTiles[column-2, row] != null)
            {
                if (totalTiles[column - 1, row].tag == tile.tag && totalTiles[column - 2, row].tag == tile.tag)
                {
                    return true;
                }
            }
           
            if(totalTiles[column,row-1] != null && totalTiles[column,row-2] != null)
            {
                if (totalTiles[column, row - 1].tag == tile.tag && totalTiles[column, row - 2].tag == tile.tag)
                {
                    return true;
                }
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if(row >1)
            {
                if(totalTiles[column,row-1] != null && totalTiles[column, row-2]!= null)
                {
                    if (totalTiles[column, row - 1].tag == tile.tag && totalTiles[column, row - 2].tag == tile.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if(totalTiles[column-1, row] != null && totalTiles[column-2, row] != null)
                {
                    if (totalTiles[column - 1, row].tag == tile.tag && totalTiles[column - 2, row].tag == tile.tag)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //매치된 타일의 개수에 따라 아이템생성 콜 하고 매칭확인된 타일 Remove
    public void DestroyMatches()                                                                                
    {
        for (int i =0; i< boardWidth; i++)
        {
            for(int j =0; j<boardheight; j++)
            {
                if(totalTiles[i,j] != null)
                {
                    if (totalTiles[i, j].GetComponent<Tile>().isMatched)
                    {
                        #region currentMatchesCount의 조건에 따른 아이템 생성
                        if (matchesCheck.currentMatches.Count == 4 || matchesCheck.currentMatches.Count == 7)
                        {
                            matchesCheck.CreateCrossItem();
                        }
                       if(matchesCheck.currentMatches.Count == 5 || matchesCheck.currentMatches.Count == 8)
                        {
                            //일자 일경우 색깔폭탄
                            if (matchesCheck.OneLineCheck())              
                            {
                                if(currentTile != null)
                                {
                                    if(currentTile.isMatched)
                                    {
                                        if(!currentTile.ItemTypeCompare(Item.ColorBomb))
                                        {
                                            currentTile.isMatched = false;
                                            currentTile.ActiveItem(Item.ColorBomb);
                                        }
                                    }
                                    else
                                    {
                                        if(currentTile.otherTile != null)
                                        {
                                            Tile otherTile = currentTile.otherTile.GetComponent<Tile>();
                                            if(otherTile.isMatched && !otherTile.ItemTypeCompare(Item.ColorBomb))
                                            {
                                                otherTile.isMatched = false;
                                                otherTile.ActiveItem(Item.ColorBomb);
                                            }
                                        }
                                    }
                                }
                            }
                            //일자가 아닐경우 3x3폭탄 
                            else
                            {
                                if (currentTile != null)
                                {
                                    if (currentTile.isMatched)
                                    {
                                        if (!currentTile.ItemTypeCompare(Item.Bomb))
                                        {
                                            currentTile.isMatched = false;
                                            currentTile.ActiveItem(Item.Bomb);
                                        }
                                    }
                                    else
                                    {
                                        if (currentTile.otherTile != null)
                                        {
                                            Tile otherTile = currentTile.otherTile.GetComponent<Tile>();
                                            if (otherTile.isMatched)
                                            {
                                                otherTile.isMatched = false;
                                                otherTile.ActiveItem(Item.Bomb);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        totalTiles[i, j].GetComponent<Tile>().DeActiveTile();
                        totalTiles[i, j] = null;
                    }
                }
            }
        }
        matchesCheck.currentMatches.Clear();
        StartCoroutine(TileDown_Coroutine());
    }

    //전체타일중 비어있는 타일있는지 검사 하고 빈 타일 만큼 내려주는 코루틴
    private IEnumerator TileDown_Coroutine()
    {
        for(int i=0; i< boardWidth; i++)
        {
            for(int j=0; j < boardheight; j++)
            {
                if(totalTiles[i, j]== null && !blankTiles[i,j])
                {
                    for(int k= j+1; k< boardheight; k++)
                    {
                        if(totalTiles[i,k] != null)
                        {
                            if (!blankTiles[i, k])
                            {
                                totalTiles[i, k].GetComponent<Tile>().row = j;
                                totalTiles[i, k] = null;
                                break;
                            }
                           
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillBoard_Coroutine());
    }

    //타일재생성 호출 및 재생성한 타일이 매칭되었는지 확인후 게임상태 변경
    private IEnumerator FillBoard_Coroutine()                                                                
    {
        RefillTile();
        yield return new WaitForSeconds(delay);
        while (MatchTileCheck())                     
        {
            DestroyMatches();
            yield return new WaitForSeconds((delay*2));
        }

        matchesCheck.currentMatches.Clear();
        currentTile = null;
        yield return new WaitForSeconds(delay);
        stateController.ChangeGameState(GameState.Continue);
    }

    //타일 재 생성하는 함수
    void RefillTile()                                                                                                  
    {
        for(int i =0; i< boardWidth; i++)
        {
            for(int j=0; j<boardheight; j++)
            {
                if(totalTiles[i,j]==null && !blankTiles[i, j])
                {
                    CreateTile(i, j, TileType.Default,boardOffSet);
                    
                }
            }
        }
    }

    //리필 후 매칭된 타일이 있는지 검사후 리턴
    bool MatchTileCheck()                                                                                       
    {
        for(int i=0; i<boardWidth; i++)
        {
            for(int j=0; j<boardheight; j++)
            {
                if(totalTiles[i,j] != null)
                {
                    if(totalTiles[i,j].GetComponent<Tile>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
