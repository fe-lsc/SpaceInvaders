using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class main : MonoBehaviour
{
    public Material mat;    //Material utilizado para renderizar os objetos do jogo
    public Vector2 sb;      //Aparentemente a tela

    bool playState = false; //Booleana responsável por renderizar ou não os objetos do jogo
    public float posY = 0;      //Posição dos objetos no eixo Y
    public float posX = 0;      //Posição dos objetos no eixo X

    public bool enemiesWalk = true;
    public bool walkAux = true;
    Stopwatch stopWatch = new Stopwatch();

    List<Enemy> enemies = new List<Enemy>();

    Player player = new Player(0, 5-Screen.height/2);

    List<Shot> gameShots = new List<Shot>();



    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*30)), 100);
            enemies.Add(enemy);
        }
        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*30)), 70);
            enemies.Add(enemy);
        }
        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*30)), 40);
            enemies.Add(enemy);
        }

        stopWatch.Start();


    }

    // Update is called once per frame
    private void Update()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
        if(stopWatch.Elapsed.Seconds % 19 == 0 && stopWatch.Elapsed.Seconds != 0 && walkAux){
            foreach(Enemy enemy in enemies){
                enemy.posY = enemy.posY - enemy.dy;
            }
            enemiesWalk = !enemiesWalk;
            walkAux = false;
        }

        if(stopWatch.Elapsed.Seconds % 3 == 0 && enemiesWalk && stopWatch.Elapsed.Seconds != 0 && stopWatch.Elapsed.Seconds % 19 != 0){
            foreach(Enemy enemy in enemies){
                enemy.posX = enemy.posX + enemy.dx;
            }
            walkAux = true;
        }
        else if(stopWatch.Elapsed.Seconds % 3 == 0 && !enemiesWalk && stopWatch.Elapsed.Seconds != 0 && stopWatch.Elapsed.Seconds % 19 != 0){
            foreach(Enemy enemy in enemies){
                enemy.posX = enemy.posX - enemy.dx;
            }
            walkAux  = true;
        }
        

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            player.posX -= player.dx;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            player.posX += player.dx;
        }
        if (((Input.GetKey(KeyCode.Space)) || (Input.GetKey(KeyCode.UpArrow))) && player.shot)
        {
            gameShots.Add(player.shooting());
            player.shot = false;
        }

        foreach (var bullet in gameShots)
        {
            bullet.posY = bullet.posY + bullet.dy;

            if(player.collides(bullet)){
                gameShots.Remove(bullet);
            }

            
            if(bullet.posY > Screen.height - 100 || (bullet.posY+20 < (0 - Screen.height/2) && bullet.dy < 0)){
                gameShots.Remove(bullet);
                
                if(bullet.Equals(player.bullet)){
                    player.shot = true;
                }
            }

            foreach (var enemy in enemies)
            {
                if(enemy.collides(bullet) && bullet.dy > 0){

                    enemies.Remove(enemy);

                    gameShots.Remove(bullet);
                    player.shot = true;
                }
            }
            
        }

        foreach (var enemy in enemies)
        {
            if(Random.Range(0,1000) > 998){
                gameShots.Add(enemy.shooting());
            } 
        }

    }

    private void OnPostRender()
    {
        foreach(Enemy enemy in enemies){
            enemy.hitBox(mat);

        }

        player.hitBox(mat);

        foreach (var bullet in gameShots)
        {
            bullet.hitBox(mat);
        }
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
        public Shot bullet;
        public Player(float posX, float posY){
            this.posX = posX;
            this.posY = posY;

            this.inGame = true;
            this.shot = true;
        }

        public bool collides(Shot shot){
            if(shot.posY+7 > this.posY && shot.posY < this.posY+10 && shot.posX+5 > this.posX && shot.posX < this.posX+30 ){
                return true;
            }

            return false;
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

        public Shot shooting(){
            Shot playerShot = new Shot(this.posX+15, this.posY+12, 2);
            this.bullet = playerShot;

            return playerShot;
        }
    }

    public class Enemy{
        public float posX;
        public float posY;
        public float dx = 0.25f;
        public float dy = 2;
        public bool inGame;
        public Enemy(float posX, float posY){
            this.posX = posX;
            this.posY = posY;

            this.inGame = true;
        }

        public bool collides(Shot shot){
            if(shot.posY+7 > this.posY && shot.posY < this.posY+20 && shot.posX+5 > this.posX && shot.posX < this.posX+20 ){
                return true;
            }

            return false;
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

        public Shot shooting(){
            Shot enemyShot = new Shot(this.posX+10, this.posY-2, -2);

            return enemyShot;
        }
    }

    public class Cover{
        public float posX;
        public float posY;
        public Cover(float posX, float posY){
            this.posX = posX;
            this.posY = posY;
        }

        public void hitBox(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Color(Color.red);
            GL.Begin(GL.QUADS);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+25, 0);
            GL.Vertex3(this.posX+25, this.posY+25, 0);
            GL.Vertex3(this.posX+25, this.posY, 0);

            GL.End();
            GL.PopMatrix();
        }
    }

    public class Shot{

        public float posX;
        public float posY;
        public float dy;
        public Shot(float posX, float posY, float dy){
            this.posX = posX;
            this.posY = posY;
            this.dy = dy;
        }

        public void hitBox(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.QUADS);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+7, 0);
            GL.Vertex3(this.posX+5, this.posY+7, 0);
            GL.Vertex3(this.posX+5, this.posY, 0);

            GL.End();
            GL.PopMatrix();
        }
    }
    #endregion
}
