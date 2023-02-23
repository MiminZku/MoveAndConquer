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
    public bool isObstacleSet = false;

    [SerializeField] Material[] materials;
    MeshRenderer meshRenderer;
    public enum TileColor { BASE, BLUE, RED };
    public TileColor color = TileColor.BASE;

    //
    public Index tileIndex;
    internal bool isCrash;
    [SerializeField] Animator tileAnimator;
    // Start is called before the first frame update 
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Flip(int mode)
    {
        if (FindObjectOfType<Player>().IsPlayerOnTile(this)) return;
        // 충돌 일어나는 tile 이면 색칠 패스
        if (isCrash) { return; }
        if (isObstacleSet) return;
        tileAnimator.SetTrigger("Flip");
        ChangeColor(mode);
    }

    public void ChangeColor(int mode)
    {
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
            case 3: // green
                meshRenderer.material = materials[3];
                break;
        }
    }
}
