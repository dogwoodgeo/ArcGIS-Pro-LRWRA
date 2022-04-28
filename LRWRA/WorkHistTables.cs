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
    internal class WorkHistTables : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    var mapView = MapView.Active.Map;
                    string urlMH = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.sewerman.tblEAM_Manhole_Work";
                    string urlLines = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.sewerman.tblEAM_Sewer_Work";
                    Uri mhURI = new Uri(urlMH);
                    Uri linesURI = new Uri(urlLines);

                    // Check to see if the tables are already added to the map.
                    IReadOnlyList<StandaloneTable> mhTables = mapView.FindStandaloneTables("Manholes Work History");
                    IReadOnlyList<StandaloneTable> slTables = mapView.FindStandaloneTables("Sewer Lines Work History");
                    {
                        if (mhTables.Count == 0 && slTables.Count == 0)
                        {
                            StandaloneTable manholesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(mhURI, mapView, "Manholes Work History");
                            SysModule.SetDisplayField(manholesHistory, "Manholes Work History", "INIT_DATE");
                            StandaloneTable linesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(linesURI, mapView, "Sewer Lines Work History");
                            SysModule.SetDisplayField(linesHistory, "Sewer Lines Work History", "INIT_DATE");
                        }
                        else if (mhTables.Count > 0 && slTables.Count == 0)
                        {
                            StandaloneTable linesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(linesURI, mapView, "Sewer Lines Work History");
                            SysModule.SetDisplayField(linesHistory, "Sewer Lines Work History", "INIT_DATE");
                            MessageBox.Show("'Manholes Work History' table is already present in map.\n\n'Sewer Lines Work History' table has been added", "Warning");
                        }

                        else if (mhTables.Count == 0 && slTables.Count > 0)
                        {
                            StandaloneTable manholesHistory = StandaloneTableFactory.Instance.CreateStandaloneTable(mhURI, mapView, "Manholes Work History");
                            SysModule.SetDisplayField(manholesHistory, "Manholes Work History", "INIT_DATE");
                            MessageBox.Show("'Sewer Lines Work History' table is already present in map.\n\n'Manholes Work History' table has been added", "Warning");
                        }

                        else if (mhTables.Count > 0 && slTables.Count > 0)
                        {
                            MessageBox.Show("Sewer Lines and Manholes Work History tables are already present in map.", "Warning");
                        }
                    }
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Failed to Load Tables";
                    string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your GIS Admin.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
            });
        }
    }
}
