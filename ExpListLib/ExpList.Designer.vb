<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ExpList
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.ExpFileList = New System.Windows.Forms.ListView()
        Me.chName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chSize = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chLastModified = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chTypeStr = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chAttributes = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chCreated = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'ExpFileList
        '
        Me.ExpFileList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ExpFileList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chName, Me.chSize, Me.chLastModified, Me.chTypeStr, Me.chAttributes, Me.chCreated})
        Me.ExpFileList.Cursor = System.Windows.Forms.Cursors.Default
        Me.ExpFileList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ExpFileList.FullRowSelect = True
        Me.ExpFileList.LabelEdit = True
        Me.ExpFileList.Location = New System.Drawing.Point(0, 0)
        Me.ExpFileList.Name = "ExpFileList"
        Me.ExpFileList.Size = New System.Drawing.Size(700, 300)
        Me.ExpFileList.TabIndex = 2
        Me.ExpFileList.UseCompatibleStateImageBehavior = False
        Me.ExpFileList.View = System.Windows.Forms.View.Details
        '
        'chName
        '
        Me.chName.Tag = "ExpTreeLib.CShItem.DisplayName"
        Me.chName.Text = "Name"
        Me.chName.Width = 150
        '
        'chSize
        '
        Me.chSize.Text = "Size"
        Me.chSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.chSize.Width = 88
        '
        'chLastModified
        '
        Me.chLastModified.Text = "LastMod Date"
        Me.chLastModified.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.chLastModified.Width = 122
        '
        'chTypeStr
        '
        Me.chTypeStr.Text = "Type"
        Me.chTypeStr.Width = 100
        '
        'chAttributes
        '
        Me.chAttributes.Text = "Attributes"
        Me.chAttributes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.chAttributes.Width = 80
        '
        'chCreated
        '
        Me.chCreated.Text = "Created"
        Me.chCreated.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.chCreated.Width = 122
        '
        'ExpList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ExpFileList)
        Me.Name = "ExpList"
        Me.Size = New System.Drawing.Size(700, 300)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ExpFileList As Windows.Forms.ListView
    Friend WithEvents chName As Windows.Forms.ColumnHeader
    Friend WithEvents chSize As Windows.Forms.ColumnHeader
    Friend WithEvents chLastModified As Windows.Forms.ColumnHeader
    Friend WithEvents chTypeStr As Windows.Forms.ColumnHeader
    Friend WithEvents chAttributes As Windows.Forms.ColumnHeader
    Friend WithEvents chCreated As Windows.Forms.ColumnHeader
End Class
