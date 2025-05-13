using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpaceShooter
{
    public partial class Form1 : Form
    {
        int speed = 10;
        Random random = new Random();

        int playerHP = 100;
        int score = 0;
        ProgressBar hpBar;
        Label scoreLabel;
        Label loseLabel;
        Button restartButton;

        bool leftPressed, rightPressed, upPressed, downPressed;

        class BlasterInfo
        {
            public PictureBox PictureBox;
        }

        class AsteroidInfo
        {
            public PictureBox PictureBox;
            public float Angle;
        }

        class EnemyInfo
        {
            public PictureBox PictureBox;
            public Timer ShootTimer;
        }

        class EnemyBulletInfo
        {
            public PictureBox PictureBox;
        }

        class BossInfo
        {
            public PictureBox PictureBox;
            public int HP;
            public ProgressBar HPBar;
            public bool MovingRight;
            public Timer ShootTimer;
        }

        List<BlasterInfo> blasters = new List<BlasterInfo>();
        List<AsteroidInfo> asteroids = new List<AsteroidInfo>();
        List<EnemyInfo> enemies = new List<EnemyInfo>();
        List<EnemyBulletInfo> enemyBullets = new List<EnemyBulletInfo>();
        BossInfo boss = null;
        bool bossActive = false;

        Timer gameTimer = new Timer();
        Timer asteroidTimer = new Timer();
        Timer enemyTimer = new Timer();

        public Form1()
        {
            InitializeComponent();

            // HP Bar
            hpBar = new ProgressBar();
            hpBar.Maximum = 100;
            hpBar.Value = playerHP;
            hpBar.Width = 200;
            hpBar.Height = 20;
            hpBar.Location = new Point(10, 10);
            hpBar.ForeColor = Color.Red;
            this.Controls.Add(hpBar);

            // Score label
            scoreLabel = new Label();
            scoreLabel.Text = "Очки: 0";
            scoreLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            scoreLabel.ForeColor = Color.Yellow;
            scoreLabel.AutoSize = true;
            scoreLabel.BackColor = Color.Transparent;
            scoreLabel.Location = new Point(hpBar.Right + 20, 10);
            this.Controls.Add(scoreLabel);

            // Lose label
            loseLabel = new Label();
            loseLabel.Text = "Вы проиграли!";
            loseLabel.Font = new Font("Arial", 32, FontStyle.Bold);
            loseLabel.ForeColor = Color.Red;
            loseLabel.AutoSize = true;
            loseLabel.Visible = false;
            loseLabel.BackColor = Color.Transparent;
            loseLabel.Location = new Point(this.ClientSize.Width / 2 - 200, this.ClientSize.Height / 2 - 100);
            loseLabel.Anchor = AnchorStyles.None;
            this.Controls.Add(loseLabel);

            // Restart button
            restartButton = new Button();
            restartButton.Text = "Начать заново";
            restartButton.Font = new Font("Arial", 16, FontStyle.Bold);
            restartButton.Size = new Size(200, 50);
            restartButton.Visible = false;
            restartButton.Location = new Point(this.ClientSize.Width / 2 - 100, this.ClientSize.Height / 2);
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);

            gameTimer.Interval = 20; // ~50 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            asteroidTimer.Interval = 1000;
            asteroidTimer.Tick += (s, a) => Asteroid();
            asteroidTimer.Start();

            enemyTimer.Interval = 3000;
            enemyTimer.Tick += (s, a) => { if (!bossActive) SpawnEnemy(); };
            enemyTimer.Start();

            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            this.KeyPreview = true;
            this.Shown += (s, e) => this.Focus();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameTimer.Enabled) return;

            if (e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.D) rightPressed = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) upPressed = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) downPressed = true;
            if (e.KeyCode == Keys.Space) Shoot();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.D) rightPressed = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) upPressed = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) downPressed = false;
        }

        public void Shoot()
        {
            if (!gameTimer.Enabled) return;

            PictureBox blaster = new PictureBox();
            blaster.Top = playerPictureBox.Top - 45;
            blaster.Left = playerPictureBox.Left + 15;
            blaster.Width = 50;
            blaster.Height = 50;
            blaster.SizeMode = PictureBoxSizeMode.StretchImage;
            blaster.Image = Properties.Resources.laser_cartoon;
            blaster.Name = "Blaster";
            blaster.BackColor = Color.Transparent;
            this.Controls.Add(blaster);
            blasters.Add(new BlasterInfo { PictureBox = blaster });
        }

        public void Asteroid()
        {
            if (!gameTimer.Enabled) return;

            PictureBox asteroid = new PictureBox();
            asteroid.Top = 0;
            asteroid.Left = random.Next(0, this.ClientSize.Width - 64);
            asteroid.Width = 64;
            asteroid.Height = 64;
            asteroid.SizeMode = PictureBoxSizeMode.StretchImage;
            asteroid.Image = Properties.Resources.asteroid;
            asteroid.Name = "asteroid";
            asteroid.BackColor = Color.Transparent;
            this.Controls.Add(asteroid);
            asteroids.Add(new AsteroidInfo { PictureBox = asteroid, Angle = 0 });
        }

        public void SpawnEnemy()
        {
            if (!gameTimer.Enabled) return;
            PictureBox enemy = new PictureBox();
            enemy.Top = 0;
            enemy.Left = random.Next(0, this.ClientSize.Width - 64);
            enemy.Width = 64;
            enemy.Height = 64;
            enemy.SizeMode = PictureBoxSizeMode.StretchImage;
            enemy.Image = Properties.Resources.enemy_ship; // ваша картинка врага
            enemy.Name = "enemy";
            enemy.BackColor = Color.Transparent;
            this.Controls.Add(enemy);

            Timer shootTimer = new Timer();
            shootTimer.Interval = 1400;
            shootTimer.Tick += (s, e) => EnemyShoot(enemy);

            var info = new EnemyInfo { PictureBox = enemy, ShootTimer = shootTimer };
            enemies.Add(info);
            shootTimer.Start();
        }

        public void EnemyShoot(PictureBox enemy)
        {
            PictureBox bullet = new PictureBox();
            bullet.Width = 20;
            bullet.Height = 40;
            bullet.SizeMode = PictureBoxSizeMode.StretchImage;
            bullet.Image = Properties.Resources.laser_cartoon; // ваша картинка пули врага
            bullet.BackColor = Color.Transparent;
            bullet.Left = enemy.Left + enemy.Width / 2 - 10;
            bullet.Top = enemy.Top + enemy.Height;
            this.Controls.Add(bullet);
            enemyBullets.Add(new EnemyBulletInfo { PictureBox = bullet });
        }

        public void SpawnBoss()
        {
            bossActive = true;
            PictureBox bossPic = new PictureBox();
            bossPic.Top = 30;
            bossPic.Left = (this.ClientSize.Width - 128) / 2;
            bossPic.Width = 128;
            bossPic.Height = 128;
            bossPic.SizeMode = PictureBoxSizeMode.StretchImage;
            bossPic.Image = Properties.Resources.boss_removebg_preview; // ваша картинка босса
            bossPic.BackColor = Color.Transparent;
            this.Controls.Add(bossPic);

            ProgressBar bossBar = new ProgressBar();
            bossBar.Maximum = 10;
            bossBar.Value = 10;
            bossBar.Width = 200;
            bossBar.Height = 20;
            bossBar.Location = new Point(this.ClientSize.Width / 2 - 100, bossPic.Top - 25);
            this.Controls.Add(bossBar);

            Timer shootTimer = new Timer();
            shootTimer.Interval = 1000;
            shootTimer.Tick += (s, e) => BossShoot(bossPic);

            boss = new BossInfo
            {
                PictureBox = bossPic,
                HP = 10,
                HPBar = bossBar,
                MovingRight = true,
                ShootTimer = shootTimer
            };
            shootTimer.Start();
        }

        public void BossShoot(PictureBox bossPic)
        {
            PictureBox bullet = new PictureBox();
            bullet.Width = 24;
            bullet.Height = 48;
            bullet.SizeMode = PictureBoxSizeMode.StretchImage;
            bullet.Image = Properties.Resources.laser_cartoon; // ваша картинка пули босса
            bullet.BackColor = Color.Transparent;
            bullet.Left = bossPic.Left + bossPic.Width / 2 - 12;
            bullet.Top = bossPic.Top + bossPic.Height;
            this.Controls.Add(bullet);
            enemyBullets.Add(new EnemyBulletInfo { PictureBox = bullet });
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Движение игрока по состоянию клавиш
            if (leftPressed) playerPictureBox.Left -= speed;
            if (rightPressed) playerPictureBox.Left += speed;
            if (upPressed) playerPictureBox.Top -= speed;
            if (downPressed) playerPictureBox.Top += speed;

            // Движение выстрелов
            foreach (var blasterInfo in blasters.ToList())
            {
                var blaster = blasterInfo.PictureBox;
                blaster.Top -= 15;

                // Проверка столкновений с астероидами
                foreach (var asteroidInfo in asteroids.ToList())
                {
                    if (blaster.Bounds.IntersectsWith(asteroidInfo.PictureBox.Bounds))
                    {
                        this.Controls.Remove(blaster);
                        this.Controls.Remove(asteroidInfo.PictureBox);
                        blasters.Remove(blasterInfo);
                        asteroids.Remove(asteroidInfo);
                        AddScore(10);
                        break;
                    }
                }

                // Проверка сбивания врагов
                foreach (var enemy in enemies.ToList())
                {
                    if (blaster.Bounds.IntersectsWith(enemy.PictureBox.Bounds))
                    {
                        this.Controls.Remove(blaster);
                        this.Controls.Remove(enemy.PictureBox);
                        blasters.Remove(blasterInfo);
                        enemy.ShootTimer.Stop();
                        enemies.Remove(enemy);
                        AddScore(15);
                        break;
                    }
                }

                // Проверка попадания по боссу
                if (bossActive && boss != null && blaster.Bounds.IntersectsWith(boss.PictureBox.Bounds))
                {
                    this.Controls.Remove(blaster);
                    blasters.Remove(blasterInfo);
                    boss.HP--;
                    boss.HPBar.Value = boss.HP;
                    if (boss.HP <= 0)
                    {
                        this.Controls.Remove(boss.PictureBox);
                        this.Controls.Remove(boss.HPBar);
                        boss.ShootTimer.Stop();
                        boss = null;
                        bossActive = false;
                        AddScore(20);
                    }
                    break;
                }

                if (blaster.Top + blaster.Height < 0)
                {
                    this.Controls.Remove(blaster);
                    blasters.Remove(blasterInfo);
                }
            }

            // Движение и вращение астероидов
            foreach (var asteroidInfo in asteroids.ToList())
            {
                var asteroid = asteroidInfo.PictureBox;
                asteroid.Top += 4;
                asteroidInfo.Angle += 3;
                asteroid.Image = RotateImage(Properties.Resources.asteroid, asteroidInfo.Angle);

                if (asteroid.Bounds.IntersectsWith(playerPictureBox.Bounds))
                {
                    this.Controls.Remove(asteroid);
                    asteroids.Remove(asteroidInfo);
                    DecreaseHP(10);
                    RemoveScore(5);
                    continue;
                }

                if (asteroid.Top + asteroid.Height > 644)
                {
                    this.Controls.Remove(asteroid);
                    asteroids.Remove(asteroidInfo);
                }
            }

            // Движение врагов
            foreach (var enemy in enemies.ToList())
            {
                enemy.PictureBox.Top += 3;
                if (enemy.PictureBox.Bounds.IntersectsWith(playerPictureBox.Bounds))
                {
                    this.Controls.Remove(enemy.PictureBox);
                    enemy.ShootTimer.Stop();
                    enemies.Remove(enemy);
                    RemoveScore(10);
                    DecreaseHP(10);
                    continue;
                }
                if (enemy.PictureBox.Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(enemy.PictureBox);
                    enemy.ShootTimer.Stop();
                    enemies.Remove(enemy);
                }
            }

            // Движение вражеских пуль
            foreach (var bullet in enemyBullets.ToList())
            {
                bullet.PictureBox.Top += 10;
                if (bullet.PictureBox.Bounds.IntersectsWith(playerPictureBox.Bounds))
                {
                    this.Controls.Remove(bullet.PictureBox);
                    enemyBullets.Remove(bullet);
                    RemoveScore(5);
                    DecreaseHP(5);
                    continue;
                }
                if (bullet.PictureBox.Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(bullet.PictureBox);
                    enemyBullets.Remove(bullet);
                }
            }

            // Движение и стрельба босса
            if (bossActive && boss != null)
            {
                if (boss.MovingRight)
                    boss.PictureBox.Left += 5;
                else
                    boss.PictureBox.Left -= 5;

                if (boss.PictureBox.Left <= 0)
                    boss.MovingRight = true;
                if (boss.PictureBox.Right >= this.ClientSize.Width)
                    boss.MovingRight = false;
            }

            // Появление босса
            if (!bossActive && score > 0 && score % 150 == 0)
            {
                SpawnBoss();
            }
        }

        private void AddScore(int amount)
        {
            score += amount;
            scoreLabel.Text = $"Очки: {score}";
        }

        private void RemoveScore(int amount)
        {
            score -= amount;
            if (score < 0) score = 0;
            scoreLabel.Text = $"Очки: {score}";
        }

        private void DecreaseHP(int amount)
        {
            playerHP -= amount;
            if (playerHP < 0) playerHP = 0;
            hpBar.Value = playerHP;

            if (playerHP == 0)
                GameOver();
        }

        private void GameOver()
        {
            gameTimer.Stop();
            asteroidTimer.Stop();
            enemyTimer.Stop();
            foreach (var enemy in enemies) enemy.ShootTimer.Stop();
            if (boss != null) boss.ShootTimer.Stop();
            loseLabel.Visible = true;
            restartButton.Visible = true;
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            foreach (var blaster in blasters)
                this.Controls.Remove(blaster.PictureBox);
            foreach (var asteroid in asteroids)
                this.Controls.Remove(asteroid.PictureBox);
            foreach (var enemy in enemies)
            {
                this.Controls.Remove(enemy.PictureBox);
                enemy.ShootTimer.Stop();
            }
            foreach (var bullet in enemyBullets)
                this.Controls.Remove(bullet.PictureBox);
            if (boss != null)
            {
                this.Controls.Remove(boss.PictureBox);
                this.Controls.Remove(boss.HPBar);
                boss.ShootTimer.Stop();
            }
            blasters.Clear();
            asteroids.Clear();
            enemies.Clear();
            enemyBullets.Clear();
            boss = null;
            bossActive = false;

            playerHP = 100;
            hpBar.Value = playerHP;
            score = 0;
            scoreLabel.Text = "Очки: 0";

            loseLabel.Visible = false;
            restartButton.Visible = false;

            playerPictureBox.Left = (this.ClientSize.Width - playerPictureBox.Width) / 2;
            playerPictureBox.Top = this.ClientSize.Height - playerPictureBox.Height - 30;

            this.Focus();

            gameTimer.Start();
            asteroidTimer.Start();
            enemyTimer.Start();
        }

        private Bitmap RotateImage(Image image, float angle)
        {
            Bitmap rotatedImage = new Bitmap(image.Width, image.Height);
            rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(image.Width / 2, image.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-image.Width / 2, -image.Height / 2);
                g.DrawImage(image, new Point(0, 0));
            }
            return rotatedImage;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Focus();
        }
    }
}