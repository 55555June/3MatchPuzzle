using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackBoard : MonoBehaviour
{
    [Header("GameBoardControll")]
    public int boardWidth;                                   //맵 가로길이
    public int boardheight;                                   //맵 세로길이
    public int boardOffSet;                                   //하늘에서 떨어지는 offset범위  
    public int bombRange;                                   //3x3 = 1, 5x5 = 2, 7*7 = 3...

    [Header("SpawnTransform")]                      //그리드타일, 게임타일 가질 부모트랜스폼
    public Transform BoardTransform;
    public Transform tileTransform;

    [Header("TileObjects")]
    public GameObject BackTilePrefab;              //그리드용 타일
    public GameObject[] tileObjects;                 //만들 타일오브젝트의 종류
    private BackGroundTile[,] totaBackTile;         //만든 그리드용 타일들 가지고있는 배열
    public GameObject[,] totalTiles;                  //만든 타일오브젝트들 가지고있는 배열

    private MatchesCheck matchesCheck;          //정답체크하는 스크립트 가진 오브젝트
    [SerializeField]
    public Tile currentTile;                              //아이템생성할 위치 담을 타일 

    [Header("StateControll")]
    [HideInInspector]
    public GameStateController stateController;  //상태체크하는 스크립트 가진 오브젝트

    void Start()
    {
        matchesCheck = FindObjectOfType<MatchesCheck>();
        stateController = new GameStateController();
        

        totaBackTile = new BackGroundTile[boardWidth, boardheight];
        totalTiles = new GameObject[boardWidth, boardheight];
        TileSetting();

        stateController.ChangeGameState(GameState.Continue);
    }

    void TileSetting()
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardheight; j++)
            {
                Vector2 tempPos = new Vector2(i, j);
                GameObject backTile = ObjectPooling.Instance().GetObject(0, tempPos);        //그리드타일 생성
                backTile.transform.parent = BoardTransform;
                backTile.name = "( " + i + " , " + j + " )";

                int tileObjectNum = Random.Range(0, tileObjects.Length);
                int randomMaxValue = 10;            //랜덤 맥스 value

                while (CreateMatchTiles(i, j, tileObjects[tileObjectNum]) && randomMaxValue >0)    //중복검사
                {
                    tileObjectNum = Random.Range(0, tileObjects.Length);
                    randomMaxValue--;
                }
                
                GameObject frontTile = ObjectPooling.Instance().GetObject((tileObjectNum + 1), tempPos);        //게임타일 생성
                frontTile.GetComponent<Tile>().row = j;
                frontTile.GetComponent<Tile>().column = i;
                frontTile.transform.parent = tileTransform;
                frontTile.name = "( " + i + " , " + j + " )";
                totalTiles[i, j] = frontTile;
            }
        }
    }

    private bool CreateMatchTiles(int column, int row, GameObject tile)                                //첫게임타일 생성시 매칭되어 없어질 타일 정리 함수
    {
        if (column > 1 && row > 1)
        {
            if (totalTiles[column - 1, row].tag == tile.tag && totalTiles[column - 2, row].tag == tile.tag)
            {
                return true;
            }
            if (totalTiles[column, row - 1].tag == tile.tag && totalTiles[column, row - 2].tag == tile.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if(row >1)
            {
                if(totalTiles[column, row-1].tag == tile.tag && totalTiles[column,row-2].tag == tile.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (totalTiles[column - 1, row].tag == tile.tag && totalTiles[column - 2, row].tag == tile.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

 
    public void DestroyMatches()                                                                                 //전체 타일중 매칭된 타일있는지 검사 후 타일생성 및 정렬 하는 함수
    {
        for (int i =0; i< boardWidth; i++)
        {
            for(int j =0; j<boardheight; j++)
            {
                if(totalTiles[i,j] != null)
                {
                    if (totalTiles[i, j].GetComponent<Tile>().isMatched)
                    {
                        if (matchesCheck.currentMatches.Count >= 4)
                        {
                            matchesCheck.CreateRandomItem();
                        }
                        totalTiles[i, j].GetComponent<Tile>().DestroyTile();
                        totalTiles[i, j] = null;
                    }
                }
            }
        }
        matchesCheck.currentMatches.Clear();
        StartCoroutine(TileDown_Coroutine());
    }
    
    private IEnumerator TileDown_Coroutine()                                                         //전체타일중 비어있는 타일있는지 검사 하고 빈 타일 만큼 내려주는 코루틴
    {
        float time = 0.1f;
        float value = 0;
        int nullCount = 0;
        for(int i=0; i< boardWidth; i++)
        {
            for(int j =0; j <boardheight; j++)
            {
                if(totalTiles[i,j] == null)
                {
                    nullCount++;
                }
                else if( nullCount > 0)
                {
                    totalTiles[i, j].GetComponent<Tile>().row -= nullCount;
                    totalTiles[i, j] = null;
                }
                value = 0;
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillBoard_Coroutine());
    }

    private IEnumerator FillBoard_Coroutine()                                                            //비어있는 타일을 재생성해주고 재생성한 타일이 매칭되었는지 확인후 게임상태 변경
    {
        RefillTile();
        yield return new WaitForSeconds(0.5f);
        while (MatchTileCheck())
        {
            DestroyMatches();
            yield return new WaitForSeconds(1.0f);
        }

        matchesCheck.currentMatches.Clear();
        currentTile = null;
        yield return new WaitForSeconds(0.5f);
        stateController.ChangeGameState(GameState.Continue);
    }

    void RefillTile()                                                                                            //비어있는 타일에 다시 생성 하는 함수
    {
        for(int i =0; i< boardWidth; i++)
        {
            for(int j=0; j<boardheight; j++)
            {
                if(totalTiles[i,j]==null)
                {
                    Vector2 tempPos = new Vector2(i, j + boardOffSet);
                    int tileObjectNum = Random.Range(0, tileObjects.Length);
                    GameObject frontTile = ObjectPooling.Instance().GetObject((tileObjectNum + 1), tempPos);
                    frontTile.transform.parent = tileTransform;
                    frontTile.name = "( " + i + " , " + j + " )";
                    totalTiles[i, j] = frontTile;
                    frontTile.GetComponent<Tile>().row = j;
                    frontTile.GetComponent<Tile>().column = i;
                }
            }
        }
    }

    bool MatchTileCheck()                                                                            //전체타일중 매칭된 타일이 있는지 검사후 리턴
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
