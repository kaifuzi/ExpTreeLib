Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports ExpTreeLib
Imports ExpTreeLib.CShItem
Imports ExpTreeLib.ShellDll
Imports ExpTreeLib.ShellDll.ShellAPI
Imports ExpTreeLib.ShellDll.ShellHelper

''' <summary>
''' This Form is a fully working start point for any form which requires an ExplorerTree and
''' ListView with enough room left for application specific controls.
''' </summary>
''' <remarks>
''' <para>This template form illustrates the use of:
''' <list type="bullet">
''' <item><description>Use of the ExpTreeNodeSelected Event Handler.</description></item>
''' <item><description>Use of LVColSorter for column sorting. See MakeLviItem for a custom ListViewItem 
''' builder which is compatible with and useful for LVColSorter. 
''' See Also SortLVItems for how to perform a Refresh of the 
''' ListView in response to a Refresh command from the Context Menu.</description></item>
''' <item><description>Full Context Menus in the ListView.</description></item>
''' <item><description>ListViewItem editing (first SubItem only) if the ListViewItem.Tag is a CShItem.</description></item>
''' <item><description>Handling of dynamic update Events from CShItemUpdate Events.</description></item>
''' <item><description>Proper handling of the Delete Key.</description></item>
''' <item><description>Shows how to handle a DoubleClick on a ListViewItem.</description></item>
''' </list>
''' </para></remarks>
Public Class ExpList
    Private Declare Auto Function SendMessage Lib "user32" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr

    'Avoid Globalization problem-- an empty timevalue
    Private ReadOnly EmptyTimeValue As New DateTime(1, 1, 1, 0, 0, 0)

    Private LastSelectedCSI As CShItem

    Private DW As CDragWrapper              'Wrapper for Drag ops originating in ExpFileList
    Private DropWrap As ClvDropWrapper      'Wrapper for Drop ops targeting ExpFileList

    Private m_CreateNew As Boolean = False  'Flag for NewMenu processing of "New" item

    Public Event ExpListItemDoubleClick(ByVal SelPath As String, ByVal Item As CShItem)  'List view item double click event
    Public Event ExpListItemMouseMBUp(ByVal SelPath As String, ByVal Item As CShItem)  'Press mouse middle button up
    Public Event ExpListItemArrowKeyUp(ByVal SelPath As String, ByVal Item As CShItem)  'Press direction key up
    Public Event ExpListItemsChanged(ByVal CurPath As String, ByVal Item As CShItem)  'Get current path and CShItem after itmes count are changed in ListView
    Public Event ExpListItemGetSelItems(ByVal items As ListView.SelectedListViewItemCollection)

    ' InitialLoadLimit is a the number of ExpFileList.Items whose IconIndex will we fetched on initial load
    ' the balance will be fetched AFTER ExpFileList.EndUpdate
    Private Const InitialLoadLimit As Integer = 32

    ' For ExpFileList label text selection
    Private Const EM_SETSEL As Integer = &HB1
    Private Const LVM_FIRST As Integer = &H1000
    Private Const LVM_GETEDITCONTROL As Integer = (LVM_FIRST + 24)

#Region "   Public Properties"
    ''' <summary></summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Browsable(True), Category("Appearance"),
    Description("Selects one of five different views that items can be shown in."),
    DefaultValue(View.Details)>
    Public Property ViewType() As Integer
        Get
            Return ExpFileList.View
        End Get
        Set(ByVal value As Integer)
            ExpFileList.View = value
        End Set
    End Property

    ''' <summary></summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Browsable(True), Category("Misc"),
    Description("The current path of ExpFileList"),
    DefaultValue("")>
    Public Property CurrentPath() As String
        Get
            Return _CurrentPath
        End Get
        Set(value As String)
            _CurrentPath = value
        End Set
    End Property
    Private _CurrentPath As String = "Desktop"

    ''' <summary></summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Browsable(True), Category("Misc"),
    Description("The current CSI of ExpFileList"),
    DefaultValue("")>
    Public ReadOnly Property CurrentCSI() As CShItem
        Get
            Return LastSelectedCSI
        End Get
    End Property
#End Region

#Region "   Form Close Methods"
#End Region

#Region "   Form Load/VisibleChanged ExpFileList HandleCreated"
    Private Sub ExpList_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Setup Drag and Drop Wrappers
        DW = New CDragWrapper(ExpFileList)
        DropWrap = New ClvDropWrapper(ExpFileList)
        'Setup Change Notification	7/1/2012
        AddHandler CShItemUpdate, AddressOf UpdateInvoke        '7/1/2012

        'LuKai 2019.05.18:Remove column Type and Attributes, it also need to comment related code in MakeLVItem()
        For Each column As ColumnHeader In ExpFileList.Columns
            If (column.Text = "Type") Or (column.Text = "Attributes") Then
                ExpFileList.Columns.Remove(column)
            End If
        Next
    End Sub

    Private Sub ExpFileList_HandleCreated(ByVal sender As Object, ByVal e As System.EventArgs) _
                Handles ExpFileList.HandleCreated
        SystemImageListManager.SetListViewImageList(ExpFileList, False, False)
        SystemImageListManager.SetListViewImageList(ExpFileList, True, False)
    End Sub

    Private Sub ExpList_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        SystemImageListManager.SetListViewImageList(ExpFileList, False, False)
        SystemImageListManager.SetListViewImageList(ExpFileList, True, False)
    End Sub
#End Region

#Region "   ExplorerTree Event Handling -- AfterNodeSelect"
    Public Sub DisplayFiles(ByVal pathName As String, ByVal CSI As CShItem, ByVal includeFolder As Boolean)
        _CurrentPath = pathName
        If LastSelectedCSI IsNot Nothing AndAlso LastSelectedCSI Is CSI Then Exit Sub

        Dim dirList As New ArrayList()
        Dim fileList As New ArrayList()
        Dim TotalItems As Integer
        If includeFolder Then
            dirList.AddRange(CSI.Directories)
        End If
        If Not CSI.DisplayName.Equals(CShItem.StrMyComputer) Then 'avoid re-query since only has dirs
            fileList.AddRange(CSI.Files)
        End If

        If (dirList.Count + fileList.Count) > 0 Then
            Dim item As CShItem
            If includeFolder Then
                dirList.Sort()
                TotalItems = dirList.Count + fileList.Count
            Else
                TotalItems = fileList.Count
            End If
            fileList.Sort()

            Dim combList As New ArrayList(TotalItems)
            If includeFolder Then
                combList.AddRange(dirList)
            End If
            combList.AddRange(fileList)

            'Build the ListViewItems & add to ExpFileList
            ExpFileList.BeginUpdate()
            ExpFileList.Items.Clear()
            If LastSelectedCSI IsNot Nothing AndAlso LastSelectedCSI IsNot CSI Then
                LastSelectedCSI.ClearItems(True)
            End If
            ExpFileList.Refresh()

            Dim InitialFillLim As Integer = Math.Min(combList.Count, InitialLoadLimit)
            Dim FirstLoad As New List(Of ListViewItem)(combList.Count)
            For Each item In combList
                Dim lvi As ListViewItem = MakeLVItem(item)
                If ExpFileList.Items.Count < InitialFillLim Then
                    lvi.ImageIndex = SystemImageListManager.GetIconIndex(lvi.Tag, False)
                End If
                FirstLoad.Add(lvi)
            Next
            ExpFileList.Items.AddRange(FirstLoad.ToArray)
            ExpFileList.EndUpdate()

            If ExpFileList.Items.Count > 0 Then  'LuKai 2019.05.18
                For i As Integer = InitialFillLim - 1 To ExpFileList.Items.Count - 1
                    ExpFileList.Items(i).ImageIndex = SystemImageListManager.GetIconIndex(ExpFileList.Items(i).Tag, False)
                Next
            End If
        Else
            ExpFileList.Items.Clear()
            If LastSelectedCSI IsNot Nothing AndAlso LastSelectedCSI IsNot CSI Then
                LastSelectedCSI.ClearItems(True)
            End If
        End If
        LastSelectedCSI = CSI
        ExpFileList.Tag = LastSelectedCSI           '7/5/2012   For ClvDropWapper

        'Now that lv.ListViewItems has been set up (and MakeLvItem does attach the appropriate tags
        ' to both the ListViewItem and the appropriate SubItems), set the ListViewItemSorter
        ExpFileList.ListViewItemSorter = New LVColSorter(ExpFileList)
    End Sub
#End Region

#Region "   MakeLVItem"
    Private Function MakeLVItem(ByVal item As CShItem) As ListViewItem
        Dim lvi As New ListViewItem(item.DisplayName)
        With lvi
            .Tag = item
            'Set Length
            If Not item.IsDisk And item.IsFileSystem And Not item.IsFolder Then
                'LuKai 2019.05.18:Change Length to Size
                'If item.Length > 1024 Then
                '    .SubItems.Add(Format(item.Length / 1024, "#,### KB"))
                'Else
                '    .SubItems.Add(Format(item.Length, "##0 Bytes"))
                'End If
                .SubItems.Add(item.Size)
                lvi.SubItems(lvi.SubItems.Count - 1).Tag = item.Length
            Else
                .SubItems.Add("")
                lvi.SubItems(lvi.SubItems.Count - 1).Tag = 0L
            End If
            'Set LastWriteTime
            If item.IsDisk OrElse item.LastWriteTime = EmptyTimeValue Then '"#1/1/0001 12:00:00 AM#" is empty
                .SubItems.Add("")
                .SubItems(.SubItems.Count - 1).Tag = EmptyTimeValue
            Else
                .SubItems.Add(item.LastWriteTime.ToString("MM/dd/yyyy HH:mm:ss"))
                .SubItems(.SubItems.Count - 1).Tag = item.LastWriteTime
            End If
            'LuKai 2019.05.18:Remove column Type and Attributes
            ''Set Type
            '.SubItems.Add(item.TypeName)
            ''Set Attributes
            'If Not item.IsDisk And item.IsFileSystem Then
            '    Dim SB As New StringBuilder()
            '    Try
            '        Dim attr As FileAttributes = item.Attributes
            '        If (attr And FileAttributes.System) = FileAttributes.System Then SB.Append("System")
            '        If (attr And FileAttributes.Hidden) = FileAttributes.Hidden Then SB.Append("Hidden")
            '        If (attr And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then SB.Append("ReadOnly")
            '        If (attr And FileAttributes.Archive) = FileAttributes.Archive Then SB.Append("Archive")
            '    Catch
            '    End Try
            '    .SubItems.Add(SB.ToString)
            'Else : .SubItems.Add("")
            'End If
            'Set CreationTime
            If item.IsDisk OrElse item.CreationTime = EmptyTimeValue Then '"#1/1/0001 12:00:00 AM#" is empty
                .SubItems.Add("")
                .SubItems(.SubItems.Count - 1).Tag = EmptyTimeValue
            Else
                .SubItems.Add(item.CreationTime.ToString("MM/dd/yyyy HH:mm:ss"))
                .SubItems(.SubItems.Count - 1).Tag = item.CreationTime
            End If
        End With
        Return lvi
    End Function
#End Region

#Region "   Dynamic Update Handler"
    ''' <summary>
    ''' To receive notification of changes to the FileSystem which may affect the GUI display, declare
    ''' DeskTopItem WithEvents. Changes to CShItem's internal tree which are caused by notification of 
    ''' FileSystem changes or by periodic refresh of the contents of the internal tree raise CShItemUpdate
    ''' events.  Since the periodic refresh runs on a different thread, while Windows Notify messages run on
    ''' the main thread, it is required that we check to see if an Invoke is required or not.
    ''' </summary>
    ''' <remarks></remarks>
    Private Delegate Sub InvokeUpdate(ByVal sender As Object, ByVal e As ShellItemUpdateEventArgs)

    'Private WithEvents DeskTopItem As CShItem = CShItem.GetDeskTop '7/1/2012

    Private ReadOnly m_InvokeUpdate As New InvokeUpdate(AddressOf DoItemUpdate)

    ''' <summary>
    ''' Returns the last CShItem Selected.
    ''' </summary>
    ''' <remarks>Not currently used.</remarks>
    Public ReadOnly Property SelectedItem() As CShItem
        Get
            Return LastSelectedCSI
        End Get
    End Property
    ''' <summary>
    ''' Determines if DoItemUpdate should be called directly or via Invoke, and then calls it.
    ''' </summary>
    ''' <param name="sender">The CShItem of the Folder of the changed item.</param>
    ''' <param name="e">Contains information about the type of change and items affected.</param>
    ''' <remarks>Responds to events raised by either WM_Notify messages or FileWatch.</remarks>
    Private Sub UpdateInvoke(ByVal sender As Object, ByVal e As ShellItemUpdateEventArgs) '7/1/2012 removed Handles clause
        If Me.InvokeRequired Then
            Invoke(m_InvokeUpdate, sender, e)
        Else
            DoItemUpdate(sender, e)
            'Get current CShItem after itmes count are changed in ListView
            If e.UpdateType = CShItemUpdateType.Created Or e.UpdateType = CShItemUpdateType.Deleted Then
                If LastSelectedCSI.Path.StartsWith(":") Then
                    RaiseEvent ExpListItemsChanged(LastSelectedCSI.DisplayName, LastSelectedCSI)
                Else
                    RaiseEvent ExpListItemsChanged(LastSelectedCSI.Path, LastSelectedCSI)
                End If
            End If
        End If
    End Sub
    ''' <summary>
    ''' Makes changes in ExpFileList GUI in response to updating events raised by CShItem.
    ''' </summary>
    ''' <param name="sender">The CShItem of the Folder of the changed item.</param>
    ''' <param name="e">Contains information about the type of change and items affected.</param>
    ''' <remarks>Responds to events raised by either WM_Notify messages or FileWatch (FileWatch not
    ''' implemented in this Form).</remarks>
    Private Sub DoItemUpdate(ByVal sender As Object, ByVal e As ShellItemUpdateEventArgs)
        ' Debug.WriteLine("Enter frmDragDrop DoItemUpdate -- " & e.UpdateType.ToString)
        'If e.Item IsNot Nothing Then  'should never be Nothing ... but ...
        Dim Parent As CShItem = DirectCast(sender, CShItem)
        If Parent Is LastSelectedCSI Then ' 6/11/2012 - OrElse (e.Item Is LastSelectedCSI AndAlso e.UpdateType = CShItemUpdateType.Updated) Then   'If not, then of no interest to us
            Try
                ExpFileList.BeginUpdate()
                Select Case e.UpdateType
                    Case CShItem.CShItemUpdateType.Created
                        Dim lvi As ListViewItem = MakeLVItem(e.Item)
                        lvi.ImageIndex = DirectCast(e.Item, CShItem).IconIndexNormal
                        InsertLvi(lvi, ExpFileList)     '6/11/2012
                        If m_CreateNew Then
                            m_CreateNew = False
                            lvi.BeginEdit()
                        End If
                    Case CShItemUpdateType.Deleted
                        Dim lvi As ListViewItem = FindLVItem(e.Item)
                        If lvi IsNot Nothing Then
                            ExpFileList.Items.Remove(lvi)
                        End If
                    Case CShItemUpdateType.Renamed
                        Dim lvi As ListViewItem = FindLVItem(e.Item)
                        If lvi IsNot Nothing Then
                            If e.Item.Parent IsNot LastSelectedCSI Then     'if true = item renamed to different directory
                                ExpFileList.Items.Remove(lvi)
                            Else
                                lvi.Text = e.Item.DisplayName
                                lvi.ImageIndex = DirectCast(e.Item, CShItem).IconIndexNormal
                                ExpFileList.Items.Remove(lvi)   '6/11/2012
                                InsertLvi(lvi, ExpFileList)     '6/11/2012
                            End If
                        End If
                    Case CShItemUpdateType.UpdateDir  'in this case Parent/sender is the item of interest
                        ' CShItemUpdater, etc. will do the appropriate Adds and Removes, generating
                        ' Created/Deleted events that will occur before an UpdateDir event. There is
                        ' no need to do anything here.
                        'ExpFileList.BeginUpdate()
                        'ExpFileList.Sort()
                        'ExpFileList.EndUpdate()

                    Case CShItemUpdateType.Updated
                        Dim lvi As ListViewItem = FindLVItem(e.Item)
                        If lvi IsNot Nothing Then
                            Dim indx As Integer = ExpFileList.Items.IndexOf(lvi)
                            Dim newLVI As ListViewItem = MakeLVItem(e.Item)
                            newLVI.ImageIndex = DirectCast(e.Item, CShItem).IconIndexNormal
                            ExpFileList.Items.RemoveAt(indx)
                            ExpFileList.Items.Insert(indx, newLVI)
                        End If
                    Case CShItemUpdateType.IconChange
                        Dim lvi As ListViewItem = FindLVItem(e.Item)
                        If lvi IsNot Nothing Then
                            lvi.ImageIndex = DirectCast(e.Item, CShItem).IconIndexNormal
                        End If
                    Case CShItemUpdateType.MediaChange
                        Dim lvi As ListViewItem = FindLVItem(e.Item)
                        If lvi IsNot Nothing Then
                            lvi.Text = e.Item.DisplayName
                            lvi.ImageIndex = DirectCast(e.Item, CShItem).IconIndexNormal
                        End If
                End Select
            Catch ex As Exception
                Debug.WriteLine("Error in frmTemplate -- ExpFileList updater -- " & ex.ToString)
            Finally
                ExpFileList.EndUpdate()
            End Try
        End If      'end of Parent Is LastSelectedCSI test
        'End If          'of e.Item IsNot Nothing test
    End Sub

    Private Function FindLVItem(ByVal item As CShItem) As ListViewItem
        For Each lvi As ListViewItem In ExpFileList.Items
            If lvi.Tag Is item Then
                Return lvi
            End If
        Next
        Return Nothing
    End Function


    ''' <summary>
    ''' Given a ListViewItem with a  CShItem in its' Tag, and a ListView whose Items all have a CShItem in
    ''' their Tags, Insert the ListViewItem in its' proper place in the ListView.
    ''' </summary>
    ''' <param name="lvi">The ListViewItem to be inserted.</param>
    ''' <param name="LV">The ListView into which the ListViewItem is to be inserted.</param>
    ''' <remarks>6/11/2012 - better than a Sort when the list is in order.<br />
    '''          Will honor any prior Column Sorts.</remarks>
    Private Sub InsertLvi(ByVal lvi As ListViewItem, ByVal LV As ListView)
        Dim Item As CShItem = lvi.Tag
        For i As Integer = 0 To LV.Items.Count - 1
            If DirectCast(LV.Items(i).Tag, CShItem).CompareTo(Item) > 0 Then
                LV.Items.Insert(i, lvi)
                lvi.EnsureVisible()
                Exit Sub
            End If
        Next
        LV.Items.Add(lvi)
        lvi.EnsureVisible()
    End Sub
#End Region

#Region "   ExpFileList_DoubleClick"
    Private Sub ExpFileList_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ExpFileList.DoubleClick
        If ExpFileList.SelectedItems.Count <= 0 Then
            Return
        End If

        Dim csi As CShItem = ExpFileList.SelectedItems(0).Tag
        If csi.IsFolder Then
            If csi.Path.StartsWith(":") Then
                RaiseEvent ExpListItemDoubleClick(csi.DisplayName, csi)
            Else
                RaiseEvent ExpListItemDoubleClick(csi.Path, csi)
            End If
        Else
            Try
                Process.Start(csi.Path)
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.OkOnly, "Error in starting application")
            End Try
        End If
    End Sub
#End Region

#Region "   ExpFileList_Leave"
    Private Sub ExpFileList_Leave(sender As Object, e As EventArgs) Handles ExpFileList.Leave
        'Clear the selections after leav me
        ExpFileList.SelectedItems.Clear()
    End Sub
#End Region

#Region "   LabelEdit Handlers (Item Rename) From Calum"
    ''' <summary>
    ''' Handles Before Item Rename for ExpFileList
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Modified version of Calum McLellan's code from ExpList.</remarks>
    Private Sub ExpFileList_BeforeLabelEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.LabelEditEventArgs) Handles ExpFileList.BeforeLabelEdit
        'Only select the label without file name extension
        Dim editWnd As IntPtr = SendMessage(ExpFileList.Handle, LVM_GETEDITCONTROL, 0, IntPtr.Zero)
        Dim textLen As Integer = System.IO.Path.GetFileNameWithoutExtension(ExpFileList.Items(e.Item).Text).Length
        SendMessage(editWnd, EM_SETSEL, 0, CType(textLen, IntPtr))
        'Start rename
        Dim item As CShItem = DirectCast(ExpFileList.Items(e.Item).Tag, CShItem)
        If (Not item.IsFileSystem) Or item.IsDisk Or
            item.Path = CShItem.GetCShItem(CSIDL.MYDOCUMENTS).Path Or
            Not (item.CanRename) Then
            System.Media.SystemSounds.Beep.Play()
            e.CancelEdit = True
        End If
    End Sub

    ''' <summary>
    ''' Handles After Item Rename for ExpFileList
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Modified version of Calum McLellan's code from ExpList.</remarks>
    Private Sub ExpFileList_AfterLabelEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.LabelEditEventArgs) Handles ExpFileList.AfterLabelEdit
        Dim item As CShItem = DirectCast(ExpFileList.Items(e.Item).Tag, CShItem)
        Dim NewName As String
        Dim index As Integer
        Dim path As String
        If e.Label Is Nothing OrElse e.Label = String.Empty Then Exit Sub '6/11/2012
        Try
            NewName = e.Label.Trim

            If NewName.Length < 1 OrElse NewName.IndexOfAny(System.IO.Path.GetInvalidPathChars) <> -1 Then
                e.CancelEdit = True
                System.Media.SystemSounds.Beep.Play()
                Exit Sub
            End If

            path = item.Path

            index = path.LastIndexOf("\"c)
            If index = -1 Then
                e.CancelEdit = True
                System.Media.SystemSounds.Beep.Play()
                Exit Sub
            End If

            Dim newPidl As IntPtr = IntPtr.Zero
            If item.Parent.Folder.SetNameOf(ExpFileList.Handle, CShItem.ILFindLastID(item.PIDL), NewName, SHGDN.NORMAL, newPidl) = S_OK Then
            Else
                System.Media.SystemSounds.Beep.Play()
                e.CancelEdit = True
            End If
        Catch ex As Exception
            e.CancelEdit = True
            System.Media.SystemSounds.Beep.Play()
            Exit Sub
        End Try
    End Sub
#End Region

#Region "   Context Menu Handlers"
    Private ReadOnly m_WindowsContextMenu As WindowsContextMenu = New WindowsContextMenu

    Private Function IsWithin(ByVal Ctl As Control, ByVal e As MouseEventArgs) As Boolean
        IsWithin = False            'default to Not Within
        If e.X < 0 OrElse e.Y < 0 Then Exit Function
        Dim CR As Rectangle = Ctl.ClientRectangle
        If e.X > CR.Width OrElse e.Y > CR.Height Then Exit Function
        IsWithin = True
    End Function
    ''' <summary>
    ''' Sort the ListViewItems based on the CShItems stored in the .Tag of each ListViewItem.
    ''' </summary>
    ''' <remarks>Cannot use LVColSorter for this since we do not know current state
    ''' </remarks>
    Private Sub SortLVItems()
        With ExpFileList
            If .Items.Count < 2 Then Exit Sub 'no point in sorting 0 or 1 items
            .BeginUpdate()
            Dim tmp(.Items.Count - 1) As ListViewItem
            .Items.CopyTo(tmp, 0)
            Array.Sort(tmp, New TagComparer)
            .Items.Clear()
            .Items.AddRange(tmp)
            .EndUpdate()
        End With
    End Sub

    ''' <summary>
    ''' m_OutOfRange is set to True on ExpFileList.MouseLeave (which happens under many circumstances) to prevent
    ''' the non-ListViewItem specific menu from firing. See Remarks
    ''' m_OutOfRange is set to False (allowing ContextMenus in ExpFileList), only on ExpFileList.MouseDown when the Right
    ''' button is pressed. MouseDown only occurs when the Mouse is really over ExpFileList.
    ''' </summary>
    ''' <remarks>
    '''If you hold down the right mouse button, then leave ExpFileList,
    ''' then let go of the mouse button, the MouseUp event is fired upon
    ''' re-entering the ExpFileList - meaning that the Windows ContextMenu will
    ''' be shown if we don't use this flag (from Calum)
    '''</remarks>
    Private m_OutOfRange As Boolean

    Private Sub ExpFileList_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles ExpFileList.MouseLeave
        m_OutOfRange = True
    End Sub

    Private Sub ExpFileList_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ExpFileList.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            m_OutOfRange = False
        End If
    End Sub

    ''' <summary>
    ''' Handles RightButton Click to display a System Context Menu
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Modified version of Calum McLellan's code from ExpList.</remarks>
    Private Sub ExpFileList_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ExpFileList.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If Not IsWithin(ExpFileList, e) Then Exit Sub
            If m_OutOfRange Then Exit Sub
            Dim tn As ListViewItem
            Dim pt As New System.Drawing.Point(e.X, e.Y)
            tn = ExpFileList.GetItemAt(e.X, e.Y)
            If Not IsNothing(tn) AndAlso ExpFileList.SelectedItems.Count > 0 Then
                Dim itms(ExpFileList.SelectedItems.Count - 1) As CShItem
                For i As Integer = 0 To ExpFileList.SelectedItems.Count - 1
                    itms(i) = DirectCast(ExpFileList.SelectedItems(i).Tag, CShItem)
                Next
                Dim cmi As CMInvokeCommandInfoEx = Nothing
                Dim allowRename As Boolean = True          'Don't allow rename of more than 1 item
                If ExpFileList.SelectedItems.Count > 1 Then allowRename = False
                If m_WindowsContextMenu.ShowMenu(Me.Handle, itms, MousePosition, allowRename, cmi) Then
                    'Check for rename
                    Dim cmdBytes(256) As Byte
                    m_WindowsContextMenu.winMenu.GetCommandString(cmi.lpVerb.ToInt32, GCS.VERBA, 0, cmdBytes, 256)

                    Dim cmdName As String = SzToString(cmdBytes).ToLower
                    If cmdName.Equals("rename") Then
                        ExpFileList.LabelEdit = True
                        tn.BeginEdit()
                    Else
                        Dim strPath As String
                        If itms(0).Parent Is CShItem.GetDeskTop Then
                            strPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                        Else
                            strPath = itms(0).Parent.Path
                        End If
                        m_WindowsContextMenu.InvokeCommand(m_WindowsContextMenu.winMenu, cmi.lpVerb, strPath, pt)
                    End If
                    Marshal.ReleaseComObject(m_WindowsContextMenu.winMenu)
                End If
            Else
                GetFolderMenu(MousePosition)
            End If
        End If

        'Get selected items after MouseUp. This haves to be front than below event.
        RaiseEvent ExpListItemGetSelItems(ExpFileList.SelectedItems)

        'Press mouse middle button to get items path and CSI
        If e.Button = Windows.Forms.MouseButtons.Middle Then
            If ExpFileList.SelectedItems.Count > 0 Then
                Dim CSI As CShItem = ExpFileList.SelectedItems(0).Tag
                Dim strPath As String = CSI.Path
                RaiseEvent ExpListItemMouseMBUp(strPath, CSI)
            End If
        End If
    End Sub

#Region "           Folder ContextMenu for a Click on the ListView itself (not on a ListViewItem)"

    Private Sub GetFolderMenu(ByVal pt As Drawing.Point)
        Dim HR As Integer
        Dim min As Integer = 1
        Dim max As Integer = 100000
        Dim cmi As New CMInvokeCommandInfoEx
        Dim comContextMenu As IntPtr = CreatePopupMenu()
        Dim viewSubMenu As IntPtr = CreatePopupMenu()
        ' Dim sortSubMenu As IntPtr = CreatePopupMenu()

        'Check item count - should always be 0 but check just in case
        Dim startIndex As Integer = GetMenuItemCount(comContextMenu.ToInt32)
        'Fill the context menu
        Dim itemInfo As New MENUITEMINFO("View") With {
            .fMask = MIIM.SUBMENU Or MIIM.STRING,
            .hSubMenu = viewSubMenu
        }
        InsertMenuItem(comContextMenu, 0, True, itemInfo)
        Dim checked As Integer = MFT.BYCOMMAND
        If ExpFileList.View = View.Tile Then checked = MFT.RADIOCHECK Or MFT.CHECKED
        AppendMenu(viewSubMenu, checked, CMD.TILES, "Tiles")
        checked = MFT.BYCOMMAND
        If ExpFileList.View = View.LargeIcon Then checked = MFT.RADIOCHECK Or MFT.CHECKED
        AppendMenu(viewSubMenu, checked, CMD.LARGEICON, "Large Icons")
        checked = MFT.BYCOMMAND
        If ExpFileList.View = View.List Then checked = MFT.RADIOCHECK Or MFT.CHECKED
        AppendMenu(viewSubMenu, checked, CMD.LIST, "List")
        checked = MFT.BYCOMMAND
        If ExpFileList.View = View.Details Then checked = MFT.RADIOCHECK Or MFT.CHECKED
        AppendMenu(viewSubMenu, checked, CMD.DETAILS, "Details")
        checked = MFT.BYCOMMAND

        AppendMenu(comContextMenu, MFT.SEPARATOR, 0, String.Empty)
        AppendMenu(comContextMenu, MFT.BYCOMMAND, CMD.REFRESH, "Refresh (F5)")
        AppendMenu(comContextMenu, MFT.BYCOMMAND, CMD.SELECT_ALL, "Select All (Ctrl+A)")
        AppendMenu(comContextMenu, MFT.SEPARATOR, 0, String.Empty)

        Dim enabled As Integer = MFT.GRAYED
        Dim effects As DragDropEffects
        If LastSelectedCSI Is Nothing Then
            enabled = MFT.BYCOMMAND
        Else
            effects = ShellHelper.CanDropClipboard(LastSelectedCSI)
            If ((effects And DragDropEffects.Copy) = DragDropEffects.Copy) Or
                    ((effects And DragDropEffects.Move) = DragDropEffects.Move) Then ' Enable paste for stand-alone ExpList
                enabled = MFT.BYCOMMAND
            End If
        End If
        AppendMenu(comContextMenu, enabled, CMD.PASTE, "Paste (Ctrl+V)")

        If LastSelectedCSI IsNot Nothing Then
            enabled = MFT.GRAYED
            If ((effects And DragDropEffects.Link) = DragDropEffects.Link) Then
                enabled = MFT.BYCOMMAND
            End If

            AppendMenu(comContextMenu, enabled, CMD.PASTELINK, "Paste Link")
            AppendMenu(comContextMenu, MFT.SEPARATOR, 0, String.Empty)

            ' Add the 'New' menu
            If LastSelectedCSI.IsFolder And
                ((Not LastSelectedCSI.Path.StartsWith("::")) Or (LastSelectedCSI Is CShItem.GetDeskTop)) Then
                Dim xIndex As Integer = GetMenuItemCount(comContextMenu)
                m_WindowsContextMenu.SetUpNewMenu(LastSelectedCSI, comContextMenu, xIndex) ' 6) ' 7)
                AppendMenu(comContextMenu, MFT.SEPARATOR, 0, String.Empty)
            End If
            AppendMenu(comContextMenu, MFT.BYCOMMAND, CMD.PROPERTIES, "Properties")
        End If

        Dim cmdID As Integer =
            TrackPopupMenuEx(comContextMenu, TPM.RETURNCMD,
            pt.X, pt.Y, Me.Handle, IntPtr.Zero)


        If cmdID >= min Then
            cmi = New CMInvokeCommandInfoEx
            cmi.cbSize = Marshal.SizeOf(cmi)
            cmi.nShow = SW.SHOWNORMAL
            cmi.fMask = CMIC.UNICODE Or CMIC.PTINVOKE
            cmi.ptInvoke = New Drawing.Point(pt.X, pt.Y)

            Select Case cmdID
                Case CMD.TILES
                    ExpFileList.View = View.Tile
                    GoTo CLEANUP
                Case CMD.LARGEICON
                    ExpFileList.View = View.LargeIcon
                    GoTo CLEANUP
                Case CMD.LIST
                    ExpFileList.View = View.List
                    GoTo CLEANUP
                Case CMD.DETAILS
                    ExpFileList.View = View.Details
                    GoTo CLEANUP
                    'Case CMD.THUMBNAILS
                    '    ExpFileList.View = View.Thumbnail
                    '    GoTo CLEANUP
                Case CMD.REFRESH
                    If LastSelectedCSI IsNot Nothing Then
                        LastSelectedCSI.UpdateRefresh()
                    End If
                    SortLVItems()
                    GoTo CLEANUP
                Case CMD.SELECT_ALL  'LuKai 2019.05.18
                    For Each item As ListViewItem In ExpFileList.Items
                        item.Selected = True
                    Next
                    GoTo CLEANUP
                Case CMD.PASTE
                    If LastSelectedCSI IsNot Nothing Then
                        cmi.lpVerb = Marshal.StringToHGlobalAnsi("paste")
                        cmi.lpVerbW = Marshal.StringToHGlobalUni("paste")
                    Else
                        GoTo CLEANUP
                    End If
                Case CMD.PASTELINK
                    cmi.lpVerb = Marshal.StringToHGlobalAnsi("pastelink")
                    cmi.lpVerbW = Marshal.StringToHGlobalUni("pastelink")
                Case CMD.PROPERTIES
                    cmi.lpVerb = Marshal.StringToHGlobalAnsi("properties")
                    cmi.lpVerbW = Marshal.StringToHGlobalUni("properties")
                Case Else
                    If CShItem.IsVista Then cmdID -= 1 '12/15/2010 Change
                    cmi.lpVerb = CType(cmdID, IntPtr)
                    cmi.lpVerbW = CType(cmdID, IntPtr)
                    m_CreateNew = True
                    HR = m_WindowsContextMenu.newMenu.InvokeCommand(cmi)
#If DEBUG Then
                    If HR <> S_OK Then
                        Marshal.ThrowExceptionForHR(HR)
                    End If
#End If

                    GoTo CLEANUP
            End Select

            ' Invoke the Paste, Paste Shortcut or Properties command
            If LastSelectedCSI IsNot Nothing Then
                Dim prgf As Integer = 0
                Dim iunk As IntPtr = IntPtr.Zero
                Dim folder As ShellDll.IShellFolder = Nothing
                If LastSelectedCSI Is CShItem.GetDeskTop Then
                    folder = LastSelectedCSI.Folder
                Else
                    folder = LastSelectedCSI.Parent.Folder
                End If

                Dim relPidl As IntPtr = CShItem.ILFindLastID(LastSelectedCSI.PIDL)
                HR = folder.GetUIObjectOf(IntPtr.Zero, 1, New IntPtr() {relPidl}, IID_IContextMenu, prgf, iunk)
#If DEBUG Then
                If Not HR = S_OK Then
                    Marshal.ThrowExceptionForHR(HR)
                End If
#End If

                m_WindowsContextMenu.winMenu = CType(Marshal.GetObjectForIUnknown(iunk), IContextMenu)
                HR = m_WindowsContextMenu.winMenu.InvokeCommand(cmi)
                m_WindowsContextMenu.ReleaseMenu()

#If DEBUG Then
                If Not HR = S_OK Then
                    Marshal.ThrowExceptionForHR(HR)
                End If
#End If
            End If
        End If      '12/15/2010 change
CLEANUP:
        m_WindowsContextMenu.ReleaseNewMenu()

        Marshal.Release(comContextMenu)
        comContextMenu = IntPtr.Zero
        Marshal.Release(viewSubMenu)
        viewSubMenu = IntPtr.Zero

    End Sub

#End Region

    ''' <summary>
    ''' Handles Windows Messages having to do with the display of Cascading menus of the Context Menu.
    ''' </summary>
    ''' <param name="m">The Windows Message</param>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        'For send to menu in the ListView context menu
        Dim hr As Integer
        If m.Msg = WM.INITMENUPOPUP Or m.Msg = WM.MEASUREITEM Or m.Msg = WM.DRAWITEM Then
            If m_WindowsContextMenu.winMenu2 IsNot Nothing Then
                hr = m_WindowsContextMenu.winMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam)
                If hr = 0 Then
                    Return
                End If
            ElseIf (m.Msg = WM.INITMENUPOPUP And m.WParam = m_WindowsContextMenu.newMenuPtr) _
                    Or m.Msg = WM.MEASUREITEM Or m.Msg = WM.DRAWITEM Then
                If m_WindowsContextMenu.newMenu2 IsNot Nothing Then
                    hr = m_WindowsContextMenu.newMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam)
                    If hr = 0 Then
                        Return
                    End If
                End If
            End If
        ElseIf m.Msg = WM.MENUCHAR Then
            If m_WindowsContextMenu.winMenu3 IsNot Nothing Then
                hr = m_WindowsContextMenu.winMenu3.HandleMenuMsg2(m.Msg, m.WParam, m.LParam, IntPtr.Zero)
                If hr = 0 Then
                    Return
                End If
            End If
        End If
        MyBase.WndProc(m)
    End Sub
#End Region

#Region "   Keyboard Events "
    ''' <summary>
    ''' Handles Delete Key processing for the ListView
    ''' </summary>
    ''' <param name="sender">object that raised the event</param>
    ''' <param name="e">a KeyEventsArgs</param>
    ''' <remarks>Modified version of Calum McLellan's code from ExpList.</remarks>
    Private Sub ExpList_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles ExpFileList.KeyUp
        'Use key up/down/left/right to preview pictures
        If e.KeyCode = Keys.Up Or e.KeyCode = Keys.Down Or e.KeyCode = Keys.Left Or e.KeyCode = Keys.Right Then
            Dim csi As CShItem = ExpFileList.SelectedItems(0).Tag
            Dim strPath As String = csi.Path
            RaiseEvent ExpListItemArrowKeyUp(strPath, csi)
        End If
    End Sub

    Private Sub ExpFileList_KeyDown(sender As Object, e As KeyEventArgs) Handles ExpFileList.KeyDown
        'Ctrl+A to select all items. Have to use KeyDown, not KeyUp
        If e.Control AndAlso e.KeyCode = Keys.A Then
            For Each item As ListViewItem In ExpFileList.Items
                item.Selected = True
            Next
            'Get selected items after press Ctrl+A
            RaiseEvent ExpListItemGetSelItems(ExpFileList.SelectedItems)
        End If
        'Ctrl+X, Ctrl+C, Ctrl+V, Ctrl+Z
        If e.Control Then
            Select Case e.KeyCode
                Case = Keys.X
                    WinMenu("cut")
                Case = Keys.C
                    WinMenu("copy")
                Case = Keys.V
                    WinMenu("paste")
                Case = Keys.Z
                    MsgBox("Don't support UNDO now!", MsgBoxStyle.Information)
            End Select
        End If

        'Use Delete key to delete files
        If e.KeyCode = Keys.Delete Then
            WinMenu("delete")
            If ExpFileList.SelectedItems.Count > 150 Then  '假定所删文件数大于150时，则需手动刷新。
                LastSelectedCSI.UpdateRefresh()
            End If
        End If

        'Use F2 to rename
        If e.KeyCode = Keys.F2 Then
            If ExpFileList.SelectedItems.Count > 0 Then
                ExpFileList.SelectedItems(0).BeginEdit()
            End If
        End If

        'Use F5 to refresh
        If e.KeyCode = Keys.F5 Then
            If LastSelectedCSI IsNot Nothing Then
                LastSelectedCSI.UpdateRefresh()
            End If
            SortLVItems()
        End If

        'Use Enter Key to open file/foler by default application
        If e.KeyCode = Keys.Enter Then
            If ExpFileList.SelectedItems.Count > 0 Then
                Dim name As String = ExpFileList.SelectedItems(0).Text
                Dim csi As CShItem = ExpFileList.SelectedItems(0).Tag
                If csi.IsFolder Then
                    If csi.Path.StartsWith(":") Then
                        RaiseEvent ExpListItemDoubleClick(csi.DisplayName, csi)
                    Else
                        RaiseEvent ExpListItemDoubleClick(csi.Path, csi)
                    End If
                Else
                    Dim path As String = csi.Path
                    Try
                        If name = IO.Path.GetFileName(path) Then  'When rename, the display file name will be different with it's path
                            Process.Start(csi.Path)
                        End If
                    Catch ex As Exception
                        MsgBox(ex.Message, MsgBoxStyle.OkOnly, "Error in starting application")
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub WinMenu(ByVal cmd As String)
        If LastSelectedCSI IsNot Nothing AndAlso LastSelectedCSI.IsFolder Then
            Dim HR As Integer
            Dim prgf As Integer = 0
            Dim iunk As IntPtr = IntPtr.Zero
            Dim folder As ShellDll.IShellFolder = Nothing

            If cmd = "paste" Then
                If LastSelectedCSI Is CShItem.GetDeskTop Then
                    folder = LastSelectedCSI.Folder
                Else
                    folder = LastSelectedCSI.Parent.Folder
                End If
                Dim relPidl As IntPtr = CShItem.ILFindLastID(LastSelectedCSI.PIDL)
                HR = folder.GetUIObjectOf(IntPtr.Zero, 1, New IntPtr() {relPidl}, IID_IContextMenu, prgf, iunk)
            Else  'delete, cut, copy
                If ExpFileList.SelectedItems.Count <= 0 Then
                    Return
                End If

                folder = LastSelectedCSI.Folder
                Dim pidls(ExpFileList.SelectedItems.Count - 1) As IntPtr
                For i As Integer = 0 To ExpFileList.SelectedItems.Count - 1
                    If Not DirectCast(ExpFileList.SelectedItems(i).Tag, CShItem).CanDelete Then
                        MsgBox("Cannot Delete: " & DirectCast(ExpFileList.SelectedItems(i).Tag, CShItem).DisplayName, MsgBoxStyle.OkOnly, "Cannot Delete")
                        Exit Sub
                    End If
                    'If Not ExpFileList.SelectedItems(i).Tag.CanRename Then AllowRename = False
                    pidls(i) = CShItem.ILFindLastID(ExpFileList.SelectedItems(i).Tag.PIDL)
                Next
                HR = folder.GetUIObjectOf(IntPtr.Zero, pidls.Length, pidls, IID_IContextMenu, prgf, iunk)
            End If
#If DEBUG Then
                If Not HR = S_OK Then
                    Marshal.ThrowExceptionForHR(HR)
                End If
#End If
            m_WindowsContextMenu.winMenu = CType(Marshal.GetObjectForIUnknown(iunk), IContextMenu)
            Dim cmi As New CMInvokeCommandInfoEx
            cmi.cbSize = Marshal.SizeOf(cmi)
            cmi.nShow = SW.SHOWNORMAL
            cmi.fMask = CMIC.UNICODE Or CMIC.PTINVOKE
            cmi.ptInvoke = New Drawing.Point(0, 0)
            cmi.lpVerb = Marshal.StringToHGlobalAnsi(cmd)
            cmi.lpVerbW = Marshal.StringToHGlobalUni(cmd)

            HR = m_WindowsContextMenu.winMenu.InvokeCommand(cmi)
            m_WindowsContextMenu.ReleaseMenu()
#If DEBUG Then
                If Not HR = S_OK Then
                    Marshal.ThrowExceptionForHR(HR)
                End If
#End If
            'Else
            '    Dim itm As ListViewItem
            '    For Each itm In ExpFileList.SelectedItems
            '        m_items.Remove(itm)
            '    Next
        End If
    End Sub
#End Region

End Class
