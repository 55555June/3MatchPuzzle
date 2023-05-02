using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Item
{
    Bomb,
    ColumnArrow,
    RowArrow
}

public class MatchesCheck : MonoBehaviour
{
    private BackBoard board;
    public List<GameObject> currentMatches = new List<GameObject>();

    void Start()
    {
        board = FindObjectOfType<BackBoard>();
    }
    
    public void FindAllMatches()
    {
        StartCoroutine(FIndAllMatchesCoroutine());
    }

   

    private IEnumerator FIndAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        for(int i =0; i< board.boardWidth; i++)
        {
            for(int j =0; j< board.boardheight; j++)
            {
                GameObject currentObject = board.totalTiles[i, j];
                
                if(currentObject != null)
                {
                    Tile currentTile = currentObject.GetComponent<Tile>();
                    if (i > 0 && i < board.boardWidth -1)                                                                //가로검사
                    {
                        GameObject leftObject = board.totalTiles[i - 1, j];
                        GameObject rightObject = board.totalTiles[i + 1, j];

                        if(leftObject != null && rightObject != null)
                        {
                            Tile leftTile = leftObject.GetComponent<Tile>();
                            Tile rightTile = rightObject.GetComponent<Tile>();

                            if (leftObject != null && rightObject != null)
                            {
                                if (leftObject.tag == currentObject.tag && rightObject.tag == currentObject.tag) //타일이 옮겨진 후 내 타일과 가로타일(x-1,x+2)이 같다면 
                                {

                                    currentMatches.Union(IsRowArrowBomb(leftTile, currentTile, rightTile));         //부서질 타일중 가로열폭탄있으면 해당 아이템 사용시킴
                               
                                    currentMatches.Union(IsColumnArrowBomb(leftTile, currentTile, rightTile));     //부서질 타일중 세로열폭탄있으면 해당 아이템 사용시킴

                                    currentMatches.Union(IsBomb(leftTile, currentTile, rightTile));                      //부서질 타일중 3x3폭탄있으면 해당 아이템 사용시킴

                                    GetMatchedTiles(leftObject, currentObject, rightObject);                            //매칭되어있는 오브젝트들을 하나로 모음
                                }
                            }
                        }
                    }
                    if (j > 0 && j < board.boardheight - 1)                                                                 //세로검사
                    {
                        GameObject upObject = board.totalTiles[i, j - 1];
                        GameObject downObject = board.totalTiles[i, j + 1];
                        if(upObject != null && downObject != null)
                        {
                            Tile upTile = upObject.GetComponent<Tile>();
                            Tile downTile = downObject.GetComponent<Tile>();
                            if (upObject != null && downObject != null)
                            {
                                if (upObject.tag == currentTile.tag && downObject.tag == currentTile.tag)         //타일이 옮겨진 후 내 타일과 세로타일(y-1,y+2)이 같다면 
                                {
                                 
                                    currentMatches.Union(IsRowArrowBomb(upTile, currentTile, downTile));         //부서질 타일중 가로열폭탄있으면 해당 아이템 사용시킴

                                    currentMatches.Union(IsColumnArrowBomb(upTile, currentTile, downTile));     //부서질 타일중 세로열폭탄있으면 해당 아이템 사용시킴

                                    currentMatches.Union(IsBomb(upTile, currentTile, downTile));                      //부서질 타일중 3x3폭탄있으면 해당 아이템 사용시킴

                                    GetMatchedTiles(upObject, currentObject, downObject);                           //매칭되어있는 오브젝트들을 하나로 모음
                              
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private List<GameObject> IsBomb(Tile _tile1, Tile _tile2, Tile _tile3)                       // 3x3폭탄아이템 왼쪽,가운데,오른쪽 , 위쪽,가운데,오른쪽 체크   
    {
        List<GameObject> currentTiles = new List<GameObject>();

        if (_tile1.isBomb)
        {
            UseItem(Item.Bomb, _tile1.column, _tile1.row);
        }
        if (_tile2.isBomb)
        {
            UseItem(Item.Bomb, _tile2.column, _tile2.row);
        }
        if (_tile3.isBomb)
        {
            UseItem(Item.Bomb, _tile3.column, _tile3.row);
        }

        return currentTiles;
    }

    private List<GameObject> IsRowArrowBomb(Tile _tile1, Tile _tile2, Tile _tile3)          // 가로열 폭탄 아이템 왼쪽,가운데,오른쪽 , 위쪽,가운데,오른쪽 체크  
    {
        List<GameObject> currentTiles = new List<GameObject>();

        if (_tile1.isRowArrowBomb)
        {
            UseItem(Item.RowArrow, _tile1.row);
        }
        if (_tile2.isRowArrowBomb)
        {
            UseItem(Item.RowArrow, _tile2.row);
        }
        if (_tile3.isRowArrowBomb)
        {
            UseItem(Item.RowArrow, _tile3.row);
        }

        return currentTiles;
    }

    private List<GameObject> IsColumnArrowBomb(Tile _tile1, Tile _tile2, Tile _tile3)       // 세로열 폭탄 아이템 왼쪽,가운데,오른쪽 , 위쪽,가운데,오른쪽 체크  
    {
        List<GameObject> currentTiles = new List<GameObject>();

        if (_tile1.isColumnArrowBomb)
        {
            UseItem(Item.ColumnArrow, _tile1.column);
        }
        if (_tile2.isColumnArrowBomb)
        {
            UseItem(Item.ColumnArrow, _tile2.column);
        }
        if (_tile3.isColumnArrowBomb)
        {
            UseItem(Item.ColumnArrow, _tile3.column);
        }

        return currentTiles;
    }

    private void GetMatchedTiles(GameObject _tile1, GameObject _tile2, GameObject _tile3)// 3매칭 된 타일들 모을 함수
    {
        AddToMatchedTylist(_tile1);
        AddToMatchedTylist(_tile2);
        AddToMatchedTylist(_tile3);
    }

    private void AddToMatchedTylist(GameObject _tile)                                               //매칭된타일을 매칭되어있는 리스트 중복검사후 추가 및 타일삭제
    {
        if (!currentMatches.Contains(_tile))
        {
            currentMatches.Add(_tile);
        }
        _tile.GetComponent<Tile>().isMatched = true;
    }

    public void UseItem(Item item, int _column = 0, int _row = 0)   //폭탄 아이템 사용
    {
        currentMatches.Union(GetBombTiles(_column, _row));
    }

    public void UseItem(Item item, int _value = 0)                      //가로열,세로열 아이템사용
    {
        switch (item)
        {
            case Item.ColumnArrow:
                currentMatches.Union(GetColumnTiles(_value));
                break;
            case Item.RowArrow:
                currentMatches.Union(GetRowTiles(_value));
                break;
        }
    }

    List<GameObject> GetBombTiles(int column,int row)               //폭탄 아이템 / 터트릴 타일 체크 및 리턴
    {
        if (board.bombRange <= 0)
        {
            Debug.LogError("폭탄범위를 0보다 크게 설정하세요");
        }

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
                        if (tile.isRowArrowBomb)
                        {
                            tilles.Union(GetRowTiles(j)).ToList();
                        }
                        if (tile.isColumnArrowBomb)
                        {
                            tilles.Union(GetColumnTiles(i)).ToList();
                        }
                        tilles.Add(board.totalTiles[i, j]);
                        board.totalTiles[i, j].GetComponent<Tile>().isMatched = true;
                    }
                }
            }
        }
        
        return tilles;
    }



    List<GameObject> GetColumnTiles(int column)                    //세로열 폭탄  / 터트릴 타일 체크 및 리턴
    {
        List<GameObject> tilles = new List<GameObject>();
        for(int i=0; i<board.boardheight; i++)
        {
            if(board.totalTiles[column, i] != null)
            {
                Tile tile = board.totalTiles[column, i].GetComponent<Tile>();
                if(tile != null)
                {
                    if (tile.isBomb)
                    {
                        tilles.Union(GetBombTiles(column, i)).ToList();
                    }
                    else if (tile.isRowArrowBomb)
                    {
                        tilles.Union(GetRowTiles(i)).ToList();
                    }
                    else
                    {
                        tilles.Add(board.totalTiles[column, i]);
                        tile.isMatched = true;
                    }
                }
            }
        }
        return tilles;
    }

    List<GameObject> GetRowTiles(int row)                             //가로열 폭탄  / 터트릴 타일 체크 및 리턴
    {
        List<GameObject> tilles = new List<GameObject>();
        for (int i = 0; i < board.boardWidth; i++)
        {
            if (board.totalTiles[i, row] != null)
            {
                Tile tile = board.totalTiles[i, row].GetComponent<Tile>();
                if(tile != null)
                {
                    if (tile.isBomb)
                    {
                        tilles.Union(GetBombTiles(i, row)).ToList();
                    }
                    else if (tile.isColumnArrowBomb)
                    {
                        tilles.Union(GetColumnTiles(i)).ToList();
                    }
                    else
                    {
                        tilles.Add(board.totalTiles[i, row]);
                        tile.isMatched = true;
                    }
                }
            }
        }
        return tilles;
    }

    public void CreateRandomItem()
    {
        if(board.currentTile != null)
        {
            if(board.currentTile.isMatched)
            {
                if (!isItemCheck()) { return; }                         //아이템 효과를 가지고있다면 리턴

                board.currentTile.isMatched = false;

                int randValue = Random.Range(0, 100);
                Debug.Log(randValue);
                if (randValue <34)
                {
                    board.currentTile.ActiveItem(Item.RowArrow);
                }
                else if (randValue >= 34 && randValue < 67)
                {
                    board.currentTile.ActiveItem(Item.ColumnArrow);
                }
                else
                {
                    board.currentTile.ActiveItem(Item.Bomb);
                }
            }
            else if( board.currentTile.otherTile != null)
            {
                Tile otherTile = board.currentTile.otherTile.GetComponent<Tile>();

                if(otherTile.isMatched)
                {
                    otherTile.isMatched = false;
               
                    int randValue = Random.Range(0, 100);
                    Debug.Log(randValue);
                    if (randValue < 34)
                    {
                        otherTile.ActiveItem(Item.RowArrow);
                    }
                    else if (randValue >= 34 && randValue < 67)
                    {
                        otherTile.ActiveItem(Item.ColumnArrow);
                    }
                    else
                    {
                        otherTile.ActiveItem(Item.Bomb);
                    }
                }
            }
        }
    }

    bool isItemCheck()
    {
        if(board.currentTile.isColumnArrowBomb || board.currentTile.isRowArrowBomb || board.currentTile.isBomb)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
