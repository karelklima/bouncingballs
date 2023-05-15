using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace BouncingBalls
{
    class Line
    {
        private Engine _Engine;

        private Anchor _Anchor1;
        public Anchor Anchor1 { get { return this._Anchor1; } }

        private Anchor _Anchor2;
        public Anchor Anchor2 { get { return this._Anchor2; } }

        private LineGeometry _Geometry;
        public LineGeometry Geometry { get { return this._Geometry; } }

        public Vector Vector;
        /*public Vector Vector
        {
            get { return this.Anchor2.Position - this.Anchor1.Position; }
        }*/

        public Point StartPoint
        {
            get
            {
                return this.Geometry.StartPoint;
            }
            set
            {
                this.Geometry.StartPoint = value;
                this.Anchor1.Position = value;
            }
        }

        public Point EndPoint
        {
            get
            {
                return this.Geometry.EndPoint;
            }
            set
            {
                this.Geometry.EndPoint = value;
                this.Anchor2.Position = value;
            }
        }

        public Line(Engine Engine)
        {
            this._Engine = Engine;
            this._Geometry = new LineGeometry();
            this._Anchor1 = new Anchor(this);
            this._Anchor1.Loaded += new RoutedEventHandler(Anchor_Loaded);
            this._Anchor2 = new Anchor(this);
            this._Anchor2.Loaded += new RoutedEventHandler(Anchor_Loaded);
        }

        public delegate void LineLoadedEventHandler(Line sender);

        public event LineLoadedEventHandler Loaded;

        private void OnLineLoaded()
        {
            /*this.*/Loaded(this);
        }

        private void Anchor_Loaded(Object sender, EventArgs e)
        {
            if (this.IsLoaded) this.OnLineLoaded();
        }

        public bool IsLoaded
        {
            get
            {
                return this.Anchor1 != null && this.Anchor1.IsLoaded
                    && this.Anchor2 != null && this.Anchor2.IsLoaded;
            }
        }

        public void Update()
        {
            this._Geometry.StartPoint = Anchor1.Position;
            this._Geometry.EndPoint = Anchor2.Position;
            this.Vector = Anchor2.Position - Anchor1.Position;
        }

        public void Drop()
        {
            this._Engine.DropLine(this);
        }
    }
}
