using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    public Material mat;    //Material utilizado para renderizar os objetos do jogo
    public Vector2 sb;      //Aparentemente a tela

    bool playState = false; //Booleana responsável por renderizar ou não os objetos do jogo
    public float posY = 0;      //Posição dos objetos no eixo Y
    public float posX = 0;      //Posição dos objetos no eixo X
    Enemy enemy = new Enemy(0-Screen.width/2, 0);

    Player player = new Player(0, 5-Screen.height/2);



    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
    }

    // Update is called once per frame
    private void Update()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            player.posX -= player.dx;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            player.posX += player.dx;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            player.shot = true;
        }
    }

    private void OnPostRender()
    {
       enemy.hitBox(mat);
       player.hitBox(mat);
    }
    #endregion

    #region My Methods

    #endregion

    
    #region My Objects

    public class Player{
        public float posX;
        public float posY;
        public float dx = 2;
        public bool inGame;
        public bool shot;
        public Player(float posX, float posY){
            this.posX = posX;
            this.posY = posY;

            this.inGame = true;
            this.shot = false;
        }

        public void hitBox(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.QUADS);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+10, 0);
            GL.Vertex3(this.posX+30, this.posY+10, 0);
            GL.Vertex3(this.posX+30, this.posY, 0);

            GL.End();
            GL.PopMatrix();
        }
    }

    public class Enemy{
        public float posX;
        public float posY;
        public float dx = 1;
        public float dy = 1;
        public bool inGame;
        public bool shot;
        public Enemy(float posX, float posY){
            this.posX = posX;
            this.posY = posY;

            this.inGame = true;
            this.shot = false;
        }

        public void hitBox(Material mat){

            GL.PushMatrix();
            mat.SetPass(0);
            GL.Color(Color.red);
            GL.Begin(GL.QUADS);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+20, 0);
            GL.Vertex3(this.posX+20, this.posY+20, 0);
            GL.Vertex3(this.posX+20, this.posY, 0);

            GL.End();
            GL.PopMatrix();

        }
    }

    public class Cover{
        public Cover(){

        }
    }
    #endregion
}
