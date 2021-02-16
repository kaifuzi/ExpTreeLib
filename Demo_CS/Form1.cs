using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo_CS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.expTree1.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
            this.expList1.ViewType = (int)View.LargeIcon;
        }

        //Load files to ExpFileList
        private void expTree1_ExpTreeNodeSelected(string SelPath, ExpTreeLib.CShItem Item)
        {
            bool includeFolder = true;
            this.expList1.DisplayFiles(SelPath, Item, includeFolder);
        }

        private void expList1_ExpListItemDoubleClick(string SelPath, ExpTreeLib.CShItem Item)
        {
            this.expTree1.ExpandANode(Item);
        }
    }
}
