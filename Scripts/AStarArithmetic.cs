/*
 * Author:      zyj 
 * Date:        2020.05.14
 * Note:        寻路算法
 * --------------------------------------
 * V1 zyj 2020_0514创建文件
 */

/* 
* 算法计算公式F = G + H
* F = 移动需消耗的值（越小越好）
* G = 从起点 A 移动到指定方格的移动代价，沿着到达该方格而生成的路径----------------------横向纵向移动代价为10 对角线移动为14
* H = 从指定的方格移动到终点 B 的估算成本。-------------从当前方格横向或纵向移动到达目标所经过的方格数，忽略对角移动，然后把总数乘以 10 
* 
* ------------A是起点，B是障碍物，E是终点
* |     |     |     |     |     |     |     |
* |     |     |     |  B  |     |     |     |
* |     |  A  |     |  B  |     |  E  |     |
* |14,60|10,50|     |  B  |     |     |     |
* |     |     |     |     |     |     |     |
*/
using System.Collections.Generic;
using UnityEngine;

public struct AStarData
{
    public int F
    {
        get
        {
            return G + H;
        }
    }
    public int G { get; set; }
    public int H { get; set; }
    public AStarData(int g,int h)
    {
        G = g;
        H = h;
    }
}
public class AStarGrid
{
    public int X { get; set; }
    public int Y { get; set; }
    public AStarData data;
    public AStarGrid parent;

    public AStarGrid(int x,int y)
    {
        X = x;
        Y = y;
    }
    public AStarGrid(int x, int y,int g,int h)
    {
        X = x;
        Y = y;
        data = new AStarData(g, h);
    }
    public override string ToString()
    {
        return string.Format("X={0}  Y={1}", X, Y);
    }
}
public class AStarArithmetic:MonoBehaviour
{
    /// <summary>
    /// 周围格子的属性
    /// </summary>
    int[,] aroundpos = new int[8, 3] { { -1, -1, 14 }, { 0, -1, 10 }, { 1, -1, 14 }, { 1, 0, 10 }, { 1, 1, 14 }, { 0, 1, 10 }, { -1, 1, 14 }, { -1, 0, 10 } };


    List<AStarGrid> openlist = new List<AStarGrid>();

    List<AStarGrid> closelist = new List<AStarGrid>();

    List<AStarGrid> barrierlist = new List<AStarGrid>();

    /// <summary>
    /// 全格子列表
    /// </summary>
    List<AStarGrid> maps = new List<AStarGrid>();

    /// <summary>
    /// 设置地图
    /// </summary>
    void InitMap()
    {
        maps.Clear();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                var AStarGrid = new AStarGrid(i, j);
                maps.Add(AStarGrid);
            }
        }
    }
    /// <summary>
    /// 设置障碍物
    /// </summary>
    void InitBarrierList()
    {
        barrierlist.Add(new AStarGrid(4, 1));
        barrierlist.Add(new AStarGrid(4, 2));
        barrierlist.Add(new AStarGrid(4, 3));       
    }
    AStarGrid GetGoToGrid()
    {
        AStarGrid grid = openlist[0];
        int min = grid.data.F;
        for (int i = 1; i < openlist.Count; i++)
        {
            var data = openlist[i].data;
            if (data.F < min)
            {
                min = data.F;
                grid = openlist[i];
            }
        }
        return grid;
    }

    AStarGrid GetGridWithList(int x, int y, List<AStarGrid> list)
    {
        foreach (var item in list)
        {
            if (x == item.X && y == item.Y)
                return item;
        }
        return new AStarGrid(0, 0);
    }

    int GetHValue(int x, int y, AStarGrid endgrid)
    {
        int xcount = Mathf.Abs(endgrid.X - x);
        int ycount = Mathf.Abs(endgrid.Y - y);
        int Hvalue = (xcount + ycount) * 10;
        return Hvalue;
    }
    AStarGrid GetHMinBarrier(int x,int y)
    {
        AStarGrid mingrid = barrierlist[0];
        int min = 0;
        foreach (var item in barrierlist)
        {
            int curvalue = GetHValue(x, y, item);
            if (min == 0 || curvalue < min)
            {
                min = curvalue;
                mingrid = item;
            }
        }
        return mingrid;
    }
    bool IsInList(int x, int y, List<AStarGrid> list)
    {
        foreach (var item in list)
        {
            if (x == item.X && y == item.Y)
                return true;
        }
        return false;
    }
    bool CanMoveGrid(AStarGrid grid)
    {
        if(grid.data.G == 14 && barrierlist.Count>0)
        {
            var minbarrier = GetHMinBarrier(grid.X, grid.Y);
            if ((minbarrier.X + 1 == grid.X || minbarrier.X - 1 == grid.X) || (minbarrier.Y + 1 == grid.Y || minbarrier.Y - 1 == grid.Y))
                return false;
        }
        return true;
    }
    void AddAroundGrid(AStarGrid curgrid, AStarGrid endgrid)
    {
        for (int i = 0; i < aroundpos.GetLength(0); i++)
        {
            var x = curgrid.X + aroundpos[i, 0];
            var y = curgrid.Y + aroundpos[i, 1];

            if (IsInList(x, y, maps) && !IsInList(x, y, closelist) && !IsInList(x, y, barrierlist))
            {
                if (IsInList(x, y, openlist))
                {
                    var grid = GetGridWithList(x, y, openlist);
                    grid.data.G = aroundpos[i, 2];
                }
                else
                {
                    int gvalue = aroundpos[i, 2];
                    int hvalue = GetHValue(x, y, endgrid);
                    var grid = new AStarGrid(x, y, gvalue, hvalue);
                    if(CanMoveGrid(grid))
                    {
                        grid.parent = curgrid;
                        openlist.Add(grid);
                    }
                }
            }
        }
    }
    public AStarGrid CreatStart(int x, int y)
    {
        AStarGrid grid = new AStarGrid(x, y, 0, 0);
        openlist.Add(grid);
        return grid;
    }
    public AStarGrid CreatEnd(int x, int y)
    {
        AStarGrid grid = new AStarGrid(x, y, 0, 0);
        return grid;
    }
    public List<AStarGrid> StartAStarArithmetic(AStarGrid start, AStarGrid end)
    {
        InitMap();
        InitBarrierList();
        if (!IsInList(end.X, end.Y, maps) || !IsInList(start.X, start.Y, maps))
        {
            print("非法终点或起点坐标");
            return null;
        }
        while (openlist.Count > 0)
        {
            var gotogrid = GetGoToGrid();
            if (gotogrid.X == end.X && gotogrid.Y == end.Y)
            {
                end.parent = gotogrid.parent;
                break;
            }
            openlist.Remove(gotogrid);
            if(!closelist.Contains(gotogrid))
                closelist.Add(gotogrid);
            AddAroundGrid(gotogrid, end);
        }

        List<AStarGrid> theWay = new List<AStarGrid>();
        AStarGrid endgrid = end;

        while (endgrid.parent != null)
        {
            theWay.Add(endgrid);
            endgrid = endgrid.parent;
        }
        print("总移动次数 = " + theWay.Count);
        int movecount = 1;
        for (int i = theWay.Count - 1; i >= 0; i--)
        {
            print(string.Format("第{0}次移动，移动后坐标为{1}", movecount, theWay[i].ToString()));
            movecount++;
        }
        return theWay;
    }
    private void Start()
    {
        var start = CreatStart(2, 2);
        var end = CreatEnd(6, 2);
        StartAStarArithmetic(start,end);
    }
}
