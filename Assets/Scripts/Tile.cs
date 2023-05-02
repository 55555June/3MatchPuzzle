using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    public int tileNumber;                                         //타일 고유넘버
    public int column;                                              //타일 세로좌표
    public int row;                                                   //타일 가로좌표
    public int previousColumn;                                  //이동전 세로좌표
    public int previousRow;                                      //이동전 세로좌표
    
    public bool isMatched = false;                             //타일 매칭되었는지 확인변수 ( 지울때 판별 )

    public GameObject otherTile;                             //교체할 타일 가질 object 변수
    
    private MatchesCheck matchesCheck;                   //클래스 선언 Start에서 Find 후 연결   
    private BackBoard board;                                   //클래스 선언 Start에서 Find 후 연결                      

    private Vector2 firstTouchPos;                              //첫 touchDown 변수
    private Vector2 finalTouchPos;                             //마지막 touchDown 변수
    private Vector2 tempPos;                                    //타일이동에 사용할 임시 변수
    
    [Header("Swipe Option")]
    public float swipeAngle = 0;                                  //터치 각도
    public float swipeResist = 0.5f;                               //스와이프 세기

    [Header("Super Tile")]                                          //타일이 아이템인지 구분 및 아이템 prefab 
    public bool isColumnArrowBomb = false;
    public bool isRowArrowBomb = false;
    public bool isBomb = false;
    public GameObject columnArrow;
    public GameObject rowArrow;
    public GameObject bomb;


    private void OnEnable()
    {
        OptionInitialization();
    }
    private void Start()
    {
        board = FindObjectOfType<BackBoard>();
        matchesCheck = FindObjectOfType<MatchesCheck>();
    }
    
    public void OptionInitialization()                                                  //bool 옵션 초기화
    {
        isBomb = false;
        isColumnArrowBomb = false;
        isRowArrowBomb = false;
        isMatched = false;
        DeActiveItems();
        this.transform.localScale = new Vector3(1, 1, 1);
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    private void Update()
    {
        if (Mathf.Abs(column - transform.position.x) > 0.1f)                      //MoveTile함수에서 column값이 변경되면 이동 후 타일이 맞았는지 검사    
        {
            tempPos = new Vector2(column, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, 0.5f);
            if (board.totalTiles[column,row] != this.gameObject)
            {
                board.totalTiles[column, row] = this.gameObject;
            }
            matchesCheck.FindAllMatches();
        }
        else  
        {
            tempPos = new Vector2(column, transform.position.y);
            transform.position = tempPos;
        }
        if (Mathf.Abs(row - transform.position.y) > 0.1f)                      //MoveTile함수에서 row값이 변경되면 이동 후 타일이 맞았는지 검사    
        {
            tempPos = new Vector2(transform.position.x, row);
            transform.position = Vector2.Lerp(transform.position, tempPos, 0.5f);
            if (board.totalTiles[column, row] != this.gameObject)
            {
                board.totalTiles[column, row] = this.gameObject;
            }
            matchesCheck.FindAllMatches();
        }
        else
        {
            tempPos = new Vector2(transform.position.x, row);
            transform.position = tempPos;
        }
    }

    public IEnumerator CheckMoveCoroutine()                                                             //타일 이동후 매칭되는 타일있는지 검사 함수
    {
        yield return new WaitForSeconds(0.5f);
        if(otherTile != null)
        {
            if(!isMatched && !otherTile.GetComponent<Tile>().isMatched)             //이동 후 맞는 타일없으면 원상태로 복구
            {
                otherTile.GetComponent<Tile>().row = row;
                otherTile.GetComponent<Tile>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(0.5f);
                board.currentTile = null;
                board.stateController.ChangeGameState(GameState.Continue);
            }
            else
            {
                board.DestroyMatches();                                                             
            }
        }
      
    }

    private void OnMouseDown()
    {
        if (board.stateController.CompareGameState(GameState.Continue))
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.stateController.CompareGameState(GameState.Continue))
        {
            finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()                                                                                   //터치 지점 각도계산 
    {
        if(Mathf.Abs(finalTouchPos.y - firstTouchPos.y) > swipeResist || Mathf.Abs(finalTouchPos.x- firstTouchPos.x) > swipeResist)    
        {
            swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) * Mathf.Rad2Deg;     
            MoveTile();
            board.stateController.ChangeGameState(GameState.Wait);
            board.currentTile = this;
        }
        else
        {
            board.stateController.ChangeGameState(GameState.Continue);
        }
    }

    void MoveTile()                                                                                         //터치 각도 계산후 타일의 가로,세로값을 올리는 함수
    {
        //오른쪽 이동
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.boardWidth -1)
        {
            otherTile = board.totalTiles[column + 1, row];
            previousColumn = column;
            previousRow = row;
            otherTile.GetComponent<Tile>().column -= 1;
            column += 1;
        }
        //위쪽 이동
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.boardheight-1)
        {
            otherTile = board.totalTiles[column, row+1];
            previousColumn = column;
            previousRow = row;
            otherTile.GetComponent<Tile>().row -= 1;
            row += 1;
        }
        //왼쪽 이동
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column >0)
        {
            otherTile = board.totalTiles[column - 1, row];
            previousColumn = column;
            previousRow = row;
            otherTile.GetComponent<Tile>().column += 1;
            column -= 1;
        }
        //아래쪽 이동
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            otherTile = board.totalTiles[column, row-1];
            previousColumn = column;
            previousRow = row;
            otherTile.GetComponent<Tile>().row += 1;
            row -= 1;
        }
        else
        {
            board.stateController.ChangeGameState(GameState.Continue);
        }

        StartCoroutine(CheckMoveCoroutine());
    }

    public void ActiveItem(Item _item)
    {
        GameObject item;
        switch (_item)
        {
            case Item.RowArrow:
                isRowArrowBomb = true;
                this.transform.GetChild(0).transform.gameObject.SetActive(true);
                item = this.transform.GetChild(0).gameObject;
                item.transform.localScale = new Vector3(1, 1, 1);
                item.transform.parent = this.transform;
                break;

            case Item.ColumnArrow:
                isColumnArrowBomb = true;
                this.transform.GetChild(1).transform.gameObject.SetActive(true);
                item = this.transform.GetChild(1).gameObject;
                item.transform.localScale = new Vector3(1, 1, 1);
                item.transform.parent = this.transform;
                break;

            case Item.Bomb:
                isBomb = true;
                this.transform.GetChild(2).transform.gameObject.SetActive(true);
                item = this.transform.GetChild(2).gameObject;
                item.transform.localScale = new Vector3(1, 1, 1);
                item.transform.parent = this.transform;
                break;
        }
    }

    void DeActiveItems()
    {
        for(int i=0; i< this.transform.childCount; i++)
        {
            this.transform.GetChild(i).transform.gameObject.SetActive(false);
        }
    }

    public void DestroyTile(UnityAction _callback =null)
    {
        StartCoroutine(DestroyEffect_Coroutine(_callback));
    }

    IEnumerator DestroyEffect_Coroutine(UnityAction _callback)
    {
        float time = 0.4f;
        float value = 0;

        Vector2 curScale = this.GetComponent<Transform>().localScale;
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        Color curColor = spriteRenderer.color;

        while(value < time)
        {
            value += Time.deltaTime;
            transform.localScale = Vector2.Lerp(curScale, new Vector2(1.5f, 1.5f),value/time);
            spriteRenderer.color = Color.Lerp(curColor, new Color(1, 1, 1, 0), value / time);

            yield return null;
        }

        ObjectPooling.Instance().ReturnObject(this.tileNumber, this.gameObject);

        if (_callback != null)
        {
            _callback();
        }


        yield return null;

    }
}
