Imports System.Runtime.InteropServices
Imports System.Drawing.Drawing2D

Imports System.Net.WebClient
Imports System.IO
Imports System.Net

Public Class Form1

    Dim Version As String = "0.93 CNS Community"

#Region "Declarations"

    Dim SizeFromOpen As Size = New Size(300, 300)

    Public CloseForUpdate As Boolean = False

    Dim ToolBrushPattern As HatchStyle = HatchStyle.BackwardDiagonal

    Dim ResizeW As Boolean = False
    Dim ResizeH As Boolean = False

    Dim CustomColorIndex As Integer = 0

    Dim LeftIsDown As Boolean = False
    Dim RightIsDown As Boolean = False

    Dim StartImage As Image

    Dim PrevPos As Point
    Dim StartPos As Point

    Dim FormStartCurPos As Point
    Dim FormStartPos As Point


    Dim ColorUpdate As Boolean = False

    Dim Filename As String = ""
    Dim Saved As Boolean = True
#End Region
#Region "Form Border"
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure MARGINS
        Public cxLeftWidth As Integer
        Public cxRightWidth As Integer
        Public cyTopHeight As Integer
        Public cyBottomHeight As Integer
    End Structure
    <DllImport("dwmapi.dll")> Public Shared Function DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef pMarinset As MARGINS) As Integer
    End Function

    'Form Moving on extended border:
    Private Sub PanelTabs_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PanelTabs.MouseDown
        If e.Button.ToString = "Left" Then
            FormStartCurPos = Cursor.Position
            FormStartPos = Me.Location
            TimerForm.Enabled = True
        End If
    End Sub
    Private Sub PanelTabs_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PanelTabs.MouseUp
        TimerForm.Enabled = False
    End Sub
    Private Sub TimerForm_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimerForm.Tick
        Me.Location = New Point(FormStartPos.X - (FormStartCurPos.X - Cursor.Position.X), FormStartPos.Y - (FormStartCurPos.Y - Cursor.Position.Y))
    End Sub
#End Region

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not CloseForUpdate Then

            If Saved = False Then
                Dim Question As String
                If Filename = "" Then Question = "Untitled" Else Question = FilenameFullToShort(Filename)

                Select Case MsgBox("Do You Want to Save Changes to " & Question & "?", MsgBoxStyle.YesNoCancel, "Mega Paint")
                    Case MsgBoxResult.Yes
                        FileSave()
                    Case MsgBoxResult.No
                    Case MsgBoxResult.Cancel
                        e.Cancel = True
                End Select
            End If

        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LabelVersion.Text = "v" & Version

        ComboBoxShape.SelectedIndex = 0
        BrushToolPattern.SelectedIndex = 0

        UpdateBrushToolPreview()

        PanelPencilSettings.Visible = True

        FontDialog1.Font = TextBox1.Font

        Dim margins As MARGINS = New MARGINS
        margins.cxLeftWidth = 0
        margins.cxRightWidth = 0
        margins.cyTopHeight = 23
        margins.cyBottomHeight = 0
        Dim hwnd As IntPtr = Handle
        Dim result As Integer = DwmExtendFrameIntoClientArea(hwnd, margins)

        PictureBox1.Image = New Bitmap(PictureBox1.Width, PictureBox1.Height)
        Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
        gfx.Clear(Color.FromArgb(255, 255, 255, 255))
        PictureBox1.Refresh()

        MainTabs.SelectedTab = TabHome

        'Pencil Preview
        PencilSizePreview.Image = New Bitmap(PencilSizePreview.Width, PencilSizePreview.Height)
        PictureBox1.Refresh()
        Dim gfx2 As Graphics = Graphics.FromImage(PencilSizePreview.Image)
        gfx2.Clear(Color.FromArgb(0, 0, 0, 0))
        If RadioButtonPencilCircle.Checked Then
            gfx2.FillEllipse(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        Else
            gfx2.FillRectangle(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        End If
        PencilSizePreview.Refresh()

        'Line Preview
        LineSizePreview.Image = New Bitmap(LineSizePreview.Width, LineSizePreview.Height)
        LineSizePreview.Refresh()
        Dim gfx3 As Graphics = Graphics.FromImage(LineSizePreview.Image)
        gfx3.Clear(Color.FromArgb(0, 0, 0, 0))
        gfx3.FillEllipse(New SolidBrush(ColorLeft.BackColor), (LineSizePreview.Width - NumericUpDownLineSize.Value) / 2, (LineSizePreview.Height - NumericUpDownLineSize.Value) / 2, NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
        LineSizePreview.Refresh()

    End Sub

    Private Sub NumericUpDownCanvasWidth_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownCanvasWidth.ValueChanged

        If ResizeW = True Then
            ResizeW = False
        Else



            NumericUpDownResizeWidth.Value = NumericUpDownCanvasWidth.Value
            NumericUpDownResizeHeight.Value = NumericUpDownCanvasHeight.Value

            Try
                Dim NewSize As Size
                NewSize.Width = PictureBox1.Width + (NumericUpDownCanvasWidth.Value - PictureBox1.Width)
                NewSize.Height = PictureBox1.Height + (NumericUpDownCanvasHeight.Value - PictureBox1.Height)

                Dim bm As Bitmap = New Bitmap(NewSize.Width, NewSize.Height)
                Dim gfx As Graphics = Graphics.FromImage(bm)
                gfx.Clear(ColorRight.BackColor)

                gfx.DrawImage(PictureBox1.Image, 0, 0)

                PictureBox1.Image = bm
                PictureBox1.Refresh()

                PictureBox1.Width = NumericUpDownCanvasWidth.Value
            Catch ex As Exception
            End Try

            Try
                If PictureBox1.Image.Size <> SizeFromOpen Then
                    Saved = False
                End If
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub NumericUpDownCanvasHeight_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownCanvasHeight.ValueChanged
        If ResizeH = True Then
            ResizeH = False
        Else



            NumericUpDownResizeWidth.Value = NumericUpDownCanvasWidth.Value
            NumericUpDownResizeHeight.Value = NumericUpDownCanvasHeight.Value

            Try
                Dim NewSize As Size
                NewSize.Width = PictureBox1.Width + (NumericUpDownCanvasWidth.Value - PictureBox1.Width)
                NewSize.Height = PictureBox1.Height + (NumericUpDownCanvasHeight.Value - PictureBox1.Height)

                Dim bm As Bitmap = New Bitmap(NewSize.Width, NewSize.Height)
                Dim gfx As Graphics = Graphics.FromImage(bm)
                gfx.Clear(ColorRight.BackColor)

                gfx.DrawImage(PictureBox1.Image, 0, 0)

                PictureBox1.Image = bm
                PictureBox1.Refresh()

                PictureBox1.Height = NumericUpDownCanvasHeight.Value
            Catch ex As Exception
            End Try

            Try
                If PictureBox1.Image.Size <> SizeFromOpen Then
                    Saved = False
                End If
            Catch ex As Exception
            End Try
        End If
    End Sub

#Region "Colors"
    Private Sub ButtonMoreColors_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMoreColors.Click
        If ColorLeft.Checked Then
            ColorDialog1.Color = ColorLeft.BackColor
        Else
            ColorDialog1.Color = ColorRight.BackColor
        End If

        If ColorDialog1.ShowDialog() = DialogResult.OK Then

            If ColorLeft.Checked Then
                ColorLeft.BackColor = ColorDialog1.Color
            Else
                ColorRight.BackColor = ColorDialog1.Color
            End If

            If Not (C25.BackColor = ColorDialog1.Color Or C26.BackColor = ColorDialog1.Color Or C27.BackColor = ColorDialog1.Color Or C28.BackColor = ColorDialog1.Color Or C29.BackColor = ColorDialog1.Color Or C30.BackColor = ColorDialog1.Color Or C31.BackColor = ColorDialog1.Color Or C32.BackColor = ColorDialog1.Color Or C33.BackColor = ColorDialog1.Color Or C34.BackColor = ColorDialog1.Color Or C35.BackColor = ColorDialog1.Color Or C36.BackColor = ColorDialog1.Color) Then
                Select Case CustomColorIndex
                    Case 0
                        C25.Enabled = True
                        C25.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 1
                        C26.Enabled = True
                        C26.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 2
                        C27.Enabled = True
                        C27.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 3
                        C28.Enabled = True
                        C28.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 4
                        C29.Enabled = True
                        C29.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 5
                        C30.Enabled = True
                        C30.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 6
                        C31.Enabled = True
                        C31.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 7
                        C32.Enabled = True
                        C32.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 8
                        C33.Enabled = True
                        C33.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 9
                        C34.Enabled = True
                        C34.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 10
                        C35.Enabled = True
                        C35.BackColor = ColorDialog1.Color
                        CustomColorIndex += 1
                    Case 11
                        C36.Enabled = True
                        C36.BackColor = ColorDialog1.Color
                        CustomColorIndex = 0
                End Select
            End If
        End If

    End Sub

    Private Sub C1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C1.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C1.BackColor
        Else
            ColorRight.BackColor = C1.BackColor
        End If
    End Sub

    Private Sub C2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C2.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C2.BackColor
        Else
            ColorRight.BackColor = C2.BackColor
        End If
    End Sub

    Private Sub C3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C3.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C3.BackColor
        Else
            ColorRight.BackColor = C3.BackColor
        End If
    End Sub

    Private Sub C4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C4.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C4.BackColor
        Else
            ColorRight.BackColor = C4.BackColor
        End If
    End Sub

    Private Sub C5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C5.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C5.BackColor
        Else
            ColorRight.BackColor = C5.BackColor
        End If
    End Sub

    Private Sub C6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C6.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C6.BackColor
        Else
            ColorRight.BackColor = C6.BackColor
        End If
    End Sub

    Private Sub C7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C7.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C7.BackColor
        Else
            ColorRight.BackColor = C7.BackColor
        End If
    End Sub

    Private Sub C8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C8.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C8.BackColor
        Else
            ColorRight.BackColor = C8.BackColor
        End If
    End Sub

    Private Sub C9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C9.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C9.BackColor
        Else
            ColorRight.BackColor = C9.BackColor
        End If
    End Sub

    Private Sub C10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C10.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C10.BackColor
        Else
            ColorRight.BackColor = C10.BackColor
        End If
    End Sub

    Private Sub C11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C11.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C11.BackColor
        Else
            ColorRight.BackColor = C11.BackColor
        End If
    End Sub

    Private Sub C12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C12.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C12.BackColor
        Else
            ColorRight.BackColor = C12.BackColor
        End If
    End Sub

    Private Sub C13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C13.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C13.BackColor
        Else
            ColorRight.BackColor = C13.BackColor
        End If
    End Sub

    Private Sub C14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C14.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C14.BackColor
        Else
            ColorRight.BackColor = C14.BackColor
        End If
    End Sub

    Private Sub C15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C15.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C15.BackColor
        Else
            ColorRight.BackColor = C15.BackColor
        End If
    End Sub

    Private Sub C16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C16.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C16.BackColor
        Else
            ColorRight.BackColor = C16.BackColor
        End If
    End Sub

    Private Sub C17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C17.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C17.BackColor
        Else
            ColorRight.BackColor = C17.BackColor
        End If
    End Sub

    Private Sub C18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C18.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C18.BackColor
        Else
            ColorRight.BackColor = C18.BackColor
        End If
    End Sub

    Private Sub C19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C19.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C19.BackColor
        Else
            ColorRight.BackColor = C19.BackColor
        End If
    End Sub

    Private Sub C20_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C20.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C20.BackColor
        Else
            ColorRight.BackColor = C20.BackColor
        End If
    End Sub

    Private Sub C21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C21.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C21.BackColor
        Else
            ColorRight.BackColor = C21.BackColor
        End If
    End Sub

    Private Sub C22_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C22.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C22.BackColor
        Else
            ColorRight.BackColor = C22.BackColor
        End If
    End Sub

    Private Sub C23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C23.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C23.BackColor
        Else
            ColorRight.BackColor = C23.BackColor
        End If
    End Sub

    Private Sub C24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C24.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C24.BackColor
        Else
            ColorRight.BackColor = C24.BackColor
        End If
    End Sub

    Private Sub C25_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C25.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C25.BackColor
        Else
            ColorRight.BackColor = C25.BackColor
        End If
    End Sub

    Private Sub C26_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C26.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C26.BackColor
        Else
            ColorRight.BackColor = C26.BackColor
        End If
    End Sub

    Private Sub C27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C27.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C27.BackColor
        Else
            ColorRight.BackColor = C27.BackColor
        End If
    End Sub

    Private Sub C28_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C28.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C28.BackColor
        Else
            ColorRight.BackColor = C28.BackColor
        End If
    End Sub

    Private Sub C29_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C29.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C29.BackColor
        Else
            ColorRight.BackColor = C29.BackColor
        End If
    End Sub

    Private Sub C30_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C30.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C30.BackColor
        Else
            ColorRight.BackColor = C30.BackColor
        End If
    End Sub

    Private Sub C31_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C31.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C31.BackColor
        Else
            ColorRight.BackColor = C31.BackColor
        End If
    End Sub

    Private Sub C32_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C32.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C32.BackColor
        Else
            ColorRight.BackColor = C32.BackColor
        End If
    End Sub

    Private Sub C33_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C33.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C33.BackColor
        Else
            ColorRight.BackColor = C33.BackColor
        End If
    End Sub

    Private Sub C34_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C34.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C34.BackColor
        Else
            ColorRight.BackColor = C34.BackColor
        End If
    End Sub

    Private Sub C35_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C35.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C35.BackColor
        Else
            ColorRight.BackColor = C35.BackColor
        End If
    End Sub

    Private Sub C36_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C36.Click
        If ColorLeft.Checked Then
            ColorLeft.BackColor = C36.BackColor
        Else
            ColorRight.BackColor = C36.BackColor
        End If
    End Sub






#End Region
#Region "Draw"
    Private Sub PictureBox1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDown

        Saved = False
        StartImage = New Bitmap(PictureBox1.Image)

        If e.Button = Windows.Forms.MouseButtons.Left And Not RightIsDown Then
            'Left

            If ToolPencil.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorLeft.BackColor = bm.GetPixel(NP.X, NP.Y)

            ElseIf ToolText.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorLeft.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolDot.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorLeft.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, e.Location, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolShape.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorLeft.BackColor, NumericUpDownShapeSize.Value)

                gfx.DrawRectangle(Pen, e.X, e.Y, e.X - StartPos.X, e.Y - StartPos.Y)

            End If

            LeftIsDown = True
        ElseIf e.Button = Windows.Forms.MouseButtons.Right And Not LeftIsDown Then
            'Right

            If ToolPencil.Checked And LeftIsDown = False Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorRight.BackColor = bm.GetPixel(NP.X, NP.Y)
            ElseIf ToolText.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorRight.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolText.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, TextBox1.Font, New SolidBrush(ColorRight.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolDot.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, e.Location, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolShape.Checked Then
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownShapeSize.Value)

                gfx.DrawRectangle(Pen, e.X, e.Y, e.X - StartPos.X, e.Y - StartPos.Y)

            End If

            RightIsDown = True
        End If
        StartPos = e.Location
        PrevPos = e.Location
    End Sub
    Private Sub PictureBox1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseMove
        If e.Button = Windows.Forms.MouseButtons.Left And Not RightIsDown Then
            'Left

            If ToolPencil.Checked Then
                Dim Pencil As Pen = New Pen(ColorLeft.BackColor, NumericUpDownPencilSize.Value)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawLine(Pencil, PrevPos, e.Location)

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), PrevPos.X - (NumericUpDownPencilSize.Value / 2), PrevPos.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorLeft.BackColor), PrevPos.X - (NumericUpDownPencilSize.Value / 2), PrevPos.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorLeft.BackColor = bm.GetPixel(NP.X, NP.Y)
            ElseIf ToolText.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorLeft.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorLeft.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, StartPos, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
                gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), StartPos.X - (NumericUpDownLineSize.Value / 2), StartPos.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolDot.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolShape.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorLeft.BackColor, NumericUpDownShapeSize.Value)

                Select Case ComboBoxShape.Text
                    Case "Rectangle"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawRectangle(Pen, Rect)
                    Case "Ellipse"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawEllipse(Pen, Rect)
                End Select
            End If

        ElseIf e.Button = Windows.Forms.MouseButtons.Right And Not LeftIsDown Then
            'Right

            If ToolPencil.Checked And LeftIsDown = False Then
                Dim Pencil As Pen = New Pen(ColorRight.BackColor, NumericUpDownPencilSize.Value)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawLine(Pencil, PrevPos, e.Location)

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownPencilSize.Value / 2), e.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                If RadioButtonPencilCircle.Checked Then
                    gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), PrevPos.X - (NumericUpDownPencilSize.Value / 2), PrevPos.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                Else
                    gfx.FillRectangle(New SolidBrush(ColorRight.BackColor), PrevPos.X - (NumericUpDownPencilSize.Value / 2), PrevPos.Y - (NumericUpDownPencilSize.Value / 2), NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorRight.BackColor = bm.GetPixel(NP.X, NP.Y)
            ElseIf ToolText.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorRight.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, StartPos, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
                gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), StartPos.X - (NumericUpDownLineSize.Value / 2), StartPos.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolDot.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolShape.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownShapeSize.Value)

                Select Case ComboBoxShape.Text
                    Case "Rectangle"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawRectangle(Pen, Rect)
                    Case "Ellipse"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawEllipse(Pen, Rect)
                End Select
            End If

        End If
        PrevPos = e.Location
    End Sub
    Private Sub PictureBox1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp

        If e.Button = Windows.Forms.MouseButtons.Left And Not RightIsDown Then
            'Left

            If ToolPencil.Checked Then
                Dim Pencil As Pen = New Pen(ColorLeft.BackColor)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                gfx.DrawLine(Pencil, PrevPos, e.Location)

                PictureBox1.Refresh()

            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorLeft.BackColor = bm.GetPixel(NP.X, NP.Y)

                If Not (C25.BackColor = bm.GetPixel(NP.X, NP.Y) Or C26.BackColor = bm.GetPixel(NP.X, NP.Y) Or C27.BackColor = bm.GetPixel(NP.X, NP.Y) Or C28.BackColor = bm.GetPixel(NP.X, NP.Y) Or C29.BackColor = bm.GetPixel(NP.X, NP.Y) Or C30.BackColor = bm.GetPixel(NP.X, NP.Y) Or C31.BackColor = bm.GetPixel(NP.X, NP.Y) Or C32.BackColor = bm.GetPixel(NP.X, NP.Y) Or C33.BackColor = bm.GetPixel(NP.X, NP.Y) Or C34.BackColor = bm.GetPixel(NP.X, NP.Y) Or C35.BackColor = bm.GetPixel(NP.X, NP.Y) Or C36.BackColor = ColorDialog1.Color) Then
                    Select Case CustomColorIndex
                        Case 0
                            C25.Enabled = True
                            C25.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 1
                            C26.Enabled = True
                            C26.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 2
                            C27.Enabled = True
                            C27.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 3
                            C28.Enabled = True
                            C28.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 4
                            C29.Enabled = True
                            C29.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 5
                            C30.Enabled = True
                            C30.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 6
                            C31.Enabled = True
                            C31.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 7
                            C32.Enabled = True
                            C32.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 8
                            C33.Enabled = True
                            C33.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 9
                            C34.Enabled = True
                            C34.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 10
                            C35.Enabled = True
                            C35.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 11
                            C36.Enabled = True
                            C36.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex = 0
                    End Select
                End If
            ElseIf ToolText.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorLeft.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorLeft.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, StartPos, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
                gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), StartPos.X - (NumericUpDownLineSize.Value / 2), StartPos.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolDot.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolFill.Checked Then
                Me.Cursor = Cursors.WaitCursor
                Dim bm As Bitmap = PictureBox1.Image
                Fill(bm, e.X, e.Y, ColorLeft.BackColor)
                PictureBox1.Image = bm
                Me.Cursor = Cursors.Default
            End If

            LeftIsDown = False
        ElseIf e.Button = Windows.Forms.MouseButtons.Right And Not LeftIsDown Then
            'Right

            If ToolPencil.Checked And LeftIsDown = False Then
                Dim Pencil As Pen = New Pen(ColorRight.BackColor)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawLine(Pencil, PrevPos, e.Location)
                PictureBox1.Refresh()
            ElseIf ToolColor.Checked Then
                Dim bm As Bitmap = PictureBox1.Image

                Dim NP As Point = New Point(e.Location)

                If NP.X < 0 Then NP.X = 0
                If NP.X > PictureBox1.Width - 1 Then NP.X = PictureBox1.Width - 1
                If NP.Y < 0 Then NP.Y = 0
                If NP.Y > PictureBox1.Height - 1 Then NP.Y = PictureBox1.Height - 1

                ColorRight.BackColor = bm.GetPixel(NP.X, NP.Y)

                If Not (C25.BackColor = bm.GetPixel(NP.X, NP.Y) Or C26.BackColor = ColorDialog1.Color Or C27.BackColor = bm.GetPixel(NP.X, NP.Y) Or C28.BackColor = bm.GetPixel(NP.X, NP.Y) Or C29.BackColor = bm.GetPixel(NP.X, NP.Y) Or C30.BackColor = bm.GetPixel(NP.X, NP.Y) Or C31.BackColor = bm.GetPixel(NP.X, NP.Y) Or C32.BackColor = bm.GetPixel(NP.X, NP.Y) Or C33.BackColor = bm.GetPixel(NP.X, NP.Y) Or C34.BackColor = bm.GetPixel(NP.X, NP.Y) Or C35.BackColor = bm.GetPixel(NP.X, NP.Y) Or C36.BackColor = bm.GetPixel(NP.X, NP.Y)) Then
                    Select Case CustomColorIndex
                        Case 0
                            C25.Enabled = True
                            C25.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 1
                            C26.Enabled = True
                            C26.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 2
                            C27.Enabled = True
                            C27.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 3
                            C28.Enabled = True
                            C28.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 4
                            C29.Enabled = True
                            C29.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 5
                            C30.Enabled = True
                            C30.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 6
                            C31.Enabled = True
                            C31.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 7
                            C32.Enabled = True
                            C32.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 8
                            C33.Enabled = True
                            C33.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 9
                            C34.Enabled = True
                            C34.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 10
                            C35.Enabled = True
                            C35.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex += 1
                        Case 11
                            C36.Enabled = True
                            C36.BackColor = bm.GetPixel(NP.X, NP.Y)
                            CustomColorIndex = 0
                    End Select
                End If
            ElseIf ToolText.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                gfx.DrawString(TextBox1.Text, FontDialog1.Font, New SolidBrush(ColorRight.BackColor), e.X, e.Y)
                PictureBox1.Refresh()
            ElseIf ToolLine.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownLineSize.Value)

                gfx.DrawLine(Pen, StartPos, e.Location)
                gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), e.X - (NumericUpDownLineSize.Value / 2), e.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
                gfx.FillEllipse(New SolidBrush(ColorRight.BackColor), StartPos.X - (NumericUpDownLineSize.Value / 2), StartPos.Y - (NumericUpDownLineSize.Value / 2), NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)
            ElseIf ToolDot.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)
                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

                If RadioButtonBrushCircle.Checked Then
                    gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                Else
                    gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor), e.X - (NumericUpDownBrushSize.Value / 2), e.Y - (NumericUpDownBrushSize.Value / 2), NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
                End If

                PictureBox1.Refresh()
            ElseIf ToolFill.Checked Then
                Me.Cursor = Cursors.WaitCursor
                Dim bm As Bitmap = PictureBox1.Image
                Fill(bm, e.X, e.Y, ColorRight.BackColor)
                PictureBox1.Image = bm
                Me.Cursor = Cursors.Default
            ElseIf ToolShape.Checked Then
                PictureBox1.Image = New Bitmap(StartImage)

                Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)
                Dim Pen As Pen = New Pen(ColorRight.BackColor, NumericUpDownShapeSize.Value)

                Select Case ComboBoxShape.Text
                    Case "Rectangle"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawRectangle(Pen, Rect)
                    Case "Ellipse"
                        Dim Rect As Rectangle = New Rectangle(StartPos, New Size(e.X - StartPos.X, e.Y - StartPos.Y))
                        Dim ValChange As Integer = 0

                        Dim NewE As Integer = 0
                        Dim NewSize As Integer = 0

                        If e.X < StartPos.X Then
                            NewE = e.X - Rect.Width
                            NewSize = NewE - e.X

                            NewE = NewE - NewSize

                            Rect.X = NewE
                            Rect.Width = NewSize
                        End If

                        If e.Y < StartPos.Y Then
                            NewE = e.Y - Rect.Height
                            NewSize = NewE - e.Y

                            NewE = NewE - NewSize

                            Rect.Y = NewE
                            Rect.Height = NewSize
                        End If

                        gfx.DrawEllipse(Pen, Rect)
                End Select
            End If

            RightIsDown = False
        End If
    End Sub
#End Region
#Region "Tool Settings"

#Region "Pencil"
    Private Sub NumericUpDownPencilSize_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownPencilSize.ValueChanged
        PencilSizePreview.Image = New Bitmap(PencilSizePreview.Width, PencilSizePreview.Height)

        PencilSizePreview.Refresh()

        Dim gfx As Graphics = Graphics.FromImage(PencilSizePreview.Image)
        gfx.Clear(Color.FromArgb(0, 0, 0, 0))

        If RadioButtonPencilCircle.Checked Then
            gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        Else
            gfx.FillRectangle(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        End If

        PencilSizePreview.Refresh()
    End Sub
    Private Sub RadioButtonPencilCircle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonPencilCircle.CheckedChanged
        PencilSizePreview.Image = New Bitmap(PencilSizePreview.Width, PencilSizePreview.Height)

        PictureBox1.Refresh()

        Dim gfx As Graphics = Graphics.FromImage(PencilSizePreview.Image)
        gfx.Clear(Color.FromArgb(0, 0, 0, 0))

        If RadioButtonPencilCircle.Checked Then
            gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        Else
            gfx.FillRectangle(New SolidBrush(ColorLeft.BackColor), (PencilSizePreview.Width - NumericUpDownPencilSize.Value) / 2, (PencilSizePreview.Height - NumericUpDownPencilSize.Value) / 2, NumericUpDownPencilSize.Value, NumericUpDownPencilSize.Value)
        End If

        PencilSizePreview.Refresh()
    End Sub
#End Region
#Region "Dot"
    Sub UpdateBrushToolPreview()


        BrushSizePreview.Image = New Bitmap(BrushSizePreview.Width, BrushSizePreview.Height)

        BrushSizePreview.Refresh()

        Dim gfx As Graphics = Graphics.FromImage(BrushSizePreview.Image)
        gfx.Clear(Color.FromArgb(0, 0, 0, 0))

        Select Case BrushToolPattern.Text
            Case "Backward Diagonal"
                ToolBrushPattern = HatchStyle.BackwardDiagonal
            Case "Cross"
                ToolBrushPattern = HatchStyle.Cross
            Case "Dark Downward Diagonal"
                ToolBrushPattern = HatchStyle.WideDownwardDiagonal
            Case "Dark Horizontal"
                ToolBrushPattern = HatchStyle.DarkHorizontal
            Case "Dark Upward Diagonal"
                ToolBrushPattern = HatchStyle.DarkUpwardDiagonal
            Case "Dark Vertical"
                ToolBrushPattern = HatchStyle.DarkVertical
            Case "Dashed Downward Diagonal"
                ToolBrushPattern = HatchStyle.DashedDownwardDiagonal
            Case "Dashed Horizontal"
                ToolBrushPattern = HatchStyle.DashedHorizontal
            Case "Dashed Upward Diagonal"
                ToolBrushPattern = HatchStyle.DashedUpwardDiagonal
            Case "Dashed Vertical"
                ToolBrushPattern = HatchStyle.DashedVertical
            Case "Diagonal Brick"
                ToolBrushPattern = HatchStyle.DiagonalBrick
            Case "Diagonal Cross"
                ToolBrushPattern = HatchStyle.DiagonalCross
            Case "Divot"
                ToolBrushPattern = HatchStyle.Divot
            Case "Dotted Diamond"
                ToolBrushPattern = HatchStyle.DottedDiamond
            Case "Percent 10"
                ToolBrushPattern = HatchStyle.Percent10
            Case "Percent 20"
                ToolBrushPattern = HatchStyle.Percent20
            Case "Percent 30"
                ToolBrushPattern = HatchStyle.Percent30
            Case "Percent 40"
                ToolBrushPattern = HatchStyle.Percent40
            Case "Percent 50"
                ToolBrushPattern = HatchStyle.Percent50
            Case "Percent 60"
                ToolBrushPattern = HatchStyle.Percent60
            Case "Percent 70"
                ToolBrushPattern = HatchStyle.Percent70
            Case "Percent 80"
                ToolBrushPattern = HatchStyle.Percent80
            Case "Percent 90"
                ToolBrushPattern = HatchStyle.Percent90
        End Select

        Dim Brush As HatchBrush = New HatchBrush(ToolBrushPattern, ColorLeft.BackColor, ColorRight.BackColor)

        If RadioButtonBrushCircle.Checked Then
            gfx.FillEllipse(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), (BrushSizePreview.Width - NumericUpDownBrushSize.Value) / 2, (BrushSizePreview.Height - NumericUpDownBrushSize.Value) / 2, NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
        Else
            gfx.FillRectangle(New HatchBrush(ToolBrushPattern, ColorRight.BackColor, ColorLeft.BackColor), (BrushSizePreview.Width - NumericUpDownBrushSize.Value) / 2, (BrushSizePreview.Height - NumericUpDownBrushSize.Value) / 2, NumericUpDownBrushSize.Value, NumericUpDownBrushSize.Value)
        End If

        BrushSizePreview.Refresh()

    End Sub
    Private Sub RadioButtonBrushCircle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonBrushCircle.CheckedChanged
        UpdateBrushToolPreview()
    End Sub
    Private Sub NumericUpDownBrushSize_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownBrushSize.ValueChanged
        UpdateBrushToolPreview()
    End Sub
    Private Sub BrushToolPattern_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BrushToolPattern.SelectedIndexChanged
        UpdateBrushToolPreview()
    End Sub
#End Region
#Region "Text"
    Private Sub FontButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FontButton.Click
        FontDialog1.ShowDialog()
    End Sub
#End Region
#Region "Line"
    Private Sub NumericUpDownLineSize_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDownLineSize.ValueChanged
        LineSizePreview.Image = New Bitmap(LineSizePreview.Width, LineSizePreview.Height)

        LineSizePreview.Refresh()

        Dim gfx As Graphics = Graphics.FromImage(LineSizePreview.Image)
        gfx.Clear(Color.FromArgb(0, 0, 0, 0))
        Dim Brush As SolidBrush = New SolidBrush(ColorLeft.BackColor)

        gfx.FillEllipse(New SolidBrush(ColorLeft.BackColor), (LineSizePreview.Width - NumericUpDownLineSize.Value) / 2, (LineSizePreview.Height - NumericUpDownLineSize.Value) / 2, NumericUpDownLineSize.Value, NumericUpDownLineSize.Value)

        LineSizePreview.Refresh()
    End Sub
#End Region

    Private Sub ToolPencil_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolPencil.CheckedChanged
        If ToolPencil.Checked Then
            PanelPencilSettings.Visible = True
        Else
            PanelPencilSettings.Visible = False
        End If
    End Sub
    Private Sub ToolText_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolText.CheckedChanged
        If ToolText.Checked Then
            PanelTextSettings.Visible = True
        Else
            PanelTextSettings.Visible = False
        End If
    End Sub
    Private Sub ToolDot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolDot.CheckedChanged
        If ToolDot.Checked Then
            PanelDotSettings.Visible = True
        Else
            PanelDotSettings.Visible = False
        End If
    End Sub
    Private Sub ToolLine_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolLine.CheckedChanged
        If ToolLine.Checked Then
            PanelLineSettings.Visible = True
        Else
            PanelLineSettings.Visible = False
        End If
    End Sub
    Private Sub ToolEllipse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolShape.CheckedChanged
        If ToolShape.Checked Then
            PanelShapeSettings.Visible = True
        Else
            PanelShapeSettings.Visible = False
        End If
    End Sub
#End Region

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonScreenSize.Click
        Saved = False

        NumericUpDownCanvasWidth.Value = Screen.PrimaryScreen.Bounds.Width
        NumericUpDownCanvasHeight.Value = Screen.PrimaryScreen.Bounds.Height
    End Sub

#Region "Effects"
    Private Sub RotateA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotateA.Click
        Saved = False

        Dim bm As Bitmap = PictureBox1.Image

        Dim PrevSize As Size = PictureBox1.Size

        NumericUpDownCanvasWidth.Value = PrevSize.Height
        NumericUpDownCanvasHeight.Value = PrevSize.Width

        bm.RotateFlip(RotateFlipType.Rotate90FlipNone)

        PictureBox1.Image = bm

        PictureBox1.Refresh()
    End Sub
    Private Sub RotateB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotateB.Click
        Saved = False

        Dim bm As Bitmap = PictureBox1.Image

        bm.RotateFlip(RotateFlipType.Rotate180FlipNone)

        PictureBox1.Image = bm

        PictureBox1.Refresh()
    End Sub
    Private Sub RotateC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotateC.Click
        Saved = False

        Dim bm As Bitmap = PictureBox1.Image

        Dim PrevSize As Size = PictureBox1.Size

        NumericUpDownCanvasWidth.Value = PrevSize.Height
        NumericUpDownCanvasHeight.Value = PrevSize.Width

        bm.RotateFlip(RotateFlipType.Rotate270FlipNone)

        PictureBox1.Image = bm

        PictureBox1.Refresh()
    End Sub
    Private Sub FlipA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlipA.Click
        Saved = False

        Dim bm As Bitmap = PictureBox1.Image

        bm.RotateFlip(RotateFlipType.RotateNoneFlipX)

        PictureBox1.Image = bm

        PictureBox1.Refresh()
    End Sub
    Private Sub FlipB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlipB.Click
        Saved = False

        Dim bm As Bitmap = PictureBox1.Image

        bm.RotateFlip(RotateFlipType.RotateNoneFlipY)

        PictureBox1.Image = bm

        PictureBox1.Refresh()
    End Sub
    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Saved = False

        Me.Cursor = Cursors.WaitCursor

        Dim bm As Bitmap = PictureBox1.Image

        For X = 0 To bm.Width - 1
            For Y = 0 To bm.Height - 1
                bm.SetPixel(X, Y, Color.FromArgb(255 - bm.GetPixel(X, Y).R, 255 - bm.GetPixel(X, Y).G, 255 - bm.GetPixel(X, Y).B))
            Next
        Next

        PictureBox1.Image = bm
        PictureBox1.Refresh()

        Me.Cursor = Cursors.Default

    End Sub
#End Region






    Private Sub ButtonOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOpen.Click
        FileOpen()
    End Sub
    Private Sub ButtonSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSave.Click
        FileSave()
    End Sub
    Private Sub ButtonSaveAs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSaveAs.Click
        FileSaveAs("Save As")
    End Sub
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        FileNew()
    End Sub

    Sub FileOpen()
        PictureBox1.Enabled = False
        If Saved = False Then

            Dim Question As String
            If Filename = "" Then Question = "Untitled" Else Question = FilenameFullToShort(Filename)

            Select Case MsgBox("Do You Want to Save Changes to " & Question & "?", MsgBoxStyle.YesNoCancel, "Mega Paint")
                Case MsgBoxResult.Yes
                    FileSave()
                Case MsgBoxResult.No
                Case MsgBoxResult.Cancel
                    GoTo EndSub
            End Select
        End If

        If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then

            Dim BeforeOpenImage As Image = New Bitmap(PictureBox1.Image)

            Try
                Me.Cursor = Cursors.WaitCursor

                Dim OpenImage As Image = System.Drawing.Image.FromFile(OpenFileDialog1.FileName)
                SizeFromOpen = OpenImage.Size
                NumericUpDownCanvasWidth.Value = OpenImage.Width
                NumericUpDownCanvasHeight.Value = OpenImage.Height
                PictureBox1.Image = OpenImage
                Saved = True
                Filename = OpenFileDialog1.FileName

                Me.Cursor = Cursors.Default
            Catch ex As Exception
                Saved = False
                MsgBox("Error Opening: " & ex.Message)
            End Try
        End If
EndSub:
        TimerCanvas.Enabled = True
    End Sub
    Sub FileSave()
        If Filename = "" Then
            FileSaveAs("Save")
        Else
            Try
                Me.Cursor = Cursors.WaitCursor
                PictureBox1.Image.Save(Filename)
                Saved = True
                SizeFromOpen = PictureBox1.Size
                Me.Cursor = Cursors.Default
            Catch ex As Exception
                Saved = False
                MsgBox("Error Saving: " & ex.Message)
            End Try
        End If
    End Sub
    Sub FileSaveAs(ByVal DialogTitle As String)
        SaveFileDialog1.Title = DialogTitle
        SaveFileDialog1.FileName = ""

        If SaveFileDialog1.ShowDialog() = DialogResult.OK Then

            Dim Successful As Boolean = True

            Try
                Me.Cursor = Cursors.WaitCursor
                PictureBox1.Image.Save(SaveFileDialog1.FileName)
                SizeFromOpen = PictureBox1.Size
                Me.Cursor = Cursors.Default
            Catch ex As Exception
                MsgBox("Error Saving: " & ex.Message)
                Successful = False
            End Try

            If Successful = True Then Filename = SaveFileDialog1.FileName
        End If
    End Sub
    Sub FileNew()
        If Saved = False Then

            Dim Question As String
            If Filename = "" Then Question = "Untitled" Else Question = FilenameFullToShort(Filename)

            Select Case MsgBox("Do You Want to Save Changes to " & Question & "?", MsgBoxStyle.YesNoCancel, "Mega Paint")
                Case MsgBoxResult.Yes
                    FileSave()
                    Saved = True
                Case MsgBoxResult.No
                    Saved = True
                Case MsgBoxResult.Cancel
                    GoTo EndSub
            End Select
        End If

        SizeFromOpen = New Size(300, 300)

        NumericUpDownCanvasWidth.Value = 300
        NumericUpDownCanvasHeight.Value = 300

        ColorLeft.BackColor = Color.FromArgb(0, 0, 0)
        ColorRight.BackColor = Color.FromArgb(255, 255, 255)
        ColorLeft.Checked = True

        Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

        gfx.Clear(Color.FromArgb(255, 255, 255, 255))

        PictureBox1.Refresh()
EndSub:
    End Sub
    Function FilenameFullToShort(ByVal Filename As String)
        Dim a As Integer = 0
        Try
            Do
                a += 1
                If Filename.Chars(Filename.Length - a) = "/" Or Filename.Chars(Filename.Length - a) = "\" Then GoTo LoopEnd
            Loop
        Catch ex As Exception
            GoTo LoopEnd
        End Try
LoopEnd:
        Return Filename.Remove(0, (Filename.Length - a) + 1)
    End Function

#Region "Shortcut Keys"
    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click
        FileOpen()
    End Sub
    Private Sub NewToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewToolStripMenuItem.Click
        FileNew()
    End Sub
    Private Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripMenuItem.Click
        FileSave()
    End Sub
    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
#End Region

    Public Sub Fill(ByVal bm As Bitmap, ByVal x As _
        Integer, ByVal y As Integer, ByVal new_color As Color)
        ' Get the old and new colors.
        Dim old_color As Color = bm.GetPixel(x, y)

        ' The following "If Then" test was added by Reuben
        ' Jollif
        ' to protect the code in case the start pixel
        ' has the same color as the fill color.
        If old_color.ToArgb <> new_color.ToArgb Then
            ' Start with the original point in the stack.
            Dim pts As New Stack(1000)
            pts.Push(New Point(x, y))
            bm.SetPixel(x, y, new_color)

            ' While the stack is not empty, process a point.
            Do While pts.Count > 0
                Dim pt As Point = DirectCast(pts.Pop(), Point)
                If pt.X > 0 Then CheckPoint(bm, pts, pt.X - _
                    1, pt.Y, old_color, new_color)
                If pt.Y > 0 Then CheckPoint(bm, pts, pt.X, _
                    pt.Y - 1, old_color, new_color)
                If pt.X < bm.Width - 1 Then CheckPoint(bm, _
                    pts, pt.X + 1, pt.Y, old_color, new_color)
                If pt.Y < bm.Height - 1 Then CheckPoint(bm, _
                    pts, pt.X, pt.Y + 1, old_color, new_color)
            Loop
        End If
    End Sub
    Private Sub CheckPoint(ByVal bm As Bitmap, ByVal pts As  _
        Stack, ByVal x As Integer, ByVal y As Integer, ByVal _
        old_color As Color, ByVal new_color As Color)
        Dim clr As Color = bm.GetPixel(x, y)
        If clr.Equals(old_color) Then
            pts.Push(New Point(x, y))
            bm.SetPixel(x, y, new_color)
        End If
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Saved = False

        Me.Cursor = Cursors.WaitCursor

        Dim bm As Bitmap = PictureBox1.Image

        Dim R As Integer
        Dim G As Integer
        Dim B As Integer

        Dim Z As Integer

        For X = 0 To bm.Width - 1
            For Y = 0 To bm.Height - 1

                R = bm.GetPixel(X, Y).R
                G = bm.GetPixel(X, Y).G
                B = bm.GetPixel(X, Y).B

                Z = 0.21 * R + 0.71 * G + 0.07 * B

                bm.SetPixel(X, Y, Color.FromArgb(Z, Z, Z))
            Next
        Next

        PictureBox1.Image = bm
        PictureBox1.Refresh()

        Me.Cursor = Cursors.Default

    End Sub
    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Saved = False

        Me.Cursor = Cursors.WaitCursor

        Dim bm As Bitmap = New Bitmap(PictureBox1.Image)
        Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

        Dim R As Integer
        Dim G As Integer
        Dim B As Integer

        Dim Z As Integer



        For X = 0 To PictureBox1.Image.Width - 1
            For Y = 0 To PictureBox1.Image.Height - 1

                Dim FillHatchBrush As HatchBrush = New HatchBrush(HatchStyle.Percent10, Color.White, Color.White)

                R = bm.GetPixel(X, Y).R
                G = bm.GetPixel(X, Y).G
                B = bm.GetPixel(X, Y).B

                Z = 0.21 * R + 0.71 * G + 0.07 * B

                Z = (((Z * 10) + 255) / 255) - 1

                Select Case Z
                    Case 0
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent10, Color.Black, Color.Black)
                    Case 1
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent10, Color.White, Color.Black)
                    Case 2
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent20, Color.White, Color.Black)
                    Case 3
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent30, Color.White, Color.Black)
                    Case 4
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent40, Color.White, Color.Black)
                    Case 5
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent50, Color.White, Color.Black)
                    Case 6
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent60, Color.White, Color.Black)
                    Case 7
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent70, Color.White, Color.Black)
                    Case 8
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent80, Color.White, Color.Black)
                    Case 9
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent90, Color.White, Color.Black)
                    Case 10
                        FillHatchBrush = New HatchBrush(HatchStyle.Percent10, Color.White, Color.White)
                End Select

                gfx.FillRectangle(FillHatchBrush, X, Y, 1, 1)

            Next
        Next

        PictureBox1.Refresh()

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        Saved = False

        Dim W As Integer = NumericUpDownResizeWidth.Value
        Dim H As Integer = NumericUpDownResizeHeight.Value

        Dim bm As Bitmap = New Bitmap(W, H)

        Dim gfx As Graphics = Graphics.FromImage(bm)

        gfx.DrawImage(PictureBox1.Image, 0, 0, W, H)

        ResizeW = True
        ResizeH = True

        NumericUpDownCanvasWidth.Value = NumericUpDownResizeWidth.Value
        NumericUpDownCanvasHeight.Value = NumericUpDownResizeHeight.Value

        PictureBox1.Image = bm

        PictureBox1.Size = PictureBox1.Image.Size

        PictureBox1.Refresh()
    End Sub

    Private Sub TimerCanvas_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimerCanvas.Tick
        TimerCanvas.Enabled = False
        PictureBox1.Enabled = True
    End Sub

    Function PointInRange(ByVal Point As Point, ByVal Size As Size)
        Dim Result As Boolean = True

        If Point.X < 0 Then Result = False
        If Point.Y < 0 Then Result = False
        If Point.X > Size.Width - 1 Then Result = False
        If Point.Y > Size.Height - 1 Then Result = False

        Return Result

    End Function

    Private Sub ButtonBlur_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBlur.Click
        Me.Cursor = Cursors.WaitCursor

        Dim bm As Bitmap = New Bitmap(PictureBox1.Image)

        For X = 1 To bm.Width - 2
            For Y = 0 To bm.Height - 1
                Dim Color1 As Color = bm.GetPixel(X, Y)
                Dim Color2 As Color = bm.GetPixel(X + 1, Y)
                Dim Color3 As Color = bm.GetPixel(X - 1, Y)

                Dim A1 As Integer = Color1.A
                Dim R1 As Integer = Color1.R
                Dim G1 As Integer = Color1.G
                Dim B1 As Integer = Color1.B
                Dim A2 As Integer = Color2.A
                Dim R2 As Integer = Color2.R
                Dim G2 As Integer = Color2.G
                Dim B2 As Integer = Color2.B
                Dim A3 As Integer = Color3.A
                Dim R3 As Integer = Color3.R
                Dim G3 As Integer = Color3.G
                Dim B3 As Integer = Color3.B


                Dim Merge As Color = Color.FromArgb((A1 + A2 + A3) / 3, (R1 + R2 + R3) / 3, (G1 + G2 + G3) / 3, (B1 + B2 + B3) / 3)
                bm.SetPixel(X, Y, Merge)
            Next
        Next

        For X = 0 To bm.Width - 1
            For Y = 1 To bm.Height - 2
                Dim Color1 As Color = bm.GetPixel(X, Y)
                Dim Color2 As Color = bm.GetPixel(X, Y + 1)
                Dim Color3 As Color = bm.GetPixel(X, Y - 1)

                Dim A1 As Integer = Color1.A
                Dim R1 As Integer = Color1.R
                Dim G1 As Integer = Color1.G
                Dim B1 As Integer = Color1.B
                Dim A2 As Integer = Color2.A
                Dim R2 As Integer = Color2.R
                Dim G2 As Integer = Color2.G
                Dim B2 As Integer = Color2.B
                Dim A3 As Integer = Color3.A
                Dim R3 As Integer = Color3.R
                Dim G3 As Integer = Color3.G
                Dim B3 As Integer = Color3.B


                Dim Merge As Color = Color.FromArgb((A1 + A2 + A3) / 3, (R1 + R2 + R3) / 3, (G1 + G2 + G3) / 3, (B1 + B2 + B3) / 3)
                bm.SetPixel(X, Y, Merge)
            Next
        Next

        PictureBox1.Image = bm
        PictureBox1.Refresh()

        Me.Cursor = Cursors.Default
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        Me.Cursor = Cursors.WaitCursor

        Saved = False

        Dim NewImage As Bitmap = New Bitmap(PictureBox1.Image.Width, PictureBox1.Image.Height)
        Graphics.FromImage(NewImage).Clear(ColorRight.BackColor)
        Dim bm As Bitmap = New Bitmap(PictureBox1.Image)

        Dim Draw As Boolean

        For X = 1 To PictureBox1.Width - 2
            For Y = 1 To PictureBox1.Height - 2
                Draw = False


                If Not ColorSimilar(bm.GetPixel(X, Y), bm.GetPixel(X + 1, Y), 255 - NumericUpDown2.Value) Then Draw = True
                If Not ColorSimilar(bm.GetPixel(X, Y), bm.GetPixel(X - 1, Y), 255 - NumericUpDown2.Value) Then Draw = True
                If Not ColorSimilar(bm.GetPixel(X, Y), bm.GetPixel(X, Y + 1), 255 - NumericUpDown2.Value) Then Draw = True
                If Not ColorSimilar(bm.GetPixel(X, Y), bm.GetPixel(X, Y - 1), 255 - NumericUpDown2.Value) Then Draw = True

                If CheckBox1.Checked Then
                    If Draw = True Then NewImage.SetPixel(X, Y, bm.GetPixel(X, Y))
                Else
                    If Draw = True Then NewImage.SetPixel(X, Y, ColorLeft.BackColor)
                End If

            Next
        Next

        PictureBox1.Image = NewImage

        Me.Cursor = Cursors.Default
    End Sub
    Function ColorSimilar(ByVal Color As Color, ByVal Comparison As Color, ByVal Range As Integer)
        Dim Result As Boolean = True

        If Comparison.A < Color.A - Range Or Comparison.A > Color.A + Range Then Result = False
        If Comparison.R < Color.R - Range Or Comparison.R > Color.R + Range Then Result = False
        If Comparison.G < Color.G - Range Or Comparison.G > Color.G + Range Then Result = False
        If Comparison.B < Color.B - Range Or Comparison.B > Color.B + Range Then Result = False

        'If Color = Comparison Then Result = True Else Result = False

        Return Result
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If NumericUpDown1.Value > PictureBox1.Width Or NumericUpDown1.Value > PictureBox1.Height Then
            MsgBox("Value Must Be Less Than Image Width and Height")
            GoTo SubEnd
        End If

        Me.Cursor = Cursors.WaitCursor

        Saved = False

        PictureBox1.Width = NumericUpDown1.Value * Int(PictureBox1.Width / NumericUpDown1.Value)
        PictureBox1.Height = NumericUpDown1.Value * Int(PictureBox1.Height / NumericUpDown1.Value)


        Dim NewBrush As SolidBrush = New SolidBrush(Color.White)

        Dim bm As Bitmap = New Bitmap(PictureBox1.Image)
        Dim gfx As Graphics = Graphics.FromImage(PictureBox1.Image)

        gfx.Clear(Color.Transparent)

        Dim Count As Integer

        Dim A As Integer
        Dim R As Integer
        Dim G As Integer
        Dim B As Integer

        For Xa = 0 To PictureBox1.Width - 1 Step NumericUpDown1.Value
            For Ya = 0 To PictureBox1.Height - 1 Step NumericUpDown1.Value

                Count = 0

                For X = Xa To Xa + NumericUpDown1.Value - 1
                    For Y = Ya To Ya + NumericUpDown1.Value - 1

                        A += bm.GetPixel(X, Y).A
                        R += bm.GetPixel(X, Y).R
                        G += bm.GetPixel(X, Y).G
                        B += bm.GetPixel(X, Y).B

                        Count += 1
                    Next
                Next

                A = A / Count
                R = R / Count
                G = G / Count
                B = B / Count

                If A > 255 Then A = 255 Else If A < 0 Then A = 0
                If R > 255 Then R = 255 Else If R < 0 Then R = 0
                If G > 255 Then G = 255 Else If G < 0 Then G = 0
                If B > 255 Then B = 255 Else If B < 0 Then B = 0

                NewBrush.Color = Color.FromArgb(A, R, G, B)
                gfx.FillRectangle(NewBrush, Xa, Ya, NumericUpDown1.Value, NumericUpDown1.Value)

            Next
        Next


        NumericUpDownCanvasWidth.Value = NumericUpDown1.Value * Int(PictureBox1.Width / NumericUpDown1.Value)
        NumericUpDownCanvasHeight.Value = NumericUpDown1.Value * Int(PictureBox1.Height / NumericUpDown1.Value)

        PictureBox1.Refresh()

        Me.Cursor = Cursors.Default

SubEnd:

    End Sub
End Class
