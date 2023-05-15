using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media;


namespace BouncingBalls
{
    class Ball
    {
        private LineGeometry _Geometry;
        public LineGeometry Geometry { get { return this._Geometry; }  }

        private Point _Position;
        public Point Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                _Geometry.StartPoint = value;
                _Geometry.EndPoint = value;
            }
        }

        public Vector Velocity;
        public double ProcessingReminder;
        
        public Ball(Engine Engine)
        {
            this._Geometry = new LineGeometry();
            this.Velocity = new Vector(0, 0);
        }
    }
}
