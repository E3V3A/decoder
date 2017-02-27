Attribute VB_Name = "SaveSheet"
Sub SaveSheet()

    On Error Resume Next
    
    Dim fileSystem As Object
    Set fileSystem = CreateObject("Scripting.FileSystemObject")

    Dim fileName As String, folderPath As String
    fileName = ActiveWorkbook.Name
    folderPath = ActiveWorkbook.Path & "\" & fileSystem.GetBaseName(fileName) & "-Saved"
    
    If fileSystem.folderexists(folderPath) Then
        Dim returnValue As Integer
        returnValue = MsgBox("???t?D¨°?¡ä??¨²¡ê?¨º?¡¤??¨¹D??¨²¨¨Y¡ê?", vbOKCancel, "Caution!")
        If returnValue = 2 Then Exit Sub
    Else
        fileSystem.CreateFolder (folderPath)
    End If

    Application.ScreenUpdating = False
    Application.DisplayAlerts = False

    Dim wkSht As Worksheet
    For Each wkSht In ActiveWorkbook.Worksheets
        wkSht.UsedRange.Copy
        wkSht.UsedRange.PasteSpecial Paste:=xlPasteValuesAndNumberFormats, Operation:= _
            xlNone, SkipBlanks:=False, Transpose:=False
        
        Set newbook = Workbooks.Add
        wkSht.Copy before:=newbook.Sheets(1)
        
        newbook.SaveAs folderPath & "\" & wkSht.Name
        newbook.Close
    Next

    Application.DisplayAlerts = True
    Application.ScreenUpdating = True
End Sub


