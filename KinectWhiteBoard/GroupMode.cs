using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWhiteBoard
{
    public class GroupMode{

        private Window1 windowRef;
        private List<ThumbGroups> thumbGroupsList;  // List of Thumb Groups
        public ThumbGroups currentSelectedGroup = null;
        private int currentIndex, nextIndexToBeFilled = -1;
        public MyThumb currentGroupSelectionThumb;
        private MyThumb dummy = null;

        public GroupMode(Window1 w)
        {
            windowRef = w;
            thumbGroupsList = new List<ThumbGroups>();
            dummy = new MyThumb();
            dummy.groupNumber = dummy.grouplinecount = -100;
        }

        public void drawLine(MyThumb one, MyThumb two)
        {
            Console.Write("\ndrawLine => grouplinecount: " + one.grouplinecount + "," + two.grouplinecount + " | groupnumbers: " + one.groupNumber + "," + two.groupNumber);
            if (one.groupNumber == -1 && two.groupNumber == -1)
            {
                Console.Write(" ....one.groupNumber == -1 && two.groupNumber == -1");
                createGroup(one, two); one.grouplinecount++; two.grouplinecount++;
                return;
            }
            if (one.groupNumber != -1 && two.groupNumber != -1)
            {
                Console.Write(" ....one.groupNumber != -1 && two.groupNumber != -1");
                if (one.groupNumber == two.groupNumber)
                {
                    Console.Write(" ....one.groupNumber == two.groupNumber");
                    windowRef.createLink(one, two, thumbGroupsList[one.groupNumber].getColor(), 3);
                    one.grouplinecount++; two.grouplinecount++;
                }else
                    windowRef.createLink(one, two); 
            }
            else
            {
                Console.Write(" ....one.groupNumber != -1 or two.groupNumber != -1");
                if (one.groupNumber != -1)
                {
                    addToAGroup(one, two); one.grouplinecount++; two.grouplinecount++;
                }
                else { addToAGroup(two, one); one.grouplinecount++; two.grouplinecount++; }
            }
        }

        public void createGroup(MyThumb one, MyThumb two)
        {
            //if (nextIndexToBeFilled != -1)
            //{
            //    currentIndex = nextIndexToBeFilled;
            //    nextIndexToBeFilled = -1;
            //}
            //else
            //{}
            currentIndex = thumbGroupsList.Count;            
            ThumbGroups group = new ThumbGroups(currentIndex + 1, 3, currentIndex);
            one.groupNumber = two.groupNumber = group.getGroupNumber();
            group.getListOfThumbs().Add(one);
            group.getListOfThumbs().Add(two);
            thumbGroupsList.Add(group);
            windowRef.createLink(one, two, currentIndex + 1, 3);   
        }

        public void addToAGroup(MyThumb old, MyThumb newThumb)
        {
            Console.Write("\naddToAGroup => grouplinecount: " + old.grouplinecount + "," + newThumb.grouplinecount + " | groupnumbers: " + old.groupNumber + "," + newThumb.groupNumber);
            newThumb.groupNumber = old.groupNumber;
            thumbGroupsList[old.groupNumber].getListOfThumbs().Add(newThumb);
            windowRef.createLink(old, newThumb, thumbGroupsList[old.groupNumber].getColor(), 3);
        }

        public void removeLink(MyThumb one, MyThumb two)
        {
            Console.Write("\nremoveLink => grouplinecount: " + one.grouplinecount + "," + two.grouplinecount + " | groupnumbers: " + one.groupNumber + "," + two.groupNumber);
            if(one.grouplinecount==0 || two.grouplinecount==0)
            {
                windowRef.deleteLink(one, two);
                return;
            }
            if (one.groupNumber == two.groupNumber)
            {
                Console.Write(" ....same group number");
                if (one.grouplinecount == 1 && two.grouplinecount == 1)
                {
                    windowRef.deleteLink(one, two);
                    removeGroup(one, two);
                    one.grouplinecount = 0; two.grouplinecount = 0;
                    one.groupNumber = -1; two.groupNumber = -1;
                }
                else
                {
                    if (one.grouplinecount > 1 && two.grouplinecount > 1)
                    {
                        windowRef.deleteLink(one, two);
                        one.grouplinecount--; two.grouplinecount--;
                        //one.groupNumber = two.groupNumber = -1;
                    }
                    else
                    {
                        if (one.grouplinecount == 1)
                        {
                            windowRef.deleteLink(one, two);
                            removeThumbFromGroup(one);
                            one.grouplinecount = 0; one.groupNumber = -1;
                            two.grouplinecount--;
                        }
                        else
                            if (two.grouplinecount == 1)
                            {
                                windowRef.deleteLink(one, two);
                                removeThumbFromGroup(two);
                                two.grouplinecount = 0; two.groupNumber = -1;
                                one.grouplinecount--;
                            }
                    }
                }
            }
            else 
            {
                windowRef.deleteLink(one, two);
            }
            //****
            
            
        }

        public void removeThumbFromGroup(MyThumb one)
        {
            ThumbGroups group = thumbGroupsList[one.groupNumber];
            bool found=false;
            for (int i=0; i < group.getListOfThumbs().Count && !found; i++)
            {
                if (one == group.getListOfThumbs()[i])
                {
                    group.getListOfThumbs()[i].grouplinecount = 0; ;
                    group.getListOfThumbs()[i].groupNumber = -1;
                    found = true;
                }
                
            }
            if(found) group.getListOfThumbs().Remove(one);
        }

        public void removeGroup(MyThumb one, MyThumb two)
        {
            Console.Write("\nREMOVE GROUP");
            nextIndexToBeFilled = one.groupNumber;
            thumbGroupsList[one.groupNumber] = null;
        }

        public void removeGroup(MyThumb one)
        {
            nextIndexToBeFilled = one.groupNumber;
            ThumbGroups group = thumbGroupsList[one.groupNumber];
            foreach (MyThumb thumb in group.getListOfThumbs())
            {
                thumb.grouplinecount = 0; ;
                thumb.groupNumber = -1;
            }
            thumbGroupsList[one.groupNumber] = null;
        }


        public bool selectWholeGroup(MyThumb one)
        {
            Console.Write("\nselectWholeGroup");
            if (one.groupNumber == -1)
            {
                return false;
            }
            if (currentGroupSelectionThumb != one)
            {
                currentGroupSelectionThumb = one;
                ThumbGroups group = thumbGroupsList[one.groupNumber];
                currentSelectedGroup = group;
                foreach (MyThumb thumb in group.getListOfThumbs())
                {
                    thumb.rect = windowRef.addRectangle(thumb, 3);
                    windowRef.myCanvas.Children.Add(thumb.rect);
                }
            
            }            
            return true;
        }

        public void unSelectWholeGroup()
        {
            Console.Write("\nUNselectWholeGroup");
            //currentGroupSelectionThumb = one;
            ThumbGroups group = thumbGroupsList[currentGroupSelectionThumb.groupNumber];
            foreach (MyThumb thumb in group.getListOfThumbs())
            {
                windowRef.myCanvas.Children.Remove(thumb.rect);
            }
            currentGroupSelectionThumb = dummy;
            currentSelectedGroup = null;
        }

        public void handleThumbDeletion(MyThumb thumb)
        {
            List<MyThumb> neighbours = windowRef.getGroupNeighboursOnThumbDeletion(thumb);
            
            if (neighbours != null)
            {
                if (neighbours.Count != 0)
                {
                    foreach (MyThumb t in neighbours)
                    {
                        removeLink(thumb, t);
                    }
                }
            }
            if (thumb.groupNumber != -1)
            {
                removeThumbFromGroup(thumb);
            }
        }

    }
}
