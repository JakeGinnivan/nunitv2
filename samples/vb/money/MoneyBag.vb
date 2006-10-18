'************************************************************************************
'
' Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright � 2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright � 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/

Option Explicit On 

Namespace NUnit.Samples

    Public Class MoneyBag
        Implements IMoney

        Private fmonies As ArrayList = New ArrayList(5)

        Private Sub New()

        End Sub

        Public Sub New(ByVal bag As Money())
            For Each m As Money In bag
                If Not m.IsZero Then
                    AppendMoney(m)
                End If
            Next
        End Sub

        Public Sub New(ByVal m1 As Money, ByVal m2 As Money)

            AppendMoney(m1)
            AppendMoney(m2)

        End Sub

        Public Sub New(ByVal m As Money, ByVal bag As MoneyBag)
            AppendMoney(m)
            AppendBag(bag)
        End Sub

        Public Sub New(ByVal m1 As MoneyBag, ByVal m2 As MoneyBag)
            AppendBag(m1)
            AppendBag(m2)
        End Sub

        Public Function Add(ByVal m As IMoney) As IMoney Implements IMoney.Add
            Return m.AddMoneyBag(Me)
        End Function

        Public Function AddMoney(ByVal m As Money) As IMoney Implements IMoney.AddMoney
            Return New MoneyBag(m, Me).Simplify
        End Function

        Public Function AddMoneyBag(ByVal s As MoneyBag) As IMoney Implements IMoney.AddMoneyBag
            Return New MoneyBag(s, Me).Simplify()
        End Function

        Private Sub AppendBag(ByVal aBag As MoneyBag)
            For Each m As Money In aBag.fmonies
                AppendMoney(m)
            Next
        End Sub

        Private Sub AppendMoney(ByVal aMoney As Money)

            Dim old As Money = FindMoney(aMoney.Currency)
            If old Is Nothing Then
                fmonies.Add(aMoney)
                Return
            End If
            fmonies.Remove(old)
            Dim sum As IMoney = old.Add(aMoney)
            If (sum.IsZero) Then
                Return
            End If
            fmonies.Add(sum)
        End Sub

        Private Function Contains(ByVal aMoney As Money) As Boolean
            Dim m As Money = FindMoney(aMoney.Currency)
            Return m.Amount.Equals(aMoney.Amount)
        End Function

        Public Overloads Overrides Function Equals(ByVal anObject As Object) As Boolean
            If IsZero Then
                If TypeOf anObject Is IMoney Then
                    Dim aMoney As IMoney = anObject
                    Return aMoney.IsZero
                End If
            End If

            If TypeOf anObject Is MoneyBag Then
                Dim aMoneyBag As MoneyBag = anObject
                If Not aMoneyBag.fmonies.Count.Equals(fmonies.Count) Then
                    Return False
                End If

                For Each m As Money In fmonies
                    If Not aMoneyBag.Contains(m) Then
                        Return False
                    End If

                    Return True
                Next
            End If

            Return False
        End Function

        Private Function FindMoney(ByVal currency As String)
            For Each m As Money In fmonies
                If m.Currency.Equals(currency) Then
                    Return m
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function GetHashCode() As Int32
            Dim hash As Int32 = 0
            For Each m As Money In fmonies
                hash += m.GetHashCode()
            Next
            Return hash
        End Function

        Public ReadOnly Property IsZero() As Boolean Implements IMoney.IsZero
            Get
                Return fmonies.Count.Equals(0)
            End Get
        End Property

        Public Function Multiply(ByVal factor As Integer) As IMoney Implements IMoney.Multiply
            Dim result As New MoneyBag
            If Not factor.Equals(0) Then
                For Each m As Money In fmonies
                    result.AppendMoney(m.Multiply(factor))
                Next
            End If
            Return result
        End Function

        Public Function Negate() As IMoney Implements IMoney.Negate
            Dim result As New MoneyBag
            For Each m As Money In fmonies
                result.AppendMoney(m.Negate())
            Next
            Return result
        End Function

        Private Function Simplify() As IMoney
            If fmonies.Count.Equals(1) Then
                Return fmonies(0)
            End If
            Return Me
        End Function


        Public Function Subtract(ByVal m As IMoney) As IMoney Implements IMoney.Subtract
            Return Add(m.Negate())
        End Function
    End Class

End Namespace
