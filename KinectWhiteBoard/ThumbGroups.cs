using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWhiteBoard
{
    public class ThumbGroups
    {
        private List<MyThumb> listOfThumbs;
        private int color, thickness;
        Window1 windowRef;
        private int groupNumber;
        //public bool activated;

        public ThumbGroups(int c, int t, int number)
        {
            //windowRef = w;
            color = c;
            thickness = t;
            groupNumber = number;
            listOfThumbs = new List<MyThumb>();
            //activated = false;
        }

        public List<MyThumb> getListOfThumbs()
        {
            return listOfThumbs;
        }

        public void addThumbToGroup(MyThumb thumb)
        {
            listOfThumbs.Add(thumb);
        }

        public void changeColor(int c)
        {
            color = c;
        }

        public void changeThickness(int t)
        {
            thickness = t;
        }

        public int getColor()
        {
            return color;
        }

        public int getThickness()
        {
            return thickness;
        }

        public void dragTheWholeGroup()
        {
            windowRef.myCanvas.UpdateLayout();
        }

        public int getGroupNumber()
        {
            return groupNumber;
        }

        public void setGroupNumber(int n)
        {
            groupNumber = n;
        }

    }

    public class ThumbGroupNumber
    {
        private int value;

        public ThumbGroupNumber(int n)
        {
            value = n;
        }

        
        public int getValue()
        {
            return value;
        }

        public void setValue(int n)
        {
            value = n;
        }

    }
}
