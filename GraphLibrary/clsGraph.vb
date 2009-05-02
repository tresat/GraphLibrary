Option Explicit On
Option Strict On

Namespace Graph
#Region "Exceptions"
#Region "VertexDoesntExistException"
    Public Class VertexDoesntExistException
        Inherits Exception

#Region "Member Vars"
        Protected mlngVertexID As Long
#End Region

#Region "Properties"
        Public ReadOnly Property VertexID() As Long
            Get
                Return mlngVertexID
            End Get
        End Property
#End Region

#Region "Constructors"
        Public Sub New(ByVal plngVertexID As Long)
            MyBase.New()

            mlngVertexID = plngVertexID
        End Sub

        Public Sub New(ByVal plngVertexID As Long, ByVal pstrMessage As String)
            MyBase.New(pstrMessage)

            mlngVertexID = plngVertexID
        End Sub
#End Region
    End Class
#End Region

#Region "EdgeDoesntExistException"
    Public Class EdgeDoesntExistException
        Inherits Exception

#Region "Member Vars"
        Protected mlngEdgeID As Long
#End Region

#Region "Properties"
        Public ReadOnly Property EdgeID() As Long
            Get
                Return mlngEdgeID
            End Get
        End Property
#End Region

#Region "Constructors"
        Public Sub New()
            MyBase.New()

            mlngEdgeID = Nothing
        End Sub

        Public Sub New(ByVal plngEdgeID As Long)
            MyBase.New()

            mlngEdgeID = plngEdgeID
        End Sub

        Public Sub New(ByVal plngEdgeID As Long, ByVal pstrMessage As String)
            MyBase.New(pstrMessage)

            mlngEdgeID = plngEdgeID
        End Sub
#End Region
    End Class
#End Region
#End Region

    Public Class clsGraph(Of GraphVertexPayload, GraphEdgePayload)
#Region "Inner Types"
#Region "Vertex"
        Public Class clsVertex(Of VertexPayload)
#Region "Member Vars"
            Protected Friend mvpPayload As VertexPayload
            Protected Friend mlngVertexID As Long
            Protected Friend mlstEdges As List(Of Long)
#End Region

#Region "Properties"
            Public ReadOnly Property VertexID() As Long
                Get
                    Return mlngVertexID
                End Get
            End Property

            Public Property Payload() As VertexPayload
                Get
                    Return mvpPayload
                End Get
                Set(ByVal value As VertexPayload)
                    mvpPayload = value
                End Set
            End Property

            Public ReadOnly Property Edges() As List(Of Long)
                Get
                    Return mlstEdges
                End Get
            End Property
#End Region

#Region "Constructors"
            Protected Friend Sub New(ByVal plngVertexID As Long, Optional ByRef pvpPayload As VertexPayload = Nothing)
                mlngVertexID = plngVertexID
                mvpPayload = pvpPayload

                mlstEdges = New List(Of Long)
            End Sub
#End Region

#Region "Public Functionality"
            Public Sub AddEdge(ByVal plngEdgeID As Long)
                mlstEdges.Add(plngEdgeID)
            End Sub

            Public Sub RemoveEdge(ByVal plngEdgeID As Long)
                If mlstEdges.Contains(plngEdgeID) Then
                    mlstEdges.Remove(plngEdgeID)
                Else
                    Throw New EdgeDoesntExistException(plngEdgeID)
                End If
            End Sub
#End Region
        End Class
#End Region

#Region "Edge"
        Public Class clsEdge(Of EdgePayload)
#Region "MemberVars"
            Protected Friend mepPayload As EdgePayload
            Protected Friend mlngEdgeID As Long
            Protected Friend mlngVertexID1 As Long
            Protected Friend mlngVertexID2 As Long
#End Region

#Region "Properties"
            Public ReadOnly Property EdgeID() As Long
                Get
                    Return mlngEdgeID
                End Get
            End Property

            Public ReadOnly Property VertexID1() As Long
                Get
                    Return mlngVertexID1
                End Get
            End Property

            Public ReadOnly Property VertexID2() As Long
                Get
                    Return mlngVertexID2
                End Get
            End Property

            Public Property Payload() As EdgePayload
                Get
                    Return mepPayload
                End Get
                Set(ByVal value As EdgePayload)
                    mepPayload = value
                End Set
            End Property
#End Region

#Region "Constructors"
            Protected Friend Sub New(ByVal plngEdgeID As Long, ByVal plngVertexID1 As Long, ByVal plngVertexID2 As Long, Optional ByRef pepPayload As EdgePayload = Nothing)
                mlngEdgeID = plngEdgeID
                mepPayload = pepPayload
                mlngVertexID1 = plngVertexID1
                mlngVertexID2 = plngVertexID2
            End Sub
#End Region

#Region "Public Functionality"
            ''' <summary>
            ''' For convienience, adds all vertices to a list, for iteration.
            ''' </summary>
            ''' <returns>A list of all current vertices.</returns>
            Public Function Vertices() As List(Of Long)
                Dim lstVertices As New List(Of Long)

                If Not mlngVertexID1 = Nothing Then
                    lstVertices.Add(mlngVertexID1)
                End If
                If Not mlngVertexID2 = Nothing Then
                    lstVertices.Add(mlngVertexID2)
                End If

                Return lstVertices
            End Function
#End Region
        End Class
#End Region
#End Region

#Region "Member Vars"
        Protected Shared mlngNextVertexID As Long
        Protected Shared mlngNextEdgeID As Long

        Protected mdctVertices As Dictionary(Of Long, clsVertex(Of GraphVertexPayload))
        Protected mdctEdges As Dictionary(Of Long, clsEdge(Of GraphEdgePayload))
#End Region

#Region "Constructors"
        Public Sub New()
            mlngNextVertexID = 1
            mlngNextEdgeID = 1

            mdctEdges = New Dictionary(Of Long, clsEdge(Of GraphEdgePayload))
            mdctVertices = New Dictionary(Of Long, clsVertex(Of GraphVertexPayload))
        End Sub
#End Region

#Region "Public Functionality"
        ''' <summary>
        ''' Accessor for vertex count.
        ''' </summary>
        ''' <returns>The number of vertices currently in the graph.</returns>
        Public Function NumVertices() As Integer
            Return mdctVertices.Count
        End Function

        ''' <summary>
        ''' Accessor for edge count.
        ''' </summary>
        ''' <returns>The number of edges currently in the graph.</returns>
        Public Function NumEdges() As Integer
            Return mdctEdges.Count
        End Function

        ''' <summary>
        ''' Returns the specified vertex.  Throws exception if it doesn't exist.
        ''' </summary>
        ''' <param name="plngID">The vertex ID.</param>
        ''' <returns>Reference to the vertex object.</returns>
        Public Overridable Function GetVertex(ByVal plngID As Long) As clsVertex(Of GraphVertexPayload)
            If mdctVertices.Keys.Contains(plngID) Then
                Return mdctVertices(plngID)
            Else
                Throw New VertexDoesntExistException(plngID)
            End If
        End Function

        ''' <summary>
        ''' Returns the specified edge.  Throws exception if it doesn't exist.
        ''' </summary>
        ''' <param name="plngID">The edge ID.</param>
        ''' <returns>Reference to the edge object.</returns>
        Public Overridable Function GetEdge(ByVal plngID As Long) As clsEdge(Of GraphEdgePayload)
            If mdctEdges.Keys.Contains(plngID) Then
                Return mdctEdges(plngID)
            Else
                Throw New EdgeDoesntExistException(plngID)
            End If
        End Function

        ''' <summary>
        ''' Adds a new, disconnected vertex to the graph
        ''' </summary>
        ''' <param name="pvpPayload">Payload for the new vertex.</param>
        ''' <returns>The new vertex's ID.</returns>
        Public Overridable Function AddNewVertex(Optional ByRef pvpPayload As GraphVertexPayload = Nothing) As Long
            Dim vNew As New clsVertex(Of GraphVertexPayload)(mlngNextVertexID, pvpPayload)

            mdctVertices.Add(vNew.VertexID, vNew)

            mlngNextVertexID += 1

            Return vNew.VertexID
        End Function

        ''' <summary>
        ''' Adds a new vertex to the graph with the specified payload, connected to the specified vertex.  
        ''' Creates a single new edge with the specified payload to connect the new vertex.
        ''' </summary>
        ''' <param name="plngExistingVertexID">The vertex to create the new vertex off of.</param>
        ''' <param name="pvpVertexPayload">The payload of the new vertex to create.</param>
        ''' <param name="pepEdgePayload">The payload of the new edge to create.</param>
        ''' <returns>VertexID of new vertex.</returns>
        Public Overridable Function AddNewVertex(ByVal plngExistingVertexID As Long, _
                                Optional ByRef pvpVertexPayload As GraphVertexPayload = Nothing, _
                                Optional ByRef pepEdgePayload As GraphEdgePayload = Nothing) As Long
            Dim vNew As clsVertex(Of GraphVertexPayload)
            Dim eNew As clsEdge(Of GraphEdgePayload)

            If Not mdctVertices.Keys.Contains(plngExistingVertexID) Then
                Throw New VertexDoesntExistException(plngExistingVertexID)
            End If

            vNew = New clsVertex(Of GraphVertexPayload)(mlngNextVertexID, pvpVertexPayload)
            mdctVertices.Add(vNew.VertexID, vNew)

            eNew = New clsEdge(Of GraphEdgePayload)(mlngNextEdgeID, plngExistingVertexID, mlngNextVertexID, pepEdgePayload)
            mdctEdges.Add(eNew.EdgeID, eNew)

            mdctVertices(plngExistingVertexID).AddEdge(eNew.EdgeID)
            vNew.AddEdge(eNew.EdgeID)

            mlngNextVertexID += 1
            mlngNextEdgeID += 1

            Return vNew.VertexID
        End Function

        ''' <summary>
        ''' Adds a new edge connecting two existing vertices.
        ''' Throws exception if the vertices don't actually exist.
        ''' </summary>
        ''' <param name="plngVertexID1">The 1st vertex ID.</param>
        ''' <param name="plngVertexID2">The 2nd vertex ID.</param>
        ''' <returns></returns>
        Public Overridable Function AddNewEdge(ByVal plngVertexID1 As Long, ByVal plngVertexID2 As Long, Optional ByRef pepEdgePayload As GraphEdgePayload = Nothing) As Long
            Dim eNew As clsEdge(Of GraphEdgePayload)

            If Not mdctVertices.Keys.Contains(plngVertexID1) Then
                Throw New VertexDoesntExistException(plngVertexID1)
            End If
            If Not mdctVertices.Keys.Contains(plngVertexID2) Then
                Throw New VertexDoesntExistException(plngVertexID2)
            End If

            eNew = New clsEdge(Of GraphEdgePayload)(mlngNextEdgeID, plngVertexID1, plngVertexID2, pepEdgePayload)

            mdctVertices(plngVertexID1).AddEdge(eNew.EdgeID)
            mdctVertices(plngVertexID2).AddEdge(eNew.EdgeID)

            mdctEdges.Add(eNew.EdgeID, eNew)

            mlngNextEdgeID += 1

            Return eNew.EdgeID
        End Function

        ''' <summary>
        ''' Removes the vertex from the graph.  
        ''' Throws exception if the vertex does not exist.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to remove.</param>
        Public Overridable Sub RemoveVertex(ByVal plngVertexID As Long)
            Dim vRemove As clsVertex(Of GraphVertexPayload)
            Dim lstEdgeIDsToRemove As List(Of Long)
            Dim intLim As Integer
            Dim lngEdgeID As Long

            If mdctVertices.Keys.Contains(plngVertexID) Then
                vRemove = mdctVertices(plngVertexID)

                lstEdgeIDsToRemove = New List(Of Long)(vRemove.Edges)
                intLim = lstEdgeIDsToRemove.Count - 1
                For intidx As Integer = 0 To intLim
                    lngEdgeID = lstEdgeIDsToRemove(intidx)
                    RemoveEdge(lngEdgeID)
                Next

                mdctVertices.Remove(plngVertexID)
            Else
                Throw New VertexDoesntExistException(plngVertexID)
            End If
        End Sub

        ''' <summary>
        ''' Removes an edge from the graph.
        ''' </summary>
        ''' <param name="plngEdgeID">The edge ID to remove.</param>
        Public Overridable Sub RemoveEdge(ByVal plngEdgeID As Long)
            Dim eRemove As clsEdge(Of GraphEdgePayload)

            If mdctEdges.Keys.Contains(plngEdgeID) Then
                eRemove = mdctEdges(plngEdgeID)

                mdctVertices(eRemove.VertexID1).RemoveEdge(eRemove.EdgeID)
                mdctVertices(eRemove.VertexID2).RemoveEdge(eRemove.EdgeID)

                mdctEdges.Remove(eRemove.EdgeID)
            Else
                Throw New EdgeDoesntExistException(plngEdgeID)
            End If
        End Sub

        ''' <summary>
        ''' Determines whether the vertex payload's are identical.
        ''' </summary>
        ''' <param name="plngVertexID1">The first vertex to compare's ID.</param>
        ''' <param name="plngVertexID2">The second vertex to compare's ID.</param>
        ''' <exception cref="VertexDoesntExistException">Thrown if one of the vertex IDs doesn't exist in the graph.</exception>
        ''' <returns>
        ''' <c>true</c> if [is identical vertex payload]; otherwise, <c>false</c>.
        ''' </returns>
        Public Function IsIdenticalVertexPayload(ByVal plngVertexID1 As Long, ByVal plngVertexID2 As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID1) Then Throw New VertexDoesntExistException(plngVertexID1)
            If Not mdctVertices.Keys.Contains(plngVertexID2) Then Throw New VertexDoesntExistException(plngVertexID2)

            Return mdctVertices(plngVertexID1).Payload.Equals(mdctVertices(plngVertexID2).Payload)
        End Function
#End Region
    End Class
End Namespace