/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeGame_2 : Hololens_LifeGame_3D
{
    /// <summary>
    /// 全てのセルを更新
    /// </summary>

    private void Update_allCell()
    {

        int[,,] range; 

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    UpdateCell(x, y, z, range);
                }
            }
        }
    }



    /// <summary>
    /// セルの状態を更新
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="z">The z coordinate.</param>

    private void UpdateCell(int cellX, int cellY, int cellZ, int[,,]range)
    {

        int count = 0;
        for (int x = cellX - 1; x <= cellX + 1; x++)
        {
            for (int y = cellY - 1; y <= cellY + 1; y++)
            {
                for (int z = cellZ - 1; z <= cellZ + 1; z++)
                {
                    if ((x == cellX && y == cellY) && z == cellZ)
                    {
                        continue;
                    }

                    if (cells[(x + gridSize) % gridSize, (y + gridSize) % gridSize, (z + gridSize) % gridSize].Life)
                    {
                        count++;
                    }
                }
            }
        }
        Cell Cell = cells[cellX, cellY, cellZ];
        if (Cell.Life)
        {

            if (count <= 1 || count >= 4)
            {
                Cell.Die();
            }
        }
        else
        {

            if (count == 3)
            {
                Cell.Birth();
            }
        }
    }
}
*/