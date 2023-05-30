using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchesCheck : MonoBehaviour
{
    private BackBoard board;
    public List<GameObject> currentMatches = new List<GameObject>();

    void Start()
    {
        board = FindObjectOfType<BackBoard>();
    }
    
    //타일이 움직이면 호출
    public void FindAllMatches()
    {
        StartCoroutine(FIndAllMatchesCoroutine());
    }

    //3매칭 되었는지 확인 하는 코루틴
    private IEnumerator FIndAllMatchesCoroutine()       
        
    {
        yield return new WaitForSeconds(0.2f);
        for (int i =0; i< board.boardWidth; i++)
        {
            for(int j =0; j< board.boardheight; j++)
            {
                GameObject currentObject = board.totalTiles[i, j];
                
                if(currentObject != null)
                {
                    Tile currentTile = currentObject.GetComponent<Tile>();
                    //가로매칭검사
                    if (i > 0 && i < board.boardWidth -1) 
                    {
                        GameObject leftObject = board.totalTiles[i - 1, j];
                        GameObject rightObject = board.totalTiles[i + 1, j];
                        
                        if (leftObject != null && rightObject != null)
                        {
                            //타일이 옮겨진 후 내 타일과 가로타일(x-1,x+2)이 같다면 
                            if (leftObject.tag == currentObject.tag && rightObject.tag == currentObject.tag) 
                            {
                                GetMatchedTiles(leftObject, currentObject, rightObject);                           
                            }
                        }
                    }
                    //세로매칭검사
                    if (j > 0 && j < board.boardheight - 1) 
                    {
                        GameObject upObject = board.totalTiles[i, j - 1];
                        GameObject downObject = board.totalTiles[i, j + 1];

                        if(upObject != null && downObject != null)
                        {
                            //타일이 옮겨진 후 내 타일과 세로타일(y-1,y+2)이 같다면 
                            if (upObject.tag == currentObject.tag && downObject.tag == currentObject.tag) 
                            {
                                GetMatchedTiles(upObject, currentObject, downObject);
                            }
                        }
                    }
                }
            }
        }
    }
   
    // 3매칭 된 타일들 모을 함수
    private void GetMatchedTiles(GameObject _tile1, GameObject _tile2, GameObject _tile3) 
    {
        AddToMatchedTylist(_tile1);
        AddToMatchedTylist(_tile2);
        AddToMatchedTylist(_tile3);
    }

    //매칭된타일을 매칭되어있는 리스트 중복검사후 매칭타일리스트에 추가 
    private void AddToMatchedTylist(GameObject _tile)                                                                   
    {
        if (!currentMatches.Contains(_tile))
        {
            currentMatches.Add(_tile);
        }
        _tile.GetComponent<Tile>().isMatched = true;
    }
    
    //색깔폭탄 사용
    public void MatchTileOfColor(string _color)
    {
        for (int i = 0; i < board.boardWidth; i++)
        {
            for (int j = 0; j < board.boardheight; j++)
            {
                if (board.totalTiles[i, j] != null)
                {
                    if (board.totalTiles[i, j].tag == _color)
                    {
                        board.totalTiles[i, j].GetComponent<Tile>().isMatched = true;
                    }
                }
            }
        }
    }

    //3x3폭탄 아이템 사용
    public void UseBombTiles(int column,int row)                                                                         
    {
        List<GameObject> tilles = new List<GameObject>();
        for (int i = (column - board.bombRange); i <= (column + board.bombRange); i++)
        {
            for (int j = (row - board.bombRange); j <= (row + board.bombRange); j++)
            {
                if (i >= 0 && i < board.boardWidth && j >= 0 && j < board.boardheight)
                {
                    if (board.totalTiles[i, j] != null)
                    {
                        Tile tile = board.totalTiles[i, j].GetComponent<Tile>();
                        if (tile.ItemTypeCompare(Item.RowArrow) && !tile.isMatched)
                        {
                            UseRowArrowTiles(j);
                        }
                        if (tile.ItemTypeCompare(Item.ColumnArrow) && !tile.isMatched)
                        {
                            UseColumnArrowTiles(i);
                        }
                        else
                        {
                            tilles.Add(board.totalTiles[i, j]);
                            board.totalTiles[i, j].GetComponent<Tile>().isMatched = true;
                        }
                    }
                }
            }
        }
    }

    //세로열폭탄 사용
    public void UseColumnArrowTiles(int column)                                                                        
    {
        List<GameObject> tilles = new List<GameObject>();
        for(int i=0; i<board.boardheight; i++)
        {
            if(board.totalTiles[column, i] != null)
            {
                Tile tile = board.totalTiles[column, i].GetComponent<Tile>();
                if(tile != null)
                {
                    if (tile.ItemTypeCompare(Item.Bomb) && !tile.isMatched)
                    {
                        UseBombTiles(column, i);
                    }
                    if (tile.ItemTypeCompare(Item.RowArrow) && !tile.isMatched)
                    {
                        UseRowArrowTiles(i);
                    }
                    else
                    {
                        tile.isMatched = true;
                    }
                }
            }
        }
    }

    //가로열폭탄 사용
    public void UseRowArrowTiles(int row)                                                                                   
    {
        List<GameObject> tilles = new List<GameObject>();
        for (int i = 0; i < board.boardWidth; i++)
        {
            if (board.totalTiles[i, row] != null)
            {
                Tile tile = board.totalTiles[i, row].GetComponent<Tile>();
                if(tile != null)
                {
                    if (tile.ItemTypeCompare(Item.Bomb) && !tile.isMatched)
                    {
                        UseBombTiles(i, row);
                    }
                    if (tile.ItemTypeCompare(Item.ColumnArrow) && !tile.isMatched)
                    {
                        UseColumnArrowTiles(i);
                    }
                    else
                    {
                        tile.isMatched = true;
                    }
                }
            }
        }
    }

    //타일의 움직임 조건에따라 가로열, 세로열 폭탄생성 
    public void CreateCrossItem()                                                                                               
    {
        if(board.currentTile != null)
        {
            if(board.currentTile.isMatched)
            {
                //현재 타일이 아이템상태를 갖고있다면 리턴
                if (!isItemCheck(board.currentTile)) { return; } 

                board.currentTile.isMatched = false;
                
                if(board.currentTile.swipeAngle> -45 && board.currentTile.swipeAngle <= 45 || board.currentTile.swipeAngle <= -135 && board.currentTile.swipeAngle > 135)
                {
                    board.currentTile.ActiveItem(Item.RowArrow);
                   
                }
                else
                {
                    board.currentTile.ActiveItem(Item.ColumnArrow);
                }
            }
            else if( board.currentTile.otherTile != null)
            {
                Tile otherTile = board.currentTile.otherTile.GetComponent<Tile>();

                if(otherTile.isMatched)
                {
                    //현재 타일이 아이템상태를 갖고있다면 리턴
                    if (!isItemCheck(otherTile)) { return; } 
                    otherTile.isMatched = false;
                    
                    if (board.currentTile.swipeAngle > -45 && board.currentTile.swipeAngle <= 45 || board.currentTile.swipeAngle <= -135 && board.currentTile.swipeAngle > 135)
                    {
                        otherTile.ActiveItem(Item.RowArrow);
                    }
                    else
                    {
                        otherTile.ActiveItem(Item.ColumnArrow);
                    }
                }
            }
        }
    }

    //일자로 5개이상 매칭되었는지확인 색깔폭탄생성
    public bool OneLineCheck()                                                                                                
    {
        int columnCount = 0;
        int rowCount = 0;

        Tile firstTile = currentMatches[0].GetComponent<Tile>();
        if (firstTile != null)
        {
            foreach (var currentTile in currentMatches)
            {
                Tile tile = currentTile.GetComponent<Tile>();
                if (tile.row == firstTile.row)
                {
                    rowCount++;
                }
                if (tile.column == firstTile.column)
                {
                    columnCount++;
                }
            }
        }
        return (columnCount >= 5 || rowCount >= 5);
    }

    //아이템 타입 겹치지않게 조건검사 함수
    bool isItemCheck(Tile _tile)                                                                                                  
    {
            if (_tile.ItemTypeCompare(Item.Default))
            {
                return true;
            }
            else
            {
                return false;
            }
    }                    
    
    
}
