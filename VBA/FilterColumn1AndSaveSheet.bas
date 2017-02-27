Attribute VB_Name = "Module2"
Sub FilterColumn1AndSaveSheet()

    Application.ScreenUpdating = False
    Application.DisplayAlerts = False

    Dim i As Long
    
    Dim dic As Object
    Set dic = CreateObject("scripting.dictionary")
    
    For i = 2 To ActiveSheet.UsedRange.Rows.count
        If Not IsEmpty(Cells(i, 1)) Then
            If Not dic.exists(Cells(i, 1).Value) Then
                dic.Add Cells(i, 1).Value, Union(ActiveSheet.Rows(i), ActiveSheet.Rows(1))
            Else
                Set dic.Item(Cells(i, 1).Value) = Union(dic.Item(Cells(i, 1).Value), ActiveSheet.Rows(i))
            End If
        End If
    Next
    
    
    Dim fileSystem As Object
    Set fileSystem = CreateObject("Scripting.FileSystemObject")

    Dim fileName As String, folderPath As String
    folderPath = ActiveWorkbook.Path & "\" & fileSystem.GetBaseName(ActiveWorkbook.Name) & "\"
    
    If fileSystem.folderexists(folderPath) Then
        Dim returnValue As Integer
        returnValue = MsgBox("Overwrite existing folder?", vbOKCancel, "Caution!")
        If returnValue = vbCancel Then
            Exit Sub
        End If
    Else
        fileSystem.CreateFolder (folderPath)
    End If

    
    Application.DisplayAlerts = False
    Dim r1 As range
    
    For Each k In dic.keys
        Set r1 = dic.Item(k)
        Debug.Print "hh"; r1.Areas.count
        r1.Select
        Selection.Copy
        Workbooks.Add
        ActiveSheet.Paste
        Application.CutCopyMode = False
        ActiveWorkbook.SaveAs fileName:=folderPath & k
        ActiveWindow.Close
    Next
    
    Application.ScreenUpdating = True
    Application.DisplayAlerts = True
    

End Sub
