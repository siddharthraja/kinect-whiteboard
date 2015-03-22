using System;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.VisualGestureBuilder;


namespace KinectWhiteBoard
{
  public partial class Window1 : Window
  {
    private bool noThumbFoundLastClick=false;
    public bool mouseClickInProgress = false;
    public bool kinectManipulationInProgress = false;   //only for thumbs
    public bool rightHandOpen=true;
    public bool leftHandOpen = true;
    public int tempCount=0;
    //public MyThumb stthumb, endthumb;
    private MyThumb currentThumb, previousThumb;
    public bool currentSelected = false, previousSelected = false, enableNodePairOps = false, enableCurrentOps=false;

    private bool groupModeOn = false;
    private GroupMode groupMode;

    private bool multipleSelectionOn = false;
    public List<MyThumb> multipleThumbsList;
    public Dictionary<MyThumb, Rectangle> multipleThumbRectanglesMap;
    
    int newThumbCount = 0;
    bool menuToggle = false, menuVisible=false, picMenuVisible = false;
    public int filenum = 0;
    public bool snapShotEnabled = true;

    public string shiftText, groupText;

    public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
    public Rectangle rect1, rect2;
    GestureHandler gestureHandler;

    // Menu Items
    Rectangle menurect = new Rectangle();
    System.Windows.Controls.Button groupButton;
    System.Windows.Controls.Button addThumbButton;
    System.Windows.Controls.Button addLineButton;
    System.Windows.Controls.Button deleteLineButton;
    System.Windows.Controls.Button deleteThumbButton;
    //System.Windows.Controls.Button thickenLinesButton;
    System.Windows.Controls.Button saveStateButton;
    System.Windows.Controls.Button groupSelectionButton;
    System.Windows.Controls.Button duplicateThumb;

    public Canvas picCanvas = new Canvas() { Height = 10000, Width = 200, Background = new SolidColorBrush(Colors.DarkGray) };
    public ScrollViewer picScroller = new ScrollViewer() { Height = 6000, Width = 200 };  // set scroll bar on left - Right to Left? 

    private List<string> fileEntries;
    int imageNum = 0;
    int imgLocation = 10;
      // System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            

      
    //private MyThumb newThumb;
    private List<MyThumb> newThumbList { get; set;}
    private HashSet<string> lineIndex { get; set; }
    private Dictionary<string, Link> lineMap { get; set; }
    private List<System.Windows.Controls.Button> buttonList { get; set; }

   Image downArrow = new Image();
   Image rightArrow = new Image(); 



    // private List
    //private Dictionary<int, int> adjacencyMatrix { get; set; }

    public Window1()
    {
      InitializeComponent();
      newThumbList = new List<MyThumb>();
      lineIndex = new HashSet<string>();
      lineMap = new Dictionary<string, Link>();
      buttonList = new List<System.Windows.Controls.Button>(); 
      fileEntries = new List<string>();
      multipleThumbsList = new List<MyThumb>();
      multipleThumbRectanglesMap = new Dictionary<MyThumb, Rectangle>();
        
      groupMode = new GroupMode(this);
      //thumbGroupsList = new List<ThumbGroups>();
    }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        Console.Write("\nWindowLoaded ");
        preLoadThumbsNObjects();
        gestureHandler = new GestureHandler(this);
        processMenuButtons();
        getPics(@"../../Screenshots/"); 
        addFromPics();
        myCanvas.Children.Add(rightArrow);
        myCanvas.Children.Add(downArrow); 

        // operator overloading?
        PreviewMouseLeftButtonDown += mouseLeftClick;
        PreviewMouseLeftButtonUp += mouseReleaseHandler;

        //PreviewMouseLeftButtonDown += thumbSelection;
    }


    private void preLoadThumbsNObjects()
    {
        // Move all the predefined thumbs to the front to be over the lines
        System.Windows.Controls.Panel.SetZIndex(myThumb1, 2);
        System.Windows.Controls.Panel.SetZIndex(myThumb2, 2);
        System.Windows.Controls.Panel.SetZIndex(myThumb3, 2);
        System.Windows.Controls.Panel.SetZIndex(myThumb4, 2);
        System.Windows.Controls.Panel.SetZIndex(picScroller, 1);
        //System.Windows.Controls.Panel.SetZIndex(RecycleBin, 1);
        shiftText = "Shift Mode Is Off";
        groupText = "Group Mode If Off";
        //GroupModeText.Text = shiftText + " | " + groupText;
        #region add thumbs to the list
        preLoadedThumbProcessing();
        #endregion
    }

    private void preLoadedThumbProcessing()
    {
        newThumbList.Add(myThumb1);        
        newThumbList.Add(myThumb2);
        newThumbList.Add(myThumb3);
        newThumbList.Add(myThumb4);

        myThumb1.imgSource = @"../../Images/j.png";
        myThumb2.imgSource = @"../../Images/j.png";
        myThumb3.imgSource = @"../../Images/sydney.jpg";
        myThumb4.imgSource = @"../../Images/j.png";

        for (int i = 0; i < newThumbList.Count; i++)
        {
            UpdateLines(newThumbList[i]);
            UpdateRectangles(newThumbList[i]);
            newThumbList[i].index = i;
            newThumbList[i].setWindowRef(this);
            newThumbList[i].rect = addRectangle(newThumbList[i], 3);
            var txt = (TextBlock)newThumbList[i].Template.FindName("tplTextBlock", newThumbList[i]);
            txt.Text = "Pre-loaded Snippet " + newThumbList[i].index;
        
        }
    }

    private void processMenuButtons()
    {
        groupButton = new System.Windows.Controls.Button() { Name = "groupButton", Height = 75, Width = 125, Content = "Group Mode" };
        addThumbButton = new System.Windows.Controls.Button() { Name = "addThumbButton", Height = 75, Width = 125, Content = "" };        
        addLineButton = new System.Windows.Controls.Button() { Name = "addLineButton", Height = 75, Width = 125, Content = "" };
        deleteLineButton = new System.Windows.Controls.Button() { Name = "deleteLineButton", Height = 75, Width = 125, Content = "" };
        deleteThumbButton = new System.Windows.Controls.Button() { Name = "deleteThumbButton", Height = 75, Width = 125, Content = "" };
        saveStateButton = new System.Windows.Controls.Button() { Name = "saveStateButton", Height = 75, Width = 125, Content = "" };
        groupSelectionButton = new System.Windows.Controls.Button() { Name = "groupSelectionButton", Height = 75, Width = 125, Content = "Select Group" };
        duplicateThumb = new System.Windows.Controls.Button() { Name = "duplicateThumb", Height = 75, Width = 125, Content = "" };
        //groupButton.Background = new ImageBrush(new BitmapImage(new Uri(@"/Images/add_thumb.png", UriKind.RelativeOrAbsolute)));
        duplicateThumb.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/copy.png", UriKind.RelativeOrAbsolute)));
        saveStateButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/camera-512.png", UriKind.RelativeOrAbsolute)));
        addThumbButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/add_thumb.png", UriKind.RelativeOrAbsolute)));
        addLineButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/add_line.png", UriKind.RelativeOrAbsolute)));
        deleteLineButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/deleteLine.png", UriKind.RelativeOrAbsolute)));
        deleteThumbButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/trash.png", UriKind.RelativeOrAbsolute)));
        groupButton.Background = new SolidColorBrush(System.Windows.Media.Colors.White);
        groupSelectionButton.Background = new SolidColorBrush(System.Windows.Media.Colors.White); 

        //thickenLinesButton.Background = new ImageBrush(new BitmapImage(new Uri(@"../../downloads/thick_line.png", UriKind.RelativeOrAbsolute)));
        //saveStateButton.Background = new ImageBrush(new BitmapImage(new Uri(@"/Images/add_thumb.png", UriKind.RelativeOrAbsolute)));

        groupButton.Click += switchMode;
        saveStateButton.Click += saveSnapShotClick;
        addThumbButton.Click += BtnNewActionClick;
        addLineButton.Click += drawLines;
        deleteLineButton.Click += deleteLinkByButton;
        deleteThumbButton.Click += deleteThumbByMouse;
        duplicateThumb.Click += duplicateThumbClick;
        groupSelectionButton.Click += groupSelection;

        rightArrow.Source = new BitmapImage((new Uri(@"../../downloads/transparent-arrow-left.png", UriKind.RelativeOrAbsolute)));
        Canvas.SetRight(rightArrow, 0);
        Canvas.SetBottom(rightArrow,50);

        downArrow.Source = new BitmapImage((new Uri(@"../../downloads/transparent-arrow-down.png", UriKind.RelativeOrAbsolute)));
        Canvas.SetLeft(downArrow, 0);


    }
      
    //*********************************************************** INITIAL LOADING DONE ******************************************************************************


    public void actionController(int actionType)
    {
        /* -1: switch 
         * 0: group drag only
         * 1: creategroup
         * 2: addthumb
         * 3: removethumb
         * 4: drawline
         * 5: deleteline
         * 6: showmenu
         * 7: deletethumb
         * */
        if (enableNodePairOps)
        {
            switch (actionType)
            {
                case -1: switchMode(); break;
                case 0: break;
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: groupMode.drawLine(currentThumb, previousThumb); break;
                case 5: groupMode.removeLink(currentThumb, previousThumb); break;
            }
        }
        

    }

    //*********************************************************** gestureController DONE *****************************************************************************

    private void switchMode()
    {
        groupModeOn=(!groupModeOn);
        
        if (groupModeOn == false) 
        {
            groupButton.Background = new SolidColorBrush(System.Windows.Media.Colors.White);
            groupText = "Group Mode is Off";
            GroupModeText.Text = shiftText + " | " + groupText;
            if (wholeGroupSelected)
            {
                groupMode.unSelectWholeGroup();
                wholeGroupSelected = false;
            }
        }
        else
        {
            groupButton.Background = new SolidColorBrush(System.Windows.Media.Colors.Green);
            groupText = "Group Mode is On";
            GroupModeText.Text = shiftText + " | " + groupText; 
        }

        // reset selections
    }

    private void switchMode(object sender, RoutedEventArgs e)
    {
        //Console.Write("\nswitchMode => " + "sender: " + sender.ToString() +" | RoutedEventArgs: "+e.ToString());
        switchMode();

    }
    // Event hanlder for dragging functionality support
    private void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (wholeGroupSelected)
        {
            
            foreach (MyThumb thumb in groupMode.currentSelectedGroup.getListOfThumbs())
            {
                var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
                var top = Canvas.GetTop(thumb) + e.VerticalChange;
                ActionText.Foreground = new SolidColorBrush(Colors.Black);
                ActionText.Text = "Dragging " + thumb.Name;

                if (Canvas.GetLeft(thumb) > 0)
                {
                    Canvas.SetLeft(thumb, left);
                    Canvas.SetTop(thumb, top);
                }
                else
                {
                    Canvas.SetLeft(thumb, 1);
                    Canvas.SetTop(thumb, top);
                }

                // Update lines's layouts
                UpdateLines(thumb);
                myCanvas.Children.Remove(rect1);
                rect1 = addRectangle(currentThumb, 1);
                myCanvas.Children.Add(rect1);
                if (previousSelected)
                {
                    myCanvas.Children.Remove(rect2);
                    rect2 = addRectangle(previousThumb, 2);
                    myCanvas.Children.Add(rect2);
                }
                myCanvas.Children.Remove(thumb.rect);
                thumb.rect = addRectangle(thumb, 3);
                myCanvas.Children.Add(thumb.rect);
            }
        }
        else
        {
            var thumb = e.Source as MyThumb;
            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;
            ActionText.Foreground = new SolidColorBrush(Colors.Black);
            ActionText.Text = "Dragging " + thumb.Name;

            if (Canvas.GetLeft(thumb) > 0)
            {
                Canvas.SetLeft(thumb, left);
                Canvas.SetTop(thumb, top);
            }
            else
            {
                Canvas.SetLeft(thumb, 1);
                Canvas.SetTop(thumb, top);
            }
            //Canvas.SetLeft(thumb, left);
            //Canvas.SetTop(thumb, top);

            //if (Canvas.GetLeft(thumb) < 0)
            //{
            //    Canvas.SetLeft(thumb, 1);
            //    Canvas.SetTop(thumb, top);
            //}

            //if (Canvas.GetTop(thumb) < 0)
            //{
            //    Canvas.SetLeft(thumb, left);
            //    Canvas.SetTop(thumb, 1);
            //}

            //if (Canvas.GetTop(thumb) > VisualTreeHelper.GetOffset(RecycleBin).Y + 50)
            //{
            //    Canvas.SetLeft(thumb, left);
            //    Canvas.SetTop(thumb, VisualTreeHelper.GetOffset(RecycleBin).Y + 50);
            //}

            


            // Update lines's layouts
            UpdateAllForAThumb(thumb); 
        }      
    
    }




    public void dragWholeGroupUpdate(MyThumb thumb)
    {
        UpdateLines(thumb);
        myCanvas.Children.Remove(rect1);
        rect1 = addRectangle(currentThumb, 1);
        myCanvas.Children.Add(rect1);
        if (previousSelected)
        {
            myCanvas.Children.Remove(rect2);
            rect2 = addRectangle(previousThumb, 2);
            myCanvas.Children.Add(rect2);
        }
        myCanvas.Children.Remove(thumb.rect);
        thumb.rect = addRectangle(thumb, 3);
        myCanvas.Children.Add(thumb.rect);

    }

    private void mouseReleaseHandler(object sender, MouseButtonEventArgs e)
    {
        //Console.Write("mouse relesaed");
        if (enableCurrentOps)
        {
            if (checkThumbOverCan())
            {
                deleteThumb(currentThumb, true);
            }else
                if (checkThumbOverRightMenu())
                {
                    addBackToPicMenu(currentThumb);
                    deleteThumb(currentThumb, false);                
                }
        }
        mouseClickInProgress = false;
    }

    private void UpdateRectangles(MyThumb thumb)
    {
       var l = Canvas.GetLeft(thumb);
       var t = Canvas.GetTop(thumb);
       if (thumb == currentThumb)
       {
           //Console.Write("\nUpdateRectangles currentThumb");
           myCanvas.Children.Remove(rect1);
           rect1 = addRectangle(currentThumb, 1);
           myCanvas.Children.Add(rect1);
       }else
           if (thumb == previousThumb)
           {
               //Console.Write("\nUpdateRectangles previousThumb");
               myCanvas.Children.Remove(rect2);
               rect2 = addRectangle(previousThumb, 2);
               myCanvas.Children.Add(rect2);
           }
           //else
           //{
           //    if(multipleSelectionOn){
           //        Rectangle r = multipleThumbRectanglesMap[thumb];
           //        myCanvas.Children.Remove(r);
           //        r = addRectangle(previousThumb, 3);
           //        multipleThumbRectanglesMap.Add(thumb, r);
           //        myCanvas.Children.Add(r);
           //    }
           //}
          
    }

    public void showMenu()
    {

        myCanvas.Children.Remove(downArrow);
        myCanvas.Children.Remove(menurect);
        myCanvas.Children.Remove(groupButton); myCanvas.Children.Remove(addThumbButton); myCanvas.Children.Remove(addLineButton); myCanvas.Children.Remove(deleteLineButton);
        myCanvas.Children.Remove(deleteThumbButton); myCanvas.Children.Remove(saveStateButton);
        myCanvas.Children.Remove(duplicateThumb); myCanvas.Children.Remove(groupSelectionButton);
        // menuToggle=(!menuToggle);
        ActionText.Foreground = new SolidColorBrush(Colors.Black);
        ActionText.Text = "Showing Top Menu"; 
        // new buttons
        Canvas.SetLeft(addThumbButton, 10);
        Canvas.SetLeft(deleteThumbButton, 150);
        Canvas.SetLeft(addLineButton, 300);

        Canvas.SetLeft(deleteLineButton, 450);
        Canvas.SetLeft(saveStateButton, 600);

        Canvas.SetLeft(duplicateThumb, 750);
        Canvas.SetLeft(groupButton, 900);
        Canvas.SetLeft(groupSelectionButton, 1050);

        //if (menuToggle)
        //{
        menurect.Height = 80;
        menurect.Width = 10000;
        menurect.Fill = new SolidColorBrush(System.Windows.Media.Colors.AliceBlue); // aliceblue is good
        myCanvas.Children.Add(menurect);

        myCanvas.Children.Add(groupButton); myCanvas.Children.Add(addThumbButton); myCanvas.Children.Add(addLineButton); myCanvas.Children.Add(deleteLineButton);
        myCanvas.Children.Add(deleteThumbButton); myCanvas.Children.Add(saveStateButton);

        myCanvas.Children.Add(duplicateThumb); myCanvas.Children.Add(groupSelectionButton);


        //}

        //else
        //{
        //    myCanvas.Children.Remove(menurect);
        //    myCanvas.Children.Remove(groupButton); myCanvas.Children.Remove(addThumbButton); myCanvas.Children.Remove(addLineButton); myCanvas.Children.Remove(deleteLineButton);
        //    myCanvas.Children.Remove(deleteThumbButton); myCanvas.Children.Remove(thickenLinesButton); myCanvas.Children.Remove(saveStateButton);
        //}
        myCanvas.UpdateLayout();
    }

    
    private void showMenuClick(object sender, RoutedEventArgs e)
    {
        //Console.Write("\nshowMenuClick => " + "sender: " + sender.ToString() + " | RoutedEventArgs: " + e.ToString());        
        showMenu();
    }

    public void hideMenu()
    {
        
        myCanvas.Children.Remove(menurect);
        myCanvas.Children.Remove(groupButton); myCanvas.Children.Remove(addThumbButton); myCanvas.Children.Remove(addLineButton); myCanvas.Children.Remove(deleteLineButton);
        myCanvas.Children.Remove(deleteThumbButton); myCanvas.Children.Remove(saveStateButton);
        myCanvas.Children.Remove(duplicateThumb); myCanvas.Children.Remove(groupSelectionButton);
        ActionText.Foreground = new SolidColorBrush(Colors.Black);
        myCanvas.Children.Remove(downArrow);
        myCanvas.Children.Add(downArrow);
        ActionText.Text = "Hiding Top Menu"; 
    }

    public void showPicMenu()
    {
        // TESTING THE SCROLLVIEWER  
        myCanvas.Children.Remove(rightArrow); 
        picMenuVisible = true; 
        picScroller.Content = picCanvas;
        myCanvas.Children.Remove(picScroller);
        Canvas.SetRight(picScroller, 0);
        myCanvas.Children.Add(picScroller);
        ActionText.Foreground = new SolidColorBrush(Colors.Black);
        ActionText.Text = "Showing Image Bank"; 
    }

    public void hidePicMenu()
    {
        picMenuVisible = false; 
        myCanvas.Children.Remove(picScroller);
        myCanvas.Children.Remove(rightArrow); 
        myCanvas.Children.Add(rightArrow); 
        ActionText.Foreground = new SolidColorBrush(Colors.Black);
        ActionText.Text = "Hiding Image Bank";
    }


    private void getPics(String path)
    {

        string[] fEntries = Directory.GetFiles(path);
        imageNum = 0;
        imgLocation = 10;
        foreach (string fileName in fEntries)
        {
            imageNum++;
            fileEntries.Add(fileName);
            System.Windows.Controls.Button picButton; 
            picButton = new System.Windows.Controls.Button() {Name = "testImage" + imageNum, Height = 75, Width = 125, Content = "testImage" + imageNum };
            picButton.Background = new ImageBrush(new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute)));
            picButton.Tag = fileName;
            buttonList.Add(picButton); 
            Canvas.SetLeft(picButton, 20);
            Canvas.SetTop(picButton, imgLocation);
            imgLocation = imgLocation + 90;
            picCanvas.Children.Add(picButton);
        }
    }


    private void addFromPics()
    {
        foreach (System.Windows.Controls.Button button in buttonList) {
            button.Click += newThumbNoDialog;
        }
    }

    // Add New Thumb without dialog
    private void newThumbNoDialog(object sender, RoutedEventArgs e)
    {
        string toRemove = null;
        string picButtonName = (String)((System.Windows.Controls.Button)sender).Tag;
        var newThumb = addNewThumb(picButtonName);
        //picCanvas.Children.Remove(((System.Windows.Controls.Button)sender));
        picCanvas.Children.RemoveRange(0, fileEntries.Count);
        foreach (string name in fileEntries)
        {  

            if (name == picButtonName)
            {
                //Console.Write("\nRemoving = " + name);
                ActionText.Foreground = new SolidColorBrush(Colors.Black);
                ActionText.Text = "Removing = " + name;
                toRemove = name;
            }            
        }
        fileEntries.Remove(toRemove);
        reloadPicMenu();
        Console.Write(picButtonName);
        picCanvas.UpdateLayout(); 
        e.Handled = true;
        ActionText.Foreground = new SolidColorBrush(Colors.Black);
        ActionText.Text = "Creating " + (String)((System.Windows.Controls.Button)sender).Tag;

    }

    private void reloadPicMenu()
    {
        imageNum = 0;
        imgLocation = 10;
        buttonList.Clear();
        foreach (string fileName in fileEntries)
        {
            imageNum++;
            //fileEntries.Add(fileName);
            System.Windows.Controls.Button picButton;
            picButton = new System.Windows.Controls.Button() { Name = "testImage" + imageNum, Height = 75, Width = 125, Content = "testImage" + imageNum };
            picButton.Background = new ImageBrush(new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute)));
            picButton.Tag = fileName;
            buttonList.Add(picButton);
            
            Canvas.SetLeft(picButton, 20);
            Canvas.SetTop(picButton, imgLocation);
            imgLocation = imgLocation + 90;
            picCanvas.Children.Add(picButton);
        }
        addFromPics();
    }

    public void addBackToPicMenu(MyThumb thumb)
    {
        string fileName=thumb.imgSource;
        
        imageNum++;
        fileEntries.Add(fileName);
        System.Windows.Controls.Button picButton;
        picButton = new System.Windows.Controls.Button() { Name = "testImage" + imageNum, Height = 75, Width = 125, Content = "testImage" + imageNum };
        picButton.Background = new ImageBrush(new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute)));
        picButton.Tag = fileName;
        buttonList.Add(picButton);
        addFromPics();
        Canvas.SetLeft(picButton, 20);
        Canvas.SetTop(picButton, imgLocation);
        imgLocation = imgLocation + 90;
        picCanvas.Children.Add(picButton);
    }


    private void showRightClick(object sender, RoutedEventArgs e)
    {
        showPicMenu();
        //flipCanvas();
    }

    private void closeRightClick(object sender, RoutedEventArgs e)
    {
        hidePicMenu();
        //flipBackCanvas();
    }


    private static void UpdateLines(MyThumb thumb)
    {
      // Console.Write("\nUpdateLines "); 
      var left = Canvas.GetLeft(thumb);
      var top = Canvas.GetTop(thumb);    

      foreach (var line in thumb.StartLines)
        line.StartPoint = new Point(left + thumb.ActualWidth / 2, top + thumb.ActualHeight / 2);
      foreach (var line in thumb.EndLines)
        line.EndPoint = new Point(left + thumb.ActualWidth / 2, top + thumb.ActualHeight / 2);
    }

    public void UpdateAllForAThumb(MyThumb thumb)
    {
        UpdateLines(thumb);
        UpdateRectangles(thumb);
    }


    private MyThumb addNewThumb(String uri)
    {
        
        //Console.Write("addnewthumbs");
        var newThumb = new MyThumb(this);
        newThumb.index = newThumbList.Count + 1;
        newThumb.Name = "snippet" + newThumb.index;
        
        // Assign our custom template to it
        newThumb.Template = Resources["template1"] as ControlTemplate;
        // Calling ApplyTemplate enables us to navigate the visual tree right now (important!)
        newThumb.ApplyTemplate();
        // Add the "onDragDelta" event handler that is common to all objects
        newThumb.DragDelta += OnDragDelta;
        
        myCanvas.Children.Add(newThumb);
        newThumbList.Add(newThumb);
        //Console.Write("\nThe next element: " + myCanvas.Children.IndexOf(newThumb));
        //Console.Write("\nThe next element: " + newThumbList.IndexOf(newThumb));
        // Access the image element of our custom template and assign it to the new image
        var img = (Image)newThumb.Template.FindName("tplImage", newThumb);
        img.Source = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
        newThumb.imgSource = uri;
        newThumb.rect = addRectangle(newThumb, 3);
        // img.Source = new BitmapImage(new Uri("Images/gear_connection.png", UriKind.Relative));

        // Access the textblock element of template and change it too
        var txt = (TextBlock)newThumb.Template.FindName("tplTextBlock", newThumb);
        txt.Text = "Snippet "+newThumb.index;

        // Set the position of the object according to the mouse pointer                
        //var position = e.GetPosition(this);
        Point position = new Point(200 + (newThumbCount%20 * 75), 500 + (newThumbCount/20 * 75));
        newThumbCount += 1;
        Canvas.SetLeft(newThumb, position.X);
        Canvas.SetTop(newThumb, position.Y);

        //Console.Write("\nThe New Thumb's position: " + Canvas.GetLeft(newThumb));
        // Move our thumb to the front to be over the lines
        System.Windows.Controls.Panel.SetZIndex(newThumb, 1);
        // Manually update the layout of the thumb (important!)
        newThumb.UpdateLayout();
        return newThumb;
    }

    public void deleteThumb(MyThumb thumb, bool toDelete)
    {
        if (wholeGroupSelected)
        {
            groupMode.unSelectWholeGroup();
            wholeGroupSelected = false;
        }
        myCanvas.Children.Remove(thumb);
        newThumbList.Remove(thumb);
        string s = ":" + thumb.index + ":";
        groupMode.handleThumbDeletion(thumb);
        deleteAllLinksOfAThumb(s);
        
        // TESTTEST TEST
        if (toDelete)
        {
            changeCanvasDeleteTimed();
            ActionText.Foreground = new SolidColorBrush(Colors.Black);
            ActionText.Text = "Deleting " + currentThumb.Name;
        }
        else
        {
            ActionText.Foreground = new SolidColorBrush(Colors.Black);
            ActionText.Text = "Putting " + currentThumb.Name + " into Image Bank";
        }
        
        handleNoThumbSelected();
        enableNodePairOps = false;
        enableCurrentOps = false;
        
    }


    private void changeCanvasDeleteTimed()
    {
        Uri[] uriList = new Uri[5];
        Image[] imageList = new Image[5];
        uriList[0] = new Uri(@"../../downloads/1.png", UriKind.RelativeOrAbsolute);
        uriList[1] = new Uri(@"../../downloads/2.png", UriKind.RelativeOrAbsolute);
        uriList[2] = new Uri(@"../../downloads/3.png", UriKind.RelativeOrAbsolute);
        uriList[3] = new Uri(@"../../downloads/4.png", UriKind.RelativeOrAbsolute);
        uriList[4] = new Uri(@"../../downloads/5.png", UriKind.RelativeOrAbsolute);
        for (int i = 0; i <= 4; i++)
        {
            imageList[i] = new Image();
            imageList[i].Width = 190;
            imageList[i].Height = 190;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = uriList [i];

            myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();
            //set image source
            imageList[i].Source = myBitmapImage;
            Canvas.SetBottom(imageList[i], 195);
           
            //myCanvas.Background = new SolidColorBrush(Colors.Yellow);
        }
        myCanvas.Children.Add(imageList[0]);
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        dispatcherTimer.Start();
        dispatcherTimer.Tick += (sender, args) =>
        {
            dispatcherTimer.Stop();
            myCanvas.Children.Remove(imageList[0]);
        };
        //myCanvas.Children.Add(imageList[1]);
        //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        //dispatcherTimer.Start();
        //dispatcherTimer.Tick += (sender, args) =>
        //{
        //    dispatcherTimer.Stop();
        //    myCanvas.Children.Remove(imageList[1]);
        //};
        //myCanvas.Children.Add(imageList[2]);
        //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        //dispatcherTimer.Start();
        //dispatcherTimer.Tick += (sender, args) =>
        //{
        //    dispatcherTimer.Stop();
        //    myCanvas.Children.Remove(imageList[2]);
        //};
        //myCanvas.Children.Add(imageList[3]);
        //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        //dispatcherTimer.Start();
        //dispatcherTimer.Tick += (sender, args) =>
        //{
        //    dispatcherTimer.Stop();
        //    myCanvas.Children.Remove(imageList[3]);
        //};
        //myCanvas.Children.Add(imageList[4]);
        //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        //dispatcherTimer.Start();
        //dispatcherTimer.Tick += (sender, args) =>
        //{
        //    dispatcherTimer.Stop();
        //    myCanvas.Children.Remove(imageList[4]);
        //};
        // myCanvas.Background = new SolidColorBrush(Colors.Yellow);
        // Console.Write("hellsdlkfa;klsdjflkasjdf;kla");
    }


    private void changeCanvasSnapShotTimed()
    {
        myCanvas.Background = new SolidColorBrush(Colors.Gold);
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        dispatcherTimer.Start();
        dispatcherTimer.Tick += (sender, args) =>
        {
            dispatcherTimer.Stop();
            myCanvas.Background = new SolidColorBrush(Colors.White);
        };
        // myCanvas.Background = new SolidColorBrush(Colors.Yellow);
        // Console.Write("hellsdlkfa;klsdjflkasjdf;kla");
    }


    public void flipCanvas()
    {
        Console.Write("\n flip canvas $####################");
        RotateTransform rotateTransform2 = new RotateTransform(180);
        rotateTransform2.CenterX = 955;
        rotateTransform2.CenterY = 505;
        myCanvas.RenderTransform = rotateTransform2;

    }

    public void flipBackCanvas()
    {

        RotateTransform rotateTransform2 = new RotateTransform(0);
        rotateTransform2.CenterX = 955;
        rotateTransform2.CenterY = 505;
        myCanvas.RenderTransform = rotateTransform2;

    }

    public void deleteThumbByMouse(object sender, RoutedEventArgs e)
    {
        if (enableCurrentOps)
        {
            deleteThumb(currentThumb, true);
        }
    }

    public void createLink(MyThumb start, MyThumb end )
    {
      //System.Diagnostics.Debug.Print("\n!@#createLink"+start.index+" | "+start.ToString());
        //int index = start.index*10 + end.index;
        string index = ":" + start.index + ":" + end.index + ":";
        if (lineIndex.Add(index))
        {
            Link tempLink = new Link(start, end, 0, 3);
            lineMap.Add(index, tempLink);
            myCanvas.Children.Add(tempLink.getPath());

            UpdateLines(start);
            UpdateLines(end);
        }     
    
    }
    public void createLink(MyThumb start, MyThumb end, int color, int thickness)
    {
        //System.Diagnostics.Debug.Print("\n!@#createLink"+start.index+" | "+start.ToString());
        //int index = start.index*10 + end.index;
        string index = ":" + start.index + ":" + end.index + ":";
        if (lineIndex.Add(index))
        {
            Link tempLink = new Link(start, end, color, thickness);
            lineMap.Add(index, tempLink);
            myCanvas.Children.Add(tempLink.getPath());

            UpdateLines(start);
            UpdateLines(end);
        }

    }

    public void deleteLink(MyThumb start, MyThumb end)
    {
        bool contains = false;
        string index = "";
        string temp = ":" + start.index + ":" + end.index + ":";
        string tempReverse = ":" + end.index + ":" + start.index + ":";

        if (lineIndex.Contains(temp))
        {
            contains = true;
            index = temp;
        }else
            if (lineIndex.Contains(tempReverse))
            {
                contains = true;
                index = tempReverse;
            }

        if (contains)
        {
            Console.Write("\ndeleteLink " + index);
            Link tempLink = lineMap[index];
            myCanvas.Children.Remove(tempLink.getPath());
            start.StartLines.Remove(tempLink.getLine());
            end.EndLines.Remove(tempLink.getLine());

            lineMap.Remove(index);
            lineIndex.Remove(index);
            UpdateLines(start);
            UpdateLines(end);
        }
        
          
    }

    public void saveSnapShot()
    {
        // render InkCanvas' visual tree to the RenderTargetBitmap
        // ActionText.Text = "Saving Current State as FileName" + filenum;
        ActionText.Foreground = new SolidColorBrush(Colors.Red);
        ActionText.Text = "Screenshot taken!";
        RenderTargetBitmap targetBitmap =
            new RenderTargetBitmap((int)myCanvas.ActualWidth,
                                   (int)myCanvas.ActualHeight,
                                   96d, 96d,
                                   PixelFormats.Default);
        targetBitmap.Render(myCanvas);

        // add the RenderTargetBitmap to a Bitmapencoder
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(targetBitmap));

        // save file to disk

        //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 800);
        //dispatcherTimer.Start();
        //dispatcherTimer.Tick += (sender, args) =>
        //{
        //    dispatcherTimer.Stop();            
        //};

        // check 
        if (snapShotEnabled)
        {
            snapShotEnabled = false;
            FileStream fs = File.Open("FileName" + filenum + ".png", FileMode.OpenOrCreate);
            encoder.Save(fs);
            filenum++;
        }
        changeCanvasSnapShotTimed();

        

    }

    private void saveSnapShotClick(object sender, RoutedEventArgs e)
    {
        saveSnapShot();  
    }

    private void deleteAllLinksOfAThumb(string s)
    {
        List<string> keys = new List<string>(this.lineMap.Keys); ;
        foreach (string key in keys)
        //for (int i = 0; i < lineIndex.Count; i++)
        {
            if (key.Contains(s))
            {
                Console.Write("\nDELETE LINK: " + key + " | " + s);
                Link tempLink = lineMap[key];
                myCanvas.Children.Remove(tempLink.getPath());
                //start.StartLines.Remove(tempLink.getLine());
                //end.EndLines.Remove(tempLink.getLine());
                lineMap.Remove(key);
                lineIndex.Remove(key);
                //UpdateLines(start);
                //UpdateLines(end);
            }
        }
    }

    public List<MyThumb> getGroupNeighboursOnThumbDeletion(MyThumb thumb)
    {
        if (thumb.groupNumber != -1)
        {
            MyThumb t;
            List<MyThumb> neighbours = new List<MyThumb>();
            string s = ":" + thumb.index + ":";
            List<string> keys = new List<string>(this.lineMap.Keys); ;
            foreach (string key in keys)
            //for (int i = 0; i < lineIndex.Count; i++)
            {
                if (key.Contains(s))
                {
                    Link tempLink = lineMap[key];
                    if (tempLink.getStart().groupNumber == tempLink.getEnd().groupNumber)
                    {
                        t = (tempLink.getStart() != thumb) ? tempLink.getStart() : tempLink.getEnd();
                        neighbours.Add(t);
                    }
                }
            }
            return neighbours;
        }
        else
        {
            return null;
        }
    }

    //******************************************************************************* SELECTION *********************************************************************************
    //***************************************************************************************************************************************************************************
    private void mouseLeftClick(object sender, MouseButtonEventArgs e)
    {
        if (noThumbFoundLastClick)
        {
            enableNodePairOps = false;
            enableCurrentOps = false;
        }
        
        MyThumb temp = getSelectedThumb(e.GetPosition(this));
        if (temp!=null)
        {
            processSelectedThumb(temp);
        }
    }

    public void thumbSelectionKinect(MyThumb thumb)
    {
        if (thumb != null)
        {
            processSelectedThumb(thumb);
            snapShotEnabled = true;
        }

    }

    private MyThumb getSelectedThumb(Point mouseClick)
    {
            //Console.Write("getSelectedThumb ");
            Boolean foundThumb = false;               
            for (int i = 0; i < newThumbList.Count && !foundThumb; i++)
            {
                if ((Canvas.GetLeft(newThumbList[i]) <= mouseClick.X) && (Canvas.GetLeft(newThumbList[i]) + 150 >= mouseClick.X)
                    && (Canvas.GetTop(newThumbList[i]) <= mouseClick.Y) && (Canvas.GetTop(newThumbList[i]) + 150 >= mouseClick.Y))
                {
                    foundThumb = true;
                    noThumbFoundLastClick = false;
                    return newThumbList[i];
                }

            }

            if (!foundThumb)
            {
                noThumbFoundLastClick = true;        
                handleNoThumbSelected();
            }

            return null;               
            
    }

    
    public void processSelectedThumb(MyThumb thumb)
    {
        if (!currentSelected)
        {
            currentThumb = thumb;
            currentSelected = true;
            enableCurrentOps = true;
            UpdateRectangles(currentThumb);
        }
        else
            if (currentThumb != thumb)
            {
                previousThumb = currentThumb;
                currentThumb = thumb;
                previousSelected = true;
                UpdateRectangles(currentThumb);
                UpdateRectangles(previousThumb);
                enableNodePairOps = true;
                // 
                ActionText.Foreground = new SolidColorBrush(Colors.Red);
                ActionText.Text = "Clap hands to draw link between nodes";
                // ActionText.Foreground = new SolidColorBrush(Colors.Black);
            }

        if (multipleSelectionOn)
        {
            multipleThumbsList.Add(thumb);

        }
        if (groupModeOn && wholeGroupSelected)
        {
            if (thumb.groupNumber != groupMode.currentGroupSelectionThumb.groupNumber)
            {
                groupMode.unSelectWholeGroup();
                wholeGroupSelected = false;
            }
        }
        
    }

    public void handleNoThumbSelected()
    {
        //if (tcount == 2 && !noThumbFoundLastClick)
        //{
        //    Console.Write("making isNewLine true");
        //    _isNewLine = true;
        //    _isDelPossible = true;
        //}
        //tcount = 0;
        previousSelected = false;
        currentSelected = false;
        myCanvas.Children.Remove(rect1);
        myCanvas.Children.Remove(rect2);
        //btnNewAction.IsEnabled = true;
        Mouse.OverrideCursor = null;
        unselectedAllThumbs();
    }

    public void unselectedAllThumbs()
    {
        multipleThumbsList.Clear();
    }
    
    // Event handler for enabling new thumb creation by left mouse button click
    private void BtnNewActionClick(object sender, RoutedEventArgs e)
    {
        
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.FileName = "Document"; // Default file name
        dlg.DefaultExt = ".png"; // Default file extension
        // dlg.Filter = "Text documents (.png)|*.png"; // Filter files by extension 

        // Show open file dialog box
        Nullable<bool> result = dlg.ShowDialog();

        // Process open file dialog box results 
        if (result == true)
        {
            // Open document 
            string filename = dlg.FileName;
            // string filename = @"C:\Users\Sonify\Desktop\_0WorkingCopies\ShapeConnectors\ShapeConnectors\ShapeConnectors\Images\beach.jpg"; 
            Console.Write("\nThis is the filename: " + filename);
            
            // Create new thumb object
            var newThumb = addNewThumb(filename);
            ActionText.Foreground = new SolidColorBrush(Colors.Black);
            ActionText.Text = "Creating new node"; 

        }

        Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeAll;
        Mouse.OverrideCursor = null;
      
        e.Handled = true;
    }

    // a method that determines if two nodes are selected
    private void drawLines(object sender, RoutedEventArgs e)
    {
        drawLines();
    }

    public void drawLines()
    {
        if (enableNodePairOps)
        {
            if (groupModeOn)
            {
                actionController(4);
            }else
                createLink(previousThumb, currentThumb);
                ActionText.Foreground = new SolidColorBrush(Colors.Black);
                ActionText.Text = "Creating Link between " + previousThumb.Name + " and " + currentThumb.Name;

        }

    }

    public void deleteLinkByGesture()
    {
        //Console.Write("\ndeleteLinkByGesture");
        if (enableNodePairOps)
        {
            //if (groupModeOn)
            //{
            //    actionController(5);
            //}
            //else
            //{
            //    deleteLink(previousThumb, currentThumb);
            actionController(5);
            ActionText.Foreground = new SolidColorBrush(Colors.Black);
            ActionText.Text = "Deleting Link between " + previousThumb.Name + " and " + currentThumb.Name;
            //}
        }
    }


    private void deleteLinkByButton(object sender, RoutedEventArgs e)
    {
        //Console.Write("\ndeleteLinkByButton => " + "sender: " + sender.ToString() + " | RoutedEventArgs: " + e.ToString());
        deleteLinkByGesture();
    }
          
    public bool checkThumbOverCan()
    {
        //Console.Write("checkThumbOverCan");
        if ((Canvas.GetLeft(currentThumb) <= 150) && ((Canvas.GetTop(currentThumb) >= VisualTreeHelper.GetOffset(RecycleBin).Y - 25) && (Canvas.GetTop(currentThumb) <= VisualTreeHelper.GetOffset(RecycleBin).Y + 90)))
        {
            // Console.Write("deletion should occur");
            return true;
        }
        return false;
    }

    public bool checkThumbOverRightMenu()
    {
        // Console.Write(VisualTreeHelper.GetOffset(currentThumb)); 
        // if ((Canvas.GetRight(currentThumb) <= 200) && picMenuVisible)
        if ((Canvas.GetLeft(currentThumb) >= VisualTreeHelper.GetOffset(picScroller).X - 25) && picMenuVisible)
        {
            // Console.Write("Over the right menu");
            return true;
        }
        return false;
    }


    private void onClick(object sender, MouseButtonEventArgs e)
    {

    }

    private void Rectangle_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {

    }

    public Rectangle addRectangle(MyThumb thumb, int thumbType)
    {
        Rectangle temp = new System.Windows.Shapes.Rectangle();
        temp.Width = 150;
        temp.Height = 150;

        Canvas.SetLeft(temp, Canvas.GetLeft(thumb));
        Canvas.SetTop(temp, Canvas.GetTop(thumb));
        temp.StrokeThickness = 2;
        switch (thumbType)
        {
            case 1: temp.Stroke = new SolidColorBrush(Colors.Blue); break;
            case 2: temp.Stroke = new SolidColorBrush(Colors.LightBlue); break;
            case 3: temp.Stroke = new SolidColorBrush(Colors.Red); temp.StrokeThickness = 4; break;
            default: temp.Stroke = new SolidColorBrush(Colors.MediumVioletRed); break;
        }
        return temp;
    }

    public bool isGroupModeOn()
    {
        return groupModeOn;
    }

    public bool wholeGroupSelected = false;

    public void groupSelection(object sender, RoutedEventArgs e)
    {
        groupSelection();        
    }

    public void groupSelection()
    {
        Console.Write("\ngroupSelection: " + wholeGroupSelected);
        if (!wholeGroupSelected)
        {
            if (groupModeOn && currentThumb != null)
            {
                Console.Write("\ngroupSelection: groupModeOn && enableCurrentOps");
                wholeGroupSelected = groupMode.selectWholeGroup(currentThumb);
            }
        }
        else
        {
            if (wholeGroupSelected)
            {
                groupMode.unSelectWholeGroup();
                wholeGroupSelected = false;
            }
        }
    }

    public void unselectGroup()
    {

    }

    public void duplicateThumbClick(object sender, RoutedEventArgs e)
    {
        Console.Write("\nduplicateThumbClick => " + "sender: " + sender.ToString() + " | RoutedEventArgs: " + e.ToString()); 
        if(enableCurrentOps){
            addNewThumb(currentThumb.imgSource); 
        }
        
    }

    public GroupMode getGroupModeObject()
    {
        return groupMode;
    }
  


  }

    
}