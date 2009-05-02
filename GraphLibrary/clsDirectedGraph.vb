Option Explicit On
Option Strict On

Imports GraphLibrary.Graph

Namespace DirectedGraph
    Public Class clsDirectedGraph(Of GraphVertexPayload, GraphEdgePayload)
        Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload)
#Region "Inner Types"
#Region "Operations"
        Public Enum enuOperationType
            FindAllNonLoopingSourceSinkPaths
        End Enum
#End Region

#Region "Vertex"
        Public Class clsDirectedGraphVertex(Of VertexPayload)
            Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload).clsVertex(Of VertexPayload)

#Region "Constructors"
            Protected Friend Sub New(ByVal plngVertexID As Long, Optional ByRef pvpPayload As VertexPayload = Nothing)
                MyBase.New(plngVertexID, pvpPayload)
            End Sub
#End Region
        End Class
#End Region

#Region "Edge"
        Public Class clsDirectedGraphEdge(Of EdgePayload)
            Inherits clsGraph(Of GraphVertexPayload, GraphEdgePayload).clsEdge(Of EdgePayload)
#Region "Member Vars"
            Protected Friend mlngStartVertexID As Long
#End Region

#Region "Constructors"
            Protected Friend Sub New(ByVal plngEdgeID As Long, ByVal plngStartVertexID As Long, ByVal plngEndVertexID As Long, Optional ByRef pepPayload As EdgePayload = Nothing)
                MyBase.New(plngEdgeID, plngStartVertexID, plngEndVertexID, pepPayload)

                mlngStartVertexID = plngStartVertexID
            End Sub
#End Region

#Region "Public Functionality"
            ''' <summary>
            ''' Reverses the direction of the edge
            ''' </summary>
            Public Sub ReverseDirection()
                mlngStartVertexID = CLng(IIf(mlngStartVertexID = mlngVertexID1, mlngVertexID2, mlngVertexID1))
            End Sub

            ''' <summary>
            ''' Returns the vertex ID where this edge originates.
            ''' </summary>
            ''' <returns>Start vertex ID.</returns>
            Public Function StartVertexID() As Long
                Return mlngStartVertexID
            End Function

            ''' <summary>
            ''' Returns the vertex ID where this edge ends.
            ''' </summary>
            ''' <returns>End vertex ID.</returns>
            Public Function EndVertexID() As Long
                Return CLng(IIf(mlngStartVertexID = mlngVertexID1, mlngVertexID2, mlngVertexID1))
            End Function
#End Region
        End Class
#End Region
#End Region

#Region "Constants"
        Protected Const MINT_FREQUENCY_OF_OPERATION_PROGRESS_STATUS_NOTIFICATIONS As Integer = 10
#End Region

#Region "Events"
        Public Event OperationProgressChanged(ByVal penuOperation As enuOperationType, ByVal plngIdx As Long, ByVal plngLimit As Long)
#End Region

#Region "Member Vars"
        Protected mdctSourceVertices As Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))
        Protected mdctSinkVertices As Dictionary(Of Long, clsDirectedGraphVertex(Of GraphVertexPayload))
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
        ''' Gets the edges joining 2 vertices in the specified order.
        ''' </summary>
        ''' <exception cref="VertexDoesntExistException">If the start or end vertices don't exist.</exception>
        ''' <param name="plngStartVertexID">The start vertex ID.</param>
        ''' <param name="plngEndVertexID">The end vertex ID.</param>
        ''' <returns>A list of the edge objects connecting the two vertices.</returns>
        Public Function GetConnectingEdges(ByVal plngStartVertexID As Long, ByVal plngEndVertexID As Long) As List(Of clsDirectedGraphEdge(Of GraphEdgePayload))
            Dim lstResult As New List(Of clsDirectedGraphEdge(Of GraphEdgePayload))
            Dim lstEdges As List(Of Long) = GetVertex(plngStartVertexID).Edges
            Dim eCurr As clsDirectedGraphEdge(Of GraphEdgePayload)

            For Each lngEdgeID As Long In lstEdges
                eCurr = GetEdge(lngEdgeID)
                If plngStartVertexID = eCurr.StartVertexID() Then
                    lstResult.Add(eCurr)
                End If
            Next

            Return lstResult
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
            Dim vNew As clsDirectedGraphVertex(Of GraphVertexPayload) = GetVertex(lngNewVertexID)

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
            Dim eNew As clsDirectedGraphEdge(Of GraphEdgePayload)

            If Not mdctVertices.Keys.Contains(plngStartVertexID) Then
                Throw New VertexDoesntExistException(plngStartVertexID)
            End If
            If Not mdctVertices.Keys.Contains(plngEndVertexID) Then
                Throw New VertexDoesntExistException(plngEndVertexID)
            End If

            eNew = New clsDirectedGraphEdge(Of GraphEdgePayload)(mlngNextEdgeID, plngStartVertexID, plngEndVertexID, pepEdgePayload)

            mdctVertices(plngStartVertexID).AddEdge(eNew.EdgeID)
            mdctVertices(plngEndVertexID).AddEdge(eNew.EdgeID)

            mdctEdges.Add(eNew.EdgeID, eNew)

            mlngNextEdgeID += 1

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

            Return eNew.EdgeID
        End Function

        ''' <summary>
        ''' Removes the vertex from the graph.  
        ''' Throws exception if the vertex does not exist.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to remove.</param>
        Public Overrides Sub RemoveVertex(ByVal plngVertexID As Long)
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

            Dim eCurr As clsDirectedGraphEdge(Of GraphEdgePayload) = GetEdge(plngEdgeID)
            Dim lngStartVertexID As Long = eCurr.StartVertexID
            Dim lngEndVertexID As Long = eCurr.EndVertexID

            MyBase.RemoveEdge(eCurr.EdgeID)

            'Check if start vertex is now a sink
            If VerifySink(lngStartVertexID) Then
                mdctSinkVertices.Add(lngStartVertexID, GetVertex(lngStartVertexID))
            End If

            'Check if end vertex is now a source
            If VerifySource(lngEndVertexID) Then
                mdctSourceVertices.Add(lngEndVertexID, GetVertex(lngEndVertexID))
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

            Dim eSwap As clsDirectedGraphEdge(Of GraphEdgePayload) = GetEdge(plngEdgeID)
            Dim vOldEnd As clsDirectedGraphVertex(Of GraphVertexPayload) = GetVertex(eSwap.EndVertexID)
            Dim vNewEnd As clsDirectedGraphVertex(Of GraphVertexPayload) = GetVertex(plngReplacementVertexID)

            'Change the end vertex of the edge using the friend accessibility
            If eSwap.StartVertexID = eSwap.VertexID1 Then
                eSwap.mlngVertexID2 = vNewEnd.VertexID
            Else
                eSwap.mlngVertexID1 = vNewEnd.VertexID
            End If

            'Now update the edge list of the old end vertex to remove the edge,
            'and add it to the new end vertex
            vOldEnd.Edges.Remove(eSwap.EdgeID)
            vNewEnd.Edges.Add(eSwap.EdgeID)

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
        ''' Gets all valid source->sink paths which do not contain loops.
        ''' </summary>
        ''' <returns>A list of lists, where each list is a list of vertex ids, starting with the source, ending with the sink.</returns>
        Public Function GetAllNonLoopingSourceSinkPaths() As List(Of List(Of Long))
            Dim lstCompletePaths As New List(Of List(Of Long))
            Dim lstWorkingPaths As New List(Of List(Of Long))
            Dim vCurr As clsDirectedGraphVertex(Of GraphVertexPayload)
            Dim lstOutgoingEdges As List(Of Long)
            Dim eCurr As clsDirectedGraphEdge(Of GraphEdgePayload)

            Dim lstCurrentPath As List(Of Long)

            'Attempt to get a quick path length, so we can use it to estimate
            'our progress.  If the function fails, complete length will be -1,
            'and we'll know we can't provide reliable estimates.
            Dim lstQuickPath As List(Of Long) = GetQuickSourceSinkPath()
            Dim lngCompletePathLength As Long
            Dim lngCompleteSearchLevelCount As Long
            Dim lngCurrentSearchLevel As Long = 0

            If Not lstQuickPath Is Nothing Then
                lngCompletePathLength = lstQuickPath.Count
                lngCompleteSearchLevelCount = lngCompletePathLength * mdctSourceVertices.Keys.Count
            Else
                lngCompleteSearchLevelCount = -1
            End If

            'Investigate each path from each source node
            For Each lngSource As Long In mdctSourceVertices.Keys
                'Create new current list, current vertex pair, using the 
                'source vertex we has iterated to, add it to a fresh working paths list
                lstCurrentPath = New List(Of Long)
                lstCurrentPath.Add(lngSource)
                lstWorkingPaths.Add(lstCurrentPath)

                'Continue until each working path from this source has hit a sink 
                '(we'll delete the paths that loop back to a previously visited vertex,
                'and we'll also remove them from this list as they hit sinks and
                'we move them to the complete paths list...so when this working
                'list is empty, we're done with this source)
                Do Until lstWorkingPaths.Count = 0
                    'Use the first list in the list of working paths
                    lstCurrentPath = lstWorkingPaths(0)
                    vCurr = GetVertex(lstCurrentPath(0))

                    If IsSink(vCurr.VertexID) Then
                        'Add the current path to the 
                        'complete paths list, and we're done with the current path,
                        'so remove it from the working paths list
                        lstCompletePaths.Add(lstCurrentPath)
                        lstWorkingPaths.RemoveAt(0)
                    Else
                        'Paths out exist, need to loop all of them and create new working path copies
                        'for the working paths list
                        lstOutgoingEdges = GetOutgoingEdges(vCurr.VertexID)
                        For Each lngEdgeID As Long In lstOutgoingEdges
                            eCurr = GetEdge(lngEdgeID)
                            vCurr = GetVertex(eCurr.EndVertexID)

                            'Prevent looping: ensure next vertex is not already in list of vertices,
                            'only continue along path if this is not the case
                            If Not lstCurrentPath.Contains(vCurr.VertexID) Then
                                lstCurrentPath = lstWorkingPaths(0) 'reset current path to first in working path
                                lstCurrentPath.Add(vCurr.VertexID)

                                'Add a copy of the current path, cloned from current path
                                lstWorkingPaths.Add(New List(Of Long)(lstCurrentPath))
                            End If
                        Next
                    End If

                    'Increment search level count, report progress if nessecary
                    lngCurrentSearchLevel += 1
                    If lngCurrentSearchLevel Mod MINT_FREQUENCY_OF_OPERATION_PROGRESS_STATUS_NOTIFICATIONS = 0 Then
                        If lngCompleteSearchLevelCount <> -1 Then
                            RaiseEvent OperationProgressChanged(clsDirectedGraph(Of GraphVertexPayload, GraphEdgePayload).enuOperationType.FindAllNonLoopingSourceSinkPaths, lngCurrentSearchLevel, lngCompleteSearchLevelCount)
                        Else
                            RaiseEvent OperationProgressChanged(clsDirectedGraph(Of GraphVertexPayload, GraphEdgePayload).enuOperationType.FindAllNonLoopingSourceSinkPaths, lngCurrentSearchLevel, Nothing)
                        End If
                    End If
                Loop
            Next

            Return lstCompletePaths
        End Function

        ''' <summary>
        ''' Quickly gets a source->sink path: using the first sink, works backwards to the
        ''' first source.  If the graph if layered (can be divided up into an ordered list of sets
        ''' of vertices, such that every edge connects a vertex in a previous set to a vertex in 
        ''' the next set, with no looping, and all sources exist in the first set, and all sinks
        ''' exist in the last set), the length of this "quick" source->sink path will be the length
        ''' of all source->sink paths, which is helpful for providing completion estimates for the
        ''' find all source->sink path process.
        ''' 
        ''' Can fail if graph is not in this layered format, or empty, etc.  No guarantees.
        ''' </summary>
        ''' <returns>List of vertex IDs in source->sink order, or nothing on fail.</returns>
        Public Function GetQuickSourceSinkPath() As List(Of Long)
            Dim lstResult As New List(Of Long)
            Dim vCurr As clsDirectedGraphVertex(Of GraphVertexPayload)

            Try
                If mdctSinkVertices.Count > 0 Then
                    'Start at first sink 
                    vCurr = mdctSinkVertices(0)
                    Do Until IsSource(vCurr.VertexID)
                        'Make sure path isn't looping
                        If lstResult.Contains(vCurr.VertexID) Then
                            Return Nothing
                        Else
                            'Add current vertex to FRONT of list (to build list in source->sink order)
                            lstResult.Insert(0, vCurr.VertexID)

                            'Continue at first parent
                            vCurr = GetVertex(GetEdge(GetIncomingEdges(vCurr.VertexID)(0)).StartVertexID)
                        End If
                    Loop

                    Return lstResult
                Else
                    'No vertices in graph, return nothing
                    Return Nothing
                End If
            Catch ex As Exception
                'Something else went wrong: I said No guarantees...
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the outgoing edges from a vertex.
        ''' </summary>
        ''' <exception cref="VertexDoesntExistException">For bad vertex IDs.</exception>
        ''' <param name="plngVertexID">The vertex ID to investigate.</param>
        ''' <returns>A list of outgoing edge IDs.</returns>
        Public Function GetOutgoingEdges(ByVal plngVertexID As Long) As List(Of Long)
            If Not mdctVertices.ContainsKey(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            Dim lstEdges As List(Of Long) = mdctVertices(plngVertexID).Edges
            Dim lstOutgoingEdges As New List(Of Long)

            For Each lngEdgeID As Long In lstEdges
                If GetEdge(lngEdgeID).StartVertexID = plngVertexID Then
                    lstOutgoingEdges.Add(lngEdgeID)
                End If
            Next

            Return lstOutgoingEdges
        End Function

        ''' <summary>
        ''' Gets the incoming edges from a vertex.
        ''' </summary>
        ''' <exception cref="VertexDoesntExistException">For bad vertex IDs.</exception>
        ''' <param name="plngVertexID">The vertex ID to investigate.</param>
        ''' <returns>A list of incoming edge IDs.</returns>
        Public Function GetIncomingEdges(ByVal plngVertexID As Long) As List(Of Long)
            If Not mdctVertices.ContainsKey(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            Dim lstEdges As List(Of Long) = mdctVertices(plngVertexID).Edges
            Dim lstIncomingEdges As New List(Of Long)

            For Each lngEdgeID As Long In lstEdges
                If GetEdge(lngEdgeID).EndVertexID = plngVertexID Then
                    lstIncomingEdges.Add(lngEdgeID)
                End If
            Next

            Return lstIncomingEdges
        End Function

        ''' <summary>
        ''' Tests for the existence of an edge connecting the two vertices in the specified order.
        ''' </summary>
        ''' <exception cref="VertexDoesntExistException">if either vertex doesn't exist</exception>
        ''' <param name="plngStartVertexID">The start vertex ID.</param>
        ''' <param name="plngEndVertexID">The end vertex ID.</param>
        ''' <returns><c>true</c> if edge exists, <c>false</c> otherwise.</returns>
        Public Function EdgeExists(ByVal plngStartVertexID As Long, ByVal plngEndVertexID As Long) As Boolean
            If Not mdctVertices.ContainsKey(plngStartVertexID) Then Throw New VertexDoesntExistException(plngStartVertexID)
            If Not mdctVertices.ContainsKey(plngEndVertexID) Then Throw New VertexDoesntExistException(plngEndVertexID)

            Dim lstEdges As List(Of Long) = mdctVertices(plngStartVertexID).Edges
            Dim eCurr As clsDirectedGraphEdge(Of GraphEdgePayload)

            For Each lngEdgeID As Long In lstEdges
                eCurr = GetEdge(lngEdgeID)
                If eCurr.StartVertexID = plngStartVertexID AndAlso _
                    eCurr.EndVertexID = plngEndVertexID Then
                    Return True
                End If
            Next

            Return False
        End Function
#End Region

#Region "Protected Helpers"
        ''' <summary>
        ''' Verifies the source-ness of a vertex: checks each vertex for incoming edges.
        ''' </summary>
        ''' <param name="plngVertexID">The vertex ID to check.</param>
        ''' <returns><c>true</c> if no incoming edges detected, false otherwise.</returns>
        Protected Function VerifySource(ByVal plngVertexID As Long) As Boolean
            If Not mdctVertices.Keys.Contains(plngVertexID) Then Throw New VertexDoesntExistException(plngVertexID)

            For Each lngEdgeID As Long In mdctVertices(plngVertexID).Edges
                If GetEdge(lngEdgeID).EndVertexID = plngVertexID Then
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
                If GetEdge(lngEdgeID).StartVertexID = plngVertexID Then
                    Return False
                End If
            Next

            Return True
        End Function
#End Region
    End Class
End Namespace
