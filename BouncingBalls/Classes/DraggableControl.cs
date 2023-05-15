using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace BouncingBalls
{
    class DraggableControl : Thumb
    {
        private Point _Position;

        public Point Position
        {
            get
            {
                Point Position = new Point();
                Position.X = (double)this.GetValue(Canvas.LeftProperty);
                Position.Y = (double)this.GetValue(Canvas.TopProperty);
                return Position;
                //return this._Position;
            }
            set
            {
                this._Position = value;
                this.SetValue(Canvas.LeftProperty, (double)value.X);
                this.SetValue(Canvas.TopProperty, (double)value.Y);
            }
        }

        public DraggableControl()
        {
            DragDelta += new DragDeltaEventHandler(this.DraggableControl_DragDelta);
        }

        protected virtual void DraggableControl_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }

    }
}
