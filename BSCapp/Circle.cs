using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSCapp
{

    public class CircleList
    {
        public static long TotalCount;
        private double _X1;
        private double _Y1;
        private double _X2;
        private double _Y2;
        

        public double X
        {
            get { return _X1; }
            set { _X1 = value; }
        }

        public double Y
        {
            get { return _Y1; }
            set { _Y1 = value; }
        }

        public double Width
        {
            get { return _X2; }
            set { _X2 = value; }
        }

        public double Height
        {
            get { return _Y2; }
            set { _Y2 = value; }
        }

        public CircleList(double _X1, double _Y1, double _X2, double _Y2)
        {
            X = _X1;
            Y = _Y1;
            Width = _X2;
            Height = _Y2;
            TotalCount += 1;
        }

        public CircleList()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            TotalCount += 1;
        }

        ~CircleList()
        {
            TotalCount -= 1;
        }
    }



}
