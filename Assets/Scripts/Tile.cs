using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public int tileNumber;                          //타일 고유넘버
    public int column;                               //타일 세로좌표
    public int row;                                    //타일 가로좌표
    public int previousColumn;                    //이동 전 세로좌표
    public int previousRow;                        //이동 전 세로좌표
    
    public bool isMatched = false;               //타일 매칭되었는지 확인변수 ( 지울때 판별 )

    [HideInInspector]
    public GameObject otherTile;                //교체할 타일 가질 object 변수
    
    private MatchesCheck matchesCheck;     //클래스 선언 Start에서 Find 후 연결   
    private BackBoard board;                     //클래스 선언 Start에서 Find 후 연결   
    public Animator animator;                    //내 오브젝트 하위 타일 애니메이터 인스펙터에서 참조

    private Vector2 firstTouchPos;               //첫 touchDown 변수
    private Vector2 finalTouchPos;              //마지막 touchDown 변수
    private Vector2 tempPos;                    //타일이동에 사용할 임시 변수

    public TileType tileType;
    public TileType TileType
    {
        get { return tileType; }
        set { tileType = value; }
    }
    
    [Header("Swipe Option")]
    public float swipeAngle = 0;                 //터치 각도
    public float swipeResist = 0.5f;              //스와이프 세기

    [Header("Item Swap")]                        //타일의 아이템상태 변경위한 object
    public Item itemType;
    public GameObject defaultFace;
    public GameObject shadow;
    public GameObject columnArrow;
    public GameObject rowArrow;
    public Sprite bombSprite;
    public Sprite colorBombSprite;
    public Sprite defaultTileSprite;
    
    private void OnEnable()
    {
        Initialization();
    }
    private void Start()
    {
        board = FindObjectOfType<BackBoard>();
        matchesCheck = FindObjectOfType<MatchesCheck>();
    }

    //타일 초기화
    public void Initialization()                                                                              
    {
        itemType = Item.Default;
        isMatched = false;
        this.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        if(TileType == TileType.Default)
        {
            defaultFace.SetActive(true);
            shadow.SetActive(true);
            this.GetComponent<SpriteRenderer>().sprite = defaultTileSprite;
        }
    }
    
    private void Update()                                                                                     
    {
        //MoveTile함수에서 column값이 변경되면 이동 후 타일이 맞았는지 검사  
        if (Mathf.Abs(column - transform.position.x) > 0.1f)                                       
        {
            tempPos = new Vector2(column, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, board.delay);
            if (board.totalTiles[column, row] != this.gameObject)
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
        //MoveTile함수에서 row값이 변경되면 이동 후 타일이 맞았는지 검사 
        if (Mathf.Abs(row - transform.position.y) > 0.1f)                         
        {
            tempPos = new Vector2(transform.position.x, row);
            transform.position = Vector2.Lerp(transform.position, tempPos, board.delay);
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (board.stateController.CompareGameState(GameState.Continue))
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        animator.SetBool("smile", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (board.stateController.CompareGameState(GameState.Continue))
        {
            finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            animator.SetBool("smile", false);
            CalculateAngle();
        }
    }

    //타일 이동 후 이동된 타일의 타입에 따른 분기처리
    public IEnumerator CheckMoveCoroutine()                                                       
    {
        Tile tmpTile = otherTile.GetComponent<Tile>();
        
        ItemUseCheck(this);
        ItemUseCheck(tmpTile);

        yield return new WaitForSeconds(board.delay);
        if(otherTile != null)
        {
            //이동 후 맞는 타일없으면 이동 전 상태로 복구
            if (!isMatched && !otherTile.GetComponent<Tile>().isMatched)                         
            {
                otherTile.GetComponent<Tile>().row = row;
                otherTile.GetComponent<Tile>().column = column;
                row = previousRow;
                column = previousColumn;
                
                board.totalTiles[previousColumn, previousRow] = this.gameObject;
                board.totalTiles[column,row] = otherTile.gameObject;

                yield return new WaitForSeconds(board.delay);
                board.currentTile = null;
                board.stateController.ChangeGameState(GameState.Continue);
            }
            else
            {
                board.DestroyMatches();                                                             
            }
        }
      
    }

    //이동 한 타일이 특수타일인지 확인
    void ItemUseCheck(Tile _tile)                                                                         
    {
        if (_tile.ItemTypeCompare(Item.ColorBomb))
        {
            if (_tile== this)
            {
                matchesCheck.MatchTileOfColor(otherTile.tag);
            }
            else
            {
                matchesCheck.MatchTileOfColor(this.gameObject.tag);
            }
            
            _tile.isMatched = true;
        }
        else if (_tile.ItemTypeCompare(Item.Bomb))
        {
            matchesCheck.UseBombTiles(_tile.column, _tile.row);
            _tile.isMatched = true;
        }
        else if (_tile.ItemTypeCompare(Item.ColumnArrow))
        {
            matchesCheck.UseColumnArrowTiles(_tile.column);
            _tile.isMatched = true;
        }
        else if (_tile.ItemTypeCompare(Item.RowArrow))
        {
            matchesCheck.UseRowArrowTiles(_tile.row);
            _tile.isMatched = true;
        }

    }

    //터치 각도계산 
    void CalculateAngle()                                                                                   
    {
        if(Mathf.Abs(finalTouchPos.y - firstTouchPos.y) > swipeResist || Mathf.Abs(finalTouchPos.x- firstTouchPos.x) > swipeResist)    
        {
            board.stateController.ChangeGameState(GameState.Wait);

            swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) * Mathf.Rad2Deg;
            //왼쪽
            if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)                                     
            {
                MoveTile(Vector2.left);
            }
            //오른쪽
            else if (swipeAngle > -45 && swipeAngle <= 45 && column < board.boardWidth - 1)      
            {
                MoveTile(Vector2.right);
            }
            //위쪽
            else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.boardheight - 1)           
            {
                MoveTile(Vector2.up);
            }
            //아래쪽
            else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)                                    
            {
                MoveTile(Vector2.down);
            }
            
            board.currentTile = this;
        }
        else
        {
            board.stateController.ChangeGameState(GameState.Continue);
        }
    }
    
    //터치 각도 계산후 타일움직임을 컨트롤 할 가로,세로값을 올리는 함수
    void MoveTile(Vector2 _direction)                                                                   
    {
        otherTile = board.totalTiles[column + (int)_direction.x, row + (int)_direction.y];
        if(otherTile.GetComponent<Tile>().TileType == TileType.Blank) { board.stateController.ChangeGameState(GameState.Continue); return;  }
        previousRow = row;
        previousColumn = column;
        if(otherTile != null)
        {
            GameObject tmpObj = this.gameObject;
            board.totalTiles[column, row] = otherTile;
            board.totalTiles[column + (int)_direction.x, row + (int)_direction.y] = tmpObj;

            otherTile.GetComponent<Tile>().column += -1 * (int)_direction.x;
            otherTile.GetComponent<Tile>().row += -1 * (int)_direction.y;

            column += (int)_direction.x;
            row += (int)_direction.y;

            StartCoroutine(CheckMoveCoroutine());
        }
        else
        {
            board.stateController.ChangeGameState(GameState.Continue);
        }
    }

    //타일의 아이템생성 ex)가로열폭탄, 세로열폭탄, 3x3폭탄, 색깔폭탄..
    public void ActiveItem(Item _item)                                                                  
    {
        defaultFace.gameObject.SetActive(false);
       
        switch (_item)
        {
            case Item.RowArrow:
                itemType = Item.RowArrow;
                rowArrow.gameObject.SetActive(true);
                break;

            case Item.ColumnArrow:
                itemType = Item.ColumnArrow;
                columnArrow.gameObject.SetActive(true);
                break;

            case Item.Bomb:
                itemType = Item.Bomb;
                shadow.SetActive(false);
                this.GetComponent<SpriteRenderer>().sprite = bombSprite;
                break;

            case Item.ColorBomb:
                itemType = Item.ColorBomb;
                shadow.SetActive(false);
                this.GetComponent<SpriteRenderer>().sprite = colorBombSprite;
                break;
        }
    }

    //타일의 아이템상태 초기화
    void DeActiveItems()                                                                                    
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).transform.gameObject.SetActive(false);
        }
    }

    //타일 반환 
    public void DeActiveTile()                                           
    {
        DeActiveItems();
        StartCoroutine(DestroyEffect_Coroutine(delegate
        {
            ObjectPooling.Instance().ReturnObject(this.tileNumber, this.gameObject);
        }));
    }

    //타일 반환시 애니메이션출력  
    IEnumerator DestroyEffect_Coroutine(UnityAction _callback)                                       
    {
        #region 삭제 애니메이션
        float time = 0.3f;
        float value = 0;
        
        Vector2 curScale = this.GetComponent<Transform>().localScale;
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        Color curColor = spriteRenderer.color;
        
        while (value < time)
        {
            value += Time.deltaTime;
            transform.localScale = Vector2.Lerp(curScale, new Vector2(2f, 2f),value/time);
            spriteRenderer.color = Color.Lerp(curColor, new Color(1, 1, 1, 0), value / time);

            yield return null;
        }
        #endregion
        
        if (_callback != null)
        {
            _callback();
        }
        
        yield return null;

    }

    //아이템 타입 비교
    public bool ItemTypeCompare(Item _type)
    {
        if (itemType.Equals(_type))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


 
}
