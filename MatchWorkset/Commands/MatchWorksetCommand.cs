using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MatchWorkset.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MatchWorksetCommand : IExternalCommand
    {
        UIApplication uiApp;
        UIDocument uiDoc;
        Document dbDoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiApp = commandData.Application;
            uiDoc = commandData.Application.ActiveUIDocument;
            dbDoc = commandData.Application.ActiveUIDocument.Document;

            //don't run the command if the project isn't workshared
            if (!dbDoc.IsWorkshared)
            {
                TaskDialog.Show("Match Workset", "This project is not workshared");
                return Result.Failed;
            }

            ElementSet selElements = uiDoc.Selection.Elements;
            //don't run the command if nothing is selected
            if (0 == selElements.Size)
            {
                TaskDialog.Show("Match Workset", "No assignable elements selected");
                return Result.Failed;
            }

            Reference target;
            try
            {                
                target = uiDoc.Selection.PickObject(ObjectType.Element, new ValidTargetFilter(), "Pick an element on the target workset");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            //retrieve the Workset of the target Reference
            Workset targetWs = dbDoc.GetWorksetTable().GetWorkset( dbDoc.GetElement(target.ElementId).WorksetId );

            using (Transaction t = new Transaction(dbDoc, "Match Workset"))
            {
                t.Start();

                foreach (Element e in selElements)
                {
                    Parameter wsParam = e.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                    if (!wsParam.IsReadOnly)
                    {
                        wsParam.Set(targetWs.Id.IntegerValue);
                    }
                }

                t.Commit();
            }

            return Result.Succeeded;
        }
        
        /// <summary>
        /// Selection filter for the element picker. Only allows the selection of elements on user worksets
        /// </summary>
        class ValidTargetFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (WorksetKind.UserWorkset == GetWorksetKind(elem))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Determines whether a reference is permitted to be selected. Geometric References should never be passed to this filter
            /// so an exception is thrown.
            /// </summary>
            /// <param name="reference">the selected reference</param>
            /// <param name="position">The XYZ of the selection</param>
            /// <returns>Whether the Reference is selectable</returns>
            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new InvalidOperationException();
            }

            /// <summary>
            /// Find the WorksetKind for an element
            /// </summary>
            /// <param name="elem">the Element to check</param>
            /// <returns>The WorksetKind of the Element</returns>
            WorksetKind GetWorksetKind(Element elem)
            {
                WorksetTable wksetTbl = elem.Document.GetWorksetTable();
                return wksetTbl.GetWorkset(elem.WorksetId).Kind;
            }
        }

    }
}
