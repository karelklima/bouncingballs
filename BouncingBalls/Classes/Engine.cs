using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Input;
//using System.Timers;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace BouncingBalls
{
    class HitRecord
    {
        public enum HitType { Anchor1, Anchor2, Line }
        public HitType Type;
        public double Distance;
        public Ball Ball;
        public Line Line;

        public HitRecord(Ball Ball, Line Line, HitType Type, double Distance)
        {
            this.Ball = Ball;
            this.Line = Line;
            this.Type = Type;
            this.Distance = Distance;
        }
    }

    class Engine
    {
        private Canvas Canvas;
        private DropZone DropZone;
        private GeometryGroup LinesContainer;
        private GeometryGroup BallsContainer;

        private Stack<Ball> Balls;
        private List<Line> Lines;

        private Line CurrentLine;
        private Point StartPoint;

        private double BallRadius;
        private double LineRadius;
        private double HitDistance;
        
        private Vector Gravity;
        private double BounceAcceleration;
        private Int16 DropRate; // 1 ball per N updates
        private Int16 UpdateRate; // n. of updates per second

        private Int16 DropCounter = 0;

        private DispatcherTimer UpdateTimer;

        private bool Updating = false;
        public bool IsUpdating { get { return this.Updating; } }

        private bool SoundEnabled = false;

        private SoundEngine SoundEngine;
        private double CanvasHeight;
        private double CanvasWidth;

        #region Initialization

        public Engine(Canvas Canvas)
        {
            // set default values
            this.BallRadius = 5;
            this.LineRadius = 1.5;
            this.HitDistance = this.BallRadius + this.LineRadius;
            this.Gravity = new Vector(0, 0.2);
            this.BounceAcceleration = 0.95;
            this.DropRate = 150;
            this.DropCounter = this.DropRate;
            this.UpdateRate = 30;

            // set frame rate
             Timeline.DesiredFrameRateProperty.OverrideMetadata(
               typeof(Timeline),
               new FrameworkPropertyMetadata { DefaultValue = 30 }
               );

            // initialize Canvas
            this.Canvas = Canvas;
            this.Canvas.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Canvas_MouseLeftButtonDown);
            this.Canvas.MouseMove += new System.Windows.Input.MouseEventHandler(Canvas_MouseMove);
            this.Canvas.MouseLeftButtonUp += new MouseButtonEventHandler(Canvas_MouseLeftButtonUp);

            // initialize DropZone
            this.DropZone = (DropZone)this.Canvas.FindName("DropZone");
            
            // initialize LinesContainer
            this.LinesContainer = (GeometryGroup)this.Canvas.FindName("LinesContainer");
            
            // initialize BallsContainer
            this.BallsContainer = (GeometryGroup)this.Canvas.FindName("BallsContainer");

            // initialize Lists
            this.Lines = new List<Line>();
            this.Balls = new Stack<Ball>();
            
        }

        public void InitializeSoundEngine()
        {
            this.SoundEngine = new SoundEngine();
        }

        public void PrecalculateSoundProperties()
        {
            this.CanvasWidth = this.Canvas.ActualWidth;
            this.CanvasHeight = this.Canvas.ActualHeight;
        }

        public void Start()
        {
            this.UpdateTimer = new DispatcherTimer();
            this.UpdateTimer.Interval = TimeSpan.FromMilliseconds(1000 / this.UpdateRate);
            this.UpdateTimer.Tick += (s, e) =>
            {
                if (!this.Updating)
                {
                    this.Updating = true;
                    this.DropCounter++;
                    if (this.DropCounter >= this.DropRate)
                    {
                        this.AddBall();
                        this.DropCounter = 0;
                        if (this.SoundEngine != null)
                            this.SoundEngine.DisposeOldBuffers();
                    }
                    this.Update();
                    
                    this.Updating = false;
                }
            };
            this.UpdateTimer.Start();
        }

        public void Stop()
        {
            if (this.SoundEngine != null) this.SoundEngine.Destroy();
        }

        #endregion

        #region Setter functions

        public void SetGravity(double Gravity)
        {
            this.Gravity = new Vector(0, Gravity);
        }

        public void SetBounceAcceleration(double BounceAcceleration)
        {
            this.BounceAcceleration = BounceAcceleration;
        }

        public void SetDropRate(Int16 DropRate)
        {
            this.DropRate = Convert.ToInt16(1000 - (int)DropRate);
        }

        public void SetBallSize(double BallSize)
        {
            this.BallRadius = BallSize / 2;
            this.HitDistance = this.BallRadius + this.LineRadius;
        }

        public void SetLineSize(double LineSize)
        {
            this.LineRadius = LineSize / 2;
            this.HitDistance = this.BallRadius + this.LineRadius;
        }

        public void SetSoundEnabled(bool Flag)
        {
            this.SoundEnabled = Flag;
        }

        #endregion

        #region Hit calculations

        private void Update()
        {
            Stack<Ball> Process = new Stack<Ball>(this.Balls);
            Stack<Ball> NewProcess = new Stack<Ball>();

            while (Process.Count > 0)
            {
                Ball Ball = Process.Pop();

                Ball.Velocity += this.Gravity;
                Ball.ProcessingReminder = 1;

                this.ApplyPotentialHit(Ball);

                if (Ball.ProcessingReminder > 0)
                {
                    Ball.Position += Ball.Velocity * Ball.ProcessingReminder;
                }

                if (this.IsOutOfBoundaries(Ball)) // remove ball out of screen
                {
                    this.BallsContainer.Children.Remove(Ball.Geometry);
                    continue;
                }

                NewProcess.Push(Ball);
            }

            this.Balls = NewProcess; // assign new group of balls

        }

        private void ApplyPotentialHit(Ball Ball)
        {

            Point OriginalPosition = Ball.Position;
            Vector Velocity = Ball.Velocity;
            double ProcessingReminder = Ball.ProcessingReminder;
            Point ProjectedPosition = OriginalPosition + Velocity * ProcessingReminder;

            Line LastLine = null;
            HitRecord Record;

            Int32 Repetitions = 0;
            while (true)
            {
                Repetitions++;
                if (Repetitions > 10)
                {
                    break;
                }
                Record = null;

                foreach (Line Line in this.Lines)
                {
                    if (LastLine == Line) continue; // can't hit one line in two consecutive steps

                    Vector LineVector = Line.Vector;
                    Point Anchor1Position = Line.Anchor1.Position;
                    Vector OriginalProjection = this.GetProjection(Anchor1Position - OriginalPosition, this.GetLeftNormal(LineVector));
                    Vector ProjectedProjection = this.GetProjection(Anchor1Position - ProjectedPosition, this.GetLeftNormal(LineVector));

                    if (OriginalProjection.Length > HitDistance && ProjectedProjection.Length > HitDistance // no hit
                        && ProjectedProjection * OriginalProjection > 0) // no pass-through
                        continue;

                    Vector Anchor1OriginalJoint = Anchor1Position - OriginalPosition;
                    Vector Anchor1ProximityProjection = this.GetProjection(Anchor1OriginalJoint, this.GetLeftNormal(Velocity));
                    if (Anchor1ProximityProjection.Length < this.HitDistance && Anchor1OriginalJoint * Velocity > 0) // anchor1 shot
                    {
                        double HitReminderLength = Math.Sqrt(Math.Pow(Anchor1OriginalJoint.Length, 2) - Math.Pow(Anchor1ProximityProjection.Length, 2))
                                                  -Math.Sqrt(Math.Pow(this.HitDistance, 2) - Math.Pow(Anchor1ProximityProjection.Length, 2));
                        if (Velocity.Length * ProcessingReminder >= HitReminderLength
                            && (Record == null || Record.Distance > HitReminderLength))
                        {
                            Record = new HitRecord(Ball, Line, HitRecord.HitType.Anchor1, HitReminderLength);
                            // anchor1 hit
                        }
                    }

                    Point Anchor2Position = Line.Anchor2.Position;
                    Vector Anchor2OriginalJoint = Anchor2Position - OriginalPosition;
                    Vector Anchor2ProximityProjection = this.GetProjection(Anchor2OriginalJoint, this.GetLeftNormal(Velocity));
                    if (Anchor2ProximityProjection.Length < this.HitDistance && Anchor2OriginalJoint * Velocity > 0) // anchor2 shot
                    {
                        double HitReminderLength = Math.Sqrt(Math.Pow(Anchor2OriginalJoint.Length, 2) - Math.Pow(Anchor2ProximityProjection.Length, 2))
                                                  - Math.Sqrt(Math.Pow(this.HitDistance, 2) - Math.Pow(Anchor2ProximityProjection.Length, 2));
                        if (Velocity.Length * ProcessingReminder >= HitReminderLength
                            && (Record == null || Record.Distance > HitReminderLength))
                        {
                            Record = new HitRecord(Ball, Line, HitRecord.HitType.Anchor2, HitReminderLength);
                            // anchor2 hit
                        }
                    }

                    Vector OriginalNormalized = OriginalProjection / OriginalProjection.Length;
                    Point TransitionedBallPosition = OriginalPosition + OriginalNormalized * this.HitDistance;
                    Point TransitionedProjectedBallPosition = ProjectedPosition + OriginalNormalized * this.HitDistance;

                    Vector OriginalTransitionedProjection = this.GetProjection(Anchor1Position - TransitionedBallPosition, this.GetLeftNormal(LineVector));
                    Vector ProjectedTransitionedProjection = this.GetProjection(Anchor1Position - TransitionedProjectedBallPosition, this.GetLeftNormal(LineVector));

                    if (OriginalTransitionedProjection * ProjectedTransitionedProjection > 0
                        || OriginalTransitionedProjection * Velocity < 0) continue; // no hit

                    Vector TransitionedLineProjection = this.GetProjection(Anchor1Position - TransitionedBallPosition, this.GetLeftNormal(LineVector));

                    Vector VelocityLineProjection = this.GetProjection(Velocity, LineVector);
                    Vector VelocityNormalProjection = this.GetProjection(Velocity, this.GetLeftNormal(LineVector));
                    Point HitPosition = TransitionedBallPosition + VelocityLineProjection * (TransitionedLineProjection.Length / VelocityNormalProjection.Length); 
                    double LineHitReminderLength = (TransitionedBallPosition - HitPosition).Length; 
                    if ((Anchor1Position - HitPosition) * (Anchor2Position - HitPosition) <= 0
                        && (Record == null || Record.Distance > LineHitReminderLength)) // Hit position is between anchors
                    {
                        // line hit
                        Record = new HitRecord(Ball, Line, HitRecord.HitType.Line, LineHitReminderLength);
                    }
                    
                } // end of iteration over lines

                if (Record == null) break;

                

                switch (Record.Type)
                {
                    case HitRecord.HitType.Anchor1:
                    case HitRecord.HitType.Anchor2:

                        double ProcessingPartition = Record.Distance / Velocity.Length;
                        Point HitPosition = OriginalPosition + Velocity * ProcessingPartition;
                        Point AnchorPosition = Record.Type == HitRecord.HitType.Anchor1 ? Record.Line.Anchor1.Position : Record.Line.Anchor2.Position;
                        Vector PerpendicularPartition = this.GetProjection(Velocity, HitPosition - AnchorPosition);
                        Vector ParallelPartition = this.GetProjection(Velocity, this.GetLeftNormal(HitPosition - AnchorPosition));

                        OriginalPosition = HitPosition;
                        ProcessingReminder -= ProcessingPartition;
                        Velocity = ParallelPartition - this.BounceAcceleration * PerpendicularPartition;

                        LastLine = Record.Line;
                        break;

                    case HitRecord.HitType.Line:

                        Vector PerpendicularLinePartition = this.GetProjection(Velocity, this.GetLeftNormal(Record.Line.Vector));
                        Vector ParallelLinePartition = this.GetProjection(Velocity, Record.Line.Vector);
                        double ProcessingLinePartition = Record.Distance / Velocity.Length;
                        
                        Vector BallProjection = this.GetProjection(Record.Line.Anchor1.Position - OriginalPosition, this.GetLeftNormal(Record.Line.Vector));
                        if (BallProjection.Length <= this.HitDistance)
                        {
                            Point NewPosition = OriginalPosition + BallProjection;
                            BallProjection.Normalize();
                            OriginalPosition += -BallProjection * this.HitDistance * 1.01;
                        }

                        OriginalPosition += Velocity * ProcessingLinePartition;

                        ProcessingReminder -= ProcessingLinePartition;
                        Velocity = ParallelLinePartition - this.BounceAcceleration * PerpendicularLinePartition;
                        LastLine = Record.Line;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (this.SoundEnabled) this.PlaySound(Record.Ball, Record.Line);

            } // end of while

            Ball.Position = OriginalPosition;
            Ball.Velocity = Velocity;
            Ball.ProcessingReminder = ProcessingReminder;
        }

        #endregion


        #region Math helper functions

        private Vector GetProjection(Vector ProjectingVector, Vector LineVector)
        {
            Vector Projection = new Vector();
            LineVector.Normalize();
            Projection.X = ProjectingVector * LineVector * LineVector.X;
            Projection.Y = ProjectingVector * LineVector * LineVector.Y;
            return Projection;
        }

        private Vector GetLeftNormal(Vector Vector)
        {
            Vector Normal = new Vector(-Vector.Y, Vector.X);
            Normal.Normalize();
            return Normal;
        }

        private bool IsOutOfBoundaries(Ball Ball)
        {
            return (Ball.Position.X < 0 - this.BallRadius || Ball.Position.X > this.CanvasWidth + this.BallRadius ||
                    /*Ball.Position.Y < 0 ||*/ Ball.Position.Y > this.CanvasHeight + BallRadius);
        }

        #endregion

        #region Sound functions

        private void PlaySound(Ball Ball, Line Line)
        {
            if (this.SoundEngine == null) return;
            if (this.GetProjection(Ball.Velocity, this.GetLeftNormal(Line.Vector)).Length < 1) return;

            double Pitch = 100 - Line.Vector.Length / 12;
            Pitch = Math.Round(Pitch);
            
            int Amplitude = Convert.ToInt32(100 * Ball.Velocity.Length);
            if (Amplitude > 5000) Amplitude = 5000;

            double Position = Math.Max(Math.Min(Ball.Position.X, this.CanvasWidth), 0);

            float Pan = (float)(Position / CanvasWidth);

            this.SoundEngine.Play(
                Pitch,
                Amplitude,
                Pan
                );
        }

        #endregion

        #region Screen manipulation functions

        private void AddBall()
        {
            Ball Ball = new Ball(this);
            Ball.Position = this.DropZone.Position;
            this.BallsContainer.Children.Add(Ball.Geometry);
            this.Balls.Push(Ball);
        }

        private void CreateLine(Point Coordinates)
        {
            this.CurrentLine = new Line(this);
            this.LinesContainer.Children.Add(this.CurrentLine.Geometry);
            this.Canvas.Children.Add(this.CurrentLine.Anchor1);
            this.Canvas.Children.Add(this.CurrentLine.Anchor2);
            this.CurrentLine.Loaded += (Line) =>
            {
                Line.StartPoint = Coordinates;
                Line.EndPoint = Coordinates;
                Line.Update();
                Lines.Add(Line);
            }; 
        }

        public void DropLine(Line Line)
        {
            this.Lines.Remove(Line);
            this.Canvas.Children.Remove(Line.Anchor1);
            this.Canvas.Children.Remove(Line.Anchor2);
            this.LinesContainer.Children.Remove(Line.Geometry);
            Line = null;
        }

        public void ClearAll()
        {
            if (this.Updating != true)
            {
                this.Lines = new List<Line>();
                this.Balls = new Stack<Ball>();
                this.LinesContainer.Children.Clear();
                this.BallsContainer.Children.Clear();
                return;
            }

            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(10);
            Timer.Tick += (s, e) =>
                {
                    this.ClearAll();
                    DispatcherTimer x = (DispatcherTimer)s;
                    x.Stop();
                };
            Timer.Start();
        }

        void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //throw new NotImplementedException();
            Point MousePosition = e.GetPosition(null);
            Vector diff = this.StartPoint - MousePosition;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                && this.CurrentLine != null
                && this.CurrentLine.IsLoaded)
            {
                this.CurrentLine.EndPoint = MousePosition;
                this.CurrentLine.Update();
            }
        }

        void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
            this.StartPoint = e.GetPosition(this.Canvas);
            this.CreateLine(e.GetPosition(this.Canvas));
        }

        void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CurrentLine = null;
        }

        #endregion

    }
}
