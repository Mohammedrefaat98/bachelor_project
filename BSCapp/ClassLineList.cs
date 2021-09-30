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

    public class LineList
    {
        public static long TotalCount; //counter of all drawings existed in the application
        private double _X1; //point X1 of the line, Circle
        private double _Y1; //point Y1 of the line, Circle
        private double _X2; //point X2 of the line
        private double _Y2; //point Y2 of the line
        private double _S; //Slope of the line
        private string _Type; //Type of the drawing
        private List<Point> _points; //list of points of curve
        private double _heightc; //height of the circle
        private double _widthc; //width of the circle

        public List<Point> Points
        {
            get { return _points; }
            set { _points = value; }
        }
        public double Widthc
        {
            get { return _widthc; }
            set { _widthc = value; }
        }
        public double Heightc
        {
            get { return _heightc; }
            set { _heightc = value; }
        }
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public double S
        {
            get { return _S; }
            set { _S = value; }
        }

        public double X1
        {
            get { return _X1; }
            set { _X1 = value; }
        }

        public double Y1
        {
            get { return _Y1; }
            set { _Y1 = value; }
        }

        public double X2
        {
            get { return _X2; }
            set { _X2 = value; }
        }

        public double Y2
        {
            get { return _Y2; }
            set { _Y2 = value; }
        }

        public LineList(double _X1, double _Y1, double _X2, double _Y2, double _S,string _Type,double _widthc, double _heightc, List<Point> _points) //constructor
        {
            X1 = _X1;
            Y1 = _Y1;
            X2 = _X2;
            Y2 = _Y2;
            S = _S;
            Type = _Type;
            Heightc = _heightc;
            Widthc = _widthc;
            Points = _points;
            TotalCount += 1;
        }

        public LineList() //Initial constructor
        {
            X1 = 0;
            Y1 = 0;
            X2 = 0;
            Y2 = 0;
            S = 0;
            Type = "";
            Heightc = 0;
            Widthc = 0;
            Points = new List<Point>();
            TotalCount += 1;
        }

        ~LineList() //deleted constructor
        {
            TotalCount -= 1;
        }
    }



}
