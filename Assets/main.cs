using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour
{
    public Material mat;    //Material utilizado para renderizar os objetos do jogo
    public Vector2 sb;      //Aparentemente a tela

    public bool playState; //Booleana responsável por renderizar ou não os objetos do jogo
    public bool winState; //Booleana responsável por renderizar ou não os objetos do jogo

    public bool enemiesWalk = true;
    public bool walkAux = true;
    Stopwatch stopWatch = new Stopwatch();

    List<Enemy> enemies = new List<Enemy>();

    Player player = new Player(0, 5-Screen.height/2);

    List<Shot> gameShots = new List<Shot>();

    public int life;
    public int points;

    public Text tPoints; 
    public Text tLife; 



    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        life = 2;
        points = 0;

        //Montagem dos inimigos
        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*38)), 100);
            enemies.Add(enemy);
        }
        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*38)), 70);
            enemies.Add(enemy);
        }
        for(int i = 0; i < 6; i++){
            Enemy enemy = new Enemy(((0-Screen.width/2)+(i*38)), 40);
            enemies.Add(enemy);
        }

        //Cronometro para controle geral do jogo
        stopWatch.Start();


    }

    // Update is called once per frame
    private void Update()
    {
        sb = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        if(playState){

            //Caso o inimigo chegue no fim da tela: descer e mudar a direcao do eixo X.
            foreach(Enemy enemy in enemies){
                if((enemy.posX > Screen.width/2 || enemy.posX < 0-Screen.width/2) && stopWatch.Elapsed.Seconds > 5 && walkAux){
                    foreach(Enemy enemyy in enemies){
                        enemyy.posY = enemyy.posY - enemyy.dy;
                    }
                    enemiesWalk = !enemiesWalk;
                    walkAux = false;
                }
            }


            //Inimigos se movem por 1s a cada 3s para a direita.
            if(stopWatch.Elapsed.Seconds % 3 == 0 && enemiesWalk && stopWatch.Elapsed.Seconds != 0){
                foreach(Enemy enemy in enemies){
                    enemy.posX = enemy.posX + enemy.dx;
                }
                walkAux = true;
            }

            //Inimigos se movem por 1s a cada 3s para a esquerda.
            else if(stopWatch.Elapsed.Seconds % 3 == 0 && !enemiesWalk && stopWatch.Elapsed.Seconds != 0){
                foreach(Enemy enemy in enemies){
                    enemy.posX = enemy.posX - enemy.dx;
                }
                walkAux  = true;
            }
            
            //Movimento nave: para a esquerda
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                player.posX -= player.dx;
            }
            //Movimento nave: para a direita
            if (Input.GetKey(KeyCode.RightArrow))
            {
                player.posX += player.dx;
            }
            //Tiro da nave, caso o player esteja vivo.
            if (((Input.GetKey(KeyCode.Space)) || (Input.GetKey(KeyCode.UpArrow))) && player.shot && player.inGame)
            {
                gameShots.Add(player.shooting());
                player.shot = false;
            }
            if(Input.GetKey(KeyCode.R) && !player.inGame)
            {
                player.inGame = true;
            }

            

            //Controle de colisao dos tiros do jogo
            foreach (var bullet in gameShots)
            {
                bullet.posY = bullet.posY + bullet.dy;

                //Se tiro colidir com o player
                if(player.collides(bullet) && player.inGame){
                    gameShots.Remove(bullet);
                    player.inGame = false;
                    life--;

                    if(life < 1){
                        playState = false;
                    }

                }

                //Se tiro sair da tela
                if(bullet.posY > Screen.height - 100 || (bullet.posY+20 < (0 - Screen.height/2) && bullet.dy < 0)){
                    gameShots.Remove(bullet);
                    
                    if(bullet.Equals(player.bullet)){
                        player.shot = true;
                    }
                }

                //Se tiro colidir com os inimigos e esteja viajando para cima
                foreach (var enemy in enemies)
                {
                    if(enemy.collides(bullet) && bullet.dy > 0){

                        enemies.Remove(enemy);

                        gameShots.Remove(bullet);
                        player.shot = true;
                        points+= 50;
                    }
                }
                
            }    
            //Controle dos tiros dos inimigos
            foreach (var enemy in enemies)
            {
                if(Random.Range(0,1000) > 998){
                    gameShots.Add(enemy.shooting());
                } 
            }

            if(enemies.Count == 0){
                playState = false;
            }

        }
        else{
            if(Input.GetKey(KeyCode.Space)){
                enemies.Clear();
                gameShots.Clear();
                Start();
                player.inGame = true;
                player.shot = true;
                playState = true;
            }
        }

        tPoints.text = "Points: "+points.ToString();
        tLife.text = "Lifes: "+life.ToString();

    }

    private void OnPostRender()
    {

        if(playState){

            //Renderização dos inimigos
            foreach(Enemy enemy in enemies){
                //enemy.hitBox(mat);
                enemy.enemyArt(mat);

            }

            //Renderização do player
            if(player.inGame){
                //player.hitBox(mat);
                player.playerArt(mat);
            }


            //Renderização das balas
            foreach (var bullet in gameShots)
            {
                bullet.hitBox(mat);
            }
        }
        
    }
    #endregion

    #region My Methods
    public void StartGame(){
        playState = true;
    }

    #endregion

    
    #region My Objects


    public class Player{
        public float posX;
        public float posY;
        public float dx = 2;    //Velocidade no eixo X
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
            if(shot.posY > this.posY && shot.posY+7 < this.posY+19 && shot.posX+5 > this.posX && shot.posX < this.posX+55 ){
                return true;
            }

            return false;
        }

        public void hitBox(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.QUADS);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+19, 0);
            GL.Vertex3(this.posX+55, this.posY+19, 0);
            GL.Vertex3(this.posX+55, this.posY, 0);

            GL.End();
            GL.PopMatrix();
        }
        public void playerArt(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            GL.Vertex3(this.posX, this.posY, 0);
            GL.Vertex3(this.posX, this.posY+3, 0);
                GL.Vertex3(this.posX, this.posY+3, 0);
                GL.Vertex3(this.posX+3, this.posY+3, 0);
            GL.Vertex3(this.posX+3, this.posY+3, 0);
            GL.Vertex3(this.posX+3, this.posY+6, 0);
                GL.Vertex3(this.posX+3, this.posY+6, 0);
                GL.Vertex3(this.posX+6, this.posY+6, 0);
            GL.Vertex3(this.posX+6, this.posY+6, 0);
            GL.Vertex3(this.posX+6, this.posY+9, 0);
                GL.Vertex3(this.posX+6, this.posY+9, 0);
                GL.Vertex3(this.posX+9, this.posY+9, 0);
            GL.Vertex3(this.posX+9, this.posY+9, 0);
            GL.Vertex3(this.posX+9, this.posY+15, 0);
                GL.Vertex3(this.posX+9, this.posY+15, 0);
                GL.Vertex3(this.posX+12, this.posY+15, 0);
            GL.Vertex3(this.posX+12, this.posY+15, 0);
            GL.Vertex3(this.posX+12, this.posY+24, 0);
                GL.Vertex3(this.posX+12, this.posY+24, 0);
                GL.Vertex3(this.posX+15, this.posY+24, 0);
            GL.Vertex3(this.posX+15, this.posY+24, 0);
            GL.Vertex3(this.posX+15, this.posY+18, 0);
                GL.Vertex3(this.posX+15, this.posY+18, 0);
                GL.Vertex3(this.posX+21, this.posY+18, 0);
            GL.Vertex3(this.posX+21, this.posY+18, 0);
            GL.Vertex3(this.posX+21, this.posY+24, 0);
                GL.Vertex3(this.posX+21, this.posY+24, 0);
                GL.Vertex3(this.posX+24, this.posY+24, 0);
            GL.Vertex3(this.posX+24, this.posY+24, 0);
            GL.Vertex3(this.posX+24, this.posY+33, 0);
                GL.Vertex3(this.posX+24, this.posY+33, 0);
                GL.Vertex3(this.posX+30, this.posY+33, 0);
            GL.Vertex3(this.posX+30, this.posY+33, 0);
            GL.Vertex3(this.posX+30, this.posY+24, 0);
                GL.Vertex3(this.posX+30, this.posY+24, 0);
                GL.Vertex3(this.posX+33, this.posY+24, 0);
            GL.Vertex3(this.posX+33, this.posY+24, 0);
            GL.Vertex3(this.posX+33, this.posY+18, 0);
                GL.Vertex3(this.posX+33, this.posY+18, 0);
                GL.Vertex3(this.posX+39, this.posY+18, 0);
            GL.Vertex3(this.posX+39, this.posY+18, 0);
            GL.Vertex3(this.posX+39, this.posY+18, 0);
                GL.Vertex3(this.posX+39, this.posY+18, 0);
                GL.Vertex3(this.posX+39, this.posY+24, 0);
            GL.Vertex3(this.posX+39, this.posY+24, 0);
            GL.Vertex3(this.posX+42, this.posY+24, 0);
                GL.Vertex3(this.posX+42, this.posY+24, 0);
                GL.Vertex3(this.posX+42, this.posY+15, 0);
            GL.Vertex3(this.posX+42, this.posY+15, 0);
            GL.Vertex3(this.posX+45, this.posY+15, 0);
                GL.Vertex3(this.posX+45, this.posY+15, 0);
                GL.Vertex3(this.posX+45, this.posY+9, 0);
            GL.Vertex3(this.posX+45, this.posY+9, 0);
            GL.Vertex3(this.posX+48, this.posY+9, 0);
                GL.Vertex3(this.posX+48, this.posY+9, 0);
                GL.Vertex3(this.posX+48, this.posY+6, 0);
            GL.Vertex3(this.posX+48, this.posY+6, 0);
            GL.Vertex3(this.posX+51, this.posY+6, 0);
                GL.Vertex3(this.posX+51, this.posY+6, 0);
                GL.Vertex3(this.posX+51, this.posY+3, 0);
            GL.Vertex3(this.posX+51, this.posY+3, 0);
            GL.Vertex3(this.posX+54, this.posY+3, 0);
                GL.Vertex3(this.posX+54, this.posY+3, 0);
                GL.Vertex3(this.posX+54, this.posY+0, 0);
            GL.Vertex3(this.posX+54, this.posY+0, 0);
            GL.Vertex3(this.posX+48, this.posY+0, 0);
                GL.Vertex3(this.posX+48, this.posY+0, 0);
                GL.Vertex3(this.posX+48, this.posY-3, 0);
            GL.Vertex3(this.posX+48, this.posY-3, 0);
            GL.Vertex3(this.posX+45, this.posY-3, 0);
                GL.Vertex3(this.posX+45, this.posY-3, 0);
                GL.Vertex3(this.posX+45, this.posY, 0);
            GL.Vertex3(this.posX+45, this.posY, 0);
            GL.Vertex3(this.posX+42, this.posY, 0);
                GL.Vertex3(this.posX+42, this.posY, 0);
                GL.Vertex3(this.posX+42, this.posY+3, 0);
            GL.Vertex3(this.posX+42, this.posY+3, 0);
            GL.Vertex3(this.posX+39, this.posY+3, 0);
            //     GL.Vertex3(this.posX+39, this.posY+3, 0);
            //     GL.Vertex3(this.posX+39, this.posY-3, 0);
            // GL.Vertex3(this.posX+39, this.posY-3, 0);
            // GL.Vertex3(this.posX+36, this.posY-3, 0);
            //     GL.Vertex3(this.posX+36, this.posY-3, 0);
            //     GL.Vertex3(this.posX+36, this.posY-6, 0);
            // GL.Vertex3(this.posX+36, this.posY-6, 0);
            // GL.Vertex3(this.posX+33, this.posY-6, 0);
            //     GL.Vertex3(this.posX+33, this.posY-6, 0);
            //     GL.Vertex3(this.posX+33, this.posY, 0);
            // GL.Vertex3(this.posX+33, this.posY, 0);
            // GL.Vertex3(this.posX+30, this.posY, 0);
                // GL.Vertex3(this.posX+30, this.posY, 0);
                // GL.Vertex3(this.posX+30, this.posY+3, 0);
            GL.Vertex3(this.posX+30, this.posY+3, 0);
            GL.Vertex3(this.posX+24, this.posY+3, 0);
                GL.Vertex3(this.posX+24, this.posY+3, 0);
                GL.Vertex3(this.posX+24, this.posY, 0);
            // GL.Vertex3(this.posX+24, this.posY, 0);
            // GL.Vertex3(this.posX+21, this.posY, 0);
            //     GL.Vertex3(this.posX+21, this.posY, 0);
            //     GL.Vertex3(this.posX+21, this.posY-6, 0);
            // GL.Vertex3(this.posX+21, this.posY-6, 0);
            // GL.Vertex3(this.posX+18, this.posY-6, 0);
            //     GL.Vertex3(this.posX+18, this.posY-6, 0);
            //     GL.Vertex3(this.posX+18, this.posY-3, 0);
            // GL.Vertex3(this.posX+18, this.posY-3, 0);
            // GL.Vertex3(this.posX+15, this.posY-3, 0);
                GL.Vertex3(this.posX+15, this.posY-3, 0);
                GL.Vertex3(this.posX+15, this.posY+3, 0);
            GL.Vertex3(this.posX+15, this.posY+3, 0);
            GL.Vertex3(this.posX+12, this.posY+3, 0);
                GL.Vertex3(this.posX+12, this.posY+3, 0);
                GL.Vertex3(this.posX+12, this.posY, 0);
            GL.Vertex3(this.posX+12, this.posY, 0);
            GL.Vertex3(this.posX+9, this.posY, 0);
                GL.Vertex3(this.posX+9, this.posY, 0);
                GL.Vertex3(this.posX+9, this.posY-3, 0);
            GL.Vertex3(this.posX+9, this.posY-3, 0);
            GL.Vertex3(this.posX+6, this.posY-3, 0);
                GL.Vertex3(this.posX+6, this.posY-3, 0);
                GL.Vertex3(this.posX+6, this.posY, 0);
            GL.Vertex3(this.posX+6, this.posY, 0);
            GL.Vertex3(this.posX, this.posY, 0);

            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(237f/255f, 0, 0, 1));              //Vermelho

            GL.Vertex3(this.posX+39, this.posY+21, 0);
            GL.Vertex3(this.posX+39, this.posY+25, 0);
            GL.Vertex3(this.posX+42, this.posY+25, 0);
            GL.Vertex3(this.posX+42, this.posY+21, 0);

            GL.Vertex3(this.posX+12, this.posY+21, 0);
            GL.Vertex3(this.posX+12, this.posY+25, 0);
            GL.Vertex3(this.posX+15, this.posY+25, 0);
            GL.Vertex3(this.posX+15, this.posY+21, 0);

            GL.Vertex3(this.posX+36, this.posY, 0);
            GL.Vertex3(this.posX+36, this.posY-6, 0);
            GL.Vertex3(this.posX+33, this.posY-6, 0);
            GL.Vertex3(this.posX+33, this.posY, 0);

            GL.Vertex3(this.posX+21, this.posY, 0);
            GL.Vertex3(this.posX+21, this.posY-6, 0);
            GL.Vertex3(this.posX+18, this.posY-6, 0);
            GL.Vertex3(this.posX+18, this.posY, 0);

            GL.Vertex3(this.posX+24, this.posY, 0);
            GL.Vertex3(this.posX+24, this.posY+3, 0);
            GL.Vertex3(this.posX+21, this.posY+3, 0);
            GL.Vertex3(this.posX+21, this.posY, 0);

            GL.Vertex3(this.posX+33, this.posY, 0);
            GL.Vertex3(this.posX+33, this.posY+3, 0);
            GL.Vertex3(this.posX+30, this.posY+3, 0);
            GL.Vertex3(this.posX+30, this.posY, 0);

            GL.Color(new Color(230f/255f, 157f/255f, 27f/255f, 1));   //Amarelo

            GL.Vertex3(this.posX+36, this.posY, 0);
            GL.Vertex3(this.posX+36, this.posY+3, 0);
            GL.Vertex3(this.posX+33, this.posY+3, 0);
            GL.Vertex3(this.posX+33, this.posY, 0);

            GL.Vertex3(this.posX+21, this.posY, 0);
            GL.Vertex3(this.posX+21, this.posY+3, 0);
            GL.Vertex3(this.posX+18, this.posY+3, 0);
            GL.Vertex3(this.posX+18, this.posY, 0);

            GL.Color(new Color(248f/255f, 105f/255f, 18f/255f, 1));   //Laranja
            
            GL.Vertex3(this.posX+18, this.posY-3, 0);
            GL.Vertex3(this.posX+18, this.posY+3, 0);
            GL.Vertex3(this.posX+15, this.posY+3, 0);
            GL.Vertex3(this.posX+15, this.posY-3, 0);
            
            GL.Vertex3(this.posX+39, this.posY-3, 0);
            GL.Vertex3(this.posX+39, this.posY+3, 0);
            GL.Vertex3(this.posX+36, this.posY+3, 0);
            GL.Vertex3(this.posX+36, this.posY-3, 0);
            
            GL.End();
            GL.PopMatrix();
        }

        public Shot shooting(){
            Shot playerShot = new Shot(this.posX+25, this.posY+12, 3);
            this.bullet = playerShot;

            return playerShot;
        }
    }

    public class Enemy{
        public float posX;
        public float posY;
        public float dx = 0.5f;
        public float dy = 20;
        public bool inGame;
        public Enemy(float posX, float posY){
            this.posX = posX;
            this.posY = posY;

            this.inGame = true;
        }

        public void enemyArt(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            GL.Vertex3(this.posX+10, this.posY, 0);
            GL.Vertex3(this.posX+14, this.posY, 0);
                GL.Vertex3(this.posX+14, this.posY, 0);
                GL.Vertex3(this.posX+14, this.posY+2, 0);
            GL.Vertex3(this.posX+14, this.posY+2, 0);
            GL.Vertex3(this.posX+16, this.posY+2, 0);
                GL.Vertex3(this.posX+16, this.posY+2, 0);
                GL.Vertex3(this.posX+16, this.posY, 0);
            GL.Vertex3(this.posX+16, this.posY, 0);
            GL.Vertex3(this.posX+20, this.posY, 0);
                GL.Vertex3(this.posX+20, this.posY, 0);
                GL.Vertex3(this.posX+20, this.posY+2, 0);
            GL.Vertex3(this.posX+20, this.posY+2, 0);
            GL.Vertex3(this.posX+22, this.posY+2, 0);
                GL.Vertex3(this.posX+22, this.posY+2, 0);
                GL.Vertex3(this.posX+22, this.posY+4, 0);
            GL.Vertex3(this.posX+22, this.posY+4, 0);
            GL.Vertex3(this.posX+24, this.posY+4, 0);
                GL.Vertex3(this.posX+24, this.posY+4, 0);
                GL.Vertex3(this.posX+24, this.posY+6, 0);
            GL.Vertex3(this.posX+24, this.posY+6, 0);
            GL.Vertex3(this.posX+26, this.posY+6, 0);
                GL.Vertex3(this.posX+26, this.posY+6, 0);
                GL.Vertex3(this.posX+26, this.posY+10, 0);
            GL.Vertex3(this.posX+26, this.posY+10, 0);
            GL.Vertex3(this.posX+28, this.posY+10, 0);
                GL.Vertex3(this.posX+28, this.posY+10, 0);
                GL.Vertex3(this.posX+28, this.posY+12, 0);
            GL.Vertex3(this.posX+28, this.posY+12, 0);
            GL.Vertex3(this.posX+30, this.posY+12, 0);
                GL.Vertex3(this.posX+30, this.posY+12, 0);
                GL.Vertex3(this.posX+30, this.posY+18, 0);
            GL.Vertex3(this.posX+30, this.posY+18, 0);
            GL.Vertex3(this.posX+26, this.posY+18, 0);
                GL.Vertex3(this.posX+26, this.posY+18, 0);
                GL.Vertex3(this.posX+26, this.posY+16, 0);
            GL.Vertex3(this.posX+26, this.posY+16, 0);
            GL.Vertex3(this.posX+22, this.posY+16, 0);
                GL.Vertex3(this.posX+22, this.posY+16, 0);
                GL.Vertex3(this.posX+22, this.posY+14, 0);
            GL.Vertex3(this.posX+22, this.posY+14, 0);
            GL.Vertex3(this.posX+20, this.posY+14, 0);
                GL.Vertex3(this.posX+20, this.posY+14, 0);
                GL.Vertex3(this.posX+20, this.posY+10, 0);
            GL.Vertex3(this.posX+20, this.posY+10, 0);
            GL.Vertex3(this.posX+18, this.posY+10, 0);
                GL.Vertex3(this.posX+18, this.posY+10, 0);
                GL.Vertex3(this.posX+18, this.posY+16, 0);
            GL.Vertex3(this.posX+18, this.posY+16, 0);
            GL.Vertex3(this.posX+16, this.posY+16, 0);
                GL.Vertex3(this.posX+16, this.posY+16, 0);
                GL.Vertex3(this.posX+16, this.posY+14, 0);
            GL.Vertex3(this.posX+16, this.posY+14, 0);
            GL.Vertex3(this.posX+14, this.posY+14, 0);
                GL.Vertex3(this.posX+14, this.posY+14, 0);
                GL.Vertex3(this.posX+14, this.posY+16, 0);
            GL.Vertex3(this.posX+14, this.posY+16, 0);
            GL.Vertex3(this.posX+12, this.posY+16, 0);
                GL.Vertex3(this.posX+12, this.posY+16, 0);
                GL.Vertex3(this.posX+12, this.posY+10, 0);
            GL.Vertex3(this.posX+12, this.posY+10, 0);
            GL.Vertex3(this.posX+10, this.posY+10, 0);
                GL.Vertex3(this.posX+10, this.posY+10, 0);
                GL.Vertex3(this.posX+10, this.posY+14, 0);
            GL.Vertex3(this.posX+10, this.posY+14, 0);
            GL.Vertex3(this.posX+8, this.posY+14, 0);
                GL.Vertex3(this.posX+8, this.posY+14, 0);
                GL.Vertex3(this.posX+8, this.posY+16, 0);
            GL.Vertex3(this.posX+8, this.posY+16, 0);
            GL.Vertex3(this.posX+4, this.posY+16, 0);
                GL.Vertex3(this.posX+4, this.posY+16, 0);
                GL.Vertex3(this.posX+4, this.posY+18, 0);
            GL.Vertex3(this.posX+4, this.posY+18, 0);
            GL.Vertex3(this.posX, this.posY+18, 0);
                GL.Vertex3(this.posX, this.posY+18, 0);
                GL.Vertex3(this.posX, this.posY+12, 0);
            GL.Vertex3(this.posX, this.posY+12, 0);
            GL.Vertex3(this.posX+2, this.posY+12, 0);
                GL.Vertex3(this.posX+2, this.posY+12, 0);
                GL.Vertex3(this.posX+2, this.posY+10, 0);
            GL.Vertex3(this.posX+2, this.posY+10, 0);
            GL.Vertex3(this.posX+4, this.posY+10, 0);
                GL.Vertex3(this.posX+4, this.posY+10, 0);
                GL.Vertex3(this.posX+4, this.posY+6, 0);
            GL.Vertex3(this.posX+4, this.posY+6, 0);
            GL.Vertex3(this.posX+6, this.posY+6, 0);
                GL.Vertex3(this.posX+6, this.posY+6, 0);
                GL.Vertex3(this.posX+6, this.posY+4, 0);
            GL.Vertex3(this.posX+6, this.posY+4, 0);
            GL.Vertex3(this.posX+8, this.posY+4, 0);
                GL.Vertex3(this.posX+8, this.posY+4, 0);
                GL.Vertex3(this.posX+8, this.posY+2, 0);
            GL.Vertex3(this.posX+8, this.posY+2, 0);
            GL.Vertex3(this.posX+10, this.posY+2, 0);
                GL.Vertex3(this.posX+10, this.posY+2, 0);
                GL.Vertex3(this.posX+10, this.posY, 0);

            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(237f/255f, 0, 0, 1));              //Vermelho

            GL.Vertex3(this.posX+12, this.posY+8, 0);
            GL.Vertex3(this.posX+14, this.posY+8, 0);
            GL.Vertex3(this.posX+14, this.posY+10, 0);
            GL.Vertex3(this.posX+12, this.posY+10, 0);

            GL.Vertex3(this.posX+16, this.posY+8, 0);
            GL.Vertex3(this.posX+18, this.posY+8, 0);
            GL.Vertex3(this.posX+18, this.posY+10, 0);
            GL.Vertex3(this.posX+16, this.posY+10, 0);

            GL.End();
            GL.PopMatrix();
        }

        public bool collides(Shot shot){
            if(shot.posY+7 > this.posY+5 && shot.posY < this.posY+20 && shot.posX+5 > this.posX && shot.posX < this.posX+30 ){
                return true;
            }

            return false;
        }

        public void hitBox(Material mat){

            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(Color.blue);

            GL.Vertex3(this.posX, this.posY+5, 0);
            GL.Vertex3(this.posX, this.posY+20, 0);
            GL.Vertex3(this.posX+30, this.posY+20, 0);
            GL.Vertex3(this.posX+30, this.posY+5, 0);

            GL.End();
            GL.PopMatrix();

        }

        public Shot shooting(){
            Shot enemyShot = new Shot(this.posX+10, this.posY-2, -3);

            return enemyShot;
        }
    }

    public class Cover{ // Não deu tempo de implementar :(
        public float posX;
        public float posY;
        public Cover(float posX, float posY){
            this.posX = posX;
            this.posY = posY;
        }

        public void hitBox(Material mat){
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Color(Color.blue);
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
            GL.Color(Color.white);

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
