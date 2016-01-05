using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace Pinball
{
    public partial class PinballForm : Form
    {
        HiResTimer timer = new HiResTimer();

        Random rand = new Random();

        SoundPlayer soundplayer = new SoundPlayer("beep-02.wav");
        SoundPlayer soundplayer2 = new SoundPlayer("boo-02.wav");

        long startTime;
        bool isRunning;
        bool isOutofTunnel;
        bool isLeftUp = false;
        bool isRightUp = false;

        long interval = (long)TimeSpan.FromSeconds(1.0 / 60).TotalMilliseconds;

        Graphics graphics;
        Graphics imageGraphics;

        Image backBuffer;
        Image blackScreen;

        int clientWidth;
        int clientHeight;
        int springHeight = 0;
        int Lives = 3;
        int Score = 0;
        int[] HighScores = { };

        RectangleF myBall;
        Rectangle myTunnel;
        Rectangle mySpring1;
        Rectangle mySpring2;
        Rectangle myBox1;
        Rectangle myBox2;
        Rectangle myBox3;

        // generate machine layout
        Rectangle myTunnelBlock;
        Rectangle[] myTunnelExit;
        RectangleF[] myLeftRamp;
        RectangleF[] myRightRamp;
        RectangleF[] myLeftBumper;
        RectangleF[] myRightBumper;
        RectangleF[] myLeftFlipper;
        RectangleF[] myRightFlipper;

        Pen myPen;
        float dx;
        float dy;
        float oldX;
        float oldY;

        int i, j, temp;

        Color ballColor = Color.Gold;
        Color pointColor = Color.Silver;
        Color flipperColor = Color.MidnightBlue;

        public PinballForm()
        {
            MessageBox.Show("Welcome to Pinball Wizard!\r\n[space]: loads the spring\r\n[enter]: launches the spring\r\n'Z': toggles left flipper\r\n'M': toggles right flipper");
            InitializeComponent();
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            

            clientWidth = this.ClientRectangle.Width;
            clientHeight = this.ClientRectangle.Height;
            myBall = new RectangleF(clientWidth - 10, clientHeight - 110, 10, 10);
            myTunnel = new Rectangle(clientWidth - 10, clientHeight - 450, 1, 450);
            mySpring1 = new Rectangle(clientWidth - 10, clientHeight - 100, 50, 100);
            mySpring2 = new Rectangle(clientWidth - 10, clientHeight - 100, 50, 1);
            myBox1 = new Rectangle(clientWidth - 300, clientHeight - 550, 50, 50);
            myBox2 = new Rectangle(clientWidth - 350, clientHeight - 450, 50, 50);
            myBox3 = new Rectangle(clientWidth - 250, clientHeight - 450, 50, 50);
            myTunnelExit = new Rectangle[10];
            myLeftRamp = new RectangleF[175];
            myRightRamp = new RectangleF[165];
            myLeftBumper = new RectangleF[145];
            myRightBumper = new RectangleF[145];
            myLeftFlipper = new RectangleF[80];
            myRightFlipper = new RectangleF[80];
            HighScores = new int[10];
            string filename = "HighScore.txt";
            if (File.Exists(filename))
            {
                StreamReader file = new StreamReader(filename);
                for (i = 0; i < 10; i++)
                {
                    HighScores[i] = Convert.ToInt32(file.ReadLine());
                }
            }
            myTunnelBlock = new Rectangle(clientWidth - 10, 0, 1, clientHeight);
            // loop for tunnel exit
            for(i = 0; i < 10; i++){
                myTunnelExit[i] = new Rectangle(clientWidth - 10 + i, 0 , 1, i + 90);
            }
            // loop for left ramp
            for(i = 0; i< 175; i++)
            {
                myLeftRamp[i] = new RectangleF(0 + i, clientHeight - 125 + i*.5F, 1, 125);
            }
            // loop for right ramp
            for (i = 0; i < 164; i++)
            {
                myRightRamp[i] = new RectangleF(clientWidth - 11 - i, clientHeight - 120 + i*.5F, 1, 125);
            }
            // loop for left bumper
            for (i = 0; i< 145; i++)
            {
                myLeftBumper[i] = new RectangleF(30 + i, clientHeight - 300 + i * 1.7F, 1, 150 - i * 1.15F);
            }
            // loop for right bumper
            for (i = 0; i < 145; i++)
            {
                myRightBumper[i] = new RectangleF(clientWidth - 30 - i, clientHeight - 300 + i * 1.7F, 1, 150 - i * 1.15F);
            }
            // loop for left flipper
            for (i = 0; i < 80; i++)
            {
                myLeftFlipper[i] = new RectangleF(175 + i, clientHeight - 39.5F + .25F * i, 1, 5);
            }
            // loop for right flipper
            for (i = 0; i < 80; i++)
            {
                myRightFlipper[i] = new RectangleF(clientWidth - 175 - i, clientHeight - 39.5F + .25F * i, 1, 5);
            }
            myPen = new Pen(Color.Silver, 2);
            
            backBuffer = (Image)new Bitmap(clientWidth, clientHeight);

            graphics = this.CreateGraphics();
            imageGraphics = Graphics.FromImage(backBuffer);
            
            
        }

        public void GameLoop()
        {
            timer.Start();

            isRunning = true;
            
            while (this.Created && isRunning)
            {
                dy += .075F;
                if (isOutofTunnel)
                {
                    Score++;
                }
                /*if (isOutofTunnel)
                {
                    if (myBall.X + myBall.Width / 2 > clientWidth / 2)
                    {
                        dx -= .05F;
                    }
                    else
                    {
                        dx += .05F;
                    }
                }*/
                startTime = timer.ElapsedMilliseconds;
                GameLogic();
                
                RenderScene();
                Application.DoEvents();
                
                
                while (timer.ElapsedMilliseconds - startTime < interval)
                {
                    
                }
            }
        }

        private void GameLogic()
        {
            if (Lives == 0)
            {
                if (Score >= HighScores[0])
                {
                    HighScores[0] = Score;
                    for (i = 1; i < 10; i++)
                    {
                        temp = HighScores[i];
                        j = i;
                        while (j > 0 && (HighScores[j-1] > temp))
                        {
                            HighScores[j] = HighScores[j - 1];
                            j--;
                        }
                        HighScores[j] = temp;
                    }
                }
                System.IO.File.WriteAllText("HighScore.txt", string.Empty);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("HighScore.txt", true))
                {
                    foreach (int score in HighScores)
                    {
                        file.WriteLine(score);
                    }
                }
                DialogResult result;
                if (Score >= HighScores[0])
                {
                    result = MessageBox.Show(String.Format("New high score! Your score was {0}. Continue playing?", Score), "Game Over!", MessageBoxButtons.YesNo);
                }
                else
                {
                    result = MessageBox.Show(String.Format("Your score was {0}. Continue playing?", Score), "Game Over!", MessageBoxButtons.YesNo);
                }
                if (result == DialogResult.No)
                {
                    isRunning = false;
                }
                else
                {
                    Score = 0;
                    Lives = 3;
                }
            }
            oldX = myBall.X;
            oldY = myBall.Y;
            myBall.X += dx;
            myBall.Y += dy;

            // collide with left wall
            if (myBall.X < 0)
            {
                myBall.X = 0;
                dx *= -.75F;
            }
            // collide with ceiling
            if (myBall.Y < 0)
            {
                myBall.Y = 0;
                dy *= -.75F;
            }
            // collide with right wall
            if (myBall.X + myBall.Width > clientWidth)
            {
                myBall.X = clientWidth - myBall.Width;
                dx *= -.75F;
            }
            // collide with block wall
            if (myBall.X + myBall.Width > clientWidth - 10 && isOutofTunnel)
            {
                myBall.X = clientWidth - 10 - myBall.Width;
                dx *= -.75F;
            }
            // collide with exit of tunnel
            if ((myBall.Y < clientHeight - 465) && (myBall.X >= clientWidth - 10))
            {
                myBall.Y = clientHeight - 465;
                int randomnumber = rand.Next(1, 4);

                dx = -randomnumber*dy;
                dy = 0;
            }
            // collide with spring
            if (myBall.Y + myBall.Height > clientHeight - 100 && myBall.X >= clientWidth - 10)
            {
                myBall.Y = clientHeight - 100 - myBall.Height;
                dy *= -.75F;
                
            }
            // collide with Boxes
            if (myBall.IntersectsWith(myBox1))
            {
                int randomnumber = rand.Next(0, 2);
                myBall.Y = oldY;
                myBall.X = oldX;
                if (randomnumber == 0)
                {
                    dx *= -1.05F;
                }
                else
                {
                    dx *= 1.05F;
                }
                dy *= -.5F;
                Score += 10;
                soundplayer.Play();
            }
            if (myBall.IntersectsWith(myBox2))
            {
                int randomnumber = rand.Next(0, 2);
                myBall.Y = oldY;
                myBall.X = oldX;
                if (randomnumber == 0)
                {
                    dx *= -1.05F;
                }
                else
                {
                    dx *= 1.05F;
                }
                dy *= -.5F;
                Score += 10;
                soundplayer.Play();
            }
            if (myBall.IntersectsWith(myBox3))
            {
                int randomnumber = rand.Next(0, 2);
                myBall.Y = oldY;
                myBall.X = oldX;
                if (randomnumber == 0)
                {
                    dx *= -1.05F;
                }
                else
                {
                    dx *= 1.05F;
                }
                dy *= -.5F;
                Score += 10;
                soundplayer.Play();
            }
            // collide with left ramp
            for (i = 0; i < 175; i++)
            {
                if (myBall.IntersectsWith(myLeftRamp[i]))
                {
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dy = -.75F;
                    dx = .75F;
                }
            }
            // collide with right ramp
            for (i = 0; i < 165; i++)
            {
                if (myBall.IntersectsWith(myRightRamp[i]))
                {
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dy = -.75F;
                    dx = -.75F;
                }
            }
            // collide with left bumper
            for (i = 0; i < 1; i++)
            {
                if (myBall.IntersectsWith(myLeftBumper[i]))
                {
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dx *= -.75F;
                    dy *= -.001F;
                }
            }
            for (i = 1; i < 145; i++)
            {
                if (myBall.IntersectsWith(myLeftBumper[i]))
                {
                    int randomnumber = rand.Next(1, 4);
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dx *= randomnumber/1.5F;
                    dy *= -randomnumber/1.1F;
                    if (dy > 0)
                    {
                        dy *= -1;
                    }
                    if (dx < 0)
                    {
                        dx *= -1;
                    }
                    Score += 10;
                    soundplayer.Play();
                }
            }
            // collide with right bumper
            for (i = 0; i < 1; i++)
            {
                if (myBall.IntersectsWith(myRightBumper[i]))
                {
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dx *= -.75F;
                    dy *= -.001F;
                }
            }
            for (i = 1; i < 145; i++)
            {
                if (myBall.IntersectsWith(myRightBumper[i]))
                {
                    int randomnumber = rand.Next(1, 4);
                    myBall.Y = oldY;
                    myBall.X = oldX;
                    dx *= randomnumber / 1.5F;
                    dy *= -randomnumber / 1.1F;
                    if (dy > 0)
                    {
                        dy *= -1;
                    }
                    if (dx > 0)
                    {
                        dx *= -1;
                    }
                    Score += 10;
                    soundplayer.Play();
                }
            }
            // collide with left flipper
            if (isLeftUp == false)
            {
                for (i = 0; i < 80; i++)
                {
                    if (myBall.IntersectsWith(myLeftFlipper[i]))
                    {
                        myBall.Y = oldY - 1;
                        myBall.X = oldX;
                        dy *= -.75F;
                        dx = .25F;
                        
                    }
                }
                
            }
            else
            {
                int randomnumber = rand.Next(5, 11);
                for (i = 0; i < 80; i++)
                {
                    if (myBall.IntersectsWith(myLeftFlipper[i]))
                    {
                        myBall.Y = oldY - 1;
                        myBall.X = oldX;
                        dy = -randomnumber;
                        dx *= -(randomnumber / 3 + 1);
                        if (dy > 0)
                        {
                            dy *= -1;
                        }
                        if (dx < 0)
                        {
                            dx *= -1;
                        }
                        soundplayer.Play();
                    }
                }
                
            }
            // collide with right flipper
            if (isRightUp == false)
            {
                for (i = 0; i < 80; i++)
                {
                    if (myBall.IntersectsWith(myRightFlipper[i]))
                    {
                        myBall.Y = oldY - 1;
                        myBall.X = oldX;
                        dy *= -.75F;
                        dx = -.25F;
                        
                    }
                }
                
            }
            else
            {
                int randomnumber = rand.Next(5, 11);
                for (i = 0; i < 80; i++)
                {
                    if (myBall.IntersectsWith(myRightFlipper[i]))
                    {
                        myBall.Y = oldY - 1;
                        myBall.X = oldX;
                        dy = -randomnumber;
                        dx *= -(randomnumber / 3 + 1);
                        if (dy > 0)
                        {
                            dy *= -1;
                        }
                        if (dx > 0)
                        {
                            dx *= -1;
                        }
                        soundplayer.Play();
                    }
                }
                
            }
            // reset ball when it falls through
            if (myBall.Y > clientHeight)
            {
                isOutofTunnel = false;
                myBall.X = clientWidth - 10;
                myBall.Y = clientHeight - 110;
                dx = 0;
                dy = 0;
                Lives--;
                soundplayer2.Play();
            }
        }

        private void RenderScene()
        {
            imageGraphics.FillRectangle(new SolidBrush(Color.Black), this.ClientRectangle);
            imageGraphics.FillRectangle(new SolidBrush(ballColor), myBall);
            imageGraphics.FillRectangle(new SolidBrush(pointColor), myTunnel);
            imageGraphics.FillRectangle(new SolidBrush(Color.SteelBlue), mySpring1);
            imageGraphics.FillRectangle(new SolidBrush(Color.SteelBlue), mySpring2);
            if (Score < 5000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Red), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Red), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Red), myBox3);
            }
            else if(Score >= 5000 && Score < 10000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Orange), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Orange), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Orange), myBox3);
            }
            else if (Score >= 10000 && Score < 20000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Yellow), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Yellow), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Yellow), myBox3);
            }
            else if (Score >= 20000 && Score < 40000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Green), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Green), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Green), myBox3);
            }
            else if (Score >= 40000 && Score < 80000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Blue), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Blue), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Blue), myBox3);
            }
            else if (Score >= 80000 && Score < 160000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Purple), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Purple), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Purple), myBox3);
            }
            else if (Score >= 160000 && Score < 320000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Pink), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Pink), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Pink), myBox3);
            }
            else if (Score >= 320000 && Score < 640000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Ivory), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Ivory), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Ivory), myBox3);
            }
            else if (Score >= 640000 && Score < 1280000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Chocolate), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Chocolate), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Chocolate), myBox3);
            }
            else if (Score >= 1280000 && Score < 2560000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Goldenrod), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Goldenrod), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Goldenrod), myBox3);
            }
            else if (Score >= 2560000)
            {
                imageGraphics.FillRectangle(new SolidBrush(Color.Firebrick), myBox1);
                imageGraphics.FillRectangle(new SolidBrush(Color.Firebrick), myBox2);
                imageGraphics.FillRectangle(new SolidBrush(Color.Firebrick), myBox3);
            }

            // loop to draw the tunnel exit
            for (i = 0; i < 10; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myTunnelExit[i]);
            }
            // loop to draw the left ramp
            for (i = 0; i < 175; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myLeftRamp[i]);
            }
            // loop to draw the right ramp
            for (i = 0; i < 165; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myRightRamp[i]);
            }
            // loop to draw the left bumper
            for (i = 0; i < 145; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myLeftBumper[i]);
            }
            // loop to draw the right bumper
            for (i = 0; i < 145; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myRightBumper[i]);
            }
            // loop to draw the left flipper
            for (i = 0; i < 80; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(flipperColor), myLeftFlipper[i]);
            }
            // loop to draw the right flipper
            for (i = 0; i < 80; i++)
            {
                imageGraphics.FillRectangle(new SolidBrush(flipperColor), myRightFlipper[i]);
            }
            // if the ball has been successfully launched, draw new boundary line
            if (myBall.X + myBall.Width < clientWidth - 9)
            {
                imageGraphics.FillRectangle(new SolidBrush(pointColor), myTunnelBlock);
                isOutofTunnel = true;
            }
            
            

            //graphics.DrawLine(myPen, 0, clientHeight - 50, clientWidth, clientHeight - 50);


            this.BackgroundImage = backBuffer;
            this.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Z:
                    if (isLeftUp == false)
                    {
                        isLeftUp = true;
                        // loop to draw the left flipper
                        for (i = 0; i < 80; i++)
                        {
                            myLeftFlipper[i] = new RectangleF(175 + i, clientHeight - 39.5F, 1, 5);
                        }
                        for (i = 0; i < 80; i++)
                        {
                            imageGraphics.FillRectangle(new SolidBrush(flipperColor), myLeftFlipper[i]);

                        }
                    }
                    else
                    {
                        isLeftUp = false;
                        // loop to draw the left flipper
                        for (i = 0; i < 80; i++)
                        {
                            myLeftFlipper[i] = new RectangleF(175 + i, clientHeight - 39.5F + .25F * i, 1, 5);
                        }
                        for (i = 0; i < 80; i++)
                        {
                            imageGraphics.FillRectangle(new SolidBrush(flipperColor), myLeftFlipper[i]);

                        }
                    }
                    break;
                case Keys.M:
                    if (isRightUp == false)
                    {
                        isRightUp = true;
                        // loop to draw the right flipper
                        for (i = 0; i < 80; i++)
                        {
                            myRightFlipper[i] = new RectangleF(clientWidth - 175 - i, clientHeight - 39.5F, 1, 5);
                        }
                        for (i = 0; i < 80; i++)
                        {
                            imageGraphics.FillRectangle(new SolidBrush(flipperColor), myRightFlipper[i]);

                        }
                    }
                    else
                    {
                        isRightUp = false;
                        // loop to draw the right flipper
                        for (i = 0; i < 80; i++)
                        {
                            myRightFlipper[i] = new RectangleF(clientWidth - 175 - i, clientHeight - 39.5F + .25F * i, 1, 5);
                        }
                        for (i = 0; i < 80; i++)
                        {
                            imageGraphics.FillRectangle(new SolidBrush(flipperColor), myRightFlipper[i]);

                        }
                    }
                    break;
                case Keys.Q:
                    Application.Exit();
                    break;
                case Keys.Space:
                    if (mySpring1.Y < clientHeight)
                    {
                        mySpring1.Y += 10;
                        springHeight++;
                    }
                    break;
                case Keys.Enter:
                    while (mySpring1.Y > mySpring2.Y)
                    {
                        mySpring1.Y -= 1;
                    }
                    if (springHeight == 1 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -1F;
                    }
                    if (springHeight == 2 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -2.5F;
                    }
                    if (springHeight == 3 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -4F;
                    }
                    if (springHeight == 4 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -5.5F;
                    }
                    if (springHeight == 5 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -7F;
                    }
                    if (springHeight == 6 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -8;
                    }
                    if (springHeight == 7 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -9F;
                    }
                    if (springHeight == 8 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -9.5F;
                    }
                    if (springHeight == 9 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -9.75F;
                    }
                    if (springHeight == 10 && myBall.Y == mySpring2.Y - myBall.Height)
                    {
                        dy += -10F;
                    }
                    springHeight = 0;
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
