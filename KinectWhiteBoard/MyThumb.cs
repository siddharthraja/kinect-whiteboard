using System;
using System.Drawing;
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
  public class MyThumb : Thumb, IKinectControl
  {
    public List<LineGeometry> EndLines { get; private set; }
    public List<LineGeometry> StartLines { get; private set; }
    public int index;
    public int groupNumber;
    public int grouplinecount;
    private Window1 windowRef;
    public string imgSource = null;
    public Rectangle rect;
    // private Path _path;
    bool IKinectControl.IsManipulatable
    {
        get
        {
            return true;
        }
    }

    bool IKinectControl.IsPressable
    {
        get
        {
            return false;
        }
    }

    IKinectController IKinectControl.CreateController(IInputModel inputModel, KinectRegion kinectRegion)
    {
        return new BasicHandOperationController(inputModel, kinectRegion);
    }

    public void setWindowRef(Window1 w)
    {
        windowRef = w;
    }
    public Window1 getWindowRef()
    {
        return windowRef;
    }

    public MyThumb(Window1 w)
        : this()
    {        
        windowRef = w;        
    }

    public MyThumb()
    {
        groupNumber = -1;
        windowRef = null;
        grouplinecount = 0;
        StartLines = new List<LineGeometry>();
        EndLines = new List<LineGeometry>();
    }
      /*
    public Point getX()
    {
        return 
    }*/
    
  }

  public class Link
  {
      private MyThumb startNode;
      private MyThumb endNode;
      private LineGeometry line;
      private Path path;
      private Rectangle rect;

      public Link(MyThumb s, MyThumb e, int color, int thickness)
      {
          line = new LineGeometry();
          startNode = s;
          endNode = e;
          createPath(color, thickness);
          path.Data = line;
          s.StartLines.Add(line);
          e.EndLines.Add(line);
          
      }

      public void createPath(int color, int thickness)
      {
          //int thickness = 3;
          
          SolidColorBrush[] b = { 
    Brushes.Black, Brushes.Aqua, Brushes.Blue,  Brushes.Brown, Brushes.Aquamarine, Brushes.BlueViolet, Brushes.Bisque, Brushes.BurlyWood, Brushes.CadetBlue, Brushes.Chartreuse, Brushes.Chocolate, Brushes.Coral, Brushes.CornflowerBlue, Brushes.Cornsilk, Brushes.Crimson, Brushes.Cyan, Brushes.BlanchedAlmond, Brushes.DarkBlue, Brushes.DarkCyan, Brushes.DarkGoldenrod, Brushes.DarkGray, Brushes.DarkGreen, Brushes.DarkKhaki, Brushes.DarkMagenta, Brushes.DarkOliveGreen, Brushes.DarkOrange, Brushes.DarkOrchid, Brushes.DarkRed, Brushes.DarkSalmon, Brushes.DarkSeaGreen, Brushes.DarkSlateBlue, Brushes.DarkSlateGray, Brushes.DarkTurquoise, Brushes.DarkViolet, Brushes.DeepPink, Brushes.DeepSkyBlue, Brushes.DimGray, Brushes.DodgerBlue, Brushes.Firebrick, Brushes.FloralWhite, Brushes.ForestGreen, Brushes.Fuchsia, Brushes.Gainsboro, Brushes.GhostWhite, Brushes.Gold, Brushes.Goldenrod, Brushes.Gray, Brushes.Green, Brushes.GreenYellow, Brushes.Honeydew, Brushes.HotPink, Brushes.IndianRed, Brushes.Indigo, Brushes.Ivory, Brushes.Khaki, Brushes.Lavender, Brushes.LavenderBlush, Brushes.LawnGreen, Brushes.LemonChiffon, Brushes.LightBlue, Brushes.LightCoral, Brushes.LightCyan, Brushes.LightGoldenrodYellow, Brushes.LightGray, Brushes.LightGreen, Brushes.LightPink, Brushes.LightSalmon, Brushes.LightSeaGreen, Brushes.LightSkyBlue, Brushes.LightSlateGray, Brushes.LightSteelBlue, Brushes.LightYellow, Brushes.Lime, Brushes.LimeGreen, Brushes.Linen, Brushes.Magenta, Brushes.Maroon, Brushes.MediumAquamarine, Brushes.MediumBlue, Brushes.MediumOrchid, Brushes.MediumPurple, Brushes.MediumSeaGreen, Brushes.MediumSlateBlue, Brushes.MediumSpringGreen, Brushes.MediumTurquoise, Brushes.MediumVioletRed, Brushes.MidnightBlue, Brushes.MintCream, Brushes.MistyRose, Brushes.Moccasin, Brushes.NavajoWhite, Brushes.Navy, Brushes.OldLace, Brushes.Olive, Brushes.OliveDrab, Brushes.Orange, Brushes.OrangeRed, Brushes.Orchid, Brushes.PaleGoldenrod, Brushes.PaleGreen, Brushes.PaleTurquoise, Brushes.PaleVioletRed, Brushes.PapayaWhip, Brushes.PeachPuff, Brushes.Peru, Brushes.Pink, Brushes.Plum, Brushes.PowderBlue, Brushes.Purple, Brushes.Red, Brushes.RosyBrown, Brushes.RoyalBlue, Brushes.SaddleBrown, Brushes.Salmon, Brushes.SandyBrown, Brushes.SeaGreen, Brushes.SeaShell, Brushes.Sienna, Brushes.Silver, Brushes.SkyBlue, Brushes.SlateBlue, Brushes.SlateGray, Brushes.Snow, Brushes.SpringGreen, Brushes.SteelBlue, Brushes.Tan, Brushes.Teal, Brushes.Thistle, Brushes.Tomato, Brushes.Transparent, Brushes.Turquoise, Brushes.Violet, Brushes.Wheat, Brushes.White, Brushes.WhiteSmoke, Brushes.Yellow, Brushes.YellowGreen };


          path = new Path { Stroke = b[color], StrokeThickness = thickness };
          //myCanvas.Children.Add(path);

      }

      public Path getPath()
      {
          return path;
      }

      public MyThumb getStart()
      {
          return startNode;
      }

      public MyThumb getEnd()
      {
          return endNode;
      }

      public LineGeometry getLine()
      {
          return line;
      }

      public Rectangle getRect()
      {
          return rect;
      }

      public void setRect(Rectangle r)
      {
          rect = r;
      }

      
  }

}
