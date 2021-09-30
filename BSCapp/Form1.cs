using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using TUIO;

namespace BSCapp
{
    public partial class Form1 : Form, TuioListener
    {
        private double startpoint; //not used
        private TuioClient client; //used for connecting the reactivision
        private bool EnableDrawing=false; //flag
        private object cursorSync = new object(); // synchronize the cursor
        private object objectSync = new object(); // synchronize the object
        private object blobSync = new object(); //not used
        private Dictionary<long, TuioDemoObject> objectList; //list of used objects
        private Dictionary<long, TuioCursor> cursorList; //list of used cursors
        private bool verbose=true; //not used
        SolidBrush blackBrush = new SolidBrush(Color.Black); //black brush
        SolidBrush whiteBrush = new SolidBrush(Color.White); //white brush
        String status; //current feature used
        SolidBrush grayBrush = new SolidBrush(Color.Gray); //gray brush
        Pen fingerPen = new Pen(new SolidBrush(Color.Blue), 1); //finger pen
        public static int heightw,widthw; // height width of application
        private List<LineList> MyLines = new List<LineList>(); // list of drawings
        private List<int> Cdist = new List<int>(); //list of distances between finger cursor's point and all points of the curve
        private List<Point> Curvepoints= new List<Point>();
        private List<String> Shaplist = new List<string>(); //list of names of drawings
        private int count = 0; //counter
        private int m_StartX; // start point of x
        private int m_StartY; // start point of y
        private int m_CurX; // current point of x
        private int m_CurY; // current point of y
        private int ir = -1; // counter of drawings begins from 0 
        private int ic = 0; // counter of circles
        private int icv = 0; // counter of curves
        private int il = 0; // counter of lines
        private double slope; // slope of lines
        private Point Point1 = new Point(); // Tuio point of 1
        private Point Point2 = new Point(); // Tuio point of 2
        private TuioPoint StartDownLocation = new TuioPoint(); // beginning position of the cursor
        private Color colour;
        private Double editx1; // edit of point line
        private Double editx2; // edit of point line
        private Double edity1; // edit of point line
        private Double edity2; // edit of point line
        private string now= "Line"; // edit of point line
        private int previousangle=0; //previous angle value
        private int countf = 0; 
        String[] p;
        String Shape = ""; //name of shape of the drawing
        private int Time; // not used
        private int widthc; //width of the circle
        private int heightc; //height of the circle
        Point V1, V2, V3, V4, V5, V6, V7, V8; //points of rectangular outlines
        int prevcir; //not used
        private int prevcirx; //
        private int prevciry; //
        private bool EnableDrawingCurve; //flag for drawing circles 
        private int[] editdots = new int[128]; //
        bool cursordown = false;
        public Form1()  //constructor of the application
        {

            //beginning of connecting the reactivision
            InitializeComponent();
            this.Closing += new CancelEventHandler(Form_Closing);
            //initialize the lists
            cursorList = new Dictionary<long, TuioCursor>(128);
            objectList = new Dictionary<long, TuioDemoObject>(128);
            //initialize the length & width of the application
            heightw = this.Height;
            widthw = this.Width;
            //trying to connect the reactivision
            try
            {
                client = new TuioClient(3333);
                client.addTuioListener(this);
                client.connect();
                Console.WriteLine("You're connected");
            }
            catch(Exception e)
            {
                Console.WriteLine("You're not connected");
            }
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e) //closing the application method
        {
            //disconnecting the reactivision
            client.removeTuioListener(this);
            client.disconnect();
            System.Environment.Exit(0);
        }

        private string Mode(Double angle) //Features method
        {
            if (angle < 50 && angle >= 0)
            {
                return "Line";
            }
            else if(angle < 100 && angle >= 50)
            {
                return "Circle";
            }
            else if (angle < 150 && angle >= 100)
            {
                return "Curve";
            }
            else if (angle < 200 && angle >= 150)
            {
                return "Copy";
            }
            else if (angle < 250 && angle >= 200)
            {
                return "Remove";
            }
            else if (angle < 300 && angle >= 250)
            {
                return "Edit";
            }
            else if (angle < 360 && angle >= 300)
            {
                return "Move";
            }
            else
            {
                return "";
            }
        }

        public void LineL(Double angle) //selecting the drawing
        {
            List<string> list2 = Shaplist.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            p = list2.ToArray(); //turning a list to an array
            bool positive = false; 
            bool neutral = true;
            if (countf >= p.Length) // the counterf(pointer) bigger than the length of the array P
            {
                countf = p.Length-1;
            }
            if ((int)angle < previousangle) //if the angle decreased
            {
                positive = false;
                neutral = false;
            }
            else if((int)angle > previousangle) //if the angle increased
            {
                positive = true;
                neutral = false;
            }
            else if ((int)angle == previousangle) //if it's stable
            {
                neutral = true;
            }
            previousangle = (int)angle; //the previous angle will be updated with the current angle
            int G = ((int) angle) % 10; // angle will be from 0 to 9

            if(G==1 && neutral==false && positive == false)// if g reach to 1 and the rotation goes negative
            {
                if (countf == 0) //if the pointer is on the first index
                {
                    countf = p.Length - 1; //pointer will go the the last index and get the drawing name
                    string[] words = p[countf].Split(' ');
                    ir = Int32.Parse(words[2]);
                    Shape = words[0];
                    Console.WriteLine(ir);
                    
                }
                else
                {
                    countf--; //pointer will step backward one step and get the drawing name
                    string[] words = p[countf].Split(' ');
                    ir = Int32.Parse(words[2]);
                    Shape = words[0];
                    Console.WriteLine(ir);
                }
            }
            else if (G == 1 && neutral == false && positive == true) // if g reach to 1 and the rotation goes positive
            {
                if (countf == p.Length - 1) //if the pointer in the last index
                {
                    countf = 0; //it will be on the first index and get the drawing name
                    string[] words = p[countf].Split(' ');
                    ir = Int32.Parse(words[2]);
                    Shape = words[0];
                    Console.WriteLine(ir);
                }
                else
                {
                    countf++; //pointer will step forward one step and get the drawing name
                    string[] words = p[countf].Split(' ');
                    ir = Int32.Parse(words[2]);
                    Shape = words[0];
                    Console.WriteLine(ir);
                }
            }

        }

        public void refresh(TuioTime ftime) //refreshing the app
        {
            heightw = this.Height;
            widthw = this.Width;
            picture.Invalidate();
        }

        public static long gcd(long n1, long n2) //greatest common factor
        {
            if (n2 == 0)
            {
                return n1;
            }
            else
            {
                return gcd(n2, n1 % n2);
            }
        }

        ////////////////////////////////Cursor//////////////////////////////////////////////////////////////////////


        public void addTuioCursor(TuioCursor c)  //if the cursor is detected
        {
            if (ir > -1 && c.getCursorID()==0)
            {
                Curvepoints = MyLines[ir].Points;
            }
            cursordown=true;
            lock (cursorSync) //lock the cursor synchronize
            {
                cursorList.Add(c.SessionID, c); //add it to the cursor list
                //get the start position and the current position at the same time
                m_StartX = c.Position.getScreenX(this.Width);
                m_StartY = c.Position.getScreenY(this.Height);
                m_CurX = c.Position.getScreenX(this.Width);
                m_CurY = c.Position.getScreenY(this.Height);
                StartDownLocation = c.Position; //get the statr position
                if (now.Equals("Circle"))  //begin creating circle
                {
                    Shape = "Circle";
                    widthc = m_CurX - m_StartX;
                    heightc = m_CurY - m_StartY;
                }
                if (now.Equals("Edit")) //will see which circle does the cursor point at it on line & curve
                {
                    prevcirx = c.getScreenX(this.Width);
                    prevciry = c.getScreenY(this.Height);
                    switch (MyLines[ir].Type)
                    {
                        case "Line":
                            int X1 = (int)MyLines[ir].X1;
                            int X2 = (int)MyLines[ir].X2;
                            int Y1 = (int)MyLines[ir].Y1;
                            int Y2 = (int)MyLines[ir].Y2;
                            int d1 = Math.Abs((c.getScreenX(this.Width) - X1) ^ 2) + Math.Abs((c.getScreenY(this.Height) - Y1) ^ 2);
                            int d2 = Math.Abs((c.getScreenX(this.Width) - X2) ^ 2) + Math.Abs((c.getScreenY(this.Height) - Y2) ^ 2);
                            Console.WriteLine(c.CursorID+" : "+d1 + " - "+d2);
                            Console.WriteLine("______________________________");
                            if (d1 < 30)
                            {
                                editdots[c.CursorID] = 1;
                            }
                            else if (d2 < 30)
                            {
                                editdots[c.CursorID] = 2;
                            }
                            else
                            {
                                editdots[c.CursorID] = 0;
                            }
                            break;
                        case "Curve": //since the curves have many points, we will store distances between cursor points and curve point circles and will see which circle is pressed at the updatetuiocursor()                            
                            for (int i = 0; i < MyLines[ir].Points.Count; i++)
                            {
                                int X = MyLines[ir].Points[i].X;
                                int Y = MyLines[ir].Points[i].Y;
                                int d = Math.Abs((c.getScreenX(this.Width) - X) ^ 2) + Math.Abs((c.getScreenY(this.Height) - Y) ^ 2);
                                if (d <30)
                                {
                                    editdots[c.CursorID] = i+1;
                                    break;
                                }
                                else
                                {
                                    editdots[c.CursorID] = 0;
                                }
                                Cdist.Add(d);
                            }
                            break;
                    }
                    
                    
                }
                if (now.Equals("Move"))
                {
                    if (MyLines[ir].Type.Equals("Line"))
                    {
                        // subtract between the current cursor's position with the line points to be moved and record the new points
                        Point1.X = (int)(c.getScreenX(this.Width) + MyLines[ir].X1 - StartDownLocation.getScreenX(this.Width));
                        Point1.Y = (int)(c.getScreenY(this.Height) + MyLines[ir].Y1 - StartDownLocation.getScreenY(this.Height));
                        Point2.X = (int)(c.getScreenX(this.Width) + MyLines[ir].X2 - StartDownLocation.getScreenX(this.Width));
                        Point2.Y = (int)(c.getScreenY(this.Height) + MyLines[ir].Y2 - StartDownLocation.getScreenY(this.Height));
                    }
                    else if (MyLines[ir].Type.Equals("Circle"))
                    {
                        // subtract between the current cursor's position with the circle points to be moved and record the new points
                        Point1.X = (int)(c.getScreenX(this.Width) + MyLines[ir].X1 - StartDownLocation.getScreenX(this.Width));
                        Point1.Y = (int)(c.getScreenY(this.Height) + MyLines[ir].Y1 - StartDownLocation.getScreenY(this.Height));
                    }
                    else if (MyLines[ir].Type.Equals("Curve") && c.getCursorID() == 0)
                    {
                        // subtract between the current cursor's position with the curve points to be moved and record the new points
                        Curvepoints = new List<Point>();
                        for (int i = 0; i < MyLines[ir].Points.Count; i++)
                        {
                            Point p = MyLines[ir].Points[i];
                            p.X = c.getScreenX(this.Width) + MyLines[ir].Points[i].X - StartDownLocation.getScreenX(this.Width);
                            p.Y = c.getScreenY(this.Height) + MyLines[ir].Points[i].Y - StartDownLocation.getScreenY(this.Height);
                            Curvepoints.Add(p);
                        }
                    }
                }
            }

        }

        

        public void removeTuioCursor(TuioCursor c) //I call it the longest method in the application due to demand of the tangible outline after drawing
        {
            cursordown = false;
            TuioCursor tcur;
            lock (cursorSync)
            {
                tcur = cursorList[c.SessionID];
                cursorList.Remove(c.SessionID);
                List<TuioPoint> path = tcur.Path;
                switch (now)
                {
                    case "Edit":
                        editdots[c.CursorID] = 0; //format the value of the distance
                        break;
                    case "Line":
                        if (EnableDrawing)
                        {
                            //record the line values
                            TuioPoint Po1 = path[0];
                            TuioPoint Po2 = path[path.Count - 1];
                            LineList DrawLine = new LineList();
                            DrawLine.X1 = (Double)(Po1.getScreenX(this.Width));
                            DrawLine.Y1 = (Double)Po1.getScreenY(this.Height);
                            DrawLine.X2 = (Double)(Po2.getScreenX(this.Width));
                            DrawLine.Y2 = (Double)Po2.getScreenY(this.Height);
                            double q = DrawLine.Y2 - DrawLine.Y1;
                            double r = DrawLine.X2 - DrawLine.X1;
                            slope = q / r;
                            DrawLine.S = slope;
                            Shape = "Line";
                            DrawLine.Type = "Line";
                            ////////////////////////////////check if this line is inside///////////////////////////////////////////////////
                            double s1y = (V2.Y - V1.Y); double s1x = (V2.X - V1.X); double slope1 = (s1y / s1x);
                            double s2y = (V3.Y - V2.Y); double s2x = (V3.X - V2.X); double slope2 = (s2y / s2x);
                            double s3y = (V4.Y - V3.Y); double s3x = (V4.X - V3.X); double slope3 = (s3y / s3x);
                            double s4y = (V1.Y - V4.Y); double s4x = (V1.X - V4.X); double slope4 = (s4y / s4x);
                            //////
                            double s1ye = Math.Abs(s1y / gcd((int)s1y, (int)s1x)); double s1xe = Math.Abs(s1x / gcd((int)s1y, (int)s1x));
                            double s2ye = Math.Abs(s2y / gcd((int)s2y, (int)s2x)); double s2xe = Math.Abs(s2x / gcd((int)s2y, (int)s2x));
                            double s3ye = Math.Abs(s3y / gcd((int)s3y, (int)s3x)); double s3xe = Math.Abs(s3x / gcd((int)s3y, (int)s3x));
                            double s4ye = Math.Abs(s4y / gcd((int)s4y, (int)s4x)); double s4xe = Math.Abs(s4x / gcd((int)s4y, (int)s4x));
                            //////
                            double cc1 = (-(slope1 * V1.X) + V1.Y); double cc2 = (-(slope2 * V2.X) + V2.Y);
                            double cc3 = (-(slope3 * V3.X) + V3.Y); double cc4 = (-(slope4 * V4.X) + V4.Y);
                            //////
                            

                            double dom1;
                            double dom12;
                            if ((-slope1) < 0)
                            {
                                dom1 = Math.Abs(((s1ye) * DrawLine.X1) - ((s1xe * DrawLine.Y1)) + (s1xe * cc1));
                                dom12 = Math.Abs(((s1ye) * DrawLine.X2) - ((s1xe * DrawLine.Y2)) + (s1xe * cc1));
                            }
                            else if((slope1)==0)
                            {
                                s1ye = 1; s1xe = 0;
                                dom1 = Math.Abs(DrawLine.Y1 - V1.Y);
                                dom12 = Math.Abs(DrawLine.Y2 - V1.Y);
                            }
                            else if (slope1.ToString().Equals("NaN"))
                            {
                                s1ye = 1; s1xe = 0;
                                dom1 = Math.Abs(DrawLine.X1 - V1.X);
                                dom12 = Math.Abs(DrawLine.X2 - V1.X);
                            }
                            else
                            {
                                dom1 = Math.Abs(((s1ye) * DrawLine.X1) + ((s1xe * DrawLine.Y1)) - (s1xe * cc1));
                                dom12 = Math.Abs(((s1ye) * DrawLine.X2) + ((s1xe * DrawLine.Y2)) - (s1xe * cc1));
                            }
                            double num1 = Math.Sqrt(Math.Abs(s1ye * s1ye) + Math.Abs(s1xe * s1xe));
                            //////
                            double dom2, dom22;
                            if ((-slope2) < 0)
                            {
                                dom2 = Math.Abs(((s2ye) * DrawLine.X1) - (s2xe * DrawLine.Y1) + (s2xe * cc2));
                                dom22 = Math.Abs(((s2ye) * DrawLine.X2) - (s2xe * DrawLine.Y2) + (s2xe * cc2));
                            }
                            else if ((slope2) == 0)
                            {
                                s2ye = 1; s2xe = 0;
                                dom2 = Math.Abs(DrawLine.Y1 - V2.Y);
                                dom22 = Math.Abs(DrawLine.Y2 - V2.Y);
                            }
                            else if (slope2.ToString().Equals("NaN"))
                            {
                                s2ye = 1; s2xe = 0;
                                dom2 = Math.Abs(DrawLine.X1 - V2.X);
                                dom22 = Math.Abs(DrawLine.X2 - V2.X);
                            }
                            else
                            {
                                dom2 = Math.Abs(((s2ye) * DrawLine.X1) + (s2xe * DrawLine.Y1) - (s2xe * cc2));
                                dom22 = Math.Abs(((s2ye) * DrawLine.X2) + (s2xe * DrawLine.Y2) - (s2xe * cc2));
                            }
                            double num2 = Math.Sqrt(Math.Abs(s2ye * s2ye) + Math.Abs(s2xe * s2xe));
                            //////
                            double dom3,dom32;
                            if ((-slope3) < 0)
                            {
                                dom3 = Math.Abs(((s3ye) * DrawLine.X1) - ((s3xe * DrawLine.Y1)) + (cc3 * s3xe));
                                dom32 = Math.Abs(((s3ye) * DrawLine.X2) - ((s3xe * DrawLine.Y2)) + (cc3 * s3xe));
                            }
                            else if ((slope3) == 0)
                            {
                                s3ye = 1; s3xe = 0;
                                dom3 = Math.Abs(DrawLine.Y1 - V3.Y);
                                dom32 = Math.Abs(DrawLine.Y2 - V3.Y);
                            }
                            else if (slope3.ToString().Equals("NaN"))
                            {
                                s3ye = 1; s3xe = 0;
                                dom3 = Math.Abs(DrawLine.X1 - V3.X);
                                dom32 = Math.Abs(DrawLine.X2 - V3.X);
                            }
                            else
                            {
                                dom3 = Math.Abs(((s3ye) * DrawLine.X1) + (s3xe * DrawLine.Y1) - (cc3*s3xe));
                                dom32 = Math.Abs(((s3ye) * DrawLine.X2) + (s3xe * DrawLine.Y2) - (cc3 * s3xe));
                            }
                            double num3 = Math.Sqrt(Math.Abs(s3ye * s3ye) + Math.Abs(s3xe * s3xe));
                            //////
                            double dom4,dom42;
                            if ((-slope4) < 0)
                            {
                                dom4 = Math.Abs((s4ye * DrawLine.X1) - (s4xe * DrawLine.Y1) + (cc4 * s4xe));
                                dom42 = Math.Abs((s4ye * DrawLine.X2) - (s4xe * DrawLine.Y2) + (cc4 * s4xe));
                            }
                            else if ((slope4) == 0)
                            {
                                s4ye = 1; s4xe = 0;
                                dom4 = Math.Abs(DrawLine.Y1 - V4.Y);
                                dom42 = Math.Abs(DrawLine.Y2 - V4.Y);
                            }
                            else if (slope4.ToString().Equals("NaN"))
                            {
                                s4ye = 1; s4xe = 0;
                                dom4 = Math.Abs(DrawLine.X1 - V4.X);
                                dom42 = Math.Abs(DrawLine.X2 - V4.X);
                            }
                            else
                            {
                                dom4 = Math.Abs((s4ye * DrawLine.X1) + (s4xe * DrawLine.Y1) - (cc4* s4xe));
                                dom42 = Math.Abs((s4ye * DrawLine.X2) + (s4xe * DrawLine.Y2) - (cc4 * s4xe));
                            }
                            double num4 = Math.Sqrt(Math.Abs(s4ye * s4ye) + Math.Abs(s4xe * s4xe));
                            ////////////////////////////////get the final results of the distance between point and the rectangle line////////////////////////////////////////////
                            //////
                            double result1 = dom1 / num1;
                            double result2 = dom2 / num2;
                            double result3 = dom3 / num3;
                            double result4 = dom4 / num4;
                            double result5 = dom12 / num1;
                            double result6 = dom22 / num2;
                            double result7 = dom32 / num3;
                            double result8 = dom42 / num4;
                            double total1 = result1 + result2 + result3 + result4;
                            double total2 = result5 + result6 + result7 + result8;
                            Console.WriteLine(total1 + " " + c.getCursorID());
                            double tot = 900 + 450;
                            /////////////////////////////////////////////////////////////////////////////////////////
                            if (!slope.ToString().Equals("NaN") && total1 <= tot && total2 <= tot)
                            {
                                MyLines.Add(DrawLine);
                                il++;
                                Shaplist.Add("Line " + il + " " + count);
                                count++;
                                ir = count - 1;
                            }
                        }
                        break;
                    case "Circle": //as same as line
                        {
                            LineList l = new LineList();
                            l.X1 = (double)m_CurX;
                            l.Y1 = (double)m_StartY - (heightc / 2);
                            l.Widthc = widthc;
                            l.Heightc = heightc;
                            l.Type = "Circle";
                            Shape = "Circle";
                            double s1y = (V2.Y - V1.Y); double s1x = (V2.X - V1.X); double slope1 = (s1y / s1x);
                            double s2y = (V3.Y - V2.Y); double s2x = (V3.X - V2.X); double slope2 = (s2y / s2x);
                            double s3y = (V4.Y - V3.Y); double s3x = (V4.X - V3.X); double slope3 = (s3y / s3x);
                            double s4y = (V1.Y - V4.Y); double s4x = (V1.X - V4.X); double slope4 = (s4y / s4x);
                            //////
                            double s1ye = Math.Abs(s1y / gcd((int)s1y, (int)s1x)); double s1xe = Math.Abs(s1x / gcd((int)s1y, (int)s1x));
                            double s2ye = Math.Abs(s2y / gcd((int)s2y, (int)s2x)); double s2xe = Math.Abs(s2x / gcd((int)s2y, (int)s2x));
                            double s3ye = Math.Abs(s3y / gcd((int)s3y, (int)s3x)); double s3xe = Math.Abs(s3x / gcd((int)s3y, (int)s3x));
                            double s4ye = Math.Abs(s4y / gcd((int)s4y, (int)s4x)); double s4xe = Math.Abs(s4x / gcd((int)s4y, (int)s4x));
                            //////
                            double cc1 = (-(slope1 * V1.X) + V1.Y); double cc2 = (-(slope2 * V2.X) + V2.Y);
                            double cc3 = (-(slope3 * V3.X) + V3.Y); double cc4 = (-(slope4 * V4.X) + V4.Y);
                            //////


                            double dom1;
                            double dom12;
                            if ((-slope1) < 0)
                            {
                                dom1 = Math.Abs(((s1ye) * l.X1) - ((s1xe * l.Y1)) + (s1xe * cc1));
                            }
                            else if ((slope1) == 0)
                            {
                                s1ye = 1; s1xe = 0;
                                dom1 = Math.Abs(l.Y1 - V1.Y);
                            }
                            else if (slope1.ToString().Equals("NaN"))
                            {
                                s1ye = 1; s1xe = 0;
                                dom1 = Math.Abs(l.X1 - V1.X);
                            }
                            else
                            {
                                dom1 = Math.Abs(((s1ye) * l.X1) + ((s1xe * l.Y1)) - (s1xe * cc1));
                            }
                            double num1 = Math.Sqrt(Math.Abs(s1ye * s1ye) + Math.Abs(s1xe * s1xe));
                            //////
                            double dom2, dom22;
                            if ((-slope2) < 0)
                            {
                                dom2 = Math.Abs(((s2ye) * l.X1) - (s2xe * l.Y1) + (s2xe * cc2));
                            }
                            else if ((slope2) == 0)
                            {
                                s2ye = 1; s2xe = 0;
                                dom2 = Math.Abs(l.Y1 - V2.Y);
                            }
                            else if (slope2.ToString().Equals("NaN"))
                            {
                                s2ye = 1; s2xe = 0;
                                dom2 = Math.Abs(l.X1 - V2.X);
                            }
                            else
                            {
                                dom2 = Math.Abs(((s2ye) * l.X1) + (s2xe * l.Y1) - (s2xe * cc2));
                            }
                            double num2 = Math.Sqrt(Math.Abs(s2ye * s2ye) + Math.Abs(s2xe * s2xe));
                            //////
                            double dom3, dom32;
                            if ((-slope3) < 0)
                            {
                                dom3 = Math.Abs(((s3ye) * l.X1) - ((s3xe * l.Y1)) + (cc3 * s3xe));
                            }
                            else if ((slope3) == 0)
                            {
                                s3ye = 1; s3xe = 0;
                                dom3 = Math.Abs(l.Y1 - V3.Y);
                            }
                            else if (slope3.ToString().Equals("NaN"))
                            {
                                s3ye = 1; s3xe = 0;
                                dom3 = Math.Abs(l.X1 - V3.X);
                            }
                            else
                            {
                                dom3 = Math.Abs(((s3ye) * l.X1) + (s3xe * l.Y1) - (cc3 * s3xe));
                            }
                            double num3 = Math.Sqrt(Math.Abs(s3ye * s3ye) + Math.Abs(s3xe * s3xe));
                            //////
                            double dom4, dom42;
                            if ((-slope4) < 0)
                            {
                                dom4 = Math.Abs((s4ye * l.X1) - (s4xe * l.Y1) + (cc4 * s4xe));
                            }
                            else if ((slope4) == 0)
                            {
                                s4ye = 1; s4xe = 0;
                                dom4 = Math.Abs(l.Y1 - V4.Y);
                            }
                            else if (slope4.ToString().Equals("NaN"))
                            {
                                s4ye = 1; s4xe = 0;
                                dom4 = Math.Abs(l.X1 - V4.X);
                            }
                            else
                            {
                                dom4 = Math.Abs((s4ye * l.X1) + (s4xe * l.Y1) - (cc4 * s4xe));
                            }
                            double num4 = Math.Sqrt(Math.Abs(s4ye * s4ye) + Math.Abs(s4xe * s4xe));
                            ////////////////////////////////part2////////////////////////////////////////////
                            //////
                            double result1 = dom1 / num1;
                            double result2 = dom2 / num2;
                            double result3 = dom3 / num3;
                            double result4 = dom4 / num4;
                            double total1 = result1 + result2 + result3 + result4;
                            Console.WriteLine(result4 + " " + c.getCursorID());
                            double tot = 900 + 450;
                            /////////////////////////////////////////////////////////////////////////////////////////
                            if (widthc != 0 && total1 <= tot)
                            {
                                MyLines.Add(l);
                                ic++;
                                Shaplist.Add("Circle " + ic + " " + count);
                                count++;
                                ir = count - 1;
                            }
                            break;
                        }
                            
                    case "Curve": //same but we check each of points
                        {
                            LineList curve = new LineList();
                            for (int i = 0; i < path.Count; i+=8)
                            {
                                TuioPoint next_point = path[i];
                                double s1y = (V6.Y - V5.Y); double s1x = (V6.X - V5.X); double slope1 = (s1y / s1x);
                                double s2y = (V7.Y - V6.Y); double s2x = (V7.X - V6.X); double slope2 = (s2y / s2x);
                                double s3y = (V8.Y - V7.Y); double s3x = (V8.X - V7.X); double slope3 = (s3y / s3x);
                                double s4y = (V5.Y - V8.Y); double s4x = (V5.X - V8.X); double slope4 = (s4y / s4x);
                                //////
                                double s1ye = Math.Abs(s1y / gcd((int)s1y, (int)s1x)); double s1xe = Math.Abs(s1x / gcd((int)s1y, (int)s1x));
                                double s2ye = Math.Abs(s2y / gcd((int)s2y, (int)s2x)); double s2xe = Math.Abs(s2x / gcd((int)s2y, (int)s2x));
                                double s3ye = Math.Abs(s3y / gcd((int)s3y, (int)s3x)); double s3xe = Math.Abs(s3x / gcd((int)s3y, (int)s3x));
                                double s4ye = Math.Abs(s4y / gcd((int)s4y, (int)s4x)); double s4xe = Math.Abs(s4x / gcd((int)s4y, (int)s4x));
                                //////
                                double cc1 = (-(slope1 * V5.X) + V5.Y); double cc2 = (-(slope2 * V6.X) + V6.Y);
                                double cc3 = (-(slope3 * V7.X) + V7.Y); double cc4 = (-(slope4 * V8.X) + V8.Y);
                                //////
                                
                                double dom1;
                                if ((-slope1) < 0)
                                {
                                    dom1 = Math.Abs(((s1ye) * next_point.getScreenX(this.Width)) - ((s1xe * next_point.getScreenY(this.Height))) + (s1xe * cc1));
                                }
                                else if ((slope1) == 0)
                                {
                                    s1ye = 1; s1xe = 0;
                                    dom1 = Math.Abs(next_point.getScreenY(this.Height) - V1.Y);
                                }
                                else if (slope1.ToString().Equals("NaN"))
                                {
                                    s1ye = 1; s1xe = 0;
                                    dom1 = Math.Abs(next_point.getScreenX(this.Width) - V1.X);
                                }
                                else
                                {
                                    dom1 = Math.Abs(((s1ye) * next_point.getScreenX(this.Width)) + ((s1xe * next_point.getScreenY(this.Height))) - (s1xe * cc1));
                                }
                                double num1 = Math.Sqrt(Math.Abs(s1ye * s1ye) + Math.Abs(s1xe * s1xe));
                                //////
                                double dom2;
                                if ((-slope2) < 0)
                                {
                                    dom2 = Math.Abs(((s2ye) * next_point.getScreenX(this.Width)) - (s2xe * next_point.getScreenY(this.Height)) + (s2xe * cc2));
                                }
                                else if ((slope2) == 0)
                                {
                                    s2ye = 1; s2xe = 0;
                                    dom2 = Math.Abs(next_point.getScreenY(this.Height) - V2.Y);
                                }
                                else if (slope2.ToString().Equals("NaN"))
                                {
                                    s2ye = 1; s2xe = 0;
                                    dom2 = Math.Abs(next_point.getScreenX(this.Width) - V2.X);
                                }
                                else
                                {
                                    dom2 = Math.Abs(((s2ye) * next_point.getScreenX(this.Width)) + (s2xe * next_point.getScreenY(this.Height)) - (s2xe * cc2));
                                }
                                double num2 = Math.Sqrt(Math.Abs(s2ye * s2ye) + Math.Abs(s2xe * s2xe));
                                //////
                                double dom3;
                                if ((-slope3) < 0)
                                {
                                    dom3 = Math.Abs(((s3ye) * next_point.getScreenX(this.Width)) - ((s3xe * next_point.getScreenY(this.Height))) + (cc3 * s3xe));
                                }
                                else if ((slope3) == 0)
                                {
                                    s3ye = 1; s3xe = 0;
                                    dom3 = Math.Abs(next_point.getScreenY(this.Height) - V3.Y);
                                }
                                else if (slope3.ToString().Equals("NaN"))
                                {
                                    s3ye = 1; s3xe = 0;
                                    dom3 = Math.Abs(next_point.getScreenX(this.Width) - V3.X);
                                }
                                else
                                {
                                    dom3 = Math.Abs(((s3ye) * next_point.getScreenX(this.Width)) + (s3xe * next_point.getScreenY(this.Height)) - (cc3 * s3xe));
                                }
                                double num3 = Math.Sqrt(Math.Abs(s3ye * s3ye) + Math.Abs(s3xe * s3xe));
                                //////
                                double dom4;
                                if ((-slope4) < 0)
                                {
                                    dom4 = Math.Abs((s4ye * next_point.getScreenX(this.Width)) - (s4xe * next_point.getScreenY(this.Height)) + (cc4 * s4xe));
                                }
                                else if ((slope4) == 0)
                                {
                                    s4ye = 1; s4xe = 0;
                                    dom4 = Math.Abs(next_point.getScreenY(this.Height) - V4.Y);
                                }
                                else if (slope4.ToString().Equals("NaN"))
                                {
                                    s4ye = 1; s4xe = 0;
                                    dom4 = Math.Abs(next_point.getScreenX(this.Width) - V4.X);
                                }
                                else
                                {
                                    dom4 = Math.Abs((s4ye * next_point.getScreenX(this.Width)) + (s4xe * next_point.getScreenY(this.Height)) - (cc4 * s4xe));
                                }
                                double num4 = Math.Sqrt(Math.Abs(s4ye * s4ye) + Math.Abs(s4xe * s4xe));
                                ////////////////////////////////part2////////////////////////////////////////////
                                //////
                                double result1 = dom1 / num1;
                                double result2 = dom2 / num2;
                                double result3 = dom3 / num3;
                                double result4 = dom4 / num4;
                                double total1 = result1 + result2 + result3 + result4;
                                Console.WriteLine(result4 + " " + c.getCursorID());
                                double tot = 900 + 450;
                                if (total1 <= tot) {
                                    curve.Points.Add(new Point(next_point.getScreenX(this.Width), next_point.getScreenY(this.Height)));
                                }
                            }
                            curve.Type = "Curve";
                            if (curve.Points.Count > 2 && EnableDrawingCurve) { 
                                MyLines.Add(curve);
                                icv++;
                                Shaplist.Add("Curve " + icv + " " + count);
                                count++;
                                ir = count - 1;
                            }
                            break;
                        }
                    case "Move":
                        {
                            if (ir >= 0)
                            {
                                if (MyLines[ir].Type.Equals("Line"))
                                {
                                    MyLines[ir].X1 = Point1.X;
                                    MyLines[ir].Y1 = Point1.Y;
                                    MyLines[ir].X2 = Point2.X;
                                    MyLines[ir].Y2 = Point2.Y;

                                }
                                else if (MyLines[ir].Type.Equals("Circle"))
                                {
                                    MyLines[ir].X1 = Point1.X;
                                    MyLines[ir].Y1 = Point1.Y;
                                }
                                else if (MyLines[ir].Type.Equals("Curve") && c.getCursorID() == 0)
                                {
                                    MyLines[ir].Points = Curvepoints;
                                    Curvepoints = new List<Point>();
                                }

                            }
                            break;
                        }
                    default: break;
                }

            }

        }
        
        public void updateTuioCursor(TuioCursor e) // if the reactivision detects changes in the cursor position
        {
            lock (cursorSync) //lock the cursor synchronize
            {
                // get the current position of the cursor
                m_CurX = e.Position.getScreenX(this.Width);
                m_CurY = e.Position.getScreenY(this.Height);
                switch (now) // switch features
                {
                    case "Move": //move features
                        {
                            if (ir >= 0)
                            {
                                if (MyLines[ir].Type.Equals("Line")) {
                                    // subtract between the current cursor's position with the line points to be moved and record the new points
                                    Point1.X = (int)(e.getScreenX(this.Width) + MyLines[ir].X1 - StartDownLocation.getScreenX(this.Width));
                                    Point1.Y = (int)(e.getScreenY(this.Height) + MyLines[ir].Y1 - StartDownLocation.getScreenY(this.Height));
                                    Point2.X = (int)(e.getScreenX(this.Width) + MyLines[ir].X2 - StartDownLocation.getScreenX(this.Width));
                                    Point2.Y = (int)(e.getScreenY(this.Height) + MyLines[ir].Y2 - StartDownLocation.getScreenY(this.Height));
                                }
                                else if (MyLines[ir].Type.Equals("Circle"))
                                {
                                    // subtract between the current cursor's position with the circle points to be moved and record the new points
                                    Point1.X = (int)(e.getScreenX(this.Width) + MyLines[ir].X1 - StartDownLocation.getScreenX(this.Width));
                                    Point1.Y = (int)(e.getScreenY(this.Height) + MyLines[ir].Y1 - StartDownLocation.getScreenY(this.Height));
                                }
                                else if (MyLines[ir].Type.Equals("Curve") && e.getCursorID() == 0)
                                {
                                    // subtract between the current cursor's position with the curve points to be moved and record the new points
                                    Curvepoints = new List<Point>();
                                    for (int i = 0; i < MyLines[ir].Points.Count; i++)
                                        {
                                        Point p = MyLines[ir].Points[i];
                                        p.X = e.getScreenX(this.Width) + MyLines[ir].Points[i].X - StartDownLocation.getScreenX(this.Width);
                                        p.Y = e.getScreenY(this.Height) + MyLines[ir].Points[i].Y - StartDownLocation.getScreenY(this.Height);
                                        Curvepoints.Add(p);
                                    }
                                }

                            }
                            break;

                        }
                    case "Edit": //edit feature
                        {
                            if (ir >= 0) 
                            {
                                if (MyLines[ir].Type.Equals("Line")) { //line
                                    if (editdots[e.CursorID] == 1) //if it's first point
                                    {
                                        //change its position
                                        MyLines[ir].X1 = e.getScreenX(this.Width);
                                        MyLines[ir].Y1 = e.getScreenY(this.Height);
                                    }
                                    else if (editdots[e.CursorID] == 2)  //if it's second point
                                    {
                                        //change its position
                                        MyLines[ir].X2 = e.getScreenX(this.Width);
                                        MyLines[ir].Y2 = e.getScreenY(this.Height);
                                    }
                                }
                                else if (MyLines[ir].Type.Equals("Circle"))// circle
                                {
                                    //---------------------------position y---------------------------------
                                    if (prevciry < e.getScreenY(this.Height)) //if the cursor moves up
                                    {
                                        MyLines[ir].Heightc++; //height will size up
                                    }
                                    else if (prevciry > e.getScreenY(this.Height)) //if the cursor moves down
                                    {
                                        MyLines[ir].Heightc--; //height will size down
                                    }
                                    //---------------------------position x---------------------------------
                                    if (prevcirx < e.getScreenX(this.Width))//if the cursor moves left
                                    {
                                        MyLines[ir].Widthc--; //width will size down
                                    }
                                    else if (prevcirx > e.getScreenX(this.Width)) //if the cursor moves right
                                    {
                                        MyLines[ir].Widthc++; //width will size up
                                    }
                                    prevcirx = e.getScreenX(this.Width); //renew the position x
                                    prevciry = e.getScreenY(this.Height); //renew the position y
                                }
                                else if (MyLines[ir].Type.Equals("Curve")) //curve
                                {
                                    for(int i=0;i< MyLines[ir].Points.Count; i++)
                                    {
                                        int check = i + 1;
                                        if (editdots[e.CursorID]== check) //
                                        {
                                            Point p = MyLines[ir].Points[i];
                                            p.X = e.getScreenX(this.Width);
                                            p.Y = e.getScreenY(this.Height);
                                            MyLines[ir].Points[i] = p;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case "Circle": // update the circle size while creating
                        {
                            widthc =  (int)(m_StartX - m_CurX);
                            heightc = widthc;
                            break;
                        }
                    default:break;

                }
            }

        }
/// //////////////////////////// Not used, but it must be here instead of making errors due to removing them///////////////////////////////////////////////////////
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void removeTuioBlob(TuioBlob tblb)
        {
        }
        public void addTuioBlob(TuioBlob tblb)
        {
        }
        public void updateTuioBlob(TuioBlob tblb)
        {
        }


        /////////////////////////Object//////////////////////////////////////////
        

        public void addTuioObject(TuioObject o) // if the object is inserted on the table and the reactivision identifies its fiducial marker
        {
            lock (objectSync)  //lock the object synchronize
            {
                objectList.Add(o.SessionID, new TuioDemoObject(o));
                if (o.SymbolID == 0) // if it's right angled triangle ruler
                {
                    EnableDrawing = true; //enable drawing line
                }
                if (o.SymbolID == 1) // if it's french curve
                {
                    EnableDrawingCurve = true; //enable drawing curve
                }
                else if (o.SymbolID == 6 && !EnableDrawing) //if it's feature selector
                { 
                    status = Mode((o.Angle / Math.PI * 180.0f)); //see the selected feature
                }
                else if (o.SymbolID == 3 && !EnableDrawing) //if it's drawing selector
                {
                    LineL((o.Angle / Math.PI * 180.0f));  //see the selected drawing
                }
            }
        }

        public void updateTuioObject(TuioObject o) //update from an object
        {
            lock (objectSync)//lock the object synchronize
            {
                objectList[o.SessionID].update(o); //update the object's value
                if (o.SymbolID == 6 && !EnableDrawing && !EnableDrawingCurve) //if it's feature selector
                {
                    status = Mode((o.Angle / Math.PI * 180.0f)); //update the selected feature
                }
                else if (o.SymbolID == 3 && !EnableDrawing && !EnableDrawingCurve) //if it's drawing selector
                {
                    LineL((o.Angle / Math.PI * 180.0f)); //update the selected drawing
                    if (now.Equals("Move"))
                    {

                    }
                }
            }
        }

        public void removeTuioObject(TuioObject o) // if the object is removed from the table
        {
            lock (objectSync) //lock the object synchronize
            {
                objectList.Remove(o.SessionID); //object's value will be removed from the list of objects
                if (o.SymbolID == 0) // if it's right angled triangle ruler
                {
                    EnableDrawing = false; // disable drawing lines, circle
                }
                if (o.SymbolID == 1) // if it's french curve
                {
                    EnableDrawingCurve = false; // disable drawing curves
                }
                else if (o.SymbolID == 6 && !EnableDrawing) //if it's feature's selector
                {
                    status = Mode((o.Angle / Math.PI * 180.0f)); //it will see the latest selected feature
                    now = status;
                }
                else if (o.SymbolID == 3 && !EnableDrawing) //if it's drawing's selector
                {
                    LineL((o.Angle / Math.PI * 180.0f)); //it will see the latest selected drawing
                }
                
                if (ir >= 0) //if there're existed drawings
                {
                    switch (now) //features
                    {
                        case "Copy":
                            {  //copy the selected drawing after removing the feature selector
                                LineList DrawLine = new LineList();
                                DrawLine.X1 = MyLines[ir].X1 + 10;
                                DrawLine.Y1 = MyLines[ir].Y1 + 10;
                                DrawLine.X2 = MyLines[ir].X2 + 10;
                                DrawLine.Y2 = MyLines[ir].Y2 + 10;
                                DrawLine.S = MyLines[ir].S;
                                DrawLine.Heightc = MyLines[ir].Heightc;
                                DrawLine.Widthc = MyLines[ir].Widthc;
                                if (MyLines[ir].Type.Equals("Curve"))
                                {
                                    List<Point> s = new List<Point>();
                                    for (int i = 0; i < MyLines[ir].Points.Count; i++)
                                    {
                                        int x = MyLines[ir].Points[i].X + 10;
                                        int y = MyLines[ir].Points[i].Y + 10;
                                        Point p = new Point(x, y);
                                        s.Add(p);
                                    }
                                    DrawLine.Points = s;
                                }
                                DrawLine.Type = MyLines[ir].Type;
                                MyLines.Add(DrawLine);
                                if (MyLines[ir].Type.Equals("Circle"))
                                {
                                    ic++;
                                    Shaplist.Add("Circle " + ic + " " + count);
                                }
                                else if (MyLines[ir].Type.Equals("Line"))
                                {
                                    il++;
                                    Shaplist.Add("Line " + il + " " + count);
                                }
                                else if (MyLines[ir].Type.Equals("Curve"))
                                {
                                    icv++;
                                    Shaplist.Add("Curve " + icv + " " + count);
                                }
                                count++;
                                ir = count - 1;
                                break;
                            }

                        case "Remove":
                            {  //remove the object after removing the feature selector
                                MyLines[ir] = new LineList();
                                Shaplist[ir] = "";
                                if (ir > -1)
                                {
                                    ir--;
                                }
                                break;
                            }
                    }
                }
                while (ir > -1) //while there're existed drawings
                {
                    if (MyLines[ir].Type.Equals("")) //if the drawing value has no type
                    {
                        ir--; //decrease the value
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        

        private void picture_Paint(object sender, PaintEventArgs e)
        {
            Pen LinePen = new Pen(Color.FromArgb(255, 255, 0, 0), 3); // defining the pen into a red
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; //swith to smoothing mode for the drawing
            g.FillRectangle(whiteBrush, 0, 0, picture.Width, picture.Height); // the background will be white blank
            double x1, y1, x2, y2;
            for (int i = 0; i <= MyLines.Count - 1; i++) // for looping the drawings
            {
                if (MyLines[i].Type.Equals("Line")) { //if it's line then it will be drawm
                    x1 = MyLines[i].X1;
                    x2 = MyLines[i].X2;
                    y1 = MyLines[i].Y1;
                    y2 = MyLines[i].Y2;
                    g.DrawLine(LinePen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
                if (MyLines[i].Type.Equals("Curve")) //if it's curve then it will be drawm
                {
                    g.DrawCurve(LinePen, MyLines[i].Points.ToArray());
                }
                else if (MyLines[i].Type.Equals("Circle")) //if it's circle then it will be drawm
                {
                    x1 = MyLines[i].X1;
                    x2 = MyLines[i].Widthc;
                    y1 = MyLines[i].Y1;
                    y2 = MyLines[i].Heightc;
                    g.DrawEllipse(LinePen, (float)x1, (float)y1, (float)x2, (float)y2);
                }

            }
            if (cursordown == true && now.Equals("Move"))
            {
                if (MyLines[ir].Type.Equals("Line"))
                {
                    g.DrawLine(LinePen, Point1.X, Point1.Y, Point2.X, Point2.Y);
                }
                
                else if (MyLines[ir].Type.Equals("Circle"))
                {
                    x1 = Point1.X;
                    x2 = MyLines[ir].Widthc;
                    y1 = Point1.Y;
                    y2 = MyLines[ir].Heightc;
                    g.DrawEllipse(LinePen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
                else if (MyLines[ir].Type.Equals("Curve") && Curvepoints.Count==MyLines[ir].Points.Count)
                {
                    g.DrawCurve(LinePen, Curvepoints.ToArray());
                }
            }

            if (now.Equals("Edit") && MyLines[ir].Type.Equals("Line")) // if the feature is edit & the selected drawing is line then there's little circles will be drawn in all points
            {
                SolidBrush semi = new SolidBrush(Color.FromArgb(40, 255, 0, 0));
                e.Graphics.FillEllipse(semi, (float)MyLines[ir].X1 - 7, (float)MyLines[ir].Y1 - 7, 14, 14);
                e.Graphics.FillEllipse(semi, (float)MyLines[ir].X2 - 7, (float)MyLines[ir].Y2 - 7, 14, 14);
            }
            if (now.Equals("Edit") && MyLines[ir].Type.Equals("Curve")) // if the feature is edit & the selected drawing is Curve then there's little circles will be drawn in all points
            {
                SolidBrush semi = new SolidBrush(Color.FromArgb(40, 255, 0, 0));
                for(int i = 0; i < MyLines[ir].Points.Count; i++)
                {
                    Point p = MyLines[ir].Points[i];
                    e.Graphics.FillEllipse(semi, (float)p.X - 7, (float)p.Y - 7, 14, 14);
                }
            }

            if (cursorList.Count > 0) // if there're one or many cursor detected
            {
                lock (cursorSync) //lock the cursor synchronize
                {
                    foreach (TuioCursor tcur in cursorList.Values) // for looping each of cursors' points
                    {
                        if (now.Equals("Circle")) // selected feature: Circle
                        {
                            // the app will draw circle from the beginning of the cursor point to the end of the cursor point  
                            List<TuioPoint> path = tcur.Path;
                            TuioPoint current_point = path[0];
                            TuioPoint next_point = path[path.Count - 1];
                            int w = current_point.getScreenX(this.Width) - next_point.getScreenX(this.Width);
                            e.Graphics.FillEllipse(grayBrush, next_point.getScreenX(this.Width), current_point.getScreenY(this.Height) - (w / 2), w, w);
                        }
                        else if (now.Equals("Curve")) // selected feature: Curve
                        {
                            // the app will draw the pen path by using all points
                            List<TuioPoint> path = tcur.Path;
                            TuioPoint current_point = path[0];
                            for (int i = 0; i < path.Count; i++)
                            {
                                TuioPoint next_point = path[i];
                                g.DrawLine(fingerPen, current_point.getScreenX(this.Width), current_point.getScreenY(this.Height), next_point.getScreenX(this.Width), next_point.getScreenY(this.Height));
                                current_point = next_point;
                            }
                            g.FillEllipse(grayBrush, current_point.getScreenX(this.Width) - this.Height / 100, current_point.getScreenY(this.Height) - this.Height / 100, this.Height / 50, this.Height / 50);
                            Font font = new Font("Arial", 10.0f);
                            g.DrawString(tcur.CursorID + "", font, blackBrush, new PointF(tcur.getScreenX(this.Width) - 10, tcur.getScreenY(this.Height) - 10));
                        }
                        else // selected feature: Line
                        {
                            // the app will draw line from the beginning of the cursor point to the end of the cursor point  
                            List<TuioPoint> path = tcur.Path;
                            TuioPoint current_point = path[0];
                            TuioPoint next_point = path[path.Count - 1];
                            e.Graphics.DrawLine(fingerPen, current_point.getScreenX(this.Width), current_point.getScreenY(this.Height), next_point.getScreenX(this.Width), next_point.getScreenY(this.Height));
                            current_point = next_point;
                            e.Graphics.FillEllipse(grayBrush, (current_point.getScreenX(this.Width)) - this.Height / 100, current_point.getScreenY(this.Height) - this.Height / 100, this.Height / 50, this.Height / 50);
                            Font font = new Font("Arial", 10.0f);
                            e.Graphics.DrawString(tcur.CursorID + "", font, blackBrush, new PointF((tcur.getScreenX(this.Width)) - 10, tcur.getScreenY(this.Height) - 10));
                        }
                        
                    }
                }
            }
            if (objectList.Count > 0) // if there're one or more objects detected
            {
                lock (objectSync) //lock the object synchronize
                {
                    foreach (TuioDemoObject tobject in objectList.Values)
                    {
                        if (tobject.SymbolID == 6) 
                        {
                            Font font = new Font("Arial", 20.0f);
                            g.DrawString(status, font, blackBrush, new PointF(40, 50));
                        }
                        if (tobject.SymbolID == 9) // if the protractor is identified
                        {
                            // the app will inform you the angle degree in text
                            Font font = new Font("Arial", 20.0f); 
                            int angle =(int) tobject.getAngleDegrees() % 180;
                            string ang = angle.ToString();
                            g.DrawString(ang, font, blackBrush, new PointF(40, 110));
                            this.Refresh();
                        }
                        if (tobject.SymbolID == 3 && p!=null)  // if the drawing selector is identified
                        {
                            // the app will inform you the selected drawing in text
                            Font font = new Font("Arial", 20.0f);
                            string[] words = p[countf].Split(' ');
                            string x = words[0] + " " + words[1];
                            g.DrawString(x, font, blackBrush, new PointF(40, 80));
                        }
                        if (tobject.SymbolID == 0)  // if the right-angled triangle ruler is identified
                        {
                            //the app will show the object's outline and it's movements
                            int objX=tobject.getScreenX(this.Width);
                            int objY = tobject.getScreenY(this.Height);
                            double a= (tobject.Angle / Math.PI * 180.0f);
                            double b = (a * (Math.PI)) / 180;
                            double sin = Math.Sin(b);
                            double cos = Math.Cos(b);
                            V1 = new Point((int)(((- 150) * cos) - ((- 650) * sin)) + objX, (int)(((- 650) * cos) + (( - 150) * sin)) + objY);
                            V2 = new Point((int)(((+ 300) * cos) - ((- 650) * sin)) + objX, (int)(((+ 300) * sin) + (( - 650) * cos)) + objY);
                            V3 = new Point((int)(((+ 300) * cos) - ((+ 250) * sin)) + objX, (int)((( + 300) * sin) + (( + 250) * cos)) + objY);
                            V4 = new Point((int)(((- 150) * cos) - ((+ 250) * sin)) + objX, (int)((( - 150) * sin) + (( + 250) * cos)) + objY);
                            g.DrawLine(LinePen,V1,V2);
                            g.DrawLine(LinePen, V2, V3);
                            g.DrawLine(LinePen, V3, V4);
                            g.DrawLine(LinePen, V4, V1);
                        }
                        if (tobject.SymbolID == 1) // if the french curve ruler is identified
                        {
                            //the app will show the object's outline and it's movements
                            int objX = tobject.getScreenX(this.Width);
                            int objY = tobject.getScreenY(this.Height);
                            double a = (tobject.Angle / Math.PI * 180.0f);
                            double b = (a * (Math.PI)) / 180;
                            double sin = Math.Sin(b);
                            double cos = Math.Cos(b);
                            V5 = new Point((int)(((-150) * cos) - ((-650) * sin)) + objX, (int)(((-650) * cos) + ((-150) * sin)) + objY);
                            V6 = new Point((int)(((+300) * cos) - ((-650) * sin)) + objX, (int)(((+300) * sin) + ((-650) * cos)) + objY);
                            V7 = new Point((int)(((+300) * cos) - ((+250) * sin)) + objX, (int)(((+300) * sin) + ((+250) * cos)) + objY);
                            V8 = new Point((int)(((-150) * cos) - ((+250) * sin)) + objX, (int)(((-150) * sin) + ((+250) * cos)) + objY);
                            g.DrawLine(LinePen, V5, V6);
                            g.DrawLine(LinePen, V6, V7);
                            g.DrawLine(LinePen, V7, V8);
                            g.DrawLine(LinePen, V8, V5);
                        }
                    }
                }
            }
            this.Text = status;

        }
        

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
