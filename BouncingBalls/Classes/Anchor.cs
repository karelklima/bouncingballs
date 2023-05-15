using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media;


namespace BouncingBalls
{
    class Anchor : DraggableControl
    {
        private Line _Line;
        public Line Line { get { return this._Line; } }

        public Anchor(Line Line)
        {
            this._Line = Line;
            MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(Anchor_MouseRightButtonDown);
        }

        protected override void DraggableControl_DragDelta(object sender, DragDeltaEventArgs e)
        {
            base.DraggableControl_DragDelta(sender, e);
            this.Line.Update();
        }

        private void Anchor_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Line.Drop();
        }
    }
}
