Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.ExpTree1.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop
        Me.ExpList1.ViewType = View.LargeIcon
    End Sub

    'Load files to ExpFileList
    Private Sub ExpTree1_ExpTreeNodeSelected(SelPath As String, Item As ExpTreeLib.CShItem) Handles ExpTree1.ExpTreeNodeSelected
        Dim includeFolder As Boolean = True
        Me.ExpList1.DisplayFiles(SelPath, Item, includeFolder)
    End Sub

    Private Sub ExpList1_ExpListItemDoubleClick(SelPath As String, Item As ExpTreeLib.CShItem) Handles ExpList1.ExpListItemDoubleClick
        Me.ExpTree1.ExpandANode(Item)
    End Sub
End Class
