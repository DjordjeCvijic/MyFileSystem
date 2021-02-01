using System;
using System.Collections.Generic;
using System.Text;


//namespace BTree
namespace OposFileSystem
{
    
    class TreeNode
    {
        public MyFile[] files = new MyFile[MyBTree.MAX + 1];//ima jedan vise jer se moze dodati ali se onda balansira
        public int count;
        public TreeNode[] link = new TreeNode[MyBTree.MAX + 1];
    }

    class MyBTree
    {
        public static int MAX = 4;
        public static int MIN = 2;
        public TreeNode root;


        public TreeNode createNode(MyFile file, TreeNode child)
        {
            TreeNode newNode = new TreeNode();
            newNode.files[1] = file;
            newNode.count = 1;
            newNode.link[0] = root;
            newNode.link[1] = child;
            return newNode;
        }
        /* Places the value in appropriate position */
        public void addValToNode(MyFile file, int pos, ref TreeNode node, ref TreeNode child)
        {
            int j = node.count;
            while (j > pos)//ide unazad odmah da stavi na odg poziciju
            {
                node.files[j + 1] = node.files[j];
                node.link[j + 1] = node.link[j];
                j--;
            }
            node.files[j + 1] = file;
            node.link[j + 1] = child;
            node.count++;
        }
        /* split the node */
        public void splitNode(MyFile file, ref MyFile pval, int pos, TreeNode node, TreeNode child, ref TreeNode newNode)
        {
            int median, j;

            if (pos > MIN)
                median = MIN + 1;
            else
                median = MIN;

            newNode = new TreeNode();
            j = median + 1;
            while (j <= MAX)//petlja za dizanje brojeva
            {
                newNode.files[j - median] = node.files[j];
                newNode.link[j - median] = node.link[j];
                j++;
            }
            node.count = median;
            newNode.count = MAX - median;

            if (pos <= MIN)
            {
                addValToNode(file, pos, ref node, ref child);
            }
            else
            {
                addValToNode(file, pos - median, ref newNode, ref child);
            }
            pval = node.files[node.count];
            newNode.link[0] = node.link[node.count];
            node.count--;
        }
        /* sets the value val in the node */
        public int setValueInNode(MyFile file, ref MyFile pval, TreeNode node, ref TreeNode child)
        {

            int pos;
            if (node == null)
            {
                pval = file;
                child = null;
                return 1;
            }

            if (file.getID() < node.files[1].getID())//gleda da li fa ga stavi na 0 poziciju
            {
                pos = 0;
            }
            else
            {
                for (pos = node.count; (file.getID() < node.files[pos].getID() && pos > 1); pos--) ;//ide opet unazad da nadje mjesto

                if (file.getID() == node.files[pos].getID())
                {
                    Console.WriteLine("Duplicates not allowed\n");
                    return 0;
                }
            }
            if (setValueInNode(file, ref pval, node.link[pos], ref child) == 1)//prerpaviti da metoda vraca true i folse
            {
                if (node.count < MAX)
                {
                    addValToNode(pval, pos, ref node, ref child);
                }
                else
                {
                    splitNode(pval, ref pval, pos, node, child, ref child);
                    return 1;
                }
            }
            return 0;
        }

        /* insert val in B-Tree */
        public void insertion(MyFile file)
        {
            int flag;
            MyFile i = null;
            TreeNode child = null;

            flag = setValueInNode(file, ref i, root, ref child);
            if (flag == 1)//vraca ejdinicu ako root ne postoji
                root = createNode(i, child);
        }
        /* copy successor for the value to be deleted */
        public void copySuccessor(TreeNode myNode, int pos)
        {
            TreeNode dummy;
            dummy = myNode.link[pos];

            for (; dummy.link[0] != null;)
                dummy = dummy.link[0];
            myNode.files[pos] = dummy.files[1];

        }
        /* removes the value from the given node and rearrange values */
        public void removeVal(TreeNode myNode, int pos)
        {
            int i = pos + 1;
            while (i <= myNode.count)
            {
                myNode.files[i - 1] = myNode.files[i];
                myNode.link[i - 1] = myNode.link[i];
                i++;
            }
            myNode.count--;
        }
        /* shifts value from parent to right child */
        public void doRightShift(TreeNode myNode, int pos)
        {
            TreeNode x = myNode.link[pos];
            int j = x.count;

            while (j > 0)
            {
                x.files[j + 1] = x.files[j];
                x.link[j + 1] = x.link[j];
            }
            x.files[1] = myNode.files[pos];
            x.link[1] = x.link[0];
            x.count++;

            x = myNode.link[pos - 1];
            myNode.files[pos] = x.files[x.count];
            myNode.link[pos] = x.link[x.count];
            x.count--;
            return;
        }

        /* shifts value from parent to left child */
        public void doLeftShift(TreeNode myNode, int pos)
        {
            int j = 1;
            TreeNode x = myNode.link[pos - 1];

            x.count++;
            x.files[x.count] = myNode.files[pos];
            x.link[x.count] = myNode.link[pos].link[0];

            x = myNode.link[pos];
            myNode.files[pos] = x.files[1];
            x.link[0] = x.link[1];
            x.count--;

            while (j <= x.count)
            {
                x.files[j] = x.files[j + 1];
                x.link[j] = x.link[j + 1];
                j++;
            }
            return;
        }
        /* merge nodes */
        public void mergeNodes(TreeNode myNode, int pos)
        {
            int j = 1;
            TreeNode x1 = myNode.link[pos], x2 = myNode.link[pos - 1];

            x2.count++;
            x2.files[x2.count] = myNode.files[pos];
            x2.link[x2.count] = myNode.link[0];

            while (j <= x1.count)
            {
                x2.count++;
                x2.files[x2.count] = x1.files[j];
                x2.link[x2.count] = x1.link[j];
                j++;
            }

            j = pos;
            while (j < myNode.count)
            {
                myNode.files[j] = myNode.files[j + 1];
                myNode.link[j] = myNode.link[j + 1];
                j++;
            }
            myNode.count--;

        }
        /* adjusts the given node */
        public void adjustNode(TreeNode myNode, int pos)
        {
            if (pos == 0)
            {
                if (myNode.link[1].count > MIN)
                {
                    doLeftShift(myNode, 1);
                }
                else
                {
                    mergeNodes(myNode, 1);
                }
            }
            else
            {
                if (myNode.count != pos)
                {
                    if (myNode.link[pos - 1].count > MIN)
                    {
                        doRightShift(myNode, pos);
                    }
                    else
                    {
                        if (myNode.link[pos + 1].count > MIN)
                        {
                            doLeftShift(myNode, pos + 1);
                        }
                        else
                        {
                            mergeNodes(myNode, pos);
                        }
                    }
                }
                else
                {
                    if (myNode.link[pos - 1].count > MIN)
                        doRightShift(myNode, pos);
                    else
                        mergeNodes(myNode, pos);
                }
            }
        }
        /* delete val from the node */
        int delValFromNode(int id, TreeNode myNode)
        {
            int pos, flag = 0;
            if (myNode != null)
            {
                if (id < myNode.files[1].getID())
                {
                    pos = 0;
                    flag = 0;
                }
                else
                {
                    for (pos = myNode.count; (id < myNode.files[pos].getID() && pos > 1); pos--) ;
                    if (id == myNode.files[pos].getID())
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }
                }
                if (flag == 1)
                {
                    if (myNode.link[pos - 1] != null)
                    {
                        copySuccessor(myNode, pos);
                        flag = delValFromNode(myNode.files[pos].getID(), myNode.link[pos]);
                        if (flag == 0)
                        {
                            Console.Write("Given data is not present in B-Tree\n");
                        }
                    }
                    else
                    {
                        removeVal(myNode, pos);
                    }
                }
                else
                {
                    flag = delValFromNode(id, myNode.link[pos]);
                }
                if (myNode.link[pos] != null)
                {
                    if (myNode.link[pos].count < MIN)
                        adjustNode(myNode, pos);
                }
            }
            return flag;
        }
        /* delete val from B-tree */
        public void deletion(int id, TreeNode myNode)
        {
            TreeNode tmp;
            if (delValFromNode(id, myNode) == 0)
            {
                Console.WriteLine("Given value is not present in B-Tree\n");
                return;
            }
            else
            {
                if (myNode.count == 0)
                {
                    tmp = myNode;
                    myNode = myNode.link[0];

                }
            }
            root = myNode;
            return;
        }
        /* search val in B-Tree */
        public void searching(int id, ref int pos, TreeNode myNode, ref MyFile res)
        {
            if (myNode == null)
            {
                return;
            }

            if (id < myNode.files[1].getID())
            {
                pos = 0;
            }
            else
            {
                for (pos = myNode.count;
                    (id < myNode.files[pos].getID() && pos > 1); (pos)--) ;
                if (id == myNode.files[pos].getID())
                {
                    //Console.WriteLine("Given data is Found\n");
                    res = myNode.files[pos];
                    return;
                }
            }
            searching(id, ref pos, myNode.link[pos], ref res);
            return;
        }
        public void searchingForNode(int id, ref int pos, TreeNode myNode, ref TreeNode res)
        {
            if (myNode == null)
            {
                return;
            }

            if (id < myNode.files[1].getID())
            {
                pos = 0;
            }
            else
            {
                for (pos = myNode.count;
                    (id < myNode.files[pos].getID() && pos > 1); (pos)--) ;
                if (id == myNode.files[pos].getID())
                {
                    //Console.WriteLine("Given data is Found\n");
                    res = myNode;
                    return;
                }
            }
            searchingForNode(id, ref pos, myNode.link[pos], ref res);
            return;
        }
        /* B-Tree Traversal */
        public void traversal(TreeNode myNode)
        {
            int i;
            if (myNode != null)
            {
                for (i = 0; i < myNode.count; i++)
                {
                    traversal(myNode.link[i]);
                    Console.WriteLine(myNode.files[i + 1].getID() << ' ');

                }
                traversal(myNode.link[i]);
            }
        }


        public string getPathFromFileName(string fileName) {
            string[] tempPath = fileName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string tempFilePath = "";
            for (int x = 0; x < tempPath.Length - 1; x++) tempFilePath += "\\" + tempPath[x];
            return tempFilePath;
        }



    }
}
