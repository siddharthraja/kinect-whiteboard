using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

namespace KinectWhiteBoard
{
    class GestureHandler : INotifyPropertyChanged
    {
        public bool clapDetected = false;
        private KinectSensor kinectSensor = null;

        private Body[] bodies = null;
        private BodyFrameReader bodyFrameReader = null;
        private string statusText = null;
        private List<GestureDetector> gestureDetectorList = null;

        private Window1 windowRef;
        
        public GestureHandler(Window1 w)
        {
            this.kinectSensor = KinectSensor.GetDefault();
            windowRef = w;
            // open the sensor
            this.kinectSensor.Open();

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            this.gestureDetectorList = new List<GestureDetector>();
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                // GestureResultView result = new GestureResultView(i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this.kinectSensor);
                this.gestureDetectorList.Add(detector);
                // bool detectedtest = detector.result.Detected;

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            //Console.Write("\nReader_BodyFrameArrived");
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    //Console.Write("\nReader_BodyFrameArrived............bodyFrame != null");
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {

                //this.kinectBodyView.UpdateBodyFrame(this.bodies);
                for (int i = 0; i < gestureDetectorList.Count; i++)
                {
                    if (gestureDetectorList[i].clapDetected == true)
                    {
                        //Console.Write("\nReader_BodyFrameArrived............dataReceived " + this.kinectSensor.BodyFrameSource.BodyCount + "|" + gestureDetectorList.Count + i);
                        windowRef.drawLines();
                    }
                    if (gestureDetectorList[i].sliceDetected == true)
                    {
                        //Console.Write("\nReader_BodyFrameArrived............dataReceived " + this.kinectSensor.BodyFrameSource.BodyCount + "|" + gestureDetectorList.Count + i);
                        windowRef.deleteLinkByGesture();
                    }

                    if (gestureDetectorList[i].swipedownDetected == true)
                    {
                        windowRef.showMenu();
                        //Console.Write("\nReader_BodyFrameArrived............dataReceived " + this.kinectSensor.BodyFrameSource.BodyCount + "|" + gestureDetectorList.Count + i);
                        // windowRef.deleteLinkByGesture();
                    }

                    if (gestureDetectorList[i].swipeUpDetected == true)
                    {
                        windowRef.hideMenu();
                    }

                    if (gestureDetectorList[i].takeSnapDetected == true)
                    {
                        windowRef.saveSnapShot();
                    }

                    if (gestureDetectorList[i].openRightDetected == true)
                    {
                        windowRef.showPicMenu();
                    }

                    if (gestureDetectorList[i].closeRightDetected == true)
                    {
                        windowRef.hidePicMenu();
                    }

                    if (gestureDetectorList[i].upsideDownDetected == true)
                    {
                        windowRef.flipCanvas();
                    }

                    //else if (gestureDetectorList[i].upsideDownDetected == false)
                    //{
                    //    windowRef.flipBackCanvas();
                    //}

                }

                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;
                        Joint handLeft = body.Joints[JointType.HandLeft];
                        Joint handRight = body.Joints[JointType.HandRight];
                        
                        if (body.HandLeftState == HandState.Open)
                        {
                            if (!windowRef.leftHandOpen)
                            {
                                windowRef.shiftText = "Shift Mode is Off";
                                windowRef.GroupModeText.Text = windowRef.shiftText + " | " + windowRef.groupText;
                                windowRef.leftHandOpen = true;
                                windowRef.groupSelection();                                
                            }
                        }
                        else if (body.HandLeftState == HandState.Closed)
                        {
                            if (windowRef.leftHandOpen)
                            {
                                windowRef.shiftText = "Shift Select is On";
                                windowRef.GroupModeText.Text = windowRef.shiftText + " | " + windowRef.groupText;
                                windowRef.leftHandOpen = false;
                                windowRef.groupSelection();
                            }
                        }

                        // RIGHT HAND....................................................
                        //if (body.HandRightState == HandState.Closed)
                        //{
                        //    if (!windowRef.rightHandOpen) {
                        //        Console.Write("\nbody.HandRightState == HandState.Closed " + windowRef.tempCount++);
                        //        windowRef.rightHandOpen = true;
                        //    }
                        //}
                        //else if (body.HandRightState == HandState.Open)
                        //{
                        //    if (windowRef.rightHandOpen)
                        //    {
                        //        windowRef.rightHandOpen = false;
                        //        Console.Write("\nbody.HandRightState == HandState.Open " + windowRef.tempCount++);
                        //    }
                        //}

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            this.gestureDetectorList[i].IsPaused = trackingId == 0;

                            
                        }
                    }
                }
            }
        }
    }
}