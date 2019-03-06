using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class GetWorkHistory : MapTool
    {
        string compkey;
        string unitID;

        public GetWorkHistory()
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
                SysModule.SewersStatus();

            });
            //return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {
                var standaloneTable = await QueuedTask.Run(() =>
                {
                    ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);

                    var map = MapView.Active.Map;
                    var mh = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(m => m.Name == "Manholes");
                    var lines = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "Sewer Lines");

                    // Get the number of selections for manholes layer
                    var selectedFeatures = map.GetSelection();
                    var mhSelectCount = mh.SelectionCount;
                    var linesSelectCount = lines.SelectionCount;
                    Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

                    // If Manholes layer has selection, remove lines selection and query work history for manhole selected.
                    if (mhSelectCount == 1)
                    {
                        lines.ClearSelection();
                        unitID = SysModule.GetUnitID();
                        //Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

                        // Set up Geodatabase Object)
                        using (Geodatabase geodatabase = new Geodatabase(new DatabaseConnectionFile(path)))
                        {
                                string queryString = $"UNITID = '{unitID}'";
                                QueryDef queryDef = new QueryDef()
                                {
                                    Tables = "SDE.sewerman.tblV8MHWorkhist",
                                    WhereClause = queryString,
                                };

                                QueryTableDescription queryTableDescription = new QueryTableDescription(queryDef)
                                {
                                    MakeCopy = true,
                                    Name = $"Manhole work history: {unitID}",
                                    PrimaryKeys = geodatabase.GetSQLSyntax().QualifyColumnName("SDE.sewerman.tblV8MHWorkhist", "UNITID")
                                };

                                var queryTable = geodatabase.OpenQueryTable(queryTableDescription);
                                int count = queryTable.GetCount();
                                if (count == 0)
                                {
                                    MessageBox.Show("Manhole selected has no work history.");
                                    return null;
                                }

                                else
                                {
                                    // Create a standalone table from the queryTable Table
                                    IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
                                    StandaloneTable whTable = tableFactory.CreateStandaloneTable(queryTable, MapView.Active.Map);

                                    return whTable;
                                }
                        }
                    }

                    // Query work history forSewer Lines selection
                    else if (linesSelectCount == 1)
                    {
                            compkey = SysModule.GetCompkey();
                            //Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

                        // Set up Geodatabase Object)
                        using (Geodatabase geodatabase = new Geodatabase(new DatabaseConnectionFile(path)))
                        {
                            string queryString = $"COMPKEY = {compkey}";
                            QueryDef queryDef = new QueryDef()
                            {
                                Tables = "SDE.sewerman.tblV8SewerWorkHist",
                                WhereClause = queryString,
                            };

                            QueryTableDescription queryTableDescription = new QueryTableDescription(queryDef)
                            {
                                MakeCopy = true,
                                Name = $"Sewer line work history: {compkey}",
                                PrimaryKeys = geodatabase.GetSQLSyntax().QualifyColumnName("SDE.sewerman.tblV8SewerWorkHist", "COMPKEY")
                            };

                            var queryTable = geodatabase.OpenQueryTable(queryTableDescription);

                            int count = queryTable.GetCount();
                            if (count == 0)
                            {
                                MessageBox.Show("Sewer line selected has no work history.");
                                return null;
                            }

                            else
                            {
                                // Create a standalone table from the queryTable Table
                                IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
                                StandaloneTable whTable = tableFactory.CreateStandaloneTable(queryTable, MapView.Active.Map);

                                return whTable;
                            }
                        }
                    }

                    else if (linesSelectCount == 0 && mhSelectCount == 0)
                    {
                        MessageBox.Show("No manhole or sewer line was selected.\n\nTry selection again.");
                    }

                    else if (linesSelectCount > 1 && mhSelectCount == 0)
                    {
                        MessageBox.Show("More than one sewer line was selected. " +
                            "\nPlease select a single sewer. " +
                            "\nZooming in may help.");
                    }

                    else if (mhSelectCount > 1 && linesSelectCount == 0)
                    {
                        MessageBox.Show("More than one manhole was selected. " +
                            "\nPlease select a single manhole. " +
                            "\nZooming in may help.");
                    }

                    else if (mhSelectCount > 1 && linesSelectCount > 1)
                    {
                        MessageBox.Show("More than one sewer feature was selected. " +
                        "\nPlease select a single feature. " +
                        "\nZooming in may help.");
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
            //return base.OnSketchCompleteAsync(geometry);
        }
         

        
    }
}
