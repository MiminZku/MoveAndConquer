using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Index
{
    public int row;
    public int col;

    public Index(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
}


public class Tile : MonoBehaviour
{
    //flag
    public bool isObstacleInput { get; set; } = false;

    [SerializeField] Material[] materials;
    MeshRenderer meshRenderer;
    public enum TileColor { BASE, RED, BLUE };
    public TileColor color = TileColor.BASE;

    //
    public Index tileIndex;
    internal bool isCrash;
    public bool isObstacleSet = false;

    // Start is called before the first frame update 
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeColor(int mode)
    {
        // 충돌 일어나는 tile 이면 색칠 패스
         if(isCrash) { return; }
        if (isObstacleInput) return;

        switch (mode)
        {
            case 0: // gray
                meshRenderer.material = materials[0];
                color = TileColor.BASE;
                break;
            case 1: // player1 (Blue)
                meshRenderer.material = materials[1];
                color = TileColor.BLUE;
                break;
            case 2: // player2 (red)
                meshRenderer.material = materials[2];
                color = TileColor.RED;
                break;
        }
    }
}
