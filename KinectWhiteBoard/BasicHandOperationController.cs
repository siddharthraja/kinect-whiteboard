using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;

using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;

using System.Windows.Forms;
using System.Linq;

namespace KinectWhiteBoard
{ 
    
    public class BasicHandOperationController : IKinectManipulatableController
    {
    
        // simple flag for enabling "New thumb" mode
        private bool _isAddNew;
        // Window1.xaml.btnNewAction.IsEnabled = true;

        public BasicHandOperationController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            this.inputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.myThumb = this.inputModel.Element as MyThumb;
            this.kinectRegion.GotTouchCapture += testGrab;
                        
            this.inputModel.ManipulationStarted += InputModel_ManipulationStarted;
            this.inputModel.ManipulationUpdated += InputModel_ManipulationUpdated;
            this.inputModel.ManipulationCompleted += InputModel_ManipulationCompleted;
        }

        private void testGrab(object sender, TouchEventArgs e)
        {
            Console.Write("\nTouchEventArgs");
        }

        private void KinectRegion_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            Console.Write("\nKinectRegion_ManipulationCompleted " + myThumb.getWindowRef().tempCount++);
        }

        private void InputModel_ManipulationCompleted(object sender, Microsoft.Kinect.Input.KinectManipulationCompletedEventArgs e)
        {
            Console.Write("\nInputModel_ManipulationCompleted: " + myThumb.index);
            // myThumb.getWindowRef().checkThumbOverCan(this.myThumb);
            myThumb.getWindowRef().kinectManipulationInProgress = true;
            // myThumb.getWindowRef().ActionText.Text = myThumb.Name + " released";

            if (myThumb.getWindowRef().enableCurrentOps)
            {
                if (myThumb.getWindowRef().checkThumbOverCan())
                {
                    myThumb.getWindowRef().deleteThumb(myThumb, true);
                    myThumb.getWindowRef().ActionText.Foreground = new SolidColorBrush(Colors.Black);
                    myThumb.getWindowRef().ActionText.Text = "Deleting " + myThumb.Name;

                }
                else
                    if (myThumb.getWindowRef().checkThumbOverRightMenu())
                    {
                        myThumb.getWindowRef().addBackToPicMenu(myThumb);
                         myThumb.getWindowRef().ActionText.Foreground = new SolidColorBrush(Colors.Black);
                         myThumb.getWindowRef().ActionText.Text = "Putting " + myThumb.Name + " into Image Bank";
                        myThumb.getWindowRef().deleteThumb(myThumb, false);
                    }
            }
        }

        private void InputModel_ManipulationUpdated(object sender, Microsoft.Kinect.Input.KinectManipulationUpdatedEventArgs e)
        {
            //Console.Write("\nInputModel_ManipulationUpdated: " + myThumb.index);
            var parentCanvas = myThumb.Parent as Canvas;
            var delta = e.Delta.Translation;
            var yDelta = delta.Y * this.kinectRegion.ActualHeight;
            var xDelta = delta.X * this.kinectRegion.ActualWidth;
            int count = 0;
            // myThumb.getWindowRef().ActionText.Text = "Dragging " + myThumb.Name;

            if (myThumb.getWindowRef().wholeGroupSelected)
            {
                //Console.Write("\nLoop..............................................................................................");
                foreach (MyThumb thumb in myThumb.getWindowRef().getGroupModeObject().currentSelectedGroup.getListOfThumbs())
                {
                    var y = Canvas.GetTop(thumb);
                    var x = Canvas.GetLeft(thumb);
                    if (double.IsNaN(y)) y = 0;
                    if (double.IsNaN(x)) x = 0;
                    //Console.Write("\nInputModel_ManipulationUpdated: " + (count++) + " => " + yDelta + " , " + xDelta + " , " + y + " , " + x);
                    
                    // delta values are 0.0 to 1.0, so we need to scale it to the number of pixels in the kinect region
                    if (parentCanvas != null)
                    {


                        if (Canvas.GetLeft(thumb) > 0)
                        {
                            Canvas.SetTop(thumb, y + yDelta);
                            Canvas.SetLeft(thumb, x + xDelta);
                        }
                        else
                        {
                            Canvas.SetTop(thumb, y + yDelta);
                            Canvas.SetLeft(thumb, 1);
                        }

                        myThumb.getWindowRef().dragWholeGroupUpdate(thumb);
                    }

                    
                }
            }
            else
            {
                var y = Canvas.GetTop(myThumb);
                var x = Canvas.GetLeft(myThumb);
                if (double.IsNaN(y)) y = 0;
                if (double.IsNaN(x)) x = 0;

                // delta values are 0.0 to 1.0, so we need to scale it to the number of pixels in the kinect region
                //var yDelta = delta.Y * this.kinectRegion.ActualHeight;
                //var xDelta = delta.X * this.kinectRegion.ActualWidth;
                if (parentCanvas != null)
                {
                    
                    if (Canvas.GetLeft(myThumb) > 0)
                    {
                        Canvas.SetTop(myThumb, y + yDelta);
                        Canvas.SetLeft(myThumb, x + xDelta);
                    }
                    else
                    {
                        Canvas.SetTop(myThumb, y + yDelta);
                        Canvas.SetLeft(myThumb, 1);
                    }
                }

                myThumb.getWindowRef().UpdateAllForAThumb(myThumb);
            }
        }
        
        private void InputModel_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            Console.Write("\nInputModel_ManipulationStarted:  " + myThumb.getWindowRef().tempCount++);
            myThumb.getWindowRef().kinectManipulationInProgress = true;
            myThumb.getWindowRef().thumbSelectionKinect(myThumb);
        }

        FrameworkElement IKinectController.Element
        {
            get
            {
                return this.inputModel.Element as FrameworkElement;
            }
        }

        ManipulatableModel IKinectManipulatableController.ManipulatableInputModel
        {
            get
            {
                return this.inputModel;
            }
        }



        private ManipulatableModel inputModel;
        private KinectRegion kinectRegion;
        // private DragDropElement dragDropElement;
        private MyThumb myThumb;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).          
                }

                this.kinectRegion = null;
                this.inputModel = null;
                this.myThumb = null;

                this.inputModel.ManipulationStarted -= InputModel_ManipulationStarted;
                this.inputModel.ManipulationUpdated -= InputModel_ManipulationUpdated;
                this.inputModel.ManipulationCompleted -= InputModel_ManipulationCompleted;

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
