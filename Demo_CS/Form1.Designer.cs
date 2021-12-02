namespace Demo_CS
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.expTree1 = new ExpTreeLib.ExpTree();
            this.expList1 = new ExpListLib.ExpList();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.expTree1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.expList1);
            this.splitContainer1.Size = new System.Drawing.Size(784, 461);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;
            // 
            // expTree1
            // 
            this.expTree1.AllowFolderRename = true;
            this.expTree1.Cursor = System.Windows.Forms.Cursors.Default;
            this.expTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expTree1.Location = new System.Drawing.Point(0, 0);
            this.expTree1.Name = "expTree1";
            this.expTree1.ShowRootLines = false;
            this.expTree1.Size = new System.Drawing.Size(250, 461);
            this.expTree1.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
            this.expTree1.TabIndex = 0;
            this.expTree1.ExpTreeNodeSelected += new ExpTreeLib.ExpTree.ExpTreeNodeSelectedEventHandler(this.expTree1_ExpTreeNodeSelected);
            // 
            // expList1
            // 
            this.expList1.CurrentPath = "Desktop";
            this.expList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expList1.Location = new System.Drawing.Point(0, 0);
            this.expList1.Name = "expList1";
            this.expList1.Size = new System.Drawing.Size(530, 461);
            this.expList1.TabIndex = 0;
            this.expList1.ExpListItemDoubleClick += new ExpListLib.ExpList.ExpListItemDoubleClickEventHandler(this.expList1_ExpListItemDoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private ExpTreeLib.ExpTree expTree1;
        private ExpListLib.ExpList expList1;
    }
}

