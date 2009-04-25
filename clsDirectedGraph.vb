﻿Option Explicit On
Option Strict On

Imports GraphLibrary.Graph

Namespace DirectedGraph
    Public Class clsDirectedGraph(Of GraphVertexPayload, GraphEdgePayload)
        Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload)
#Region "Inner Types"
#Region "Vertex"
        Public Class clsDirectedGraphVertex(Of VertexPayload)
            Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload).clsVertex(Of VertexPayload)

            Public Sub New(ByVal plngVertexID As Long, Optional ByRef pvpPayload As VertexPayload = Nothing)
                MyBase.New(plngVertexID, pvpPayload)
            End Sub
        End Class
#End Region

#Region "Edge"
        Public Class clsDirectedGraphEdge(Of EdgePayload)
            Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload).clsEdge(Of EdgePayload)
#Region "Member Vars"
            Protected Friend mlngStartVertex As Long
#End Region

#Region "Constructors"
            Protected Sub New(ByVal plngEdgeID As Long, ByVal plngStartVertexID As Long, ByVal plngEndVertexID As Long, Optional ByRef pepPayload As EdgePayload = Nothing)
                MyBase.New(plngEdgeID, plngStartVertexID, plngEndVertexID, pepPayload)

                mlngStartVertex = plngStartVertexID
            End Sub
#End Region

#Region "Public Functionality"
            ''' <summary>
            ''' Reverses the direction of the edge
            ''' </summary>
            Public Sub ReverseDirection()
                mlngStartVertex = CLng(IIf(mlngStartVertex = mlngVertexID1, mlngVertexID2, mlngVertexID1))
            End Sub

            ''' <summary>
            ''' Returns the vertex ID where this edge originates.
            ''' </summary>
            ''' <returns>Start vertex ID.</returns>
            Public Function StartVertexID() As Long
                Return mlngStartVertex
            End Function

            ''' <summary>
            ''' Returns the vertex ID where this edge ends.
            ''' </summary>
            ''' <returns>End vertex ID.</returns>
            Public Function EndVertexID() As Long
                Return CLng(IIf(mlngStartVertex = mlngVertexID1, mlngVertexID2, mlngVertexID1))
            End Function
#End Region
        End Class
#End Region
#End Region

#Region "Member Vars"
        Private mdctSourceVertices As Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))
        Private mdctSinkVertices As Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))
#End Region

#Region "Constructors"
        ''' <summary>
        ''' Initializes a new instance of the <see cref="clsDirectedGraph(Of GraphVertexPayload, GraphEdgePayload)" /> class.
        ''' If source payloads provided, count must match number of source nodes to create.
        ''' </summary>
        ''' <exception cref="ArgumentException">When count of source payloads doesn't match num source nodes to create.</exception>
        ''' <param name="pintNumSources">The number of disconnected source nodes to create.</param>
        ''' <param name="plstSourcePayloads">The source payloads for each source node.</param>
        Public Sub New(Optional ByVal pintNumSources As Integer = 1, Optional ByVal plstSourcePayloads As List(Of GraphVertexPayload) = Nothing)
            MyBase.New()

            If plstSourcePayloads IsNot Nothing AndAlso pintNumSources <> plstSourcePayloads.Count Then
                Throw New ArgumentException("If source payloads list provided, count must match number of sources!")
            End If

            mdctSourceVertices = New Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))
            mdctSinkVertices = New Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))

            For intIdx As Integer = 0 To pintNumSources - 1
                'Use nothing as source for all nodes, if no list of payloads provided.
                If plstSourcePayloads IsNot Nothing Then
                    AddSourceVertex(plstSourcePayloads(intIdx))
                Else
                    AddSourceVertex(Nothing)
                End If
            Next
        End Sub
#End Region

#Region "Public Functionality"
        ''' <summary>
        ''' Returns the specified vertex.  Throws exception if it doesn't exist.
        ''' </summary>
        ''' <param name="plngID">The vertex ID.</param>
        ''' <returns>Reference to the vertex object.</returns>
        Public Shadows Function GetVertex(ByVal plngID As Long) As clsDirectedGraphVertex(Of GraphVertexPayload)
            If mdctVertices.Keys.Contains(plngID) Then
                Return CType(mdctVertices(plngID), clsDirectedGraphVertex(Of GraphVertexPayload))
            Else
                Throw New VertexDoesntExistException(plngID)
            End If
        End Function

        ''' <summary>
        ''' Returns the specified edge.  Throws exception if it doesn't exist.
        ''' </summary>
        ''' <param name="plngID">The edge ID.</param>
        ''' <returns>Reference to the edge object.</returns>
        Public Shadows Function GetEdge(ByVal plngID As Long) As clsDirectedGraphEdge(Of GraphEdgePayload)
            If mdctEdges.Keys.Contains(plngID) Then
                Return CType(mdctEdges(plngID), clsDirectedGraphEdge(Of GraphEdgePayload))
            Else
                Throw New EdgeDoesntExistException(plngID)
            End If
        End Function

        ''' <summary>
        ''' Adds a new source vertex.  Ironically, as this vertex has no outgoing
        ''' edges, it is also a sink vertex, and must be marked as such.
        ''' </summary>
        ''' <param name="pvpPayload">The payload for the new vertex.</param>
        ''' <returns>The new vertex ID.</returns>
        Public Function AddSourceVertex(Optional ByRef pvpPayload As GraphVertexPayload = Nothing) As Long
            Dim vNew As New clsDirectedGraphVertex(Of GraphVertexPayload)(mlngNextVertexID, pvpPayload)

            mdctSourceVertices.Add(vNew.VertexID, vNew)
            mdctSinkVertices.Add(vNew.VertexID, vNew)
            mdctVertices.Add(vNew.VertexID, vNew)

            mlngNextVertexID += 1

            Return vNew.VertexID
        End Function

        ''' <summary>
        ''' Gets a list of source vertices.
        ''' </summary>
        ''' <returns>A list of source vertices.</returns>
        Public Function GetSources() As List(Of clsVertex(Of GraphVertexPayload))
            Dim lstSources As New List(Of clsVertex(Of GraphVertexPayload))

            For Each lngSourceID In mdctSourceVertices.Keys
                lstSources.Add(mdctSourceVertices(lngSourceID))
            Next

            Return lstSources
        End Function

        ''' <summary>
        ''' Gets a list of sink vertices.
        ''' </summary>
        ''' <returns>A list of sink vertices.</returns>
        Public Function GetSinks() As List(Of clsVertex(Of GraphVertexPayload))
            Dim lstsinks As New List(Of clsVertex(Of GraphVertexPayload))

            For Each lngSourceID In mdctSinkVertices.Keys
                lstsinks.Add(mdctSinkVertices(lngSourceID))
            Next

            Return lstsinks
        End Function

        ''' <summary>
        ''' Determines whether the specified vertex ID is a sink (has no outgoing edges).
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID.</param>
        ''' <exception cref="VertexDoesntExistException">If bad vertex ID.</exception>
        ''' <returns>
        ''' <c>true</c> if the specified vertex is a sink; otherwise, <c>false</c>.
        ''' </returns>
        Public Function IsSink(ByVal plngVertexID As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            Return mdctSinkVertices.Keys.Contains(plngVertexID)
        End Function

        ''' <summary>
        ''' Determines whether the specified vertex ID is a source (has no incoming edges).
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID.</param>
        ''' <exception cref="VertexDoesntExistException">If bad vertex ID.</exception>
        ''' <returns>
        ''' <c>true</c> if the specified vertex is a source; otherwise, <c>false</c>.
        ''' </returns>
        Public Function IsSource(ByVal plngVertexID As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            Return mdctSourceVertices.Keys.Contains(plngVertexID)
        End Function

        ''' <summary>
        ''' Adds a new, disconnected vertex to the graph: this is a source vertex by definition, 
        ''' also a sink vertex, needs to be marked as both: AddSourceVertex function will handle it.
        ''' </summary>
        ''' <param name="pvpPayload">Payload for the new vertex.</param>
        ''' <returns>The new vertex's ID.</returns>
        Public Overrides Function AddNewVertex(Optional ByRef pvpPayload As GraphVertexPayload = Nothing) As Long
            Return AddSourceVertex(pvpPayload)
        End Function

        ''' <summary>
        ''' Adds a new vertex to the graph with the specified payload, connected to the specified vertex.  
        ''' Creates a single new edge with the specified payload to connect the new vertex.
        ''' Edge goes from Old Vertex to New Vertex.
        ''' </summary>
        ''' <param name="plngExistingVertexID">The vertex to create the new vertex off of.</param>
        ''' <param name="pvpVertexPayload">The payload of the new vertex to create.</param>
        ''' <param name="pepEdgePayload">The payload of the new edge to create.</param>
        ''' <returns>VertexID of new vertex.</returns>
        Public Overrides Function AddNewVertex(ByVal plngExistingVertexID As Long, _
                                Optional ByRef pvpVertexPayload As GraphVertexPayload = Nothing, _
                                Optional ByRef pepEdgePayload As GraphEdgePayload = Nothing) As Long
            Dim lngNewVertexID As Long = MyBase.AddNewVertex(plngExistingVertexID, pvpVertexPayload, pepEdgePayload)
            Dim vNew As clsDirectedGraphVertex(Of GraphVertexPayload) = CType(mdctVertices(lngNewVertexID), clsDirectedGraphVertex(Of GraphVertexPayload))

            'This new vertex we've added should automatically be a sink: it can't
            'have any outgoing edges yet.
            mdctSinkVertices.Add(vNew.VertexID, vNew)

            'If the existing vertex we connected it to WAS a sink, we'll
            'have to remove its sink status (it now has a new outgoing edge)
            If mdctSinkVertices.Keys.Contains(plngExistingVertexID) Then
                mdctSinkVertices.Remove(plngExistingVertexID)
            End If

            Return vNew.VertexID
        End Function

        ''' <summary>
        ''' Adds a new edge connecting two existing vertices.
        ''' Throws exception if the vertices don't actually exist.
        ''' </summary>
        ''' <param name="plngStartVertexID">The vertex ID where the edge starts.</param>
        ''' <param name="plngEndVertexID">The vertex ID where the edge ends.</param>
        ''' <returns></returns>
        Public Overrides Function AddNewEdge(ByVal plngStartVertexID As Long, ByVal plngEndVertexID As Long, Optional ByRef pepEdgePayload As GraphEdgePayload = Nothing) As Long
            Dim lngNewEdgeID As Long = MyBase.AddNewEdge(plngStartVertexID, plngEndVertexID, pepEdgePayload)

            'Now check if the vertex we added an outgoing edge to WAS a sink,
            'it will be no more
            If mdctSinkVertices.Keys.Contains(plngStartVertexID) Then
                mdctSinkVertices.Remove(plngStartVertexID)
            End If

            'Now check if the vertex we added an incoming edge to WAS a source,
            'it will be no more
            If mdctSourceVertices.Keys.Contains(plngEndVertexID) Then
                mdctSourceVertices.Remove(plngEndVertexID)
            End If

            Return lngNewEdgeID
        End Function

        ''' <summary>
        ''' Removes the vertex from the graph.  
        ''' Throws exception if the vertex does not exist.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to remove.</param>
        ''' <param name="pblnRemoveAttachedEdges">if set to <c>true</c> [remove attached edges].</param>
        Public Overrides Sub RemoveVertex(ByVal plngVertexID As Long, Optional ByVal pblnRemoveAttachedEdges As Boolean = True)
            MyBase.RemoveVertex(plngVertexID)

            If mdctSinkVertices.Keys.Contains(plngVertexID) Then
                mdctSinkVertices.Remove(plngVertexID)
            End If
            If mdctSourceVertices.Keys.Contains(plngVertexID) Then
                mdctSourceVertices.Remove(plngVertexID)
            End If
        End Sub

        ''' <summary>
        ''' Removes an edge from the graph.
        ''' </summary>
        ''' <param name="plngEdgeID">The edge ID to remove.</param>
        Public Overrides Sub RemoveEdge(ByVal plngEdgeID As Long)
            If Not mdctEdges.ContainsKey(plngEdgeID) Then Throw New EdgeDoesntExistException(plngEdgeID)

            Dim eCurr As clsDirectedGraphEdge(Of GraphEdgePayload) = CType(mdctEdges(plngEdgeID), clsDirectedGraphEdge(Of GraphEdgePayload))
            Dim lngStartVertexID As Long = eCurr.StartVertexID
            Dim lngEndVertexID As Long = eCurr.EndVertexID

            MyBase.RemoveEdge(plngEdgeID)

            'Check if start vertex is now a sink
            If VerifySink(lngStartVertexID) Then
                mdctSinkVertices.Add(lngStartVertexID, CType(mdctVertices(lngStartVertexID), clsDirectedGraphVertex(Of GraphVertexPayload)))
            End If

            'Check if end vertex is now a source
            If VerifySource(lngEndVertexID) Then
                mdctSourceVertices.Add(lngEndVertexID, CType(mdctVertices(lngEndVertexID), clsDirectedGraphVertex(Of GraphVertexPayload)))
            End If
        End Sub

        ''' <summary>
        ''' Sets the end vertex of an edge.
        ''' </summary>
        ''' <exception cref="VertexDoesntExistException">if the new end vertex isn't real</exception>
        ''' <exception cref="EdgeDoesntExistException">if the edge doesn't exist</exception>
        ''' <param name="plngEdgeID">The edge vertex to swap the endpoint on.</param>
        ''' <param name="plngReplacementVertexID">The replacement end vertex ID.</param>
        Public Sub SetEdgeEnd(ByVal plngEdgeID As Long, ByVal plngReplacementVertexID As Long)
            If Not mdctEdges.ContainsKey(plngEdgeID) Then Throw New EdgeDoesntExistException(plngEdgeID)
            If Not mdctVertices.ContainsKey(plngReplacementVertexID) Then Throw New VertexDoesntExistException(plngReplacementVertexID)

            Dim eSwap As clsDirectedGraphEdge(Of GraphEdgePayload) = CType(mdctEdges(plngEdgeID), clsDirectedGraphEdge(Of GraphEdgePayload))
            Dim vOldEnd As clsDirectedGraphVertex(Of GraphVertexPayload) = CType(mdctVertices(eSwap.EndVertexID), clsDirectedGraphVertex(Of GraphVertexPayload))
            Dim vNewEnd As clsDirectedGraphVertex(Of GraphVertexPayload) = CType(mdctVertices(plngReplacementVertexID), clsDirectedGraphVertex(Of GraphVertexPayload))

            'Change the end vertex of the edge using the friend accessibility
            If eSwap.StartVertexID = eSwap.VertexID1 Then
                eSwap.mlngVertexID2 = vNewEnd.VertexID
            Else
                eSwap.mlngVertexID1 = vNewEnd.VertexID
            End If

            'Check if the old end vertex (which now lacks an incoming edge)
            'is now a source
            If VerifySource(vOldEnd.VertexID) Then
                mdctSourceVertices.Add(vOldEnd.VertexID, vOldEnd)
            End If

            'Check if the new end vertex (which has now gained an incoming edge)
            'is no longer a source
            If Not VerifySource(vNewEnd.VertexID) Then
                mdctSourceVertices.Remove(vNewEnd.VertexID)
            End If
        End Sub

        ''' <summary>
        ''' Gets all valid source->sink paths.
        ''' </summary>
        ''' <returns>A list of lists, where each list is a list of vertex ids, starting with the source, ending with the sink.</returns>
        Public Function GetAllSourceSinkPaths() As List(Of List(Of Long))
            Dim lstPaths As New List(Of List(Of Long))
            Dim vCurr As clsDirectedGraphVertex(Of GraphVertexPayload)

            For Each lngSource As Long In mdctSourceVertices.Keys

            Next

            Return lstPaths
        End Function
#End Region

#Region "Private Helpers"
        ''' <summary>
        ''' Verifies the source-ness of a vertex: checks each vertex for incoming edges.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to check.</param>
        ''' <returns><c>true</c> if no incoming edges detected, false otherwise.</returns>
        Protected Function VerifySource(ByVal plngVertexID As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            For Each lngEdgeID As Long In mdctVertices(plngVertexID).Edges
                If CType(mdctEdges(lngEdgeID), clsDirectedGraphEdge(Of GraphEdgePayload)).EndVertexID = plngVertexID Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' Verifies the sink-ness of a vertex: checks each vertex for outgoing edges.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to check.</param>
        ''' <returns><c>true</c> if no outgoing edges detected, false otherwise.</returns>
        Protected Function VerifySink(ByVal plngVertexID As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            For Each lngEdgeID As Long In mdctVertices(plngVertexID).Edges
                If CType(mdctEdges(lngEdgeID), clsDirectedGraphEdge(Of GraphEdgePayload)).StartVertexID = plngVertexID Then
                    Return False
                End If
            Next

            Return True
        End Function
#End Region
    End Class
End Namespace