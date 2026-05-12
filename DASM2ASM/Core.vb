'This program's imports and settings.
Option Compare Binary
Option Explicit On
Option Infer Off
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Environment
Imports System.IO
Imports System.Linq
Imports System.Text

'This module contains this program's main interface.
Public Module CoreModule
   'This procedure is executed when this program is started.
   Public Sub Main()
      Try
         Dim Assembler As New List(Of String)
         Dim Code As String = Nothing
         Dim Comment As String = Nothing
         Dim InputFile As String = Nothing
         Dim Label As String = Nothing
         Dim Lines() As String = {}
         Dim MemoryLabel As String = Nothing
         Dim MemoryValueDefine As String = Nothing
         Dim MemoryValueDefines As New List(Of String)
         Dim OutputFile As String = Nothing
         Dim Target As String = Nothing

         If GetCommandLineArgs.Count > 1 Then
            InputFile = GetCommandLineArgs.Last
            OutputFile = $"{Path.GetFileNameWithoutExtension(InputFile)}.asm{Path.GetExtension(InputFile)}"
            Lines = File.ReadAllLines(InputFile)
            For Each Line As String In Lines
               Line = Line.Trim()
               If Line.Contains(" "c) Then
                  Label = $"L{TrimLeadingZeroes(Line.Substring(0, Line.IndexOf(" "c))).ToLower()}: "
                  If Line.Contains(" "c) Then
                     Line = Line.Substring(Line.IndexOf(" "c)).Trim()
                     If Line.Contains(" "c) Then
                        Line = Line.Substring(Line.IndexOf(" "c)).Trim()
                        Code = Line
                        Comment = Nothing
                        If Code.Contains(";"c) Then
                           Comment = Code.Substring(Code.IndexOf(";"c))
                           Code = Code.Substring(0, Code.Length - Comment.Length)
                        End If
                        If IsNearControlFlow(Code) Then
                           Target = Code.Substring(Code.ToLower().IndexOf("0x"))
                           Code = Code.Substring(0, Code.ToLower.IndexOf("0x"))
                           Target = $"L{Target.Substring(2)}"
                           Code = $"{Code}{Target}"
                        ElseIf Code.Contains("["c) AndAlso Code.Contains("]"c) Then
                           Code = ReplaceMemoryReference(Code, MemoryLabel)

                           MemoryValueDefine = $"{MemoryLabel} db 0x0, 0x0"

                           If Not MemoryValueDefineListed(MemoryValueDefine, MemoryValueDefines) Then
                              If ReferenceIsInitialization(Code) Then
                                 MemoryValueDefine = $"{MemoryValueDefine} ; ***"
                              End If
                              MemoryValueDefines.Add(MemoryValueDefine)
                           End If
                        End If

                        Assembler.Add($"{Label}{Code}{Comment}")
                     End If
                  End If
               End If
            Next Line

            If MemoryValueDefines.Any Then
               MemoryValueDefines.Sort()
               Assembler.Add($"{NewLine}; *** = initialization value.{NewLine}")
               MemoryValueDefines.ForEach(AddressOf Assembler.Add)
            End If

            File.WriteAllLines(OutputFile, Assembler.ToArray())
            Console.WriteLine($"Assembly written to: ""{Path.GetFullPath(OutputFile)}""")
            Console.ReadLine()
         Else
            With My.Application.Info
               Console.WriteLine($"{ .Title} v{ .Version}, by: { .CompanyName} - { .Copyright}{NewLine}")
               Console.WriteLine($"Usage: { .AssemblyName}.Exe INPUT_FILE")
               Console.ReadLine()
            End With
         End If
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try
   End Sub

   'This procedure displays any exceptions that occur.
   Private Sub DisplayException([Exception] As Exception)
      Try
         Console.WriteLine($"ERROR: {Exception.Message}")
      Catch
         [Exit](0)
      End Try
   End Sub

   'This procedure returns the reference to a memory address in the specified code.
   Private Function GetMemoryReference(Code As String) As String
      Try
         Dim MemoryReference As String = Code.Substring(Code.IndexOf("["c) + 1)

         Return MemoryReference.Substring(0, MemoryReference.IndexOf("]"c))
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return ""
   End Function

   'This procedure returns whether or not the specified code is a near control flow instruction.
   Private Function IsNearControlFlow(Code As String) As Boolean
      Try
         Code = Code.Trim().ToLower()

         Return Code.StartsWith("call 0x") OrElse Code.StartsWith("j") OrElse Code.StartsWith("loop")
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return Nothing
   End Function

   'This procedure returns whether or not the specified memory value define is already on the specified list.
   Private Function MemoryValueDefineListed(ValueDefine As String, ValueDefines As List(Of String)) As Boolean
      Try
         Dim Listed As Boolean = False

         ValueDefine = ValueDefine.Trim().ToLower()
         For Each Item As String In ValueDefines
            If Item.Trim().ToLower().StartsWith(ValueDefine) Then
               Listed = True
               Exit For
            End If
         Next Item

         Return Listed
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return Nothing
   End Function

   'This procedure returns the reference to a memory address converted to a label.
   Private Function ReferenceToLabel(MemoryReference As String) As String
      Try
         Dim Label As New StringBuilder

         For Index As Integer = 0 To MemoryReference.ToLower().Length - 1
            Select Case MemoryReference.Chars(Index)
               Case "0"c To "9"c, "a"c To "z"c, "_"c
                  Label.Append(MemoryReference.Chars(Index))
               Case Else
                  Label.Append("_"c)
            End Select
         Next Index

         Return $"L{Label.ToString()}"
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return ""
   End Function

   'This procedure returns whether or not the specified code contains a memory reference that is used for initialization.
   Private Function ReferenceIsInitialization(Code As String) As Boolean
      Try
         Return Code.ToLower.StartsWith("mov ") AndAlso (Code.IndexOf(","c) < Code.IndexOf("["c))
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return Nothing
   End Function

   'This procedure returns the specified code with the memory address reference converted to a label along with the label itself.
   Private Function ReplaceMemoryReference(Code As String, ByRef MemoryLabel As String) As String
      Try
         Dim LeftPart As String = Code.Substring(0, Code.IndexOf("["c) + 1)
         Dim MemoryReference As String = GetMemoryReference(Code)
         Dim RightPart As String = Code.Substring(Code.IndexOf("]"c))

         MemoryLabel = ReferenceToLabel(MemoryReference)

         Return $"{LeftPart}{MemoryLabel}{RightPart}"
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return ""
   End Function

   'This procedure the specified text with any multiple leading zeroes removed.
   Private Function TrimLeadingZeroes(Text As String) As String
      Try
         Do While Text.StartsWith("0"c) AndAlso Text.Length > 1
            Text = Text.Substring(2)
         Loop

         Return Text
      Catch [Exception] As Exception
         DisplayException([Exception])
      End Try

      Return ""
   End Function
End Module
