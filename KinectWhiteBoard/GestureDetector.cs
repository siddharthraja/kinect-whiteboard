//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace KinectWhiteBoard
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    public class GestureDetector : IDisposable
    {
        private readonly string gestureClap = @"Database\Clapping.gbd";
        private readonly string gestureSlice = @"Database\SliceGesture.gbd";
        private readonly string gestureMenu = @"Database\MenuDown.gbd";
        private readonly string menuUp = @"Database\MenuUp.gbd";
        private readonly string gestureTakeSnap = @"Database\Take_Snap.gbd";

        private readonly string rightMenuOpen = @"Database\OpenRightMenu.gbd";
        private readonly string rightMenuClose = @"Database\CloseRightMenu.gbd";
        private readonly string upsideDown = @"Database\UpsideDown.gbd";


        private readonly string seatedGestureName = "clapping"; //name of the project while creating the gesture (in the solution)
        private readonly string deleteGestureName = "slice_Right";
        private readonly string gestureMenuName = "menu_down";
        private readonly string menuUpName = "menu_up";
        private readonly string takeSnapName = "take_snap";
        private readonly string upsideDownName = "upside_down";


        private readonly string rightMenuOpenName = "open_right_menu";
        private readonly string rightMenuCloseName = "right_menu_close";

        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        public DiscreteGestureResult result = null;

        public bool clapDetected = false;
        public bool sliceDetected = false;
        public bool swipedownDetected = false;
        public bool swipeUpDetected = false;
        public bool takeSnapDetected = false;
        public bool openRightDetected = false;
        public bool closeRightDetected = false;
        public bool upsideDownDetected = false; 

        public int takeSnapDetectedInc = 0;
        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public GestureDetector(KinectSensor kinectSensor)
        {

            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            //if (gestureResultView == null)
            //{
            //    throw new ArgumentNullException("gestureResultView");
            //}
            //Console.Write("\ngesture detector");
            //this.GestureResultView = gestureResultView;

            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the 'Seated' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureClap))
            {
                 foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | " + gesture.Name);
                    if (gesture.Name.Equals(this.seatedGestureName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureSlice))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.deleteGestureName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureMenu))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.gestureMenuName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.menuUp))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.menuUpName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureTakeSnap))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.takeSnapName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.rightMenuOpen))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.rightMenuOpenName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.rightMenuClose))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.rightMenuCloseName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.upsideDown))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //Console.Write("\nCurrent Gesture = " + gesture.ToString() + " | "+gesture.Name);
                    if (gesture.Name.Equals(this.upsideDownName))
                    {
                        //Console.Write("\nFOUND GESTURE");
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }
        }

        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.Name.Equals(this.seatedGestureName) && gesture.GestureType == GestureType.Discrete)
                            {
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.400)
                                    {
                                        //Console.Write("\nRESULT: " + result.Detected + "|" + result.Confidence);
                                        clapDetected = true;
                                    }
                                    else clapDetected = false;

                                }
                            }

                            if (gesture.Name.Equals(this.takeSnapName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // need to retrain this
                                    if (result.Detected.Equals(true) && result.Confidence > 0.400)
                                    {
                                        Console.Write("\n Snap Pic Result: " + result.Detected + "|" + result.Confidence);

                                       
                                            takeSnapDetected = true;
                                            //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                                            //dispatcherTimer.Start();
                                            //dispatcherTimer.Tick += (test, args) =>
                                            //{
                                            //    dispatcherTimer.Stop();
                                            //    takeSnapDetected = false;
                                            //};

                                    }
                                    else { takeSnapDetected = false; }

                                }
                            }

                            if (gesture.Name.Equals(this.deleteGestureName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.350)
                                    {
                                        // Console.Write("\n sliceDetected RESULT: " + result.Detected + "|" + result.Confidence);
                                        sliceDetected = true;
                                    }
                                    else { sliceDetected = false; }

                                }
                            }

                            if (gesture.Name.Equals(this.gestureMenuName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.400)
                                    {
                                        // Console.Write("\n swipe down RESULT: " + result.Detected + "|" + result.Confidence);
                                        swipedownDetected = true;
                                    }
                                    else { swipedownDetected = false; }

                                }
                            }

                            if (gesture.Name.Equals(this.menuUpName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.600)
                                    {
                                        // Console.Write("\n Swipe Up RESULT: " + result.Detected + "|" + result.Confidence);
                                        swipeUpDetected = true;
                                    }
                                    else { swipeUpDetected = false; }

                                }
                            }

                            

                            if (gesture.Name.Equals(this.rightMenuOpenName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.600)
                                    {
                                        // Console.Write("\n rightMenuOpenName: " + result.Detected + "|" + result.Confidence);
                                        openRightDetected = true; 
                                    }
                                    else { openRightDetected = false; }

                                }
                            }


                            if (gesture.Name.Equals(this.rightMenuCloseName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.800)
                                    {
                                        //Console.Write("\n Snap Pic Result: " + result.Detected + "|" + result.Confidence);
                                        closeRightDetected = true; 
                                    }
                                    else { closeRightDetected = false; }

                                }
                            }

                            if (gesture.Name.Equals(this.upsideDownName) && gesture.GestureType == GestureType.Discrete)
                            {
                                //  DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                    if (result.Detected.Equals(true) && result.Confidence > 0.800)
                                    {
                                        Console.Write("\n UPSIDE DOWN!!!!!!: " + result.Detected + "|" + result.Confidence);
                                        upsideDownDetected = true;
                                    }
                                    else { upsideDownDetected = false; }

                                }
                            }
                        }
                    }
                }
            }
        }

        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            // this.GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }
    }
}
