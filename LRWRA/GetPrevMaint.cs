using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace LRWRA
{
    internal class GetPrevMaint : MapTool
    {
        string pipeID;

        public GetPrevMaint()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;
            UseSnapping = true;
        }


        protected override Task OnToolActivateAsync(bool active)
        {
            return QueuedTask.Run(() =>
            {
                SysModule.SewerLinesStatus();

            });
            //return base.OnToolActivateAsync(active);
        }


        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {
                StandaloneTable standaloneTable = await QueuedTask.Run(() =>
                {
                    ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);

                    var map = MapView.Active.Map;
                    var linesLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Sewer Lines"));

                    // Get the currently selected features in the map
                    var selectedFeatures = map.GetSelection();
                    var selectCount = linesLayer.SelectionCount;

                    if (selectCount == 0)
                    {
                        MessageBox.Show("No Sewer Line was selected.\n\nTry selection again.");
                    }

                    else if (selectCount > 1)
                    {
                        MessageBox.Show("More than one sewer line was selected.\n" +
                            "Try selecting sewer line again.\nZooming in may help.");
                    }

                    else
                    {
                        pipeID = SysModule.GetPipeID();
                        if (!string.IsNullOrEmpty(pipeID))
                        {
                            Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

                            // Set up Geodatabase Object)
                            using (Geodatabase geodatabase = new Geodatabase(new DatabaseConnectionFile(path)))
                            {
                                string queryString = $"PIPE_ID = '{pipeID}'";
                                QueryDef queryDef = new QueryDef()
                                {
                                    Tables = "SDE.sewerman.tblEAM_PM",
                                    WhereClause = queryString,
                                    SubFields = "PIPE_ID, TASK, PREVDATE, NEXTDATE, INTERVAL, UNIT, OBJECTID",
                                };

                                QueryTableDescription queryTableDescription = new QueryTableDescription(queryDef)
                                {
                                    MakeCopy = true,
                                    Name = $"Preventive Maintenance: {pipeID}",
                                    PrimaryKeys = geodatabase.GetSQLSyntax().QualifyColumnName("SDE.sewerman.tblEAM_PM", "PIPE_ID")
                                };

                                Table queryTable = geodatabase.OpenQueryTable(queryTableDescription);

                                int count = queryTable.GetCount();
                                if (count == 0)
                                {
                                    MessageBox.Show("Sewer line selected has no preventive maintenance scheduled.");

                                }

                                else
                                {
                                    // Create a standalone table from the queryTable Table
                                    IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
                                    StandaloneTable pmTable = tableFactory.CreateStandaloneTable(queryTable, MapView.Active.Map);

                                    return pmTable;
                                }
                            }
                        };
                    }
                    return null;
                });

                // Open the standalone table pane
                FrameworkApplication.Panes.OpenTablePane(standaloneTable, TableViewMode.eAllRecords);

            }

            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occured";
                string message = "Process failed.\nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
            return true;


        }
    }
}
