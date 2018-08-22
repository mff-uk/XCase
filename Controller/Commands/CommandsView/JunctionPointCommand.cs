using System;
using System.Collections.Generic;
using System.Windows;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Command works with the points of junctions, actual action 
	/// depends on <see cref="Action"/> field. 
	/// </summary>
	public class JunctionPointCommand : ViewCommand
	{
		/// <summary>
		/// Possible action of <see cref="JunctionPointCommand"/>
		/// </summary>
		public enum EJunctionPointAction
		{
			/// <summary>
			/// Removes inner point from junction 
			/// </summary>
			RemovePoint, 
			/// <summary>
			/// Adds inner point to junction 
			/// </summary>
			AddPoint,
			/// <summary>
			/// Moves points of a junction
			/// </summary>
			MovePoints
		}

		/// <summary>
		/// Information about moving a point
		/// </summary>
		public struct PointMoveData
		{
			/// <summary>
			/// Index of the moved point
			/// </summary>
			public int Index;
			/// <summary>
			/// Old position of the moved point
			/// </summary>
			public rPoint OldPosition;
			/// <summary>
			/// New poisition of the moved 
			/// </summary>
			public rPoint NewPosition;
		}

		/// <summary>
		/// Information about moved points. For collections of points contains list of 
		/// <see cref="PointMoveData"/> - information about moved points in the collection
		/// </summary>
		public class PointMoveDataDictionary : Dictionary<ObservablePointCollection, List<PointMoveData>> {}

		/// <summary>
		/// Actual action of the command. One of possible action must be selected. 
		/// </summary>
		[MandatoryArgument]
		public EJunctionPointAction ? Action { get; set; }

		/// <summary>
		/// Location of a new inner point of a junction. Used for 
		/// <see cref="EJunctionPointAction.AddPoint"/> action.
		/// </summary>
		public Point NewPoint { get; set; }

		/// <summary>
		/// Index where <see cref="NewPoint"/> is inserted (if <see cref="Action"/> is
		/// <see cref="EJunctionPointAction.AddPoint"/> or index from which a point is 
		/// deleted if <see cref="Action"/> is <see cref="EJunctionPointAction.RemovePoint"/>.
		/// </summary>
		public int PointIndex { get; set; }

		private Point removedPoint;

		/// <summary>
		/// Modified collection of points (for following actions: <see cref="EJunctionPointAction.RemovePoint"/> 
		/// and <see cref="EJunctionPointAction.AddPoint"/>)
		/// </summary>
		public ObservablePointCollection ViewHelperPointCollection { get; set; }

		/// <summary>
		/// Data for moving points of junctions (for <see cref="EJunctionPointAction.MovePoints"/> action)
		/// </summary>
		public PointMoveDataDictionary PointMoveDataCollection { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="JunctionPointCommand" />. 
		/// </summary>
		/// <param name="Controller">command controller</param>
		public JunctionPointCommand(DiagramController Controller)
			: base(Controller)
		{
			Description = CommandDescription.MOVE_JUNCTION_POINTS;
		}

		public override bool CanExecute()
		{
			if (Action == null)
			{
				ErrorDescription = CommandError.CMDERR_COMMAND_ACTION_MISSING;
				return false; 
			}
			switch (Action)
			{
				case EJunctionPointAction.RemovePoint:
					if (ViewHelperPointCollection == null)
					{
						ErrorDescription = String.Format(CommandError.CMDERR_POINT_COLLECTION_MISSING, Action);
						return false;
					}
					if (PointIndex == 0 || PointIndex == ViewHelperPointCollection.Count - 1)
					{
						ErrorDescription = CommandError.CMDERR_REMOVE_ENDPOINT;
						return false; 
					}
					break; 
				case EJunctionPointAction.AddPoint:
					if (ViewHelperPointCollection == null)
					{
						ErrorDescription = String.Format(CommandError.CMDERR_POINT_COLLECTION_MISSING, Action);
						return false;
					}
					if (PointIndex == 0)
					{
						ErrorDescription = CommandError.CMDERR_ADD_BEFORE_STARTPOINT;
						return false;
					}
					if (PointIndex == ViewHelperPointCollection.Count)
					{
						ErrorDescription = CommandError.CMDERR_ADD_BEHIND_ENDPOINT;
						return false; 
					}
					break;
				case EJunctionPointAction.MovePoints:
					if (PointMoveDataCollection == null)
					{
						ErrorDescription = CommandError.CMDERR_MISSING_POINTMOVEDATACOLLECTION;
						return false;
					}
					break;
				default:
					ErrorDescription = string.Format(CommandError.CMDERR_UNKNOWN_ACTION, Action);
					return false;
			}
			return true; 
		}

		internal override void CommandOperation()
		{
			switch (Action)
			{
				case EJunctionPointAction.AddPoint:
					ViewHelperPointCollection.Insert(PointIndex, new rPoint(NewPoint));
					break;
				case EJunctionPointAction.RemovePoint:
					removedPoint = ViewHelperPointCollection[PointIndex];
					ViewHelperPointCollection.RemoveAt(PointIndex);
					break;
				case EJunctionPointAction.MovePoints:
					{
						int count = 0;
						foreach (KeyValuePair<ObservablePointCollection, List<PointMoveData>> keyValuePair in PointMoveDataCollection)
						{
							foreach (PointMoveData pointMoveData in keyValuePair.Value)
							{
								keyValuePair.Key[pointMoveData.Index] = pointMoveData.NewPosition;
								count++;
							}
							keyValuePair.Key.PointsChanged();
						}
						this.Description += String.Format(CommandDescription.MOVED_POINTS, count);
					}
					break;
			}
		}

		internal override OperationResult UndoOperation()
		{
			switch (Action)
			{
				case EJunctionPointAction.RemovePoint:
					if (PointIndex == 0 || PointIndex == ViewHelperPointCollection.Count - 1)
						return OperationResult.Failed;
					ViewHelperPointCollection.Insert(PointIndex, new rPoint(removedPoint));
					break;
				case EJunctionPointAction.AddPoint:
					if (PointIndex == 0 || PointIndex == ViewHelperPointCollection.Count)
						return OperationResult.Failed;
					ViewHelperPointCollection.RemoveAt(PointIndex);
					break;
				case EJunctionPointAction.MovePoints:
					foreach (KeyValuePair<ObservablePointCollection, List<PointMoveData>> keyValuePair in PointMoveDataCollection)
					{
						foreach (PointMoveData pointMoveData in keyValuePair.Value)
						{
							keyValuePair.Key[pointMoveData.Index] = pointMoveData.OldPosition;
						}
						keyValuePair.Key.PointsChanged();
					}
					break;
			}

			return OperationResult.OK;
		}
	}

	#region JunctionPointCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="JunctionPointCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class JunctionPointCommandFactory : DiagramCommandFactory<JunctionPointCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private JunctionPointCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of JunctionPointCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new JunctionPointCommand(diagramController);
		}
	}

	#endregion
}